using System;
using System.Text.RegularExpressions;
using DrDax.RadioClient;

namespace Swh {
	/// <summary>
	/// SWH dienas raidījumu saraksts. Ar mokām ņemts no HTML, jo XML versija neatbilst patiesībai.
	/// </summary>
	public class SwhListedGuide : CaptionListedGuide {
		private static readonly Regex
			tagRx=new Regex("<[^>]+>", RegexOptions.Compiled), // HTML tega vienkāršojums.
			blockCaptionRx=new Regex(@"<strong>(?'caption'[^012].+)", RegexOptions.Compiled), // Raidījumu grupas nosaukums.
			broadcastRx=new Regex(@"(?'hours'[012][0-9])\.(?'minutes'[0-5][0-9]) (?'caption'.+)", RegexOptions.Compiled); // Raidījums ar sākumlaiku un nosaukumu.
		/// <summary>Kanāla numurs: 1 SWH, 2 SWH+.</summary>
		private readonly byte number;

		internal SwhListedGuide(byte number, TimeZoneInfo timezone) : base(timezone) {
			this.number=number;
			Initialize();
		}
		protected override void FillGuide(DateTime date) {
			Regex blockRx=number == 1 ? new Regex(@"<td><strong> ?(?'start'[012][0-9])\.00 - (?'end'[012][0-9])\.00 ?<\/strong></td>\r\n<td>(?'broadcasts'.+?)<\/td>")
				: new Regex(@">(?'start'[012][0-9])\.00 - (?'end'[012][0-9])\.00[\s\S]+?<td>(?'broadcasts'[\s\S]+?)<\/td>", RegexOptions.Multiline);
			// Diena sākas sešos vai septiņos. Pēdējais raidījums sākas šodien.
			using (var client=new ProperWebClient()) {
				foreach (Match block in blockRx.Matches(client.DownloadString(GetGuideUrl(date.DayOfWeek)))) {
					string blockCaption=null, // Raidījumu bloka nosaukums.
						broadcasts=null; // Raidījumu laiki un nosaukumi.
					bool emptyBlock=true; // Vai raidījumu blokā nav raidījumi ar savu laiku.
					if (number == 1) // SWH
						broadcasts=block.Groups["broadcasts"].Value;
					else { // SWH+
						string[] ps=block.Groups["broadcasts"].Value.Replace("&nbsp;", " ").Split(new[] { "</p>\r\n<p>", "<br /><br />" }, StringSplitOptions.None);
						if (ps[0].IndexOf(".00") == -1) { // Bloka nosaukumam nevajadzētu saturēt laiku.
							blockCaption=tagRx.Replace(ps[0].Replace("<br />", " "), string.Empty).Trim();
							if (ps.Length != 1) broadcasts=ps[1];
						} else {
							if (ps.Length > 1) blockCaption=tagRx.Replace(ps[1], string.Empty).Trim();
							broadcasts=ps[0];
						}
					}
					if (broadcasts != null) {
						DateTime endTime=date.AddHours(int.Parse(block.Groups["end"].Value));
						foreach (string broadcast in broadcasts.Split(new[] { "<br />" }, StringSplitOptions.None)) {
							Match match=broadcastRx.Match(broadcast);
							if (match.Success) {
								DateTime time=date.AddHours(int.Parse(match.Groups["hours"].Value)).AddMinutes(int.Parse(match.Groups["minutes"].Value));
								// SWH+ programmā iepriekšejais bloks var saturēt raidījumu no nākamā. Šādu gadījumu apstrāde nav paredzēta.
								AddBroadcast(time, tagRx.Replace(match.Groups["caption"].Value, string.Empty));
								if (endTime != time.AddMinutes(5))
									AddBroadcast(time.AddMinutes(5), blockCaption); // Pieņemsim, ka ziņas ilgst piecas minūtes.
								emptyBlock=false;
							} else {
								match=blockCaptionRx.Match(broadcast);
								if (match.Success)
									blockCaption=tagRx.Replace(match.Groups["caption"].Value, string.Empty);
							}
						}
					}
					if (emptyBlock)
						AddBroadcast(date.AddHours(int.Parse(block.Groups["start"].Value)), blockCaption);
				}
			}
		}
		/// <returns>Nedēļas dienai <paramref name="day"/> atbilstošā raidījumu saraksta adrese.</returns>
		private string GetGuideUrl(DayOfWeek day) {
			if (number == 1) // SWH
				return string.Format("http://www.radioswh.lv/swh/index.php?option=com_content&view=article&id=1{0:00}&Itemid=1{1}",
					day == DayOfWeek.Sunday ? 10:3+(int)day, day == DayOfWeek.Sunday ? 44:37+(int)day);
			// SWH+
			string urlPrefix="http://www.radioswhplus.lv/index.php/2011-03-09-12-40-54/2011-03-25-11-5";
			switch (day) {
				case DayOfWeek.Monday:   return urlPrefix+"0-34";
				case DayOfWeek.Tuesday:  return urlPrefix+"1-49";
				case DayOfWeek.Wednesday:return urlPrefix+"2-34";
				case DayOfWeek.Thursday: return urlPrefix+"3-46";
				case DayOfWeek.Friday:   return urlPrefix+"4-34";
				case DayOfWeek.Saturday: return urlPrefix+"5-01";
				default:     /*Sunday*/  return urlPrefix+"5-30";
			}
		}
	}
}