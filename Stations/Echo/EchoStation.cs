using System.Windows.Media;
using DrDax.RadioClient;

namespace Echo {
	public class EchoStation : Station {
		public EchoStation() : base(new StationChannelList {
			"ЭХО Москвы"
		}, "Russian Standard Time") {} // Maskavas laika josla.
		public override Channel GetChannel(uint number) {
			return new IcyChannel("http://81.19.85.197/echo.mp3", GetResourceImage("Echo.png"), timezone, true, new Brand(
				Colors.Black, 0xED1C24.ToColor(), Colors.White, 0x303338.ToColor(), 0x231F20.ToColor(), Colors.White, Colors.White
				), new ChannelMenu());
		}
		public override Guide GetGuide(uint channelNumber) {
			// Vēl ir piecu nākamo raidījumu saraksts http://echo.msk.ru/sounds/current.html, bet tas neietver pašreizējo un arī ne visus nākamos (piemēram, ziņu izlaidumus).
			return new EchoGuide(timezone);
		}
		public override string GetHomepage(uint channelNumber) {
			return "http://echo.msk.ru/";
		}
	}
}