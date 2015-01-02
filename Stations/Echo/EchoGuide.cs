using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Echo {
	public class EchoGuide : PagedListedGuide {
		private static readonly Regex broadcastRx=new Regex(@"<h2>(?'title'[^<]+)</h2>\n\s+<div class=""lite"">\n\s+(?'time'[0-2][0-9]:[0-5][0-9])|datetime iblock"">\n\s+(?'time'[0-2][0-9]:[0-5][0-9])\n\s+</div>\n[\s\S]+?(<div class=""title"">\n(\s+<a href=""(?'url'[^""]+)""[^>]+>\n\s+)?(?'title'[\s\S]+?)(\n\s+</a>)?\n\s+</div>\n[\s\S]+?)?<div class=""notice"">\n\s+(?'notice'.*)\n\s+</div>\n\s+<div class=""persons"">\n(\s+гости: (?'guests'[^\n]+)\n)?(\s+<br />)?(\s+ведущие: (?'presenters'[^\n]+)\n)?\s+</div>", RegexOptions.Compiled | RegexOptions.ExplicitCapture),
			newsRx=new Regex("^(?'caption'.+)\\. +Новости - +(?'time'[012][0-9]:[0-5][0-9](; )?)+", RegexOptions.Compiled | RegexOptions.ExplicitCapture),
			quoteRx=new Regex("\"([^\"]+)\"", RegexOptions.Compiled);
		internal EchoGuide(TimeZoneInfo timezone) : base(timezone, new SimpleGuideMenu("О передаче", "http://echo.msk.ru")) { }
		protected override async Task FillGuide(DateTime date) {
			Queue<TimeSpan> newsQueue=new Queue<TimeSpan>(5); TimeSpan previousTime=new TimeSpan(-1);
			foreach (Match match in broadcastRx.Matches(
				await client.DownloadStringTaskAsync(string.Concat("http://echo.msk.ru/schedule/", date.ToString(@"yyyy\-MM\-dd"), ".html")))) {
				GroupCollection groups=match.Groups;
				string title=htmlRx.Replace(groups["title"].Success ? groups["title"].Value:groups["notice"].Value, string.Empty);
				Match newsMatch=newsRx.Match(title);
				if (newsMatch.Success) {
					newsQueue.Clear();
					title=newsMatch.Groups["caption"].Value;
					foreach (Capture cap in newsMatch.Groups["time"].Captures)
						newsQueue.Enqueue(TimeSpan.Parse(cap.Value.Substring(0, 5))); // Substring lai atstātu tikai laiku.
				}

				var sb=new StringBuilder();
				if (groups["title"].Success && groups["notice"].Value.Length != 0) sb.Append(groups["notice"].Value);
				AddPeople("guests", "Гость ", "Гости ", sb, groups);
				AddPeople("presenters", "Ведущий ", "Ведущие ", sb, groups);

				TimeSpan time=TimeSpan.Parse(groups["time"].Value);
				while (newsQueue.Count != 0 && time >= newsQueue.Peek()) {
					TimeSpan newsTime=newsQueue.Dequeue();
					if (time != newsTime) AddBroadcast(date.Add(newsTime), "Новости", null, null);
				}
				if (time == previousTime) continue;
				AddBroadcast(date.Add(time),
					quoteRx.Replace(title.Replace("&quot;", "\""), "«$1»"),
					sb.Length != 0 ? sb.ToString():null,
					groups["url"].Success ? groups["url"].Value:null);
				previousTime=time;
			}
		}
		private void AddPeople(string groupName, string singular, string plural, StringBuilder sb, GroupCollection groups) {
			if (groups[groupName].Success) {
				if (sb.Length != 0) sb.AppendLine();
				string people=htmlRx.Replace(groups[groupName].Value, string.Empty);
				sb.Append(people.Contains(",") ? plural:singular).Append(people);
			}
		}
	}
}