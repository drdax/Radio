using System;
using DrDax.RadioClient;

namespace Swh {
	public class ChannelMenu : Menu<Channel> {
		internal ChannelMenu(bool hasArchive, string videoUrl) : base(new MenuItemList()) {
			this.Items.Add(MenuIcon.Playlist, "50 dziesmas");
			if (hasArchive) { archiveIndex=this.Items.Count; this.Items.Add(MenuIcon.Playlist, "Arhīvs"); } else archiveIndex=-1;
			if (videoUrl != null) this.Items.Add(MenuIcon.Video, "Skatīties tiešraidi");
			this.videoUrl=videoUrl;
		}

		public override void HandleCommand(int itemIndex) {
			if (itemIndex == 0) {
				string url;
				switch (Source.Number) {
					case 1: url="http://www.radioswh.lv/eters/pedejas-50-dziesmas/"; break;
					case 2: url="http://www.radioswhplus.lv/последние-50-песен/"; break;
					case 3: url="http://old.radioswh.lv/rss/rock_playlist.html"; break;
					case 4: url="http://www.spinfm.lv/eters/pedejas-50-dziesmas/"; break;
					default: url="http://radioswhgold.lv/dziesmas"; break;
				}
				DefaultProgram.OpenPage(url); return;
			}
			if (itemIndex == archiveIndex) {
				DefaultProgram.OpenPage(string.Concat(Source.HomepageUrl, "player?archive=",
					TimeZoneInfo.ConvertTime(DateTime.UtcNow, Source.Timezone).ToString("yyyy-MM-dd"))); // Neņem vērā faktu, ka SpinFM nav brīvdienu arhīva un tas mēdz kavēties par nedēļu.
				return;
			}
			StudioWindow.Open("Tiešraide no studijas", videoUrl, 640, 360, @"chunklist_w([0-9]+)\.m3u8");
		}
		private readonly string videoUrl;
		private readonly int archiveIndex;
	}
}