using DrDax.RadioClient;

namespace Super {
	public class ChannelMenu : Menu<Channel> {
		/// <param name="listCaption">Atskaņošanas saraksta lappuses nosaukums.</param>
		/// <param name="listUrl">Atskaņošanas saraksta lappuses pilna adrese.</param>
		public ChannelMenu(string listCaption, string listUrl) : base(new MenuItemList {
			{ MenuIcon.Playlist, listCaption }
		}) {
			this.listUrl=listUrl;
		}
		public override void HandleCommand(int itemIndex) {
			DefaultProgram.OpenPage(listUrl);
		}

		private readonly string listUrl;
	}
}