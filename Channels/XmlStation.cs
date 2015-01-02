using System.IO;
using System.Xml.Linq;

namespace DrDax.RadioClient {
	/// <summary>Raidstacija, kuras kanāli aprakstīti XML failā.</summary>
	public abstract class XmlStation : Station {
		protected XDocument radioDoc;

		/// <summary>Ielādē stacijas datus. Jābūt izsauktam konstruktorā.</summary>
		/// <param name="xmlName">XML faila nosaukums ar paplašinājumu.</param>
		protected XmlStation(string xmlName, string timezoneName=null) : base(new StationChannelList(), timezoneName) {
			try {
				// Ielādē kanālu sarakstu. Try/catch, jo kopš otrās Radio versijas XML fails drīkst nebūt.
				radioDoc=XDocument.Load(Path.Combine(Path.GetDirectoryName(IconPath), xmlName));
			} catch {}
			if (radioDoc != null) {
				byte k=0;
				foreach (var channelX in radioDoc.Descendants("Channel")) {
					uint number;
					// Kanālam drīkst būt identificējošais numurs. Ja tāda nav, tad ņem skaitli pēc kārtas.
					if (channelX.Attribute("Id") == null || !uint.TryParse(channelX.Attribute("Id").Value, out number))
						number=k++;
					Channels.Add(number, channelX.Attribute("Caption").Value);
				}
			}
		}
	}
}