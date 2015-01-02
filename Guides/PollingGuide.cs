using System;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DrDax.RadioClient {
	/// <summary>Raidījumu programma, kura bieži mainās.</summary>
	/// <remarks>Faktiski modificēta TimedGuide: HasKnownDuration=false un konstants datu izgūšanas laiks.</remarks>
	public abstract class PollingGuide : Guide, IDisposable {
		private readonly Timer guideTimer=new Timer();
		/// <summary>HTTP klients raidījma datu izgūšanai.</summary>
		protected readonly ProperWebClient client=new ProperWebClient();

		protected PollingGuide(int timerTimeout, Menu<Guide> menu) : this(timerTimeout, Encoding.UTF8, menu) {}
		/// <param name="timerTimeout">Raidījumu saraksta izgūšanas biežums sekundēs.</param>
		/// <param name="clientEncoding">HTTP klienta kodu tabula.</param>
		protected PollingGuide(int timerTimeout, Encoding clientEncoding, Menu<Guide> menu) : base(menu, false, false) {
			guideTimer.Interval=timerTimeout*1000;
			client.Encoding=clientEncoding;
			guideTimer.AutoReset=true;
			guideTimer.Elapsed+=guideTimer_Elapsed;
		}

		/// <summary>Nomaina aktuālos raidījumus.</summary>
		protected abstract Task UpdateBroadcasts();

		public override async Task Start(bool initialize) {
			await UpdateBroadcasts();
			guideTimer.Start();
		}
		public override void Stop() {
			base.Stop();
			guideTimer.Stop();
		}
		private void guideTimer_Elapsed(object sender, ElapsedEventArgs e) {
			UpdateBroadcasts();
		}
		public virtual void Dispose() {
			guideTimer.Stop();
			// Noņemam cirkulāro atsauci.
			guideTimer.Elapsed-=guideTimer_Elapsed;
		}
	}
}