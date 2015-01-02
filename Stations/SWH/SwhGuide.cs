using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DrDax.RadioClient;

namespace Swh {
	public class SwhGuide : IcyGuide, IDisposable {
		private readonly string nowUrl;
		private readonly CaptionListedGuide listedGuide;
		private readonly bool capitalize;

		internal SwhGuide(string nowUrl, CaptionListedGuide listedGuide, bool capitalize) {
			this.nowUrl=nowUrl;
			this.listedGuide=listedGuide;
			if (listedGuide != null) {
				listedGuide.NextBroadcastChanged+=listedGuide_NextBroadcastChanged;
				NextBroadcast=listedGuide.NextBroadcast;
			}
			this.capitalize=capitalize; // Visi kanāli, izņemot Spin, raksta nosaukumus lieliem burtiem.
		}
		public override async Task Start(bool initialize) {
			if (listedGuide != null)
				await listedGuide.Start(initialize);
			await base.Start(initialize);
		}
		public override void Stop() {
			if (listedGuide != null) listedGuide.Stop();
			base.Stop();
		}

		private void listedGuide_NextBroadcastChanged(Broadcast nextBroadcast) {
			NextBroadcast=nextBroadcast;
		}
		protected override async Task<Broadcast> GetBroadcast(string title) {
			if (title == null) return null;
			string caption, description;
			if ((title == "LIVE" || title == "Radio SWH LIVE") && listedGuide != null) {
				caption=listedGuide.CurrentBroadcast.Caption;
				description=null;
			} else {
				if (title[title.Length-1] == '&')
				{
					// Gadījumos, kad nosaukumā jābūt apostrofam, tas tiek aizvietots ar ampersandu un pārējās daļas nav.
					// Tādēļ izgūst pilnos dziesmas datus no XML.
					/*
<PLAYBACKSTATE>
	<lastBuildDate>2012-03-29 21:35:45</lastBuildDate>
	 <PLAY INDEX="0"> 
	 <ARTIST>BEATLES</ARTIST> 
	 <TITLE>A HARD DAY'S NIGHT</TITLE> 
	 <start_time>02:21</start_time> 
	 </PLAY> 
  </PLAYBACKSTATE>
<PLAYBACKSTATE>
	<lastBuildDate>2012-03-30 22:50:48</lastBuildDate>
	 <PLAY INDEX="0"> 
	 <ARTIST>SWHPLUS</ARTIST> 
	 <TITLE>LIVE</TITLE> 
	 <start_time>14:48</start_time> 
	 </PLAY> 
  </PLAYBACKSTATE> */
					// Vēl ir JSON formātā un ar lielāku informāciju (SWH gadījumā): sobrid_etera_title, bildes sobrid_etera, imgfull, img300 un img135.
					using (var client=new ProperWebClient(System.Text.Encoding.ASCII))
					{
						Match match=new Regex("<ARTIST>(?'artist'[^<]+)<\\/ARTIST>\\s+<TITLE>(?'title'[^<]+)<", RegexOptions.Multiline).Match(await client.DownloadStringTaskAsync(nowUrl));
						caption=match.Groups["title"].Value.ToCapitalized();
						description=match.Groups["artist"].Value.ToCapitalized();
					}
				}
				else caption=(capitalize ? title.ToCapitalized():title).SplitCaption(out description);
				if (listedGuide != null)
					description+=Environment.NewLine+listedGuide.CurrentBroadcast.Caption;
			}
			return new Broadcast(DateTime.Now, DateTime.Now.AddHours(1), caption, description);
		}
		public void Dispose() {
			if (listedGuide != null) {
				listedGuide.NextBroadcastChanged-=listedGuide_NextBroadcastChanged;
				listedGuide.Dispose();
			}
		}
	}
}