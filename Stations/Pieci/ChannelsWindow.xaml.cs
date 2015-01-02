using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Pieci {
	/// <summary>Kanālu izvēles logs. Rāda visus pašlaik pieejamos pieci.lv kanālus.</summary>
	public partial class ChannelsWindow : Window {
		public ChannelsWindow() {
			InitializeComponent();
			this.Loaded+=Window_Loaded;
			this.Closing+=Window_Closing;
		}
		private async void Window_Loaded(object sender, EventArgs e) {
			this.Loaded-=Window_Loaded;
			HashSet<uint> allIds=new HashSet<uint>();
			var selectedIds=new HashSet<uint>(Settings.Default.Channels.Select(c => c.Id));
			Color defaultColor=0x99A2A8.ToColor(); // Retos gadījumos skanošo sarakstā ir kanāls, kuru nerāda ar krāsainām saitēm.
			// Iegūst pamata datus: nosaukumu, īsu aprakstu un identifikatoru.
			foreach (Match match in channelRx.Matches(await client.DownloadStringTaskAsync("http://live.pieci.lv/"))) {
				string idString=match.Groups["id"].Value;
				uint id=idString.Length == 0 ? PieciStation.EmptyId:uint.Parse(idString);
				channels.Add(new ChannelItem(id, match.Groups["caption"].Value, defaultColor,
					match.Groups["description"].Value, match.Groups["name"].Value, selectedIds.Contains(id)));
				allIds.Add(id);
			}
			// Iegūst krāsas. Lielākai daļai kanālu tās ir zināmas (un tāpēc ir iekļautas ikonas), bet dažiem sezonāliem kanāliem nevar paredzēt, tāpēc izgūst no atskaņotāja lapas.
			foreach (Match match in colorRx.Matches(await client.DownloadStringTaskAsync("http://fm.pieci.lv"))) {
				uint id=uint.Parse(match.Groups["id"].Value);
				channels.Single(c => c.Id == id).SetColor(match.Groups["color"].Value);
			}
			list.ItemsSource=channels;

			// Izņem no saraksta zudušos kanālus.
			int beforeCount=selectedIds.Count;
			selectedIds.IntersectWith(allIds);
			HasChanges=beforeCount != selectedIds.Count;
		}
		private void Window_Closing(object sender, CancelEventArgs e) {
			this.Closing-=Window_Closing;
			if (hasChanges) {
				List<ChannelItem> newChannels=new List<ChannelItem>(channels.Count);
				newChannels.AddRange(channels.Where(channel => channel.Selected));
				newChannels.Sort();
				Settings.Default.Channels=newChannels;
			}
		}

		private void CheckBox_Toggled(object sender, RoutedEventArgs e) {
			var item=((CheckBox)sender).DataContext as ChannelItem;
			if (item == null) return;
			HasChanges=true;
		}

		private readonly Regex channelRx=new Regex(@"server-name"">(?'caption'[^<]+)</span><span class=""stripe rt""></span>\n</h1>\n<p class=""stream-description"" title=""Stream Description"">(?'description'[^<]+)</p></header><div class=""mount-point-data"">\n<p class=""current-song"" title=""Current Song"">[\s\S]+?<a target=""_blank"" href=""http://(?'name'[a-z]+)\.[\s\S]+?class=""playlist"" href=""/live(?'id'[0-9]*)-hq.mp3.m3u""");
		private readonly Regex colorRx=new Regex(@"<span style=""color: #(?'color'[0-9a-fA-F]{6});"" class=""channelId(?'id'[0-9]+)Color"">");
		private readonly ProperWebClient client=new ProperWebClient();
		private readonly List<ChannelItem> channels=new List<ChannelItem>(8);
		/// <summary>Vai ir mainījusies izvēlēto kanālu kopa.</summary>
		private bool hasChanges=false;
		private bool HasChanges {
			set {
				if (value) {
					if (!hasChanges) this.Title+=" *";
					hasChanges=true;
				}
			}
		}
	}
}