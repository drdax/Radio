using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Ru101 {
	public class Ru101Guide : TimedGuide {
		/// <summary>Pašreiz skanošās dziesmas informācijas pilns URL.</summary>
		private readonly string currentUrl;
		/// <summary>HTTP klients pašreiz skanošās dziesmas noskaidrošanai.</summary>
		private readonly ProperWebClient client=new ProperWebClient(Encoding.GetEncoding("Windows-1251"), true);

		internal Ru101Guide(uint number) : base(null) {
			currentUrl=string.Format("http://101.ru/api/getplayingtrackinfo.php?station_id={0}&short=1&typechannel=channel", number);
		}
		public override void Dispose() {
			base.Dispose(); client.Dispose();
		}

		protected override async Task UpdateBroadcasts() {
			PreviousBroadcast=CurrentBroadcast;
			// NextBroadcast vienmēr null, jo zināma tikai pašreizējā dziesma.
			try {
				// {"status":0,"result":{"duration_sec":"341","title":"\u041b\u0410\u0421\u041a\u041e\u0412\u042b\u0419 \u041c\u0410\u0419 - \u0410 \u042f \u0422\u0430\u043a \u0416\u0434\u0443","id":"57966","start_time":"1361529715","finish_time":1361530056,"mdb_idtrack":"3702","mdb_idexecutor":"376","mdb_retry":"0","module":"channel","track_title":"\u0410 \u042f \u0422\u0430\u043a \u0416\u0434\u0443","executor_title":"\u041b\u0410\u0421\u041a\u041e\u0412\u042b\u0419 \u041c\u0410\u0419","end_time":125,"sample":""},"errorCode":0,"errorMsg":""}
				/* <root type="object">
  <status type="number">0</status>
  <result type="object">
    <duration_sec type="string">284</duration_sec>
    <title type="string">DIGITAL EMOTION - Get Up Action</title>
    <id type="string">74493</id>
    <start_time type="string">1362592329</start_time>
    <finish_time type="number">1362592613</finish_time>
    <mdb_idtrack type="string">31325</mdb_idtrack>
    <mdb_idexecutor type="string">9362</mdb_idexecutor>
    <mdb_retry type="string">0</mdb_retry>
    <module type="string">channel</module>
    <track_title type="string">Get Up Action</track_title>
    <executor_title type="string">DIGITAL EMOTION</executor_title>
    <end_time type="number">65</end_time>
    <sample type="string">http://wz5.101.ru/full/1/74493.mp3</sample>
  </result>
  <errorCode type="number">0</errorCode>
  <errorMsg type="string"></errorMsg>
</root>*/
				DateTime now=DateTime.Now;
				XElement result=(await client.GetEncodedJson(currentUrl)).Element("result");
				if (result.Element("track_title") == null)
					// Ētera stacijas nekādi sevi neatpazīst, tāpēc nevar vienkārši atslēgt nestrādājošo raidījumu sarakstu.
					// result ir title elements ar tekstu "Ожидание...", kā arī nepareizi laika elementi, tāpēc te tos aizstāj ar mājaslapā redzamo tekstu.
					CurrentBroadcast=new Broadcast(now, now.AddHours(1), "Прямой эфир");
				else {
					string caption=WebUtility.HtmlDecode(result.Element("track_title").Value), description=WebUtility.HtmlDecode(result.Element("executor_title").Value);
					if (description != null) {
						// Izpildītājs. Nomaina lielos burtus pret normālu pierakstu, sabojājot, protams, ABBA un dažu citu kolektīvu nosaukumus.
						int splitIdx=description.IndexOf(',');
						if (splitIdx != -1) // Samaina vietām vārdu un uzvārdu.Piemēram, TOZZI, Umberto => Umberto Tozzi
							description=description.Substring(splitIdx+2)+' '+description.Substring(0, splitIdx).ToCapitalized();
						else description=description.ToCapitalized();
					}
					short duration=short.Parse(result.Element("duration_sec").Value); // Dziesmas ilgums sekundēs.
					short delta=short.Parse(result.Element("end_time").Value); // Sekunžu skaits, kurš atlicis līdz dziesmas beigām.
					if (delta <= 0 || duration <= 0) // Kanāla reklāma, pāragrs izsaukums vai kāda cita aizkave.
						CurrentBroadcast=new Broadcast(now, now.AddSeconds(4), caption, description);
					else CurrentBroadcast=new Broadcast(now.AddSeconds(delta-duration), now.AddSeconds(delta), caption, description);
					// start_time un finish_time būtu ērti, ja tie būtu precīzi, bet bieži finish_time izrādās pagātnē, kad end_time vēl manāms.
				}
			} catch { CurrentBroadcast=null; }
		}
	}
}