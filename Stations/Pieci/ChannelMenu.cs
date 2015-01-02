using DrDax.RadioClient;

namespace Pieci {
	public class ChannelMenu : Menu<Channel> {
		public ChannelMenu() : base(new MenuItemList {
			{ MenuIcon.Settings, "Kanāli" },
			{ MenuIcon.Playlist, "Pirms tam skanēja" }
		}) {}

		public override void HandleCommand(int itemIndex) {
			if (itemIndex == 0)
				new ChannelsWindow().ShowDialog();
			else new PlaylistWindow(Source.Number, Source.Caption, Source.Brand.CaptionForeground, Source.Timezone).ShowDialog();
		}
	}
}