using System;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Eurovision {
	public class EurovisionStation : Station {
		public EurovisionStation() : base(new StationChannelList(true) {
			"Eurovision"
		}, "GMT Standard Time") {}

		public override Channel GetChannel(uint number) {
			if (number != 1) throw new ChannelNotFoundException(number);
			return new ForcedIcyChannel("http://stream.escradio.com:8030/", GetResourceImage("ESC.png"),
			timezone, true,
			new Brand(0xEEEEEE.ToColor(), 0x6BB6DE.ToColor(), 0xF7F7F7.ToColor(), 0x161415.ToColor(),
				new LinearGradientBrush(
					new GradientStopCollection {
						new GradientStop(0xD65A54.ToColor(), 0),
						new GradientStop(0xCC413B.ToColor(), 0.5),
						new GradientStop(0xB73A34.ToColor(), 0.5),
						new GradientStop(0x9E2F29.ToColor(), 1)
					}, 90),
				new ImageBrush(GetResourceImage("ESCbackground.jpg")) { Stretch=Stretch.None, AlignmentX=AlignmentX.Left }),
				new ChannelMenu());
		}

		public override Guide GetGuide(uint channelNumber) {
			return new EurovisionGuide();
		}
		public override string GetHomepage(uint number) {
			return "http://www.escradio.com/";
		}
	}
}