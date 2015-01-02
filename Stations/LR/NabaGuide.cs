using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Lr {
	public class NabaGuide : PollingGuide {
		/// <summary>Raidījumu saraksta izgūšanas intervāls sekundēs.</summary>
		private const int TimerTimeout=15; // NABA mājaslapa pārbauda ik pēc 15 sekundēm.
		private readonly NabaListedGuide listedGuide;

		internal NabaGuide(TimeZoneInfo timezone) : base(TimerTimeout, null) {
			listedGuide=new NabaListedGuide(timezone);
			listedGuide.NextBroadcastChanged+=listedGuide_NextBroadcastChanged;
		}

		public override async Task Start(bool initialize) {
			await listedGuide.Start(initialize);
			await base.Start(initialize);
		}
		public override void Stop() {
			listedGuide.Stop(); base.Stop();
		}

		private void listedGuide_NextBroadcastChanged(Broadcast nextBroadcast) {
			NextBroadcast=nextBroadcast;
		}
		protected override async Task UpdateBroadcasts() {
			string description, // Atstāj tikai tekstu: <a href="raidijumi/bistamie-gadi/" title="Bīstamie gadi" >Bīstamie gadi</a>
				caption=htmlRx.Replace((await client.DownloadStringTaskAsync("http://www.naba.lv/naba_skan.php?act=skan&_="+DateTime.UtcNow.Ticks)), string.Empty).
					SplitCaption(out description, "&#160;-&#160;"); // Izpildītājs - dziesma (latīņu burtiem).
			// act=time 14:19 (reiz desmit sekundēs), act=prog Mūzika (UTF8, reizi trijās minūtēs)
			if (description == "\n") { // Ja dziesmai nav norādīts nosaukums un izpildītājs, tad paņem raidījuma nosaukumu.
				caption=await client.DownloadStringTaskAsync("http://www.naba.lv/naba_skan.php?act=prog");
				description=null;
			}
			if (CurrentBroadcast == null || CurrentBroadcast.Caption != caption) {
				PreviousBroadcast=CurrentBroadcast;
				if (listedGuide.CurrentBroadcast.Caption != caption) {
					if (description == null) description=listedGuide.CurrentBroadcast.Caption;
					else description+=Environment.NewLine+listedGuide.CurrentBroadcast.Caption;
				}
				DateTime now=DateTime.Now;
				CurrentBroadcast=new Broadcast(now, now.AddSeconds(TimerTimeout), caption, description);
			}
		}
		public override void Dispose() {
			base.Dispose();
			listedGuide.NextBroadcastChanged-=listedGuide_NextBroadcastChanged;
			listedGuide.Dispose();
		}

		private class NabaListedGuide : CaptionListedGuide {
			/// <summary>Raidījuma datu regulārais izteikums.</summary>
			private static readonly Regex guideRx=new Regex(@"(?'hours'[012][0-9])\.(?'minutes'[0-5][0-9])</div>\r\n	<div class=""event_title""><a href=""[^""]+"" title=""(?'caption'[^""]+)""", RegexOptions.Compiled);

			public NabaListedGuide(TimeZoneInfo timezone) : base(timezone, null) {}
			protected override async Task FillGuide(DateTime date) {
				foreach (Match match in guideRx.Matches(await client.DownloadStringTaskAsync(
					string.Format("http://www.naba.lv/programma/diena/?tx_cal_controller%5Byear%5D={0}&tx_cal_controller%5Bmonth%5D={1:00}&tx_cal_controller%5Bday%5D={2:00}", date.Year, date.Month, date.Day))))
					AddBroadcast(date.AddHours(int.Parse(match.Groups["hours"].Value)).AddMinutes(int.Parse(match.Groups["minutes"].Value)), System.Net.WebUtility.HtmlDecode(match.Groups["caption"].Value));
			}
		}
	}
}