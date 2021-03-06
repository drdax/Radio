﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Riga {
	// SIA "Vārds & Co" Latvijas Kristīgais radio
	public class KristigaisGuide : DescriptionListedGuide {
		private static readonly Regex broadcastRx=new Regex(@"<span><i>(?'time'[012][0-9]:[0-5][0-9])<\/i>(<a href=""[^""]+"">)?(?'caption'.+?)(<\/a>)?((<br>)?<b>(?'description'.+?)<\/b>)*</span>", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

		internal KristigaisGuide(TimeZoneInfo timezone) : base(timezone, null) {}

		protected override async Task FillGuide(DateTime date) {
			// Raidījumu saraksts tiek sniegts tikai uz nedēļu sākot ar šodienu.
			foreach (Match match in broadcastRx.Matches(await client.DownloadStringTaskAsync(
				"http://lkr.lv/lat/programma/?day="+(date > TimeZoneInfo.ConvertTime(DateTimeOffset.Now, timezone).Date ? '2':'1')))) {
				string description=null;
				if (match.Groups["description"].Success) {
					StringBuilder sb=new StringBuilder();
					bool notFirst=false;
					foreach (Capture capture in match.Groups["description"].Captures) {
						if (notFirst) sb.Append(Environment.NewLine);
						sb.Append(capture.Value.Replace("&nbsp;", string.Empty));
						notFirst=true;
					}
					description=sb.ToString();
				}
				AddBroadcast(date.Add(TimeSpan.Parse(match.Groups["time"].Value)), // HH:mm
					System.Net.WebUtility.HtmlDecode(match.Groups["caption"].Value.Trim()), description);
			}
		}
	}
}