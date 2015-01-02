using System;
using System.Threading.Tasks;
using System.Timers;

namespace DrDax.RadioClient {
	/// <summary>Raidījumu saraksts, kuri mainās zināmā laikā.</summary>
	public abstract class TimedGuide : Guide, IDisposable {
		/// <summary>Taimers, pēc kura iztecēšanas nomainās pašreizējais raidījums.</summary>
		private readonly Timer guideTimer=new Timer();

		/// <summary>Nomaina aktuālos raidījumus.</summary>
		protected abstract Task UpdateBroadcasts();

		public override async Task Start(bool initialize) {
			await UpdateBroadcasts();
			guideTimer.Interval=CurrentBroadcast == null ?
				Channel.DefaultTimeout : (CurrentBroadcast.EndTime-DateTime.Now).TotalMilliseconds;
			guideTimer.Start();
		}
		public override void Stop() {
			base.Stop();
			guideTimer.Stop();
		}

		protected TimedGuide(Menu<Guide> menu) : base(menu, true, false) {
			guideTimer.AutoReset=true;
			guideTimer.Elapsed+=guideTimer_Elapsed;
		}
		private async void guideTimer_Elapsed(object sender, ElapsedEventArgs e) {
			await UpdateBroadcasts();
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