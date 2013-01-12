using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using DrDax.RadioClient;

namespace Nrcu {
	[Export(typeof(Station))]
	public class NrcuStation : Station {
		private Brand brand;

		public NrcuStation() : base(new StationChannelList {
			"Перший канал",
			"Другий канал «Промiнь»",
			"Третій канал «Культура»",
			"Всесвітня служба «Радіомовлення України»"
		}, "E. Europe Standard Time") { } // Ukrainas laika josla.

		public override Channel GetChannel(byte number) {
			if (number == 0 || number > 4) throw new ChannelNotFoundException(number);
			if (brand == null)
				brand=new Brand(0x003F8F.ToColor(), 0x005CAA.ToColor(), 0x003F8F.ToColor(),
					0xD6F5FF.ToColor(), 0xA8EAFF.ToColor(), 0xA8EAFF.ToColor(), 0xFCEEAB.ToColor());
			return new MmsChannel("mms://89.187.1.165/NRCU"+number,
				GetResourceImage(number == 2 ? "Promin.png":"UkrRadio.png"),
				timezone,
				Station.UseGuide ? new NrcuGuide(number, timezone):null,
				brand);
		}
	}
}