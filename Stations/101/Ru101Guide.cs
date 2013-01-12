using System;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Ru101 {
	public class Ru101Guide : TimedGuide {
		/// <summary>Pašreiz skanošās dziesmas informācijas pilns URL.</summary>
		private readonly string currentUrl;
		/// <summary>HTTP klients pašreiz skanošās dziesmas noskaidrošanai.</summary>
		private readonly ProperWebClient client=new ProperWebClient(Encoding.GetEncoding("Windows-1251"), true);

		internal Ru101Guide(byte number) {
			currentUrl="http://101.ru/a_php/channel/ajax/now_efir.php?channel="+number;
			StartTimer();
		}
		public override void Dispose() {
			base.Dispose(); client.Dispose();
		}

		protected override void UpdateBroadcasts() {
			PreviousBroadcast=CurrentBroadcast;
			// NextBroadcast vienmēr null, jo zināma tikai pašreizējā dziesma.
			try {
				XElement json=XElement.Load(JsonReaderWriterFactory.CreateJsonReader(
					// Tāda ņemšanās ar parkodēšanu, jo JSON lasītājs atbalsta tikai Unicode paveidus.
					Encoding.UTF8.GetBytes(client.DownloadString(currentUrl)),
					XmlDictionaryReaderQuotas.Max));
				// { "title":"ABBA - Mamma Mia", "duration":"213", "startsong":"1331147864", "delta":197, "servertime":1331147880, "losttime":197, "song_id":156215, "rbt":"0" }
				/* <root type="object">
				  <title type="string">RICCHI E POVERI - Acapulco</title>
				  <duration type="string">224</duration>
				  <startsong type="string">1331152909</startsong>
				  <delta type="number">102</delta>
				  <servertime type="number">1331153031</servertime>
				  <losttime type="number">102</losttime>
				  <song_id type="number">39605</song_id>
				  <rbt type="string">0</rbt>
				</root>*/
				//
				System.Diagnostics.Debug.WriteLine(json.ToString());
				string description, caption=WebUtility.HtmlDecode(json.Element("title").Value).SplitCaption(out description);
				if (description != null) {
					// Izpildītājs. Nomaina lielos burtus pret normālu pierakstu, sabojājot, protams, ABBA un dažu citu kolektīvu nosaukumus.
					int splitIdx=description.IndexOf(',');
					if (splitIdx != -1) // Samaina vietām vārdu un uzvārdu.Piemēram, TOZZI, Umberto => Umberto Tozzi
						description=description.Substring(splitIdx+2)+' '+description.Substring(0, splitIdx).ToCapitalized();
					else description=description.ToCapitalized();
				}

				DateTime now=DateTime.Now;
				short duration=short.Parse(json.Element("duration").Value); // Dziesmas ilgums sekundēs.
				short delta=short.Parse(json.Element("delta").Value); // Sekunžu skaits, kurš atlicis līdz dziesmas beigām.
				if (duration == -10 || delta <= 2) // Kanāla reklāma, pāragrs izsaukums vai kāda cita aizkave.
					CurrentBroadcast=new Broadcast(now, now.AddSeconds(4), caption, description);
				else CurrentBroadcast=new Broadcast(now.AddSeconds(delta-duration), now.AddSeconds(delta), caption, description);
			} catch { CurrentBroadcast=null; }
		}
	}
}