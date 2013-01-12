using System;
using DrDax.RadioClient;

namespace Riga {
	// Labākās jaunās un vecās dziesmas, SIA Star FM (www.starfm.lv)
	public class StarGuide : PollingGuide {
		/// <summary>Raidījumu saraksta izgūšanas intervāls sekundēs.</summary>
		private const int TimerTimeout=15; // Oriģinālais Star FM uztvērējs pārbauda ik pēc 15 sekundēm.

		public StarGuide() : base(System.Text.Encoding.ASCII) { StartTimer(TimerTimeout); }

		protected override void UpdateBroadcasts() {
			string description,
				caption=client.DownloadString("http://www.starfm.lv/online/").SplitCaption(out description); // Izpildītājs - dziesma (latīņu burtiem).
			if (CurrentBroadcast == null || CurrentBroadcast.Caption != caption) {
				PreviousBroadcast=CurrentBroadcast;
				DateTime now=DateTime.Now;
				CurrentBroadcast=new Broadcast(now, now.AddSeconds(TimerTimeout), caption, description);
			}
		}
	}
}