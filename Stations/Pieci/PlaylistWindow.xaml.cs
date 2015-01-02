using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DrDax.RadioClient;

namespace Pieci {
	/// <summary>Kanālā skanejošo dziesmu saraksta logs.</summary>
	public partial class PlaylistWindow : ProperWindow {
		public PlaylistWindow(uint channelNumber, string channelCaption, SolidColorBrush channelBrush, TimeZoneInfo timezone) {
			InitializeComponent();
			Title="Pirms tam skanēja "+channelCaption;
			this.url=string.Format("http://koncerti.pieci.lv/shared/cache/timeline_st{0}.json", channelNumber);
			this.timezone=timezone;
			this.Resources["channelBrush"]=channelBrush;
			this.Loaded+=Window_Loaded;
		}
		private async void Window_Loaded(object sender, EventArgs e) {
			// [{"id":"988747","artist":"Phoenix","title":"Armistice (Live at Hordern Pavilion, Sydney, 2010)","runtime":"197.037","airtime":"2013-12-24 14:09:35","status":"playing","song_id":"929","artist_id":"299","station_id":"1","images":{"ios":"http:\/\/cdn.pieci.lv\/images\/phoenix-ios.jpg","metadata":{"ios":null,"android":null,"desktop":null},"android":"http:\/\/cdn.pieci.lv\/images\/phoenix-android.jpg","desktop":"http:\/\/cdn.pieci.lv\/images\/phoenix-desktop.jpg"}}, ... ]
			// Novērots, ka sarakstā dziesmas atkārtojas ar vienādu laiku, bet dažadiem ID.
			using (var client=new ProperWebClient()) {
				list.ItemsSource=
					from song in (await client.GetJson(url)).Elements("item")
					let title=song.Element("title").Value
					let splitIdx=title.IndexOf('(')
					select new PlaylistItem {
						StartTime=TimeZoneInfo.ConvertTime(DateTime.ParseExact(song.Element("airtime").Value, PieciGuide.TimeFormat, CultureInfo.InvariantCulture), timezone, TimeZoneInfo.Local),
						Duration=TimeSpan.FromSeconds(double.Parse(song.Element("runtime").Value, CultureInfo.InvariantCulture)),
						Artist=song.Element("artist").Value,
						Caption=splitIdx < 1 ? title:title.Substring(0, splitIdx-1), // -1 tukšumam pirms iekavas. <1, jo var būt nosaukums, kurš viss iekavās
						Description=splitIdx < 1 ? null:title.Substring(splitIdx+1, title.Length-splitIdx-2) // -2 iekavām
					};
			}
		}
		private void PlaySong(object sender, RoutedEventArgs e) {
			string url=(string)((Button)sender).DataContext;
			if (url != string.Empty)
				DefaultProgram.OpenFile(url);
		}
		private void CopyCaption(object sender, RoutedEventArgs e) {
			var item=(PlaylistItem)((Control)sender).DataContext;
			Clipboard.SetText(item.Caption+Environment.NewLine+item.Artist);
		}

		private readonly string url;
		private readonly TimeZoneInfo timezone;
	}
}