using System.Windows;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Riga {
	public class RigaStation : Station {
		public RigaStation() : base(new StationChannelList(true) {
			"Capital FM",
			"Latvijas Kristīgais radio",
			"Rīga Radio",
			"Star FM",
			"Top Radio"
		}, "E. Europe Standard Time") {} // Latvijas laika josla.

		public override Channel GetChannel(uint number) {
			switch (number) {
				case 1: return new ForcedIcyChannel("http://radio2.capitalfm.lv:8000/", GetResourceImage("Capital.png"), timezone, true,
					new Brand(0x353434.ToColor(), 0xCC0000.ToColor(), Colors.White, Colors.Black,
						0xCC0000.ToColor(), 0x7A0000.ToColor(), Colors.White, Colors.White), new ChannelMenu()); // Alternatīva http://www.capitalfm.lv/script/onair/?0.06887976755776848 (gadijumskaitlis)
				case 2:return new ForcedIcyChannel("http://91.203.71.10:8006/", GetResourceImage("LKR.png"), timezone, true,
					new Brand(Colors.White, 0xBDBDBD.ToColor(), Colors.White, 0x621C3D.ToColor(), 0x621C3D.ToColor(), 0x621C3D.ToColor(), 0x9A5E74.ToColor()));
				case 3: return new IcyChannel("http://r.rigaradio.lv:8000/mp3", GetResourceImage("Riga.png"), timezone, true,
					new Brand(0x222222.ToColor(), 0x151515.ToColor(), 0xEEEEEE.ToColor(), Colors.Black,
						new LinearGradientBrush(0x151515.ToColor(), 0x5C5C5C.ToColor(), 0),
						new ImageBrush(GetResourceImage("RigaBackground.png")) { Stretch=Stretch.None, AlignmentX=AlignmentX.Left }
					), new ChannelMenu());
				case 4: return new SegmentedChannel("http://starfm.deac.lv:1935/live/starfm/",
					GetResourceImage("Star.png"), timezone, true,
					new Brand(Colors.White, 0xF5D80D.ToColor(), 0xF5D80D.ToColor(), 0x071726.ToColor(),
						new RadialGradientBrush(0x071726.ToColor(), 0x03728C.ToColor()) {
							Center=new Point(0, 20),
							MappingMode=BrushMappingMode.Absolute,
							RadiusX=250, RadiusY=250
						},
						new RadialGradientBrush(0x071726.ToColor(), 0x03728C.ToColor()) {
							Center=new Point(0, 0),
							MappingMode=BrushMappingMode.Absolute,
							RadiusX=250, RadiusY=250
						},
						new SolidColorBrush(0X679FAC.ToColor())));
				case 5: return new ForcedIcyChannel("http://195.13.200.164:8000/", GetResourceImage("Top.png"), timezone, true,
					new Brand(Colors.Black, 0xE0001D.ToColor(), Colors.White, 0x0080C1.ToColor(),
						new LinearGradientBrush(0x0080C1.ToColor(), 0x0077BA.ToColor(), 90),
						new SolidColorBrush(0xFAFAFA.ToColor()),
						new LinearGradientBrush(
							new GradientStopCollection {
								new GradientStop(0xFFDA00.ToColor(), 0),
								new GradientStop(0xF1F200.ToColor(), 0.15),
								new GradientStop(0xFFDA00.ToColor(), 1)
							}, 90)
					));
				default: throw new ChannelNotFoundException(number);
			}
		}
		public override Guide GetGuide(uint number) {
			switch (number) {
				case 1: return new CapitalGuide(timezone);
				case 2: return new KristigaisGuide(timezone);
				case 3: return new RigaGuide(timezone); // SimpleIcyGuide agrāk derēja, bet tagad visu laiku teksts "Rīga radio" dziesmas datu vietā
				case 4: return new StarGuide();
				default: return new TopGuide(timezone);
			}
		}
		public override string GetHomepage(uint number) {
			switch (number) {
				case 1: return "http://www.capitalfm.lv/";
				case 2: return "http://lkr.lv/";
				case 3: return "http://www.rigaradio.lv/";
				case 4: return "http://www.starfm.lv/";
				default: return "http://topradio.lv/";
			}
		}
	}
}