using System.Text.RegularExpressions;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Vgtrk {
	public class VgtrkStation : Station {
		public VgtrkStation() : base(new StationChannelList {
			"Радио России",
			"Радио Культура",
			"Маяк",
			{ "Радио Юность", 1 },
			"Вести ФМ"
		}, "Russian Standard Time") {} // Maskavas laika josla.

		public override Channel GetChannel(uint number) {
			switch (number) {
				case 1: return new UrlChannel("http://icecast.vgtrk.cdnvideo.ru/rrzonam_mp3_192kbps", GetResourceImage("RadioRus.png"), timezone, true,
						new Brand(0x333333.ToColor(), 0xCE0000.ToColor(), 0x2173B7.ToColor(), 0x686868.ToColor(),
							new LinearGradientBrush(0xF6F6F6.ToColor(), 0xE7E7E7.ToColor(), 90), new LinearGradientBrush(0xE7E7E7.ToColor(), 0xF6F6F6.ToColor(), 90),
							new LinearGradientBrush(new GradientStopCollection {
								new GradientStop(0xFBD2AD.ToColor(), 0),
								new GradientStop(0xFFF0E3.ToColor(), 0.2),
								new GradientStop(0xFEE9D5.ToColor(), 0.2),
								new GradientStop(0xEFBC8A.ToColor(), 1)
							}, 90)));
				case 2: return new IcyChannel("http://livestream.rfn.ru:8080/kulturafm/mp3_192kbps", GetResourceImage("RadioCult.png"), timezone, true,
					new Brand(0x344A81.ToColor(), 0x394172.ToColor(), Colors.White, 0x344A81.ToColor(), 0x374171.ToColor(), Colors.White, Colors.White));
				case 3: return new IcyChannel("http://livestream.rfn.ru:8080/mayakfm/mp3_192kbps", GetResourceImage("Mayak.png"), timezone, true,
					new Brand(Colors.Black, 0xC03B3E.ToColor(), 0x485764.ToColor(), 0x316E9A.ToColor(), 0xFCFCFC.ToColor(), Colors.White, Colors.White), new MayakMenu());
				case 4: return new UrlChannel("http://icecast.vgtrk.cdnvideo.ru/ufm_mp3_192kbps", GetResourceImage("Unost.png"), timezone, false, null);
				case 5: return new UrlChannel("http://icecast.vgtrk.cdnvideo.ru/vestifm_mp3_192kbps", GetResourceImage("Vesti.png"), timezone, true,
					new Brand(0x535865.ToColor(), 0xD40909.ToColor(), Colors.White, 0x111111.ToColor(), new LinearGradientBrush(
					new GradientStopCollection {
						new GradientStop(0x3B414F.ToColor(), 0),
						new GradientStop(0x3B414F.ToColor(), 0.5),
						new GradientStop(0x0A1123.ToColor(), 0.5),
						new GradientStop(0x0A1123.ToColor(), 1)
					}, 90), Brushes.White));
				default: throw new ChannelNotFoundException(number);
			}
		}
		public override Guide GetGuide(uint channelNumber) {
			switch (channelNumber) {
				case 1: return new RusGuide("radiorus", timezone);
				case 2: return new RusGuide("cultradio", timezone);
				case 3: return new MayakGuide(timezone);
				default /*5*/: return new VestiGuide(timezone);
			}
		}
		public override string GetHomepage(uint channelNumber) {
			switch (channelNumber) {
				case 1: return "http://www.radiorus.ru/";
				case 2: return "http://www.cultradio.ru/";
				case 3: return "http://www.radiomayak.ru/";
				case 4: return "http://www.radiounost.ru/";
				default: return "http://radiovesti.ru/";
			}
		}
	}

	internal static class Quotes {
		private static readonly Regex quoteRx=new Regex("\"([^\"]+)\"", RegexOptions.Compiled);
		public static string Format(string text) {
			return quoteRx.Replace(text, "«$1»");
		}
	}
}