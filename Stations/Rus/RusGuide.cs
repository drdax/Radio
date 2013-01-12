using System;
using System.Text;
using System.Text.RegularExpressions;
using DrDax.RadioClient;

namespace RadioRus {
	public class RusGuide : DescriptionListedGuide {
		private static readonly Regex
			spaceRx=new Regex("&nbsp|  ", RegexOptions.Compiled),
			tagRx=new Regex("<[^>]+>", RegexOptions.Compiled),
			quoteRx=new Regex("\"([^\"]+)\"", RegexOptions.Compiled),
			broadcastRx=new Regex(@"<span class=""newsDate"">(?'time'[012][0-9]:[0-5][0-9])<\/span><\/a><td><td width=100% style=""padding-left: 10px;""><a href=""\/issue\.html\?iid=[0-9]+&rid="" class=""titleNews"">( <font color=""red"">)?(?'caption'[^<]+)(<\/font>)?<br>[\s\S]*?class=""textAnons"">(?'description'.+)<\/a>", RegexOptions.Multiline | RegexOptions.Compiled);

		internal RusGuide(TimeZoneInfo timezone) : base(timezone) {
			Initialize();
		}

		protected override void FillGuide(DateTime date) {
			using (var client=new ProperWebClient(Encoding.GetEncoding("Windows-1251"))) {
				foreach (Match match in broadcastRx.Matches(client.DownloadString("http://www.radiorus.ru/radio.html?date="+date.ToString("dd-MM-yyyy"))))
					AddBroadcast(date.Add(TimeSpan.Parse(match.Groups["time"].Value)),
						spaceRx.Replace(match.Groups["caption"].Value, " ").Replace("\"", string.Empty).Trim(),
						quoteRx.Replace(spaceRx.Replace(tagRx.Replace(match.Groups["description"].Value, string.Empty), " ").Trim().Replace("''", "\""), "«$1»"));
			}
		}
	}
}