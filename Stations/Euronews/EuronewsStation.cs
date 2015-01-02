using System.Windows.Media;
using DrDax.RadioClient;

namespace Euronews {
	public class EuronewsStation : Station {
		private Brand brand;

		public EuronewsStation() : base(new StationChannelList {
			"Euronews en français",
			"Euronews in English",
			"Euronews in deutscher",
			"Euronews en español",
			"Euronews по-русски",
			"Euronews in italiano"
		}, "Central European Standard Time") {} // Viduseiropas laika josla.

		public override Channel GetChannel(uint number) {
			if (number == 0 || number > 6) throw new ChannelNotFoundException(number);
			if (brand == null)
				brand=new Brand(Colors.White, Colors.White, Colors.White, 0x088484.ToColor(), 0x088484.ToColor(), 0x0CB6B5.ToColor(), 0x0CB6B5.ToColor());
			// Astkaņošanas adrese no http://www.euronewsradio.com/ (.ru, .fr, utt.) oriģinālā ir .aac, bet strādā arī .mp3.
			return new UrlChannel(string.Format("http://euronews-0{0}.ice.infomaniak.ch/euronews-0{0}.mp3", number),
				GetResourceImage("Euronews.png"), timezone, true, brand);
		}

		public override Guide GetGuide(uint number) {
			return new EuronewsGuide(number, timezone);
		}
		public override string GetHomepage(uint number) {
			return "http://euronews.com/";
		}
	}
}