using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using System.Xml.Linq;
using DrDax.RadioClient;

namespace Ru101 {
	[Export(typeof(Station))]
	public class Ru101Station : XmlStation {
		private Brand brand;
		public Ru101Station() : base("101.xml", "Russian Standard Time") {} // Maskavas laika josla.

		public override Channel GetChannel(byte number) {
			XElement channelX=null; // Kanāla dati XML veidā.
			try { channelX=radioDoc.Descendants("Channel").FirstOrDefault(c => c.Attribute("Id").Value == number.ToString()); }
			catch {}
			if (channelX == null) throw new ChannelNotFoundException(number);
			if (brand == null)
				brand=new Brand(0x515151.ToColor(), 0x462E94.ToColor(), Colors.White,
					0x6C54D5.ToColor(), 0x483998.ToColor(), 0xF1EEFD.ToColor(), 0xF2F0FA.ToColor());
			Uri url; // MP3 plūsmas adrese.
			if (Uri.TryCreate(new Uri(radioDoc.Root.Attribute("BaseUrl").Value), new Uri(channelX.Attribute("Url").Value, UriKind.Relative), out url))
				// Ja predefinēsim vērtības, tad 44100 Hz, 128000 bps, 418 bytes.
				return new HttpChannel(url.ToString(), GetResourceImage("101ru.png"), timezone,
					Station.UseGuide ? new Ru101Guide(number):null, brand)
					{ UserAgent="Mozilla/5.0 (compatible; MSIE 9.0)" }; // Lai 101.ru neteiktu, ka jāklausās pārlūkā.
			return null;
		}
	}
}