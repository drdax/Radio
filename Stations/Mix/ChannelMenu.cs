using DrDax.RadioClient;

namespace Mix {
	public class ChannelMenu : Menu<Channel> {
		public ChannelMenu() : base(new MenuItemList {
			{ MenuIcon.Video, "Вид на студию" }
		}) {}

		public override void HandleCommand(int itemIndex) {
			// http://www2.mixnews.lv/radio_mixfm/live/ ir rtmpt://live.mixnews.lv/live/mp4:mixfm, no kura iegūta m3u8 adrese
			// Tāpat http://www2.mixnews.lv/radio_baltcom/live/ lieto RTMP.
			StudioWindow.Open(Items[0],
				Source.Number == 2 ? "http://live.mixnews.lv/live/mp4:mixfm/playlist.m3u8":"http://live.mixnews.lv/live/mp4:baltkom/playlist.m3u8",
				640, 360, @"chunklist\.m3u8\?wowzasessionid=[0-9]+");
		}
	}
}