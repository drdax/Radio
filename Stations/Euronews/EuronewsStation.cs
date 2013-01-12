using System.ComponentModel.Composition;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Euronews {
	[Export(typeof(Station))]
	public class EuronewsStation : Station {
		private Brand brand;

		public EuronewsStation() : base(new StationChannelList {
			"Euronews en français",
			"Euronews in English",
			"Euronews in deutscher",
			"Euronews en español",
			"Euronews по-русски",
			"Euronews in italiano"
		}, "Central European Standard Time") { } // Viduseiropas laika josla.

		public override Channel GetChannel(byte number) {
			if (number == 0 || number > 6) throw new ChannelNotFoundException(number);
			if (brand == null)
				brand=new Brand(Colors.White, 0x787878.ToColor(), Colors.White, 0x333333.ToColor(), 0x222222.ToColor(), 0x222222.ToColor(), 0x222222.ToColor());
			// Astkaņošanas adrese no http://www.euronewsradio.com/ (.ru, .fr, utt.) oriģinālā ir .aac, bet strādā arī .mp3.
			return new HttpChannel(string.Format("http://euronews-0{0}.ice.infomaniak.ch/euronews-0{0}.mp3", number),
				GetResourceImage("Euronews.png"), timezone, Station.UseGuide ? new EuronewsGuide(number, timezone):null, brand);
		}
	}
}