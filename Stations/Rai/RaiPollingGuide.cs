using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
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

		/// <param name="number">4 vai 5, jo attiecas tikai uz filodiffusione kanāliem.</param>
		internal RaiPollingGuide(byte number) : base(Encoding.ASCII) {
			guideUrl=string.Format("http://service.rai.it/xml2json.php?xmlurl=http://frog.prodradio.rai.it/orainonda/fd{0}/onair_fd{0}.xml", number);
			StartTimer(TimerTimeout);
		}

		protected override void UpdateBroadcasts() {
			try { // Var gadīties tukša lapa.
				// XML tiek pārkodēts uz JSON un tad atpakaļ, bet veids, kā nokļūt pie oriģinālā XML nav noskaidrots (guideUrl iekļautā adrese pa tiešo neveras).
				XElement json=XElement.Load(JsonReaderWriterFactory.CreateJsonReader(client.DownloadData(guideUrl), XmlDictionaryReaderQuotas.Max));
				System.Diagnostics.Debug.WriteLine(json.ToString());
				XElement guide=json.Element("xml").Element("radio");
				DateTime now=DateTime.Now;
				Broadcast current=GetBroadcast(now, now.AddSeconds(TimerTimeout), guide.Element("now_playing"));
				if (CurrentBroadcast == null || current.Caption != CurrentBroadcast.Caption) {
					PreviousBroadcast=CurrentBroadcast;
					CurrentBroadcast=current;
					NextBroadcast=GetBroadcast(now.AddSeconds(TimerTimeout), now.AddSeconds(TimerTimeout*2), guide.Element("next_event").Element("item"));
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