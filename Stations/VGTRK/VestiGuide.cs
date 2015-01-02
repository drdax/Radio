using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Vgtrk {
	public class VestiGuide : CaptionListedGuide {
		/// <summary>Nedēļas raidījumu nosaukumu saraksts ar sākuma datumiem un laikiem.</summary>
		private readonly Dictionary<DayOfWeek, List<Tuple<DateTime, string>>> days=new Dictionary<DayOfWeek, List<Tuple<DateTime, string>>>(7);
		/// <summary>Atbilst vienam raidījumam vai dienas numuram. Satur sākuma laiku, lappuses identifikatoru un nosaukumu.</summary>
		private static readonly Regex broadcastRx=new Regex(@"id=""day_0(?'day'[1-7])""|span>(?'time'[012][0-9]:[0-5][0-9])<\/span>\r\n +<h4><a href=""\/brand\/show\/brand_id\/(?'program'[0-9]+)"">(?'caption'[^<]+)<\/a", RegexOptions.Singleline);

		public VestiGuide(TimeZoneInfo timezone) : base(timezone, null) {}
		protected override async Task FillGuide(DateTime date) {
			List<Tuple<DateTime, string>> broadcasts=null;
			if (days.Count == 7) {
				if (days.TryGetValue(date.DayOfWeek, out broadcasts))
					guide.AddRange(broadcasts);
				return;
			}

			byte guideDay=date.DayOfWeek == DayOfWeek.Sunday ? (byte)7:(byte)(date.DayOfWeek+1);
			DateTime broadcastDate=date, sunday=date.AddDays(-guideDay); // Iepriekšējā svētdiena
			guideDay--; // Noņem vienu dienu, lai atbalstītu iepriekšējās dienas saraksta atgriešanu naktī, kad šodienas raidījumi sākas vēlāk.
			foreach (Match match in broadcastRx.Matches(await client.DownloadStringTaskAsync("http://radiovesti.ru/setka")))
				if (match.Groups["day"].Success) {
					byte day=byte.Parse(match.Groups["day"].Value);
					broadcasts=new List<Tuple<DateTime,string>>(12);
					broadcastDate=sunday.AddDays(day < guideDay ? day+7:day); // Pagājušās šīs nedēļas dienas pārceļ uz nākamo nedēļu.
					days.Add(broadcastDate.DayOfWeek, broadcasts);
					System.Diagnostics.Debug.WriteLine(match.Groups["time"].Value);
				} else
					broadcasts.Add(Tuple.Create(broadcastDate.Add(TimeSpan.Parse(match.Groups["time"].Value)), match.Groups["caption"].Value));

			if (days.TryGetValue(date.DayOfWeek, out broadcasts))
				guide.AddRange(broadcasts);
		}
	}
}