using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace DrDax.RadioClient {
	public class IcyChannel : StreamChannel<IcyStream> {
		/// <summary>Programmas identifikators, griežoties pie servera.</summary>
		public string UserAgent {
			set { stream.UserAgent=value; }
		}
		public IcyChannel(string url, BitmapSource logo, TimeZoneInfo timezone, bool hasGuide, Brand brand, Menu<Channel> menu=null)
			: base(new IcyStream(url), logo, timezone, hasGuide, brand, menu) {
			PropertyChanged+=PropertyChangedHandler;
		}

		private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e) {
			if (Guide is IcyGuide) {
				if (e.PropertyName == "Guide")
					Settings.Default.PropertyChanged+=PropertyChangedHandler;
				if ((e.PropertyName == "UseGuide" || e.PropertyName == "Guide")) {
					if (Settings.Default.UseGuide)
						stream.MetaHeaderCallback=((IcyGuide)Guide).ProcessMetaHeader;
					else stream.MetaHeaderCallback=null;
					if (PlaybackState != PlaybackState.Stopped) {
						base.Stop(); base.Play();
					}
				}
			}
		}
		public override void Dispose() {
			PropertyChanged-=PropertyChangedHandler;
			Settings.Default.PropertyChanged-=PropertyChangedHandler;
			base.Dispose();
		}
	}
}