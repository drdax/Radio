using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Vgtrk {
	public class RusGuide : PagedListedGuide {
		private static readonly Regex broadcastRx=new Regex(@" <a  (href=""/brand/(?'url'[^""]+)"" )?class='[a-zA-Z\- ]+' style=""[^""]+"">\r\n\s+<span class='[a-zA-Z\- ]+'></span>\r\n\r\n\s+<span class='networkBlock-time'>\r\n\s+(?'time'[012][0-9]:[0-5][0-9])\s+<\/span>\r\n\r\n\s+<div class='networkBlock-overall'>\r\n\s+<label class='icon small icon-arrow-tvp-blue'><\/label>[\r\n\s]+<span class='networkBlock-title'>\r\n\s+(""(?'caption'[^<""]+?)""|(?'caption'[^<]+?))\s+(<span class=""networkBlock-sound small icon-sound-blue""><\/span>\r\n\s+)?<\/span>\r\n\r\n\s+<span class='networkBlock-content'>\r\n\s+(?'description'([^<]|\r\n)*)<", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		private readonly string url;

		internal RusGuide(string domain, TimeZoneInfo timezone)
			: base(timezone, new SimpleGuideMenu("О передаче", "http://www."+domain+".ru/brand/")) {
			url=string.Concat("http://www.", domain, ".ru/tvp/index/date/");
		}

		protected override async Task FillGuide(DateTime date) {
			foreach (Match match in broadcastRx.Matches(await client.DownloadStringTaskAsync(url+date.ToString("yyyy-MM-dd\\/"))))
				AddBroadcast(date.Add(TimeSpan.Parse(match.Groups["time"].Value)),
					Quotes.Format(match.Groups["caption"].Value),
					Quotes.Format(match.Groups["description"].Value.TrimEnd().Replace("''", "\"")),
					match.Groups["url"].Success ? match.Groups["url"].Value:null);
		}
	}
}