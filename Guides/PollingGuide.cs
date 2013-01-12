using System;
using System.Text;
using System.Timers;

namespace DrDax.RadioClient {
	/// <summary>Raidījumu programma, kura bieži mainās.</summary>
	/// <remarks>Faktiski modificēta TimedGuide: HasKnownDuration=false un konstants datu izgūšanas laiks.</remarks>
	public abstract class PollingGuide : Guide, IDisposable {
		private readonly Timer guideTimer=new Timer();
		/// <summary>HTTP klients raidījma datu izgūšanai.</summary>
		protected readonly ProperWebClient client=new ProperWebClient();
		public override bool HasKnownDuration { get { return false; } }

		protected PollingGuide() {
			client=new ProperWebClient();
		}
		/// <param name="clientEncoding">HTTP klienta kodu tabula.</param>
		protected PollingGuide(Encoding clientEncoding) {
			client=new ProperWebClient(clientEncoding);
		}

		/// <summary>Nomaina aktuālos raidījumus.</summary>
		protected abstract void UpdateBroadcasts();

		/// <summary>
		/// Palaiž raidījumu saraksta izgūšanas taimeri. Šī metode jāizsauc pareizā brīdī konstruktorā.
		/// </summary>
		/// <param name="timeout">Raidījumu saraksta izgūšanas biežums sekundēs.</param>
		protected void StartTimer(int timeout) {
			guideTimer.AutoReset=true;
			guideTimer.Elapsed+=guideTimer_Elapsed;
			UpdateBroadcasts();
			guideTimer.Interval=timeout*1000;
			guideTimer.Start();
		}
		private void guideTimer_Elapsed(object sender, ElapsedEventArgs e) {
			UpdateBroadcasts();
		}
		public virtual void Dispose() {
			guideTimer.Stop();
			// Noņemam cirkulāro atsauci.
			guideTimer.Elapsed-=guideTimer_Elapsed;
			client.Dispose();
		}
	}
}