using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Swh {
	public class SwhListedGuide : CaptionListedGuide {
		private static readonly Regex broadcastRx=new Regex(@"^(?'start'[0-2][0-9]\.[0-5][0-9])( &#8211; (?'end'[0-2][0-9]\.[0-5][0-9]))?\s+(&#8220;)?(?'caption'[^0-9&\n].+?)(&#8221;)?\s*$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
		/// <summary>Vai SWH+ (true) vai SWH (false) kanāla raidījumu saraksts.</summary>
		private readonly bool isPlus;
		internal SwhListedGuide(bool isPlus, TimeZoneInfo timezone) : base(timezone, null) {
			this.isPlus=isPlus;
		}
		protected override async Task FillGuide(DateTime date) {
			// Atstāj tikai tekstu, kuru vieglāk parsēt. Aiz borta paliek dažu raidījumu bloku mājaslapu adreses.
			string html=htmlRx.Replace(await client.DownloadStringTaskAsync(GetGuideUrl(date.DayOfWeek)), string.Empty);
			TimeSpan blockEndTime=new TimeSpan(0), previousStartTime=new TimeSpan(0); TimeSpan? blockStartTime=null; string blockCaption=null;
			foreach (Match match in broadcastRx.Matches(html)) {
				TimeSpan startTime=GetTime(match, "start");
				if (startTime == previousStartTime) startTime+=TimeSpan.FromMinutes(5); // Ziņas un izklaides raidījumi parasti ir īsi.
				if (blockStartTime.HasValue && startTime > blockStartTime) {
					AddBroadcast(date.Add(blockStartTime.Value), blockCaption);
					blockStartTime=null;
				}
				if (match.Groups["end"].Success) {
					TimeSpan endTime=GetTime(match, "end");
					if (blockEndTime < endTime) {
						blockStartTime=startTime;
						blockEndTime=endTime;
						blockCaption=match.Groups["caption"].Value;
					} else // Mēdz būt iekļautie bloki, kurus uzskata par parastu raidījumu.
						AddBroadcast(date.Add(startTime), match.Groups["caption"].Value);
				} else {
					if (blockStartTime.HasValue && startTime > blockStartTime) {
						AddBroadcast(date.Add(blockStartTime.Value), blockCaption);
						blockStartTime=null;
					}
					AddBroadcast(date.Add(startTime), match.Groups["caption"].Value);
					previousStartTime=startTime;
					if (startTime < blockEndTime) {
						previousStartTime+=TimeSpan.FromMinutes(5);
						AddBroadcast(date.Add(previousStartTime), blockCaption);
						blockStartTime=null;
					}
				}
			}
		}
		/// <returns>Nedēļas dienai <paramref name="day"/> atbilstošā raidījumu saraksta adrese.</returns>
		private string GetGuideUrl(DayOfWeek day) {
			string dayName;
			switch (day) {
				case DayOfWeek.Monday:   dayName=isPlus ? "den-1":"pirmdiena"; break;
				case DayOfWeek.Tuesday:  dayName=isPlus ? "den-2":"otardiena"; break;
				case DayOfWeek.Wednesday:dayName=isPlus ? "den-3":"tresdiena"; break;
				case DayOfWeek.Thursday: dayName=isPlus ? "den-4":"ceturdiena"; break;
				case DayOfWeek.Friday:   dayName=isPlus ? "den-5":"piektdiena"; break;
				case DayOfWeek.Saturday: dayName=isPlus ? "суббота":"sestdiena"; break;
				default:     /*Sunday*/  dayName=isPlus ? "den-7":"svetdiena"; break;
			}
			return string.Concat("http://www.radio", isPlus ? "swhplus":"swh", ".lv/", dayName, "/");
		}
		private TimeSpan GetTime(Match match, string timeName) {
			return TimeSpan.ParseExact(match.Groups[timeName].Value, "hh\\.mm", CultureInfo.InvariantCulture);
		}
	}
}