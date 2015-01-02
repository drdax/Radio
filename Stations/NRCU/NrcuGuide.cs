using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Nrcu {
	public class NrcuGuide : ListedGuide<int> {
		private static readonly Regex 
			/// <summary>Raidījuma sākumlaiks un identifikators.</summary>
			guideRx=new Regex(@"class=""time"">\n +(?'time'[0-9:]+) - ([0-9:]+)\n +<\/div>\n +<div class=""listen"">\n +<a onclick=""listenPopup\('\/grid\/channel\/period\/item-listen-popup\.html\?periodItemID=(?'id'[0-9]+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture),
			/// <summary>Visas raidījuma detaļas no tā lappuses.</summary>
			broadcastRx=new Regex(@"class=""prog-name"">(&quot;(?'caption'[^<]+)&quot;|(?'caption'[^<]+))<\/p>([\s\S]+class=""value"">\n +(?'presenter'[^\n]+?) *\n)?[\s\S]+id=""idProgDescr""[^>]+>\n(?'description'[\s\S]+)<\/div>", RegexOptions.Compiled | RegexOptions.ExplicitCapture),
			teamRx=new Regex(@"staff\.html\?id=(?'id'[0-9]+)"">(?'name'[^ ]+) +(?'surname'[^< ]+)", RegexOptions.Compiled);
		/// <summary>Raidstacijas darbinieku lappušu ID un pilni vārdi.</summary>
		private static Dictionary<string, Tuple<int, string>> team;
		private readonly string guideUrl;

		internal NrcuGuide(uint number, TimeZoneInfo timezone) : base(timezone, new GuideMenu()) {
			guideUrl=string.Format("http://schedule.nrcu.gov.ua/grid/channel/period/items/list.html?channelID={0}&date=", number);
		}

		protected override async Task FillGuide(DateTime date) {
			if (team == null) {
				// Noskaidro raidījumu vadītāju vārdus un identifikatorus.
				team=new Dictionary<string, Tuple<int, string>>(63);
				foreach (Match match in teamRx.Matches(await client.DownloadStringTaskAsync("http://www.promin.fm/team.html")))
					try {
						team.Add(string.Concat(match.Groups["surname"].Value, " ", match.Groups["name"].Value.Substring(0,1)), Tuple.Create(int.Parse(match.Groups["id"].Value), string.Concat(match.Groups["name"].Value, " ", match.Groups["surname"].Value)));
					} catch (ArgumentException) {} // Cilvēks var atkārtoties vairākās grupās (raidījumu vadītāji, režisori un priekšnieki).
			}
			// Saglabā raidījumu identifikatorus. Mēdz būt pāri simtam ierakstu.
			foreach (Match match in guideRx.Matches(await client.DownloadStringTaskAsync(guideUrl+date.ToString("yyyy-MM-dd"))))
				guide.Add(Tuple.Create(date.Add(TimeSpan.Parse(match.Groups["time"].Value)), // HH:mm:ss
					int.Parse(match.Groups["id"].Value))); 
		}
		protected override Broadcast GetBroadcast(DateTime startTime, DateTime endTime, int id) {
			// Raidījumu sarakstā apraksti ir aplausti un konkrētas pārraides īpatnības nolasāmas tikai tās lappusē.
			var matchGroups=broadcastRx.Match(client.DownloadString("http://schedule.nrcu.gov.ua/grid/channel/period/item-listen-popup.html?periodItemID="+id)).Groups;
			var sb=new StringBuilder(700);
			if (matchGroups["description"].Success) {
				bool notFirst=false;
				// Kamēr HTMLā rindu atdalītājs kļuva \n, apraksta blokā tas saglabājās \r\n.
				foreach (string line in matchGroups["description"].Value.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)) {
					string clean=htmlRx.Replace(line, string.Empty).Trim();
					if (clean.Length == 0) continue;
					if (notFirst) sb.Append(Environment.NewLine);
					notFirst=true;
					sb.Append(WebUtility.HtmlDecode(clean));
				}
			}
			int presenterId=0;
			if (matchGroups["presenter"].Success) {
				Tuple<int, string> presenter=null;
				string surname=matchGroups["presenter"].Value; int pos=surname.IndexOf('.');
				if (pos != -1 && team.TryGetValue(surname.Substring(0, pos), out presenter))
					presenterId=presenter.Item1;
				if (sb.Length != 0) sb.Append(Environment.NewLine);
				sb.Append("Ведучий ").Append(presenter == null ? surname:presenter.Item2);
			}
			return new NrcuBroadcast(id, startTime, endTime, WebUtility.HtmlDecode(matchGroups["caption"].Value), sb.Length != 0 ? sb.ToString():null, presenterId);
		}
		protected override Tuple<DateTime, int> DataFromBroadcast(Broadcast broadcast) {
			return Tuple.Create(TimeZoneInfo.ConvertTime(broadcast.StartTime, TimeZoneInfo.Local, timezone), ((NrcuBroadcast)broadcast).Id);
		}
	}
}