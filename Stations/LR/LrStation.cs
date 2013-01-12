using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using DrDax.RadioClient;

namespace LR {
	[Export(typeof(Station))]
	public class LrStation : Station {
		public LrStation() : base(new StationChannelList(true) {
			"Latvijas Radio 1",
			"Latvijas Radio 2",
			"Latvijas Radio 3 “Klasika”",
			"Latvijas Radio 4 “Doma laukums”",
			"NABA"
		}, "E. Europe Standard Time") {} // Latvijas laika josla.

		public override Channel GetChannel(byte number) {
			if (number == 5) {
				return new IcyHttpChannel("http://nabamp0.latvijasradio.lv:8016/", GetResourceImage("Naba.png"), timezone, Station.UseGuide ? new NabaGuide(timezone):null,
					new Brand(0x58595B.ToColor(), 0xEE3A43.ToColor(), Colors.White, Colors.Black, Colors.White, Colors.White));
			}
			Int32 baseHex, guideHex;
			switch (number) {
				case 1: baseHex=0xF20000; guideHex=0xFFD1D1; break;
				case 2: baseHex=0xFFBF00; guideHex=0xFFF1CC; break;
				case 3: baseHex=0xBF7200; guideHex=0xF2D5AE; break;
				case 4: baseHex=0x408CFF; guideHex=0xCCE0FF; break;
				default: throw new ChannelNotFoundException(number);
			}
			return new MmsChannel(string.Format("mms://lr{0}w.latvijasradio.lv/pplr{0}", number),
				GetResourceImage(string.Format("LR{0}.png", number)),
				timezone,
				Station.UseGuide ? new LrGuide(number, timezone):null,
				new Brand(Colors.Black, baseHex.ToColor(), Colors.White,
					baseHex.ToColor(), guideHex.ToColor(), guideHex.ToColor()));
		}
	}
}