using System;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Retro {
	public class RetroStation : Station {
		private Brand brand;
		public RetroStation() : base(new StationChannelList {
			"Ретро FM Москва",
			"Ретро FM Киев",
			"Ретро FM Киев романтика",
			"Ретро FM Киев русская",
			"Ретро FM Киев танцевальная",
			"Ретро FM Рига"
		}) {}

		public override Channel GetChannel(uint number) {
			if (brand == null)
				brand=new Brand(Colors.White, 0XF5BC48.ToColor(), Colors.White, 0x0168B5.ToColor(),
					new LinearGradientBrush(0xE74CB5.ToColor(), 0xBE358E.ToColor(), 90), new LinearGradientBrush(0x0168B5.ToColor(), 0x015797.ToColor(), 90));
			switch (number) {
				case 1: return new IcyChannel("http://retro128.streamr.ru/", GetResourceImage("Retro.png"), // Pastāv vēl "Hi-Fi" 256 bitu plūsma un šaurākas plūsmas.
					TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"), true, brand);
				case 2:
				case 3:
				case 4:
				case 5:
					return new IcyChannel(number == 2 ? "http://cast.retro.ua/retro":(number == 3 ? "http://cast2.retro.ua/retro_romantic":(number == 4 ? "http://cast2.retro.ua/retro_russian":"http://cast2.retro.ua/retro_dance")),
						GetResourceImage("Retro.png"), TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"), true, brand);
				case 6: return new ForcedIcyChannel("http://lifemedia.cloud.makonix.com:8000/", GetResourceImage("Retro.png"), TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time"), false, brand);
				default: throw new ChannelNotFoundException(number);
			}
		}
		public override Guide GetGuide(uint number) {
			switch (number) {
				case 1: return new RetroRuGuide();
				case 2: return new RetroUaGuide("online");
				case 3: return new RetroUaGuide("romantic");
				case 4: return new RetroUaGuide("russian");
				case 5: return new RetroUaGuide("dance");
				default: return null;
			}
		}
		public override string GetHomepage(uint number) {
			switch (number) {
				case 1: return "http://retrofm.ru/";
				case 2:
				case 3:
				case 4:
				case 5: return "http://retro.ua/";
				default: return "http://retrofm.lv/";
			}
		}
	}
}