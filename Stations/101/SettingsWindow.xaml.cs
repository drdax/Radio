using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Linq;

namespace Ru101 {
	/// <summary>Kanālu un reģiona izvēles logs.</summary>
	public partial class SettingsWindow : ConnectedWindow {
		public SettingsWindow() {
			InitializeComponent();
			if (Settings.Default.Channels == null)
				selectedChannels=new List<ChannelItem>(0);
			else selectedChannels=Settings.Default.Channels;
			selectedIds=new SortedSet<uint>(selectedChannels.Select(c => c.Id));
			this.Loaded+=Window_Loaded;
			this.Closing+=Window_Closing;
		}

		private async void Window_Loaded(object sender, EventArgs e) {
			this.Loaded-=Window_Loaded;
			XElement json=await GetJson("http://101.ru/api/getgroup.php");
			// {"group_id":"2","name":"\u0422\u0430\u043d\u0446\u0435\u0432\u0430\u043b\u044c\u043d\u044b\u0435","name_eng":"Dance Music","count":"17","picUrl":"http:\/\/101.ru\/vardata\/modules\/channel\/image\/group_2.png"}
			groups=new List<ChannelGroup>(17);
			foreach (var item in json.Elements()) {
				short id=short.Parse(item.Element("group_id").Value);
				if (id > 0) // Perosnalizēto staciju atbalsts atskaņošanā prasa īpašas izmaiņas :(
					groups.Add(new ChannelGroup(id, item.Element("name").Value, int.Parse(item.Element("count").Value)));
			}
			groupList.ItemsSource=groups;
			regionList.ItemsSource=Enum.GetNames(typeof(Region));
			regionList.SelectedItem=Settings.Default.Region.ToString(); // Lai netaisītu lieku konvertoru, vērtību pieškir šeit un nolasa, aizverot logu.
		}

		private void Window_Closing(object sender, CancelEventArgs e) {
			this.Closing-=Window_Closing;
			if (hasChanges) {
				List<ChannelItem> newChannels=new List<ChannelItem>(10);
				// Savāc kanālu nosaukumus no ielādētajiem datiem.
				foreach (var g in groups)
					if (g.Channels != null)
						foreach (var c in g.Channels) {
							if (c.Selected)
								newChannels.Add(c);
							selectedIds.Remove(c.Id);
						}
				// Savāc atlikušos kanālus no iepriekšējā saraksta.
				foreach (var c in selectedChannels)
					if (selectedIds.Contains(c.Id)) newChannels.Add(c);
				newChannels.Sort();
				Settings.Default.Channels=newChannels;
			};
			Settings.Default.Region=(Region)Enum.Parse(typeof(Region), (string)regionList.SelectedItem);
		}

		private async void groupList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e.AddedItems.Count != 1) return;
			var group=e.AddedItems[0] as ChannelGroup;
			if (group.Channels == null) {
				bool focused=DisableLists(groupList);
				XElement json=await GetJson("http://101.ru/api/getstationsbygroup.php?group_id="+group.Id);
				// {"id":"111","name":"Elvis Presley","group_id":"12","picUrl":"http:\/\/www3.101.ru\/vardata\/modules\/channel\/dynamics\/pro\/111.jpg"},{"id":"55","name":"The Beatles","group_id":"12","picUrl":"http:\/\/www3.101.ru\/vardata\/modules\/channel\/dynamics\/pro\/55.jpg"}
				var channels=new List<ChannelItem>(group.ChannelCount);
				foreach (var item in json.Elements()) {
					uint id=uint.Parse(item.Element("id").Value);
					channels.Add(new ChannelItem(id, WebUtility.HtmlDecode(item.Element("name").Value), // Personalizēto staciju nosaukumos gadās <, > un &.
						item.Element("picUrl").Value) {
							Selected=selectedIds.Contains(id)
						});
				}
				group.Channels=channels;
				EnableLists(focused, groupList, group);
			}
			channelList.ItemsSource=group.Channels;
		}
		private async void channelList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (e.AddedItems.Count != 1) return;
			var channel=e.AddedItems[0] as ChannelItem;
			if (channel.Description == null) {
				var focused=DisableLists(channelList);
				string description=(await GetJson("http://101.ru/api/getaboutstation.php?station_id="+channel.Id)).Element("item").Element("desc").Value;
				channel.Description=description.Replace("<p>", string.Empty).Replace("</p>", Environment.NewLine).Replace("<br>", string.Empty); // Gadās mazliet HTMLa.
				// {"id":"128","name":"Madonna","name_eng":"Madonna","group_id":"12","picUrl":"\/vardata\/modules\/channel\/dynamics\/pro\/128.jpg","desc":"...","short_desc":"...","editor":"DJ \u041a\u043e\u0441\u0442\u044f DEEP","uideditor":"551846","photo_editor":"http:\/\/101.ru\/vardata\/modules\/channel\/image\/dff5563a4292747310079fcc45c4a04a.jpg","censor":"0"}
				EnableLists(focused, channelList, channel);
			}
		}

		private void CheckBox_Toggled(object sender, RoutedEventArgs e) {
			var item=((CheckBox)sender).DataContext as ChannelItem;
			if (item == null) return;
			if (item.Selected) selectedIds.Add(item.Id);
			else selectedIds.Remove(item.Id);
			if (!hasChanges) this.Title+=" *";
			hasChanges=true;
		}
		private bool DisableLists(ListBox list) {
			bool focused=list.IsKeyboardFocusWithin;
			groupList.IsEnabled=false;
			channelList.IsEnabled=false;
			return focused;
		}
		private void EnableLists(bool focused, ListBox list, object item) {
			channelList.IsEnabled=true;
			groupList.IsEnabled=true;
			if (focused) Keyboard.Focus((ListBoxItem)list.ItemContainerGenerator.ContainerFromItem(item));
		}

		/// <summary>Kanālu grupas.</summary>
		private List<ChannelGroup> groups;
		/// <summary>Izvēlēto kanālu numuri, ieskaitot grupās, kuras netika ielādētas.</summary>
		private readonly SortedSet<uint> selectedIds;
		/// <summary>Vai ir mainījusies izvēlēto kanālu kopa.</summary>
		private bool hasChanges=false;
		/// <summary>Sākotnēji (pirms loga atvēšanas) izvēlētie kanāli.</summary>
		private readonly List<ChannelItem> selectedChannels;
	}
}