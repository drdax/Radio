using System;
using System.Text.RegularExpressions;
using DrDax.RadioClient;

namespace LR {
	public class LrGuide : DescriptionListedGuide {
		/// <summary>Raidījuma datu regulārais izteikums.</summary>
		private static readonly Regex guideRx=new Regex(@"(?'time'[12]?[0-9]:[0-5][0-9])<\/A><\/P><\/TD>\r\n  <TD valign=""top""><P class=""k12"">(?'caption'[^<]+)<(\/|BR>(?'description'.+)<\/P>)", RegexOptions.Compiled);
		/// <summary>Regulārais izteikums <B> un <I> tega dzēšanai.</summary>
		private static readonly Regex boldRx=new Regex(@"<\/?(B|I)>", RegexOptions.Compiled);
		/// <summary>Radiokanāla numurs (1 līdz 4).</summary>
		private readonly byte number;

		internal LrGuide(byte number, TimeZoneInfo timezone) : base(timezone) {
			this.number=number;
			Initialize();
		}
		protected override void FillGuide(DateTime date) {
			TimeSpan lastTime=new TimeSpan(0); // Diena sākas mazliet pirms sešiem, piemēram, ar valsts himnu.
			using (var client=new ProperWebClient()) {
				foreach (Match match in guideRx.Matches(client.DownloadString(
					string.Format("http://www.latvijasradio.lv/program/{0}/{1}/{2:00}/{1}{2:00}{3:00}.htm", number, date.Year, date.Month, date.Day)))) {
					TimeSpan time=TimeSpan.Parse(match.Groups["time"].Value); // H:mm
					if (time < lastTime) date=date.AddDays(1);
					lastTime=time;
					AddBroadcast(date.Add(time), match.Groups["caption"].Value,
						match.Groups["description"].Success ? boldRx.Replace(match.Groups["description"].Value, string.Empty).Replace("<BR>", " "):null);
				}
			}
		}
	}
}