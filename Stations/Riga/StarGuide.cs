using System;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Riga {
	// Labākās jaunās un vecās dziesmas, SIA Star FM (www.starfm.lv)
	public class StarGuide : PollingGuide {
		/// <summary>Raidījumu saraksta izgūšanas intervāls sekundēs.</summary>
		private const int TimerTimeout=15; // Oriģinālais Star FM uztvērējs pārbauda ik pēc 15 sekundēm.

		public StarGuide() : base(TimerTimeout, System.Text.Encoding.ASCII, null) {}

		protected override async Task UpdateBroadcasts() {
			string description,
				caption=(await client.DownloadStringTaskAsync("http://www.starfm.lv/online/song.txt")).SplitCaption(out description); // Izpildītājs - dziesma (latīņu burtiem).
			if (caption.Length == 0) caption="Star FM";
			if (CurrentBroadcast == null || CurrentBroadcast.Caption != caption) {
				PreviousBroadcast=CurrentBroadcast;
				DateTime now=DateTime.Now;
				CurrentBroadcast=new Broadcast(now, now.AddSeconds(TimerTimeout), caption, description);
			}
		}
	}
}