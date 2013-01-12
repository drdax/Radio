using System;
using System.Text.RegularExpressions;
using DrDax.RadioClient;

namespace Riga {
	public class TopGuide : PollingGuide {
		/// <summary>Raidījumu saraksta izgūšanas intervāls sekundēs.</summary>
		private const int TimerTimeout=20; // Patvaļīgi izvēlēts pārbaudes intervāls, jo Top Radio pašreizējais raidījums tiek rakstīts tikai pilnā lapā.
		private static readonly Regex songRx=new Regex(@"<span class=""song"">(?'artist'.+?) - (?'caption'.+?)</span>", RegexOptions.Compiled);
		private readonly TopListedGuide listedGuide;

		internal TopGuide(TimeZoneInfo timezone) {
			listedGuide=new TopListedGuide(timezone);
			listedGuide.CurrentBroadcastChanged+=listedGuide_CurrentBroadcastChanged;
			PreviousBroadcast=listedGuide.PreviousBroadcast;
			NextBroadcast=listedGuide.NextBroadcast;
			StartTimer(TimerTimeout);
		}

		private void listedGuide_CurrentBroadcastChanged(Broadcast currentBroadcast) {
			NextBroadcast=listedGuide.NextBroadcast;
		}
		protected override void UpdateBroadcasts() {
			Match match=songRx.Match(client.DownloadString("http://topradio.lv/"));
			string description=null, caption=null;
			if (match.Success) {
				caption=match.Groups["caption"].Value;
				description=match.Groups["artist"].Value;
			} else caption=listedGuide.CurrentBroadcast.Caption;
			if (CurrentBroadcast == null || CurrentBroadcast.Caption != caption) {
				PreviousBroadcast=CurrentBroadcast;
				if (description != null)
					description+=Environment.NewLine+listedGuide.CurrentBroadcast.Caption;
				DateTime now=DateTime.Now;
				CurrentBroadcast=new Broadcast(now, now.AddSeconds(TimerTimeout), caption, description);
			}
		}
		public override void Dispose() {
			base.Dispose();
			listedGuide.CurrentBroadcastChanged-=listedGuide_CurrentBroadcastChanged;
			listedGuide.Dispose();
		}
		private class TopListedGuide : CaptionListedGuide {
			/// <summary>Raidījuma datu regulārais izteikums.</summary>
			private static readonly Regex guideRx=new Regex(@"width=""102""><strong>(?'hours'[012][0-9])[:\.](?'minutes'[0-5][0-9])-[012][0-9][:\.][0-5][0-9]<\/strong><\/td>\s+<td width=""455"">(?'caption'[^<]+)<", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

			internal TopListedGuide(TimeZoneInfo timezone) : base(timezone) {
				Initialize();
			}

			protected override void FillGuide(DateTime date) {
				TimeSpan lastTime=new TimeSpan(0);
				using (var client=new ProperWebClient()) {
					foreach (Match match in guideRx.Matches(client.DownloadString(
						"http://topradio.lv/index.php?name=data&group_id=" + (date.DayOfWeek == DayOfWeek.Sunday ? 44:37+(int)date.DayOfWeek)))) {
						TimeSpan time=new TimeSpan(int.Parse(match.Groups["hours"].Value), int.Parse(match.Groups["minutes"].Value), 0); // HH:mm
						if (time < lastTime) break; // Pēc dienas beigām mēdz parādīties lieki raidījumi.
						AddBroadcast(date.Add(time), match.Groups["caption"].Value);
					}
				}
			}
		}
	}
}