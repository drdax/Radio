using System;
using System.Timers;

namespace DrDax.RadioClient {
	/// <summary>Raidījumu saraksts, kuri mainās zināmā laikā.</summary>
	public abstract class TimedGuide : Guide, IDisposable {
		/// <summary>Taimers, pēc kura iztecēšanas nomainās pašreizējais raidījums.</summary>
		private readonly Timer guideTimer=new Timer();

		public override bool HasKnownDuration { get { return true; } }

		/// <summary>Nomaina aktuālos raidījumus.</summary>
		protected abstract void UpdateBroadcasts();

		/// <summary>
		/// Palaiž raidījumu saraksta izgūšanas taimeri. Šī metode jāizsauc pareizā brīdī konstruktorā.
		/// </summary>
		protected void StartTimer() {
			guideTimer.AutoReset=true;
			guideTimer.Elapsed+=guideTimer_Elapsed;
			UpdateBroadcasts();
			guideTimer.Interval=CurrentBroadcast == null ?
				Channel.DefaultTimeout : (CurrentBroadcast.EndTime-DateTime.Now).TotalMilliseconds;
			guideTimer.Start();
		}
		private void guideTimer_Elapsed(object sender, ElapsedEventArgs e) {
			UpdateBroadcasts();
			guideTimer.Interval=CurrentBroadcast == null ?
				Channel.DefaultTimeout : (CurrentBroadcast.EndTime-DateTime.Now).TotalMilliseconds;
		}
		public virtual void Dispose() {
			guideTimer.Stop();
			// Noņemam cirkulāro atsauci.
			guideTimer.Elapsed-=guideTimer_Elapsed;
		}
	}
}