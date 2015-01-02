using System.Windows.Media;
using DrDax.RadioClient;

namespace Tavr {
	public class TavrStation : Station {
		public TavrStation() : base(new StationChannelList {
			{ "Радио Мелодия", 2 },
			{ "Русское Радио", 5 },
	
			{ "ХiтFM", 0 },
			{ "Хэппи Ранок", 0 },
			{ "Michael Jackson", 0 },
			{ "Romantic Collection", 0 },
			{ "Дискотекa 90-х", 0 },
			{ "Лучшие хиты 90-х", 0 },

			{ "KISS FM", 1 },
			{ "KISS FM Deep", 1 },
			{ "KISS FM Digital", 1 },
			{ "KISS FM Trance", 1 },

			{ "Radio ROKS", 4 },
			{ "[КАМТУГЕЗА]", 4 },
			{ "Рок-баллады", 4 },
			{ "Рок-концерт", 4 },
			{ "Український рок", 4 },
			{ "Beatles", 4 },
			{ "Hard'n'Heavy", 4 },

			{ "Relax", 3 },
			{ "Звуки природы", 3 },
			{ "Музыка без слов", 3 }
		}, "E. Europe Standard Time") {}

		public override Channel GetChannel(uint number) {
			switch (number) {
				case 1: return new IcyChannel("http://online-radiomelodia.tavrmedia.ua/RadioMelodia", GetResourceImage("Melodija.png"), timezone, true,
					new Brand(Colors.Black, 0xDC6E30.ToColor(), Colors.White, 0x5582D2.ToColor(), 0x81A4E4.ToColor(), 0xF2E8D0.ToColor(), 0xF2E8D0.ToColor()),
					new ChannelMenu());
				case 2: return new IcyChannel("http://online-rusradio.tavrmedia.ua/RusRadio", GetResourceImage("Russkoe.png"), timezone, true,
					new Brand(Colors.Black, 0x17289A.ToColor(), Colors.White, 0x6E0018.ToColor(), 0xC10F03.ToColor(), 0x6E0018.ToColor(), Colors.White, Colors.White),
					new ChannelMenu());

				case 3: return GetHitChannel("http://195.95.206.17:80/HitFM");
				case 4: return GetHitChannel("http://online-hitfm2.tavrmedia.ua/HitFM_HappyRanok");
				case 5: return GetHitChannel("http://online-hitfm2.tavrmedia.ua/HitFM_MJ");
				case 6: return GetHitChannel("http://online-hitfm2.tavrmedia.ua/HitFM_Romantic");
				case 7: return GetHitChannel("http://online-hitfm2.tavrmedia.ua/HitFM_Disco90");
				case 8: return GetHitChannel("http://online-hitfm2.tavrmedia.ua/HitFM_Best90");

				case 9: return GetKissChannel("http://online-kissfm.tavrmedia.ua/KissFM", 0x0E87B2, true);
				case 10: return GetKissChannel("http://online-kissfm.tavrmedia.ua/KissFM_deep", 0x05569D, false);
				case 11: return GetKissChannel("http://online-kissfm.tavrmedia.ua/KissFM_digital", 0xA101AA, false);
				case 12: return GetKissChannel("http://online-kissfm.tavrmedia.ua/KissFM_trance", 0x6907CD, false);

				case 13: return GetRoksChannel("http://online-radioroks.tavrmedia.ua/RadioROKS");
				case 14: return GetRoksChannel("http://online-radioroks2.tavrmedia.ua/RadioROKS_KAMTUGEZA");
				case 15: return GetRoksChannel("http://online-radioroks2.tavrmedia.ua/RadioROKS_Ballads");
				case 16: return GetRoksChannel("http://online-radioroks2.tavrmedia.ua/RadioROKS_Concert");
				case 17: return GetRoksChannel("http://online-radioroks2.tavrmedia.ua/RadioROKS_Ukr");
				case 18: return GetRoksChannel("http://online-radioroks2.tavrmedia.ua/RadioROKS_Beatles");
				case 19: return GetRoksChannel("http://online-radioroks2.tavrmedia.ua/RadioROKS_HardnHeavy");

				case 20: return GetRelaxChannel("http://online-radiorelax.tavrmedia.ua/RadioRelax", true);
				case 21: return GetRelaxChannel("http://online-radiorelax2.tavrmedia.ua/RadioRelax_Nature", false);
				case 22: return GetRelaxChannel("http://online-radiorelax2.tavrmedia.ua/RadioRelax_Instrumental", true);
				default: throw new ChannelNotFoundException(number);
			}
		}

		public override Guide GetGuide(uint channelNumber) {
			switch (channelNumber) {
				case 3: return new HitGuide(timezone);
				default: return new SimpleIcyGuide(channelNumber >= 9 && channelNumber <= 12, null);
			}
		}
		public override string GetHomepage(uint channelNumber) {
			switch (channelNumber) {
				case 1: return "http://www.radiomelodia.ua/";
				case 2: return "http://www.rusradio.ua/";
				case 3: case 4: case 5: case 6: case 7: case 8:
					return "http://www.hitfm.ua/";
				case 9: case 10: case 11: case 12: return "http://www.kissfm.ua/";
				case 20: case 21: case 22:
					return "http://www.radiorelax.ua/";
				default: return "http://www.radioroks.ua/";
			}
		}

		private IcyChannel GetHitChannel(string url) {
			return new IcyChannel(url, GetResourceImage("HitFM.png"), timezone, true,
				new Brand(Colors.Black, 0x990000.ToColor(), Colors.White, 0x990000.ToColor(), 0xD11F1B.ToColor(), 0xA30C0B.ToColor(), 0xEFEFEF.ToColor(), Colors.White),
				new ChannelMenu());
		}
		private Channel GetKissChannel(string url, int captionColor, bool hasPlaylist) {
			return new IcyChannel(url, GetResourceImage("Kiss.png"), timezone, true,
				new Brand(0x00B7EB.ToColor(), 0xD603CA.ToColor(), Colors.White, Colors.Black,
					new LinearGradientBrush(captionColor.ToColor(), Colors.Black, 90), new LinearGradientBrush(Colors.Black, 0x252025.ToColor(), 90)),
				hasPlaylist ? new ChannelMenu():null); // rtmp://46.182.84.24/kiss video plūsma
		}
		private IcyChannel GetRoksChannel(string url) {
			return new IcyChannel(url, GetResourceImage("Roks.png"), timezone, true,
				new Brand(0xFFCC00.ToColor(), 0xC6C6C6.ToColor(), 0xD4D4D4.ToColor(), 0x141414.ToColor(), 0x141414.ToColor(), 0x191919.ToColor(), 0x191919.ToColor()),
				new ChannelMenu());
		}
		private IcyChannel GetRelaxChannel(string url, bool hasGuide) {
			return new IcyChannel(url, GetResourceImage("Relax.png"), timezone, hasGuide,
				new Brand(0x019BB3.ToColor(), 0x00B4CF.ToColor(), Colors.White, 0x019BB3.ToColor(), 0x3CCFE6.ToColor(), 0x23B6CE.ToColor(), Colors.White, Colors.White),
				hasGuide ? new ChannelMenu():null);
		}
	}
}