using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Bbc {
	public class BbcGuide : PagedListedGuide {
		/// <summary>Raidījumu saraksta lapas adrese bez datuma un formāta daļas.</summary>
		private readonly string guideUrl;

		internal BbcGuide(string guideUrl, TimeZoneInfo timezone)
			: base(timezone, new SimpleGuideMenu("Programme Info", "http://www.bbc.co.uk/programmes/")) {
			this.guideUrl=guideUrl;
		}

		protected override async Task FillGuide(DateTime date) {
			int lastDate=date.Day; string dateString=lastDate.ToString("00/"); // Dažiem raidījumiem norādīts to datums, ja tas sakrīt ar šodienu, nav vērts rādīt.
			foreach (XElement xBroadcast in XDocument.Parse(
				await client.DownloadStringTaskAsync(string.Concat(guideUrl, date.ToString(@"yyyy\/MM\/dd"), ".xml"))).
				Root.Element("day").Element("broadcasts").Elements("broadcast")) {
				DateTime startTime=DateTime.Parse(xBroadcast.Element("start").Value); // @"yyyy-MM-dd\THH:mm:ssZ", kur Z ir gan burts, gan UTC laika josla.
				if (lastDate < startTime.Day) break; // Ņovērš pārklāšanos, kad raidījumu saraksts sākas ap pusnakti un beidzas nākamajā dienā ap sešiem.
				var p=xBroadcast.Element("programme");
				string subTitle=p.Element("title").Value; // Alternatīvs ceļš ir display_titles/subtitle
				AddBroadcast(TimeZoneInfo.ConvertTime(startTime, timezone),
					p.Element("display_titles").Element("title").Value,
					subTitle.StartsWith(dateString) ? p.Element("short_synopsis").Value:string.Concat(subTitle, Environment.NewLine, p.Element("short_synopsis").Value),
					p.Element("pid").Value
				);
				lastDate=startTime.Day;
			}
		}
	}
}