namespace DrDax.RadioClient {
	public class SimpleGuideMenu : Menu<Guide> {
		public SimpleGuideMenu(string infoItemCaption, string urlPrefix=null) : base(new MenuItemList {
			{ MenuIcon.Information, infoItemCaption }
		}) {
			this.urlPrefix=urlPrefix;
		}

		public override void HandleCommand(int itemIndex) {
			string url=((PagedBroadcast)Source.CurrentBroadcast).PageUrl;
			if (url != null) DefaultProgram.OpenPage(urlPrefix+url);
		}
		private readonly string urlPrefix;
	}
}