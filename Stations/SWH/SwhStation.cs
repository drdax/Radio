using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Swh {
	[Export(typeof(Station))]
	public class SwhStation : Station {
		public SwhStation() : base(new StationChannelList {
			{ "SWH", 3 },
			{ "SWH+", 0 },
			{ "SWH Rock", 1 },
			{ "SpinFM", 2 }
		}, "E. Europe Standard Time") {} // Latvijas laika josla.

		public override Channel GetChannel(byte number) {
			string name; Int32 captionHex=0, bodyHex=0;
			switch (number) {
				case 1: name="swh";
					captionHex=0xFF7800; bodyHex=0xFFBE69;
					break;
				case 2: name="plus";
					captionHex=0xF4C300; bodyHex=0xFFE47A;
					break;
				case 3: name="rock";
					captionHex=0x6E1219; bodyHex=0xFFBECA;
					break;
				case 4: name="spin"; break;
				default: throw new ChannelNotFoundException(number);
			}
			// Šāda plūsmas adrese vairs nav atrodama raidstacijas mājaslapā, bet tā turpina strādāt.
			return new HttpChannel(string.Format("http://80.232.162.149:8000/{0}96mp3", name),
				GetResourceImage(name+".png"),
				timezone,
				Station.UseGuide ? new SwhGuide(string.Format("http://www.radioswh.lv/rss/{0}_online.xml", name), number == 1 || number == 2 ? new SwhListedGuide(number, timezone):null):null,
				number == 4 ?
					new Brand(Colors.White, 0xF2208D.ToColor(), 0xF2208D.ToColor(),
						Brushes.Black, Brushes.Black, new LinearGradientBrush(
							new GradientStopCollection {
								new GradientStop(0xE21091.ToColor(), 0.1),
								new GradientStop(0xE25091.ToColor(), 0.1), new GradientStop(0xE25091.ToColor(), 0.6),
								new GradientStop(0xE21081.ToColor(), 0.6)
							}, 90
						)):
					new Brand(Colors.Black, captionHex.ToColor(), Colors.White,
						captionHex.ToColor(), bodyHex.ToColor(), bodyHex.ToColor()));
		}
	}
}