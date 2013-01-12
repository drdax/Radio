using System;
using DrDax.RadioClient;

namespace Super {
	public class KhrGuide : PollingGuide {
		/// <summary>Raidījumu saraksta izgūšanas intervāls sekundēs.</summary>
		private const int TimerTimeout=15; // Oriģinālais Krievijas hītu uztvērējs pārbauda ik pēc 15 sekundēm.

		public KhrGuide() { StartTimer(TimerTimeout); }

		protected override void UpdateBroadcasts() {
			string description,
				caption=client.DownloadString("http://www.hitirossii.com/prg/onair.txt").SplitCaption(out description);
			if (CurrentBroadcast == null || CurrentBroadcast.Caption != caption) {
				PreviousBroadcast=CurrentBroadcast;
				DateTime now=DateTime.Now;
				CurrentBroadcast=new Broadcast(now, now.AddSeconds(TimerTimeout), caption, description);
			}
		}
	}
}