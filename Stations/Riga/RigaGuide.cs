using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Riga {
	public class RigaGuide : TimedGuide {
		internal RigaGuide(TimeZoneInfo timezone) : base(null) {
			this.timezone=timezone;
		}
		public override void Dispose() {
			base.Dispose(); client.Dispose();
		}
		protected override async Task UpdateBroadcasts() {
			MatchCollection songs=songRx.Matches(await client.DownloadStringTaskAsync("http://www.rigaradio.lv/skan-un-skaneja")); // Pēdējās 10 dziesmas.
			var today=TimeZoneInfo.ConvertTime(DateTime.Today, TimeZoneInfo.Local, timezone);
			var song=songs[0].Groups;
			var times=GetTimes(song, today);
			// Iepriekšejā dziesma
			if (CurrentBroadcast == null) {
				if (times.Item2 < DateTime.Now) PreviousBroadcast=GetBroadcast(times.Item1, times.Item2, song);
				else {
					var times2=GetTimes(songs[1].Groups, today);
					bool daySwitch=times2.Item1 > times.Item1; // Vai notika pāreja starp diennaktīm, kad sāka pašreizējo dziesmu.
					PreviousBroadcast=GetBroadcast(daySwitch ? times2.Item1.AddDays(-1):times2.Item1, daySwitch ? times2.Item2.AddDays(-1):times2.Item2, songs[1].Groups);
				}
			} else if (CurrentBroadcast.Caption != "Rīga radio") PreviousBroadcast=CurrentBroadcast;
			// Pašreizējā dziesma
			if (times.Item2 < DateTime.Now) {
				CurrentBroadcast=new Broadcast(times.Item2, DateTime.Now.AddSeconds(20), "Rīga radio");
			} else CurrentBroadcast=GetBroadcast(times.Item1, times.Item2, song);
		}
		private Tuple<DateTime, DateTime> GetTimes(GroupCollection song, DateTime today) {
			DateTime start=TimeZoneInfo.ConvertTime(today.Add(TimeSpan.Parse(song["start"].Value)), timezone, TimeZoneInfo.Local); // hh:mm:ss
			return Tuple.Create(start, start.Add(TimeSpan.ParseExact(song["duration"].Value, "mm\\:ss", System.Globalization.CultureInfo.InvariantCulture)));
		}
		private Broadcast GetBroadcast(DateTime startTime, DateTime endTime, GroupCollection song) {
			return new Broadcast(startTime, endTime, song["caption"].Value,
				song["artist"].Value+(song["album"].Value.Length == 0 ? string.Empty:Environment.NewLine+song["album"].Value));
		}

		private readonly TimeZoneInfo timezone;
		/// <summary>HTTP klients dziesmu saraksta noskaidrošanai.</summary>
		private readonly ProperWebClient client=new ProperWebClient();
		/// <summary>Dziesmas informācija tabulā.</summary>
		private static readonly Regex songRx=new Regex(@"(?'start'[012][0-9]:[0-5][0-9]:[0-5][0-9])<\/td>\n			<td>(?'duration'[0-9][0-9]:[0-5][0-9])<\/td>\n			<td>(?'artist'[^<]+)<\/td>\n			<td>(?'caption'[^<]+)<\/td>\n			<td>(?'album'[^<]*)", RegexOptions.Compiled);
	}
}