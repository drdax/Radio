using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Retro {
	[Export(typeof(Station))]
	public class RetroStation : Station {
		private Brand brand;
		public RetroStation() : base(new StationChannelList {
			"Ретро FM Москва",
			"Ретро FM Киев",
			"Ретро FM Рига"
		}) {}

		public override Channel GetChannel(byte number) {
			if (brand == null)
				brand=new Brand(Colors.White, 0XF5BC48.ToColor(), Colors.White,
					new LinearGradientBrush(0xE74CB5.ToColor(), 0xBE358E.ToColor(), 90), new LinearGradientBrush(0x0168B5.ToColor(), 0x015797.ToColor(), 90));
			switch (number) {
				case 1: return new HttpChannel("http://webcast1.emg.fm:55655/retro128.mp3", GetResourceImage("Retro.png"),
					TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"), Station.UseGuide ? new RetroRuGuide():null, brand);
				case 2: return new HttpChannel("http://cast.retro.ua/retro", GetResourceImage("Retro.png"),
					TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"), Station.UseGuide ? new RetroUaGuide():null, brand);
				case 3: return new IcyHttpChannel("http://lifemedia.cloud.makonix.com:8000/", GetResourceImage("Retro.png"), TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"), null, brand);
				default: throw new ChannelNotFoundException(number);
			}
		}
	}
}