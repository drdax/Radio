using System.ComponentModel.Composition;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Super {
	[Export(typeof(Station))]
	public class SuperStation : Station {
		public SuperStation() : base(new StationChannelList {
			{ "Super FM", 2 },
			{ "European Hit Radio", 0 },
			{ "Хиты России", 1 }
		}, "E. Europe Standard Time") {} // Latvijas laika josla.

		public override Channel GetChannel(byte number) {
			switch (number) {
				case 1: return new HttpChannel("http://stream.superfm.lv:8000/lhr.mp3", GetResourceImage("SuperFM.png"), timezone, Station.UseGuide ? new EhrGuide():null,
					new Brand(0x555555.ToColor(), 0x8D810B.ToColor(), Colors.White, 0xD5C30F.ToColor(), 0x8E820B.ToColor(), 0xFAFAFA.ToColor(), 0xF7F7F7.ToColor()));
				case 2:
					return new HttpChannel("http://stream.europeanhitradio.lv:8000/ehr64", GetResourceImage("EHR.png"), timezone, Station.UseGuide ? new EhrGuide():null,
					new Brand(Colors.White, 0x333333.ToColor(), Colors.White,
						new LinearGradientBrush(0x3A4044.ToColor(), 0x191C1E.ToColor(), 90), new LinearGradientBrush(0x700101.ToColor(), 0xDD0202.ToColor(), 90)));
				case 3: return new HttpChannel("http://stream.hitirossii.com:8000/khr.mp3", GetResourceImage("HitiRossii.png"), timezone, Station.UseGuide ? new KhrGuide():null,
					new Brand(Colors.Black, 0xB0B0B1.ToColor(), 0xF6F6F6.ToColor(), 0x3F4044.ToColor(), 0x010101.ToColor(), 0xE6E6E6.ToColor(), 0xF2F0F0.ToColor()));
				default: throw new ChannelNotFoundException(number);
			}
		}
	}
}