using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DrDax.RadioClient {
	public partial class StudioWindow : StudioWindowBase {
		private StudioWindow(string title, string streamUrl, int width, int height, string segmentsRegex=null) : base(title) {
			InitializeComponent();
			player.Width=width; player.Height=height;
			player.LoadedBehavior=MediaState.Manual; // Atļauj Play un Stop izsaukumus.
			player.BufferingStarted+=player_BufferingStarted; player.BufferingEnded+=player_BufferingEnded;
			player.MediaFailed+=player_MediaFailed;
			playerSource=new Uri(segmentsRegex == null ? streamUrl:VideoServer.StreamUrl);
			server=segmentsRegex == null ? null:new VideoServer(streamUrl, segmentsRegex);
			stateDisplay.State=PlaybackState.Connecting;
		}

		/// <summary>Atver radio studijas video translācijas logu.</summary>
		/// <param name="title">Loga virsraksts.</param>
		/// <param name="streamUrl">Video translācijas plūsmas vai atskaņošanas saraksta adrese.</param>
		/// <param name="width">Video platums DIPos.</param>
		/// <param name="height">Video augstums DIPos.</param>
		/// <param name="segmentsRegex">Frgamentu faila regulārais izteiciens, ja video plūsma ir fragmentēta.</param>
		public static void Open(string title, string streamUrl, int width, int height, string segmentsRegex=null) {
			if (window == null) {
				window=new StudioWindow(title, streamUrl, width, height, segmentsRegex);
				window.Show();
			}
			window.Activate();
		}

		protected override void OnLoaded() {
			if (server != null) server.Start();
			player.Source=playerSource;
			player.Play();
			player.Width=double.NaN; player.Height=double.NaN;
		}
		protected override void OnClosing() {
			player.Stop();
			if (server != null) server.Dispose();
			player.BufferingStarted-=player_BufferingStarted; player.BufferingEnded-=player_BufferingEnded;
			player.MediaFailed-=player_MediaFailed;
		}
		private void player_BufferingEnded(object sender, RoutedEventArgs e) {
			stateDisplay.State=PlaybackState.Playing;
		}
		private void player_BufferingStarted(object sender, RoutedEventArgs e) {
			stateDisplay.State=PlaybackState.Buffering;
		}
		private void player_MediaFailed(object sender, ExceptionRoutedEventArgs e) {
			stateDisplay.State=PlaybackState.Stopped;
			player.Source=null;
			if (server != null) { server.Stop(); server.Start(); }
			player.Source=playerSource;
			player.Play();
			stateDisplay.State=PlaybackState.Connecting;
		}

		private readonly Uri playerSource;
		/// <summary>Segmentētas plūsmas retranslācijas serveris.</summary>
		private readonly VideoServer server;
	}
}