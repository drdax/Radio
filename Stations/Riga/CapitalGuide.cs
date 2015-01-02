using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Riga {
	public class CapitalGuide : CombinedIcyGuide {
		public CapitalGuide(TimeZoneInfo timezone) : base(new CapitalListedGuide(timezone), Encoding.UTF8) {}

		protected override Task<Broadcast> GetBroadcast(string title) {
			string description, caption=title.SplitCaption(out description);
			if (description != null)
				description+=Environment.NewLine+listedGuide.CurrentBroadcast.Caption;
			else description=listedGuide.CurrentBroadcast.Caption;
			return Task.FromResult(new Broadcast(DateTime.Now, DateTime.Now.AddMilliseconds(Channel.DefaultTimeout), caption, description));
		}

		private class CapitalListedGuide : CaptionListedGuide {
			public CapitalListedGuide(TimeZoneInfo timezone) : base(timezone, null) {}

			protected override async Task FillGuide(DateTime date) {
				string day;
				switch (date.DayOfWeek) {
					case DayOfWeek.Monday:   day="pirmdiena"; break;
					case DayOfWeek.Tuesday:  day="otrdiena"; break;
					case DayOfWeek.Wednesday:day="tresdiena"; break;
					case DayOfWeek.Thursday: day="ceturdiena"; break;
					case DayOfWeek.Friday:   day="piektdiena"; break;
					case DayOfWeek.Saturday: day="sestdiena"; break;
					default:     /*Sunday*/  day="svetdiena"; break;
				}
				// Raidījumu saraksts visai diennaktij, izņemot svētdienu, kad līdz 21:00, bet tā kā tad nav profilakse, pieņem ka līdz pusnaktij.
				foreach (Match match in guideRx.Matches(await client.DownloadStringTaskAsync(string.Concat("http://www.capitalfm.lv/lv/", day, "/index.html"))))
					AddBroadcast(date.AddHours(int.Parse(match.Groups["hours"].Value)), System.Net.WebUtility.HtmlDecode(match.Groups["caption"].Value));
			}

			/// <summary>Dienas raidījuma nosaukuma regulārais izteikums.</summary>
			/// <remarks>Dokumentā pārsvarā LF, bet raidījumu sarakstā CRLF.</remarks>
			private static readonly Regex guideRx=new Regex(@"align=center>(?'hours'[012][0-9]):00 - [012][0-9]:00<\/td>\r\n\t\t<td class=program_title><a href=""http:\/\/www\.capitalfm\.lv\/lv\/nedelas_programma\/index.html"">(?'caption'[^<]+)", RegexOptions.Compiled);
		}
	}
}