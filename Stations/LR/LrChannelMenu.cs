using DrDax.RadioClient;

namespace Lr {
	public class LrChannelMenu : Menu<Channel> {
		public LrChannelMenu() : base(new MenuItemList {
			{ MenuIcon.Video, "Skats uz studiju" }
		}) {}

		public override void HandleCommand(int itemIndex) {
			StudioWindow.Open(Items[0],
				Source.Number == 1 ? "http://muste.radio.org.lv/livea/mp4:rez3.mp4_360p/playlist.m3u8":"http://muste.radio.org.lv/live/mp4:lr4h/playlist.m3u8",
				640, 360, @"chunklist\.m3u8\?wowzasessionid=[0-9]+");
		}
	}
}