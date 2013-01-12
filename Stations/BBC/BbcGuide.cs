using System;
using System.Globalization;
using System.Text.RegularExpressions;
using DrDax.RadioClient;

namespace Bbc {
	public class BbcGuide : DescriptionListedGuide {
		private static readonly Regex dateRx=new Regex(@": [0-3][0-9]\/[01][0-9]\/[0-9]{4}", RegexOptions.Compiled),
			broadcastRx=new Regex(@"episode-time"">[\s\S]+?<p>(?'time'[01]?[0-9]:[0-5][0-9](?'ampm'a|p))m<\/p>[\s\S]+?class=""episode-title""[\s\S]+?title=""(?'caption'[^""]+)""[\s\S]+?class=""episode-synopsis"">(?'description'[^<]+)<\/p>", RegexOptions.Compiled);
		private readonly string guideUrl;
		private readonly DateTimeFormatInfo dateFormat=new CultureInfo("en-GB").DateTimeFormat;

		internal BbcGuide(string radioName, TimeZoneInfo timezone) : base(timezone) {
			this.guideUrl=string.Format("http://www.bbc.co.uk/iplayer/radio/bbc_{0}/", radioName);
			dateFormat.AMDesignator="a"; dateFormat.PMDesignator="p";
			Initialize();
		}

		protected override void FillGuide(DateTime date) {
			string lastAmPm="a"; // Diena sākas sešos..
			using (var client=new ProperWebClient()) {
				foreach (Match match in broadcastRx.Matches(client.DownloadString(guideUrl+date.ToString("yyyyMMdd")))) {
					if (match.Groups["ampm"].Value != lastAmPm) {
						if (lastAmPm == "p") date=date.AddDays(1); // ..un beidzās nākošajā dienā sešos.
						lastAmPm=match.Groups["ampm"].Value;
					}
					AddBroadcast(date.Add(DateTime.ParseExact(match.Groups["time"].Value, "h:mmt", dateFormat).TimeOfDay),
						dateRx.Replace(match.Groups["caption"].Value, string.Empty), // Raidījuma datums tikai traucē.
						match.Groups["description"].Value.Trim());
				}
			}
		}
	}
}