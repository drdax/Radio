using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Pieci {
	public class PieciListedGuide : CaptionListedGuide {
		private static readonly Regex broadcastRx=new Regex(@"(?'time'[012][0-9]:[0-5][0-9]) - [012][0-9]:[0-5][0-9]\n( +<\/a>\n)? +<\/div>\n +<div class=""apraksts"">\n +(<a href=""[^""]+"" class=""none"">)?(?'caption'[^<]+?) *<", RegexOptions.Compiled | RegexOptions.ExplicitCapture); // Lapas sākumā un beigās ir \r\n, bet saturā tikai \n

		public PieciListedGuide(TimeZoneInfo timezone) : base(timezone, null) {}

		protected override async Task FillGuide(DateTime date) {
			TimeSpan lastTime=new TimeSpan(0);
			foreach (Match match in broadcastRx.Matches(await client.DownloadStringTaskAsync(
				"http://www.pieci.lv/lv/fm-programma/?day="+(date.DayOfWeek == DayOfWeek.Sunday ? 7:(int)date.DayOfWeek)))) {
				TimeSpan time=TimeSpan.Parse(match.Groups["time"].Value); // HH:mm
				if (time < lastTime) date=date.AddDays(1); // Diena sākas septiņos un beidzas nākamajā rītā.
				lastTime=time;
				AddBroadcast(date.Add(time), match.Groups["caption"].Value.Replace(" / ", Environment.NewLine));
			}
		}
	}
}