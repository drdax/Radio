using System;
using System.Text.RegularExpressions;
using DrDax.RadioClient;

namespace Swh {
	public class SwhGuide : IcyGuide, IDisposable {
		private readonly string nowUrl;
		private readonly SwhListedGuide listedGuide;

		internal SwhGuide(string nowUrl, SwhListedGuide listedGuide) {
			this.nowUrl=nowUrl;
			this.listedGuide=listedGuide;
			if (listedGuide != null) {
				listedGuide.CurrentBroadcastChanged+=listedGuide_CurrentBroadcastChanged;
				NextBroadcast=listedGuide.NextBroadcast;
			}
		}
		private void listedGuide_CurrentBroadcastChanged(Broadcast currentBroadcast) {
			NextBroadcast=listedGuide.NextBroadcast;
		}
		protected override Broadcast GetBroadcast(string title) {
			if (title == null) return null;
			string caption, description;
			if (title == "LIVE" && listedGuide != null) {
				caption=listedGuide.CurrentBroadcast.Caption;
				description=null;
			} else {
				if (title[title.Length-1] == '&') {
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
					using (var client=new ProperWebClient(System.Text.Encoding.ASCII)) {
						Match match=new Regex("<ARTIST>(?'artist'[^<]+)<\\/ARTIST>\\s+<TITLE>(?'title'[^<]+)<", RegexOptions.Multiline).Match(client.DownloadString(nowUrl));
						caption=match.Groups["title"].Value.ToCapitalized();
						description=match.Groups["artist"].Value.ToCapitalized();
					}
				} else caption=title.ToCapitalized().SplitCaption(out description);
				if (listedGuide != null)
					description+=Environment.NewLine+listedGuide.CurrentBroadcast.Caption;
			}
			return new Broadcast(DateTime.Now, DateTime.Now.AddHours(1), caption, description);
		}
		public void Dispose() {
			if (listedGuide != null) {
				listedGuide.CurrentBroadcastChanged-=listedGuide_CurrentBroadcastChanged;
				listedGuide.Dispose();
			}
		}
	}
}