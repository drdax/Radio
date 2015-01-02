using DrDax.RadioClient;

namespace Riga {
	public class ChannelMenu : Menu<Channel> {
		public ChannelMenu() : base(new MenuItemList {
			{ MenuIcon.Video, "Skats uz studiju" }
		}) {}

		public override void HandleCommand(int itemIndex) {
			switch (Source.Number) {
				case 1:
					StudioWindow.Open(Items[0], "http://185.8.60.8/capitalfm.m3u8", 720, 436, @"http://185\.8\.60\.8/hls-live/livepkgr/_definst_/capitalfmevent/capitalfmlive\.m3u8");
					break;
				case 3:
					StudioWindow.Open(Items[0], "http://r.rigaradio.lv:443/live/playlist.m3u8", 852, 480, @"video2-2\/playlist\.m3u8"); // Sesijas numurs ir TS failu nevis atskaņošanas sarakstu nosaukumā.
					// http://api.rigaradio.lv/2/onair/stream/current.json ņemts no iPhone lietotnes un satur skaņas un bildes plūsmu adreses.
					// http://r.rigaradio.lv:443/live/playlist.m3u8 satur divas adreses, kuru apraksts ir vienāds, bet faktiski pirmajā ir mazāka bilde (480x270). Šeit lieto otro.
					break;
			}
		}
	}
}