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
		private readonly BitmapSource logo;
		private readonly Guide guide;
		/// <summary>Programmas noformējums pēc noklusējuma.</summary>
		private static Brand defaultBrand;
		/// <summary>Taimeru intervāls pēc noklusējuma.</summary>
		public const double DefaultTimeout=60000; // Minūte.
		/// <summary>Taimers, pēc kura iztecēšanas klusums pārtop apstādinātā atskaņošanā.</summary>
		/// <remarks>Ieviests, lai ekonomētu trafiku, kad radio netiek klausīts.</remarks>
		private Timer muteTimer;
		/// <summary>Taimers, kurš atkārtoti palaiž atskaņošanu, ja tā tika neparedzēti pārtraukta.</summary>
		private Timer restartTimer;
		/// <summary>Vai pilnīga klusināšana notika pēc taimera (<c>true</c>) vai ārēja iemesla pēc (<c>false</c>).</summary>
		private bool stoppedByTimer=false;
		/// <summary>Kanāla raidošā interneta adrese.</summary>
		protected readonly string url;
		private volatile PlaybackState playbackState=PlaybackState.Stopped;

		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>Raidstacijas laika josla.</summary>
		/// <remarks>Izmanto studijas laika aprēķinam. Drīkst nenorādīt.</remarks>
		public readonly TimeZoneInfo Timezone;
		/// <summary>Informācija par pašreizējiem raidījumiem.</summary>
		public Guide Guide { get { return guide; } }
		/// <summary>Vai kanālam ir dati par pašreizējiem raidījumiem. Ņem vērā lietotāja iestatījumus.</summary>
		public bool HasGuide { get { return Station.UseGuide && guide != null; } }
		/// <summary>Raidstacijas ikona, kuru piešķir programmas logam.</summary>
		public BitmapSource Icon;
		/// <summary>Programmas saskarnes krāsas saskaņā ar kanāla noformējumu.</summary>
		public Brand Brand { get { return brand; } }
		/// <summary>Kanāla nosaukums.</summary>
		public string Caption {
			get { return caption; }
			internal set { caption=value; }
		}
		/// <summary>Kanāla identifikators formā {stacijas nosaukums}.{kanāla numurs}</summary>
		public string Id {
			get { return id; }
			internal set { id=value; }
		}
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

		protected Channel(string url, BitmapImage logo, TimeZoneInfo timezone, Guide guide, Brand brand) {
			if (logo == null)
				this.logo=(BitmapImage)Application.Current.Resources["RadioLogo"];
			else this.logo=logo;
			this.url=url; this.Timezone=timezone; this.guide=guide;
			if (brand == null) {
				if (defaultBrand == null)
					defaultBrand=new Brand(0x33537C.ToColor(), 0x568CC6.ToColor(), 0x33537C.ToColor(),
						0xC6D9ED.ToColor(), 0xB7D2ED.ToColor(), 0xB7D2ED.ToColor(), 0xC6D9ED.ToColor());
				this.brand=defaultBrand;
			} else this.brand=brand;
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
		}

		public virtual void Dispose() {
			// Noņemam cirkulāro atsauci.
			if (muteTimer != null) muteTimer.Elapsed-=muteTimer_Elapsed;
			if (restartTimer != null) restartTimer.Elapsed-=resetTimer_Elapsed;
			if (guide is IDisposable) ((IDisposable)guide).Dispose();
		}
	}
}