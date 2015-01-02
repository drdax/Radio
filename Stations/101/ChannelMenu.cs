using DrDax.RadioClient;

namespace Ru101 {
	public class ChannelMenu : Menu<Channel> {
		public ChannelMenu() : base(new MenuItemList {
			{ MenuIcon.Settings, "Выбор каналов" },
			{ MenuIcon.Playlist, "Последние песни" }
		}) {}

		public override void HandleCommand(int itemIndex) {
			switch (itemIndex) {
				case 0:
					new SettingsWindow().ShowDialog();
					break;
				case 1:
					if (Source.Number != 0)
						new PlaylistWindow(Source.Number, Source.Caption, Source.Timezone).ShowDialog();
					break;
			}
		}
	}
}