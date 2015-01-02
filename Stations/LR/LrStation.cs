using System;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Lr {
	public class LrStation : Station {
		public LrStation() : base(new StationChannelList(true) {
			"Latvijas Radio 1",
			"Latvijas Radio 2",
			"Latvijas Radio 3 “Klasika”",
			"Latvijas Radio 4 “Doma laukums”",
			"NABA"
		}, "E. Europe Standard Time") {} // Latvijas laika josla.

		public override Channel GetChannel(uint number) {
			if (number == 5)
				return new ForcedIcyChannel("http://nabamp0.latvijasradio.lv:8016/", GetResourceImage("Naba.png"), timezone, true,
					new Brand(0x58595B.ToColor(), 0xEE3A43.ToColor(), Colors.White, Colors.Black, Colors.Black, Colors.White, Colors.White), new NabaChannelMenu());
			Int32 baseHex, guideHex; string prefix, port;
			switch (number) {
				case 1: baseHex=0xF20000; guideHex=0xFFD1D1; prefix="1mp1"; port="12"; break;
				case 2: baseHex=0xFFBF00; guideHex=0xFFF1CC; prefix="2mp1"; port="02"; break;
				case 3: baseHex=0xBF7200; guideHex=0xF2D5AE; prefix="3mp0"; port="04"; break;
				case 4: baseHex=0x408CFF; guideHex=0xCCE0FF; prefix="4mp1"; port="20"; break;
				default: throw new ChannelNotFoundException(number);
			}
			var brand=new Brand(Colors.Black, baseHex.ToColor(), Colors.White, baseHex.ToColor(), baseHex.ToColor(), guideHex.ToColor(), guideHex.ToColor());
			if (number == 4) // LR4 nezināmu iemeslu pēc HTTP plūsmas tikai AAC formātā, tāpēc lieto MMS.
				return new UrlChannel("mms://lr4w.latvijasradio.lv/pplr4",
					GetResourceImage("LR4.png"), timezone, true,
					brand, new LrChannelMenu());
			// Kopš LR1 ieciklēšanās atteicāmies no MMS. Meta datu plūsmā nav.
			return new ForcedIcyChannel(string.Format("http://lr{0}.latvijasradio.lv:80{1}/", prefix, port),
				GetResourceImage(string.Format("LR{0}.png", number)),
				timezone, true, brand,
				number == 1 /*|| number == 4*/ ? new LrChannelMenu():null);
		}

		public override Guide GetGuide(uint number) {
			if (number == 5) return new NabaGuide(timezone);
			return new LrGuide(number, timezone);
		}
		public override string GetHomepage(uint number) {
			if (number == 2) return "http://lr2.latvijasradio.lv/";
			else if (number == 5) return "http://www.naba.lv/";
			return string.Concat("http://lr", number, ".latvijasradio.lv/lv/lr", number, "/");
		}
	}
}