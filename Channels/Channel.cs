using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DrDax.RadioClient {
	/// <summary>Radio kanāls.</summary>
	public abstract class Channel : INotifyPropertyChanged, IDisposable {
		private readonly Brand brand;
		private string caption;
		private string id;
		private uint number;
		private Station station;
		private readonly BitmapSource logo;
		private Guide guide;
		/// <summary>Programmas noformējums pēc noklusējuma.</summary>
		private static Brand defaultBrand;
		/// <summary>Taimeru intervāls pēc noklusējuma milisekundēs.</summary>
		public const double DefaultTimeout=60000; // Minūte.
		/// <summary>Taimers, pēc kura iztecēšanas klusums pārtop apstādinātā atskaņošanā.</summary>
		/// <remarks>Ieviests, lai ekonomētu trafiku, kad radio netiek klausīts.</remarks>
		private Timer muteTimer;
		/// <summary>Taimers, kurš atkārtoti palaiž atskaņošanu, ja tā tika neparedzēti pārtraukta.</summary>
		private Timer restartTimer;
		/// <summary>Vai pilnīga klusināšana notika pēc taimera (<c>true</c>) vai ārēja iemesla pēc (<c>false</c>).</summary>
		private bool stoppedByTimer=false;
		private volatile PlaybackState playbackState=PlaybackState.Stopped;

		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>Raidstacijas laika josla.</summary>
		/// <remarks>Izmanto studijas laika aprēķinam. Drīkst nenorādīt.</remarks>
		public readonly TimeZoneInfo Timezone;
		/// <summary>Ar kanālu saistīto iespēju izvēlne.</summary>
		public readonly Menu<Channel> Menu;
		/// <summary>Informācija par pašreizējiem raidījumiem.</summary>
		public Guide Guide {
			get { return guide; }
			#if DEBUG
			internal set { guide=value; } // Iestatāma, lai varētu attēlot izstrādes vides dizainerī.
			#endif
		}
		/// <summary>Vai kanālam ir dati par pašreizējiem raidījumiem.</summary>
		public readonly bool HasGuide;
		/// <summary>Raidstacijas ikona, kuru piešķir programmas logam.</summary>
		public BitmapSource Icon;
		/// <summary>Programmas saskarnes krāsas saskaņā ar kanāla noformējumu.</summary>
		public Brand Brand { get { return brand; } }
		/// <summary>Kanāla nosaukums.</summary>
		public string Caption {
			get { return caption; }
			internal set { caption=value; }
		}
		/// <summary>Kanāla identifikators formā {stacijas nosaukums}.{kanāla numurs}.</summary>
		public string Id {
			get { return id; }
			internal set { id=value; }
		}
		/// <summary>Kanāla numurs stacijas ietvaros.</summary>
		public uint Number {
			get { return number; }
			internal set { number=value; }
		}
		/// <summary>Kanāla stacija raidījumu saraksta un mājaslapas iegūšanai.</summary>
		internal Station Station {
			get { return station; }
			set { station=value; }
		}
		/// <summary>Kanāla mājaslapas pilna adrese, ja tāda zināma.</summary>
		public string HomepageUrl { get { return station != null ? station.GetHomepage(number):null; } }
		/// <summary>Stacijas vai kanāla logotips.</summary>
		public BitmapSource Logo { get { return logo; } }
		/// <summary>Radio signāla atskaņošanas stāvoklis.</summary>
		public PlaybackState PlaybackState {
			get { return playbackState; }
			protected set { playbackState=value; NotifiyPropertyChanged("PlaybackState"); }
		}
		/// <summary>
		/// Vai skaņa ir izslēgta (<c>true</c>) vai ieslēgta (<c>false</c>).
		/// </summary>
		public bool IsMuted {
			get { // MmsChannel pēc apstāšanās nepareizi atgriež klusuma stāvokli, tāpēc vadās pēc stoppedByTimer.
				return stoppedByTimer || GetIsMuted();
			}
			set {
				if (value != IsMuted) {
					if (value) {
						if (muteTimer == null) {
							muteTimer=new Timer(DefaultTimeout);
							muteTimer.Elapsed+=muteTimer_Elapsed;
							muteTimer.AutoReset=true;
						}
						muteTimer.Start();
					} else {
						if (stoppedByTimer) {
							stoppedByTimer=false;
							Play();
							if (Settings.Default.UseGuide && guide != null && guide.IsStreamDependent) guide.Start(false);
						}
						if (muteTimer != null) muteTimer.Stop();
					}
					SetIsMuted(value);
					NotifiyPropertyChanged("IsMuted");
				}
			}
		}

		/// <summary>Skaļums no 0 līdz 1.</summary>
		/// <remarks><c>Volume=0</c> nav tas pats, kas <c>IsMuted=true</c></remarks>
		public abstract double Volume { get; set; }
		/// <returns><c>true</c>, ja skaņa ir izslēgta, vai <c>false</c>, ja tā ir ieslēgta.</returns>
		protected abstract bool GetIsMuted();
		/// <summary>Ieslēdz vai izslēdz skaņu.</summary>
		protected abstract void SetIsMuted(bool value);
		/// <summary>Sāk uztvert un atskaņot kanālu.</summary>
		public abstract void Play();
		/// <summary>Pārtrauc kanāla uztveršanu un atskaņošanu.</summary>
		public abstract void Stop();

		protected Channel(BitmapSource logo, TimeZoneInfo timezone, bool hasGuide, Brand brand, Menu<Channel> menu) {
			if (logo == null)
				this.logo=(BitmapImage)Application.Current.Resources["RadioLogo"];
			else this.logo=logo;
			this.Timezone=timezone; this.HasGuide=hasGuide;
			if (brand == null) {
				if (defaultBrand == null)
					defaultBrand=new Brand(0x33537C.ToColor(), 0x568CC6.ToColor(), 0xD2CFBC.ToColor(), 0x33537C.ToColor(),
						new LinearGradientBrush(
							new GradientStopCollection {
								new GradientStop(0x191919.ToColor(), 0),
								new GradientStop(0x4F4F4F.ToColor(), 0.48),
								new GradientStop(0x191919.ToColor(), 0.48)
							}, new Point(0,0), new Point(205, 30)) { MappingMode=BrushMappingMode.Absolute },
						new LinearGradientBrush(
							new GradientStopCollection {
								new GradientStop(0xE2F1F7.ToColor(), 0),
								new GradientStop(0xB8CBD8.ToColor(), 0.85),
								new GradientStop(0xD8E7F2.ToColor(), 1)
							}, 90.0),
						new LinearGradientBrush(
							new GradientStopCollection {
								new GradientStop(0xE7F2F8.ToColor(), 0),
								new GradientStop(0xD7E3ED.ToColor(), 0.85),
								new GradientStop(0xDFECF4.ToColor(), 1)
							}, 90.0));
				this.brand=defaultBrand;
			} else this.brand=brand;
			this.Menu=menu;
			if (menu != null) menu.Source=this;
		}
		protected void NotifiyPropertyChanged(string propertyName) {
			// Šo salīdzināšanu nevar veikt apakšklasē, tāpēc tā iznesta šajā metodē.
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		/// <summary>
		/// Apstādināt atskaņošanu dēļ nelabvēlīgiem apstākļiem (tīkla savienojuma pazušanas gadījumā).
		/// </summary>
		protected void UnexpectedStop() {
			System.Diagnostics.Debug.WriteLine("Unexpected stop");
			if (restartTimer == null) {
				restartTimer=new Timer(5000) { AutoReset=true };
				restartTimer.Elapsed+=resetTimer_Elapsed;
			}
			Stop();
			restartTimer.Start();
		}
		public void SetGuide() {
			guide=station.GetGuide(number);
			NotifiyPropertyChanged("Guide");
		}

		private void resetTimer_Elapsed(object sender, ElapsedEventArgs e) {
			// if (irTīklaSavienojums) // TODO: pārbaudīt tīkla savienojumu.
			restartTimer.Stop();
			Play();
			// else
			// Palielina par piecām sekundēm kamēr sasniedz minūti.
			if (restartTimer.Interval < 60000) restartTimer.Interval+=5000;
			else restartTimer.Interval=1000;
			//restartTimer.Start()
		}
		private void muteTimer_Elapsed(object sender, ElapsedEventArgs e) {
			System.Diagnostics.Debug.WriteLine("Mute timer elapsed");
			stoppedByTimer=true;
			muteTimer.Stop();
			Stop();
			if (Settings.Default.UseGuide && guide != null && guide.IsStreamDependent) guide.Stop();
		}

		public virtual void Dispose() {
			// Noņemam cirkulāro atsauci.
			if (muteTimer != null) muteTimer.Elapsed-=muteTimer_Elapsed;
			if (restartTimer != null) restartTimer.Elapsed-=resetTimer_Elapsed;
			if (Menu != null) Menu.Dispose();
			if (guide is IDisposable) ((IDisposable)guide).Dispose();
		}
	}
}