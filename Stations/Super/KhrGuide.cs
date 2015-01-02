using System;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Super {
	// Skanošās dziesmas vietā saņem HTTP 500 :( bet ICE plūsmā nav datu par dziesmām.
	/*public class KhrGuide : PollingGuide {
		/// <summary>Raidījumu saraksta izgūšanas intervāls sekundēs.</summary>
		private const int TimerTimeout=15; // Oriģinālais Krievijas hītu uztvērējs pārbauda ik pēc 15 sekundēm.

		public KhrGuide() : base(TimerTimeout, null) {}

		protected override async Task UpdateBroadcasts() {
			string description,
				caption=(await client.DownloadStringTaskAsync("http://www.hitirossii.com/prg/onair.txt")).SplitCaption(out description);
			if (CurrentBroadcast == null || CurrentBroadcast.Caption != caption) {
				PreviousBroadcast=CurrentBroadcast;
				DateTime now=DateTime.Now;
				CurrentBroadcast=new Broadcast(now, now.AddSeconds(TimerTimeout), caption, description);
			}
		}
	}*/
}