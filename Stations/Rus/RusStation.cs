using System.ComponentModel.Composition;
using System.Windows.Media;
using DrDax.RadioClient;

namespace RadioRus {
	[Export(typeof(Station))]
	public class RusStation : Station {
		public RusStation() : base(new StationChannelList {
			"Радио России"
		}, "Russian Standard Time") {} // Maskavas laika josla.

		public override Channel GetChannel(byte number) {
			if (number != 1) throw new ChannelNotFoundException(number);
			return new MmsChannel("mms://live.rfn.ru/radiorussia", GetResourceImage("RadioRus.png"), timezone, Station.UseGuide ? new RusGuide(timezone):null,
				new Brand(0x666666.ToColor(), 0x9E0B0E.ToColor(), Colors.White, 0x006699.ToColor(), 0xECF0F3.ToColor(), Colors.White));
		}
	}
}