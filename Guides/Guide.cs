using System;
using System.ComponentModel;

namespace DrDax.RadioClient {
	/// <summary>Tuvākā laika raidījumu programma.</summary>
	public abstract class Guide : INotifyPropertyChanged {
		private Broadcast previousBroadcast=null;
		private Broadcast currentBroadcast;
		private Broadcast nextBroadcast=null;
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
		/// <summary>
		/// Tipizēts PropertyChanged notikums, kurš iestājas, kad nomainās pašreizējais raidījums.
		/// </summary>
		public event Action<Broadcast> CurrentBroadcastChanged;
		/// <summary>
		/// <c>true</c>, ja raidījumiem ir zināms sākuma un beigu laiks,
		/// vai <c>false</c>, ja zināms tikai nosaukums.
		/// </summary>
		/// <remarks>Nepieciešams, lai atslēgtu maldinošo atskaņošanas progresa indikatoru.</remarks>
		public abstract bool HasKnownDuration { get; }

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged(string propertyName) {
			if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}