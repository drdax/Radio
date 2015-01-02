using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrDax.RadioClient {
	/// <summary>Tuvākā laika raidījumu programma.</summary>
	public abstract class Guide : INotifyPropertyChanged {
		private Broadcast previousBroadcast=null;
		private Broadcast currentBroadcast;
		private Broadcast nextBroadcast=null;
		/// <summary>HTML tegu vienkāršots regulārais izteikums.</summary>
		protected static readonly Regex htmlRx=new Regex("<[^>]+>", RegexOptions.Compiled);
		/// <summary>Null raidījuma vietā atgriešanai no sinhronām async metodēm.</summary>
		protected static readonly Task<Broadcast> NullTaskBroadcast=Task.FromResult<Broadcast>(null);

		public readonly bool HasInfoCommand;
		/// <summary>
		/// Raidījums, kurš skanēja pirs pašreizējā. Drīkst būt <c>null</c>.
		/// </summary>
		public Broadcast PreviousBroadcast {
			get { return previousBroadcast; }
			protected set {
				previousBroadcast=value;
				NotifyPropertyChanged("PreviousBroadcast");
			}
		}
		/// <summary>
		/// Pašlaik skanošais raidījums. Nedrīkst būt <c>null</c>.
		/// </summary>
		public Broadcast CurrentBroadcast {
			get { return currentBroadcast; }
			protected set {
				currentBroadcast=value;
				NotifyPropertyChanged("CurrentBroadcast");
				if (CurrentBroadcastChanged != null) CurrentBroadcastChanged(currentBroadcast);
			}
		}
		/// <summary>
		/// Raidījums, kurš skanēs pēc pašreizējā. Drīkst būt <c>null</c>.
		/// </summary>
		public Broadcast NextBroadcast {
			get { return nextBroadcast; }
			protected set {
				nextBroadcast=value;
				NotifyPropertyChanged("NextBroadcast");
			}
		}
		public readonly Menu<Guide> Menu;
		/// <summary>
		/// Tipizēts PropertyChanged notikums, kurš iestājas, kad nomainās pašreizējais raidījums.
		/// </summary>
		public event Action<Broadcast> CurrentBroadcastChanged;
		/// <summary>
		/// <c>true</c>, ja raidījumiem ir zināms sākuma un beigu laiks,
		/// vai <c>false</c>, ja zināms tikai nosaukums.
		/// </summary>
		/// <remarks>Nepieciešams, lai atslēgtu maldinošo atskaņošanas progresa indikatoru.</remarks>
		public readonly bool HasKnownDuration;
		public readonly bool IsStreamDependent;
		/// <summary>Iedarbina raidījumu sarakstu.</summary>
		/// <param name="initialize">Vai atkārtoti jāiegūst raidījumu saraksts.</param>
		public abstract Task Start(bool initialize);
		/// <summary>Apstādina raidījumu saraksta darbību.</summary>
		public virtual void Stop() {
			CurrentBroadcast=null;
			PreviousBroadcast=null;
			NextBroadcast=null;
		}

		protected Guide(Menu<Guide> menu, bool hasKnownDuration, bool isStreamDependent) {
			this.Menu=menu; HasKnownDuration=hasKnownDuration; IsStreamDependent=isStreamDependent;
			if (menu != null) {
				menu.Source=this;
				HasInfoCommand=menu.Items.InformationIndex != -1;
			} else HasInfoCommand=false;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged(string propertyName) {
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}