using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Riga {
	public class TopGuide : CombinedIcyGuide {
		internal TopGuide(TimeZoneInfo timezone) : base(new TopListedGuide(timezone), Encoding.ASCII) {}

		protected override Task<Broadcast> GetBroadcast(string title) {
			if (title == null) return Task.FromResult(listedGuide.CurrentBroadcast);
			title=title.Substring(0, title.Length-27); // 28 = len(" TOPradio - Dance HIT MUSIC")
			if (title.Length == 0) return Task.FromResult(listedGuide.CurrentBroadcast);
			string description, caption=title.SplitCaption(out description);
			DateTime now=DateTime.Now;
			return Task.FromResult(new Broadcast(now, now.AddMinutes(3), caption, description));
		}

		private class TopListedGuide : CaptionListedGuide {
			/// <summary>Raidījuma datu regulārais izteikums.</summary>
			private static readonly Regex guideRx=new Regex(@"width=""102""><strong>(?'hours'[012][0-9])[:\.](?'minutes'[0-5][0-9])-[012][0-9][:\.]{1,2}[0-5][0-9]<\/strong><\/td>\s+<td width=""455"">(?'caption'[^<]+)<", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

			public TopListedGuide(TimeZoneInfo timezone) : base(timezone, null) {}

			protected override async Task FillGuide(DateTime date) {
				TimeSpan lastTime=new TimeSpan(0);
				foreach (Match match in guideRx.Matches(await client.DownloadStringTaskAsync(
					"http://topradio.lv/index.php?name=data&group_id=" + (date.DayOfWeek == DayOfWeek.Sunday ? 44:37+(int)date.DayOfWeek)))) {
					TimeSpan time=new TimeSpan(int.Parse(match.Groups["hours"].Value), int.Parse(match.Groups["minutes"].Value), 0); // HH:mm
					if (time < lastTime) break; // Pēc dienas beigām mēdz parādīties lieki raidījumi.
					AddBroadcast(date.Add(time), match.Groups["caption"].Value);
				}
			}
		}
	}
}