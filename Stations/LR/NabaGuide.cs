using System;
using System.Text.RegularExpressions;
using DrDax.RadioClient;

namespace LR {
	public class NabaGuide : PollingGuide {
		/// <summary>Raidījumu saraksta izgūšanas intervāls sekundēs.</summary>
		private const int TimerTimeout=15; // NABA mājaslapa pārbauda ik pēc 15 sekundēm.
		/// <summary>HTML tegu vienkāršots regulārais izteikums.</summary>
		private static readonly Regex htmlRx=new Regex("<[^>]+>", RegexOptions.Compiled);
		private readonly NabaListedGuide listedGuide;

		internal NabaGuide(TimeZoneInfo timezone) {
			listedGuide=new NabaListedGuide(timezone);
			listedGuide.CurrentBroadcastChanged+=listedGuide_CurrentBroadcastChanged;
			NextBroadcast=listedGuide.NextBroadcast;
			StartTimer(TimerTimeout);
		}

		private void listedGuide_CurrentBroadcastChanged(Broadcast currentBroadcast) {
			NextBroadcast=listedGuide.NextBroadcast;
		}
		protected override void UpdateBroadcasts() {
			string description,
				caption=client.DownloadString("http://www.naba.lv/naba_skan.php?act=skan&_="+DateTime.UtcNow.Ticks).SplitCaption(out description, "&#160;-&#160;"); // Izpildītājs - dziesma (latīņu burtiem).
			// act=time 14:19 (reiz desmit sekundēs), act=prog Mūzika (UTF8, reizi trijās minūtēs)
			if (caption == "\n") // Ja dziesmai nav norādīts nosaukums, tad paņem raidījuma nosaukumu.
				caption=client.DownloadString("http://www.naba.lv/naba_skan.php?act=prog");
			else if (caption.IndexOf('<') == 0) caption=htmlRx.Replace(caption, string.Empty); // Atstāj tikai tekstu: <a href="raidijumi/bistamie-gadi/" title="Bīstamie gadi" >Bīstamie gadi</a>
			if (CurrentBroadcast == null || CurrentBroadcast.Caption != caption) {
				PreviousBroadcast=CurrentBroadcast;
				if (description == null) description=listedGuide.CurrentBroadcast.Caption;
				else description+=Environment.NewLine+listedGuide.CurrentBroadcast.Caption;
				DateTime now=DateTime.Now;
				CurrentBroadcast=new Broadcast(now, now.AddSeconds(TimerTimeout), caption, description);
			}
		}
		public override void Dispose() {
			base.Dispose();
			listedGuide.CurrentBroadcastChanged-=listedGuide_CurrentBroadcastChanged;
			listedGuide.Dispose();
		}

		private class NabaListedGuide : CaptionListedGuide {
			/// <summary>Raidījuma datu regulārais izteikums.</summary>
			private static readonly Regex guideRx=new Regex(@"(?'hours'[012][0-9])\.(?'minutes'[0-5][0-9])</div>\r\n	<div class=""event_title""><a href=""[^""]+"" title=""(?'caption'[^""]+)""", RegexOptions.Compiled);

			public NabaListedGuide(TimeZoneInfo timezone) : base(timezone) {
				Initialize();
			}
			protected override void FillGuide(DateTime date) {
				using (var client=new ProperWebClient()) {
					foreach (Match match in guideRx.Matches(client.DownloadString(
						string.Format("http://www.naba.lv/programma/diena/?tx_cal_controller%5Byear%5D={0}&tx_cal_controller%5Bmonth%5D={1:00}&tx_cal_controller%5Bday%5D={2:00}", date.Year, date.Month, date.Day))))
						AddBroadcast(date.AddHours(int.Parse(match.Groups["hours"].Value)).AddMinutes(int.Parse(match.Groups["minutes"].Value)), match.Groups["caption"].Value);
				}
			}
		}
	}
}