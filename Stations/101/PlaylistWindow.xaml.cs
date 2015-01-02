using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using DrDax.RadioClient;

namespace Ru101 {
	/// <summary>Kanālā skanejošo dziesmu saraksta logs.</summary>
	public partial class PlaylistWindow : ConnectedWindow {
		public PlaylistWindow(uint channelNumber, string channelCaption, TimeZoneInfo timezone) {
			InitializeComponent();
			Title=channelCaption;
			this.channelNumber=channelNumber;
			this.timezone=timezone;
			this.Loaded+=Window_Loaded;
		}
		private async void Window_Loaded(object sender, EventArgs e) {
			// {"id":"191665","name":"FARMER, Mylene - Greta","duration":"00:04:49","last_time":"2013-12-08 17:39:03","count_listen":"1016","sample":"http:\/\/wz5.101.ru\/full\/166\/191665.mp3"}
			list.ItemsSource=
				from item in (await GetJson("http://101.ru/api/gethistorybroadcast.php?station_id="+channelNumber)).Elements("item")
				select new PlaylistItem {
					StartTime=TimeZoneInfo.ConvertTime(DateTime.Parse(item.Element("last_time").Value), timezone, TimeZoneInfo.Local),
					Duration=TimeSpan.Parse(item.Element("duration").Value),
					Caption=WebUtility.HtmlDecode(item.Element("name").Value),
					Url=item.Element("sample").Value
				};
		}
		private void PlaySong(object sender, RoutedEventArgs e) {
			string url=(string)((Button)sender).DataContext;
			if (url != string.Empty)
				DefaultProgram.OpenFile(url);
		}
		private void CopyCaption(object sender, RoutedEventArgs e) {
			Clipboard.SetText(((PlaylistItem)((Control)sender).DataContext).Caption);
		}
		private void CopyUrl(object sender, RoutedEventArgs e) {
			Clipboard.SetText(((PlaylistItem)((Control)sender).DataContext).Url);
		}

		private readonly uint channelNumber;
		private readonly TimeZoneInfo timezone;
	}
}