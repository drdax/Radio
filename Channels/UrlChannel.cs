using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DrDax.RadioClient {
	/// <summary>
	/// Atskaņo kanālu pēc tā plūsmas adreses. Jālieto, ja plūsma nāk caur MMS (Microsoft Media Server) protokolu.
	/// </summary>
	/// <remarks>Patiesībā spēj atskaņot arī caur HTTP, bet tad tiek veidota kopija uz cietā diska.
	/// Tāpat tiek atbalstīti pārējie <see cref="MediaPlayer"/> protokoli un formāti.</remarks>
	public class UrlChannel : Channel {
		private readonly MediaPlayer player;
		/// <remarks>
		/// Skaļums glabājas šajā laukā, lai tam būtu lietotāja iestatīta vertība pēc atskaņošanas beigām.
		/// </remarks>
		private double volume=Settings.Default.Volume;
		/// <summary>Kanāla raidošā interneta adrese.</summary>
		private readonly Uri url;

		public override double Volume {
			get { return volume; }
			set {
				if (value != volume) {
					player.Volume=value;
					volume=value;
					NotifiyPropertyChanged("Volume");
				}
			}
		}
		public UrlChannel(string url, BitmapSource logo, TimeZoneInfo timezone, bool hasGuide, Brand brand, Menu<Channel> menu=null)
			: base(logo, timezone, hasGuide, brand, menu) {
			player=new MediaPlayer();
			this.url=new Uri(url);
			player.BufferingStarted+=player_BufferingStarted;
			player.BufferingEnded+=player_BufferingEnded;
			player.MediaFailed+=player_MediaFailed;
			//player.MediaEnded+=player_MediaEnded;
		}

		protected override bool GetIsMuted() {
			return player.IsMuted;
		}
		protected override void SetIsMuted(bool value) {
			player.IsMuted=value;
		}
		public override void Play() {
			PlaybackState=PlaybackState.Connecting;
			player.Dispatcher.BeginInvoke((Action)(() => {
				player.Open(url);
				player.Play();
				player.Volume=volume;
			}));
		}
		public override void Stop() {
			player.Dispatcher.BeginInvoke((Action)(() => {
				player.Close();
				PlaybackState=PlaybackState.Stopped;
			}));
		}

		/*private void player_MediaEnded(object sender, EventArgs e) {
			PlaybackState=PlaybackState.Stopped;
		}*/
		private void player_MediaFailed(object sender, ExceptionEventArgs e) {
			UnexpectedStop();
		}
		private void player_BufferingEnded(object sender, EventArgs e) {
			PlaybackState=PlaybackState.Playing;
		}
		private void player_BufferingStarted(object sender, EventArgs e) {
			PlaybackState=PlaybackState.Buffering;
		}

		public override void Dispose() {
			base.Dispose();
			player.BufferingStarted-=player_BufferingStarted;
			player.BufferingEnded-=player_BufferingEnded;
			player.MediaFailed-=player_MediaFailed;
		}
	}
}