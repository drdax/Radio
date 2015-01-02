using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Rai {
	/// <summary>Rai kanāla pašreizējo raidījumu saraksts, kurš bieži jāatjaunina.</summary>
	public class RaiPollingGuide : PollingGuide {
		/// <summary>
		/// Raidījumu saraksta izgūšanas intervāls sekundēs.
		/// </summary>
		private const int TimerTimeout=15; // Oriģinālais Rai uztvērējs pārbauda ik pēc 15 sekundēm.
		private readonly string guideUrl;

		/// <param name="number">5, jo attiecas tikai uz filodiffusione kanāliem.</param>
		internal RaiPollingGuide(uint number) : base(TimerTimeout, Encoding.ASCII, null) {
			guideUrl=string.Format("http://service.rai.it/xml2json.php?xmlurl=http://netiaweb1.radio.radiofonia.rai.it/orainonda/fd5/onair_fd{0}.xml", number);
		}

		protected override async Task UpdateBroadcasts() {
			try { // Var gadīties tukša lapa.
				// XML tiek pārkodēts uz JSON un tad atpakaļ, bet veids, kā nokļūt pie oriģinālā XML nav noskaidrots (guideUrl iekļautā adrese pa tiešo neveras).
				XElement guide=(await client.GetJson(guideUrl)).Element("xml").Element("radio");
				DateTime now=DateTime.Now;
				Broadcast current=GetBroadcast(now, now.AddSeconds(TimerTimeout), guide.Element("now_playing"));
				if (CurrentBroadcast == null || current.Caption != CurrentBroadcast.Caption) {
					PreviousBroadcast=CurrentBroadcast;
					CurrentBroadcast=current;
					var nextEvent=guide.Element("next_event"); // FD4 nextEvent ir masīvs, bet FD5 tas satur vienīgo elementu.
					if (nextEvent.IsEmpty)
						NextBroadcast=null;
					else NextBroadcast=GetBroadcast(now.AddSeconds(TimerTimeout), now.AddSeconds(TimerTimeout*2), nextEvent.Element("item") ?? nextEvent);
				}
			} catch { CurrentBroadcast=null; NextBroadcast=null; }
		}
		private Broadcast GetBroadcast(DateTime startTime, DateTime endTime, XElement xml) {
			XElement music=xml.Element("jingle");
			if (music == null) {
				music=xml.Element("music");
				return new Broadcast(startTime, endTime, WebUtility.HtmlDecode(music.Element("title").Value).ToCapitalized(),
					string.Format("Executor: {1}{0}Author: {2}", Environment.NewLine,
						WebUtility.HtmlDecode(music.Element("executor").Value).ToCapitalized(),
						WebUtility.HtmlDecode(music.Element("author").Value).ToCapitalized()));
			} else return new Broadcast(startTime, endTime, WebUtility.HtmlDecode(music.Element("title").Value).ToCapitalized(), "Jingle");
		}
	}
}