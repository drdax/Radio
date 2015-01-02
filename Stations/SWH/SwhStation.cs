using System;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Swh {
	public class SwhStation : Station {
		public SwhStation() : base(new StationChannelList {
			{ "SWH", 4 },
			{ "SWH+", 1 },
			{ "SWH Rock", 2 },
			{ "SpinFM", 3 },
			{ "SWH Gold", 0 }
		}, "E. Europe Standard Time") {} // Latvijas laika josla.

		public override Channel GetChannel(uint number) {
			string name, videoUrl=null; Int32 captionHex=0, bodyHex=0;
			switch (number) {
				case 1: name="swh";
					captionHex=0xFF7800; bodyHex=0xFFBE69;
					// Jā, SWH video plūsmas nosaukums ir Spin, un Spin vairs nav video.
					videoUrl="http://todayfm.mpl.miisolutions.net:1935/todayfm-live01/_definst_/mp4:CGV_SPINLV/playlist.m3u8";
					break;
				case 2: name="plus";
					captionHex=0xF4C300; bodyHex=0xFFE47A;
					videoUrl="http://nodeb.gocaster.net:1935/CGL/_definst_/mp4:CGV_SWHEXTRA/playlist.m3u8";
					break;
				case 3: name="rock";
					captionHex=0x6E1219; bodyHex=0xFFBECA;
					break;
				case 4: name="spin";
					//videoUrl="http://nodeb.gocaster.net:1935/CGL/_definst_/mp4:CGV_SPINLV/playlist.m3u8";
					break;
				case 5: name="gold";
					captionHex=0xF7B646; bodyHex=0xF8EECA;
					break;
				default: throw new ChannelNotFoundException(number);
			}
			return new IcyChannel(string.Format("http://80.232.162.149:8000/{0}96mp3", name),
				GetResourceImage(name+".png"),
				timezone,
				true,
				number == 4 ?
					new Brand(Colors.White, 0xF2208D.ToColor(), 0xF2208D.ToColor(), Colors.Black,
						Brushes.Black, Brushes.Black, new LinearGradientBrush(
							new GradientStopCollection {
								new GradientStop(0xE21091.ToColor(), 0.1),
								new GradientStop(0xE25091.ToColor(), 0.1), new GradientStop(0xE25091.ToColor(), 0.6),
								new GradientStop(0xE21081.ToColor(), 0.6)
							}, 90
						)):
					new Brand(Colors.Black, captionHex.ToColor(), Colors.White, captionHex.ToColor(),
						captionHex.ToColor(), bodyHex.ToColor(), bodyHex.ToColor()),
					number == 3 ? null:new ChannelMenu(number == 1 || number == 2 || number == 4, videoUrl));
		}

		public override Guide GetGuide(uint number) {
			string name; CaptionListedGuide listedGuide=null;
			switch (number) {
				case 1: name="swh"; listedGuide=new SwhListedGuide(false, timezone); break;
				case 2: name="plus"; listedGuide=new SwhListedGuide(true, timezone); break;
				case 3: name="rock"; break;
				case 4: name="spin"; break;
				default: name="gold"; break;
			}
			return new SwhGuide(string.Format("http://195.13.237.142:8080/{0}_online.xml", name), listedGuide, number < 4);
		}
		public override string GetHomepage(uint number) {
			switch (number) {
				case 1: return "http://www.radioswh.lv/";
				case 2: return "http://www.radioswhplus.lv/";
				case 3: return "http://player.radioswhrock.lv/";
				case 4: return "http://www.spinfm.lv/";
				default: return "http://radioswhgold.lv/";
			}
		}
	}
}