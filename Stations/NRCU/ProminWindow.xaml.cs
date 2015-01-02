using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DrDax.RadioClient;

namespace Nrcu {
	/// <summary>Rāda Промінь studijas webkameras fotouzņēmumus.</summary>
	public partial class ProminWindow : StudioWindowBase {
		private ProminWindow()
			: base("Промінь Онлайн") {
			InitializeComponent();
			image1.Width=640; image1.Height=480;
			timer=new Timer(3000); // Reizi trīs sekundēs, kā mājaslapā (pēc iegultā pulksteņa redzams, ka biežāk neatjauninās).
			timer.Elapsed+=timer_Elapsed;
			image=image1;
		}

		private void timer_Elapsed(object sender, ElapsedEventArgs e) {
			Dispatcher.BeginInvoke((Action)(() => {
				try {
					source=JpegBitmapDecoder.Create(new Uri("http://promin.fm/webcam/camera9995.jpg"), BitmapCreateOptions.IgnoreImageCache, BitmapCacheOption.None).Frames[0];
					oldImage=image;
					source.DownloadCompleted+=source_DownloadCompleted;
				} catch {}
			}));
		}

		private void source_DownloadCompleted(object sender, EventArgs e) {
			source.DownloadCompleted-=source_DownloadCompleted;
			image=image == image1 ? image2:image1;
			// Nomaina attēlu pēc ielādes, savādāk ir redzama raustīšanās.
			image.Source=source;
			source=null;
			image.Visibility=Visibility.Visible;
			oldImage.Visibility=Visibility.Hidden;
		}
		public static void Open() {
			if (window == null) {
				window=new ProminWindow();
				window.Show();
			}
			window.Activate();
		}
		protected override void OnLoaded() {
			image1.Width=Double.NaN; image1.Height=Double.NaN;
			timer.Start();
		}
		protected override void OnClosing() {
			timer.Stop();
			timer.Elapsed-=timer_Elapsed;
			timer.Dispose();
		}

		private readonly Timer timer;
		/// <summary>Pašreiz redzamais kadrs.</summary>
		private Image image;
		/// <summary>Iepriekšējais kadrs.</summary>
		private Image oldImage;
		/// <summary>Ielādējamais kadrs.</summary>
		private BitmapSource source;
	}
}
