using System;
using System.Text;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Riga {
	/// <summary>Raidījumu saraksts, kurš pa daļai datus ņem no plūsmas un pa daļai no dienas saraksta.</summary>
	public abstract class CombinedIcyGuide : IcyGuide, IDisposable {
		protected readonly CaptionListedGuide listedGuide;

		protected CombinedIcyGuide(CaptionListedGuide listedGuide, Encoding icyEncoding) : base(icyEncoding, null) {
			this.listedGuide=listedGuide;
			listedGuide.NextBroadcastChanged+=listedGuide_NextBroadcastChanged;
		}

		public override async Task Start(bool initialize) {
			await listedGuide.Start(initialize);
			PreviousBroadcast=listedGuide.PreviousBroadcast;
			CurrentBroadcast=listedGuide.CurrentBroadcast;
			NextBroadcast=listedGuide.NextBroadcast;
			await base.Start(initialize);
		}
		public override void Stop() {
			listedGuide.Stop(); base.Stop();
		}
		public void Dispose() {
			listedGuide.NextBroadcastChanged-=listedGuide_NextBroadcastChanged;
			listedGuide.Dispose();
		}

		private void listedGuide_NextBroadcastChanged(Broadcast nextBroadcast) {
			NextBroadcast=nextBroadcast;
		}
	}
}