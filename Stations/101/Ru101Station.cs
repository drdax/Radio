using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DrDax.RadioClient;
using D=System.Drawing;
using DI=System.Drawing.Imaging;

namespace Ru101 {
	public class Ru101Station : DynamicStation {
		public Ru101Station() : base("Russian Standard Time") { // Maskavas laika josla.
			SetChannels();
			Settings.Default.PropertyChanged+=settings_PropertyChanged;
		}

		public override Channel GetChannel(uint id) {
			if (brand == null)
				brand=new Brand(Colors.White, 0x606060.ToColor(), Colors.White, 0x6C54D5.ToColor(),
					0x6C54D5.ToColor(), 0x483998.ToColor(), Colors.Black, Colors.Black);
			if (id == 0 && Channels.ContainsKey(0)) // Nav izvēlēts neviens kanāls.
				return new EmptyChannel("101.ru", GetResourceImage("101ru.png"), brand, new ChannelMenu());
			var channel=channels.FirstOrDefault(c => c.Id == id);
			if (channel == null) throw new ChannelNotFoundException(id);

			BitmapSource logo=GetCachedImage(id.ToString());
			if (logo == null)
				using (var client=new ProperWebClient()) {
					using (Stream imageStreamSource=client.OpenRead(channel.LogoUrl))
					using (var srcBitmap=new D.Bitmap(imageStreamSource)) // Drawing.Bitmap, lai WPF neizstieptu attēlu, pārrēķinot DPI.
					// Ielādē caurspīdīguma masku, kura izveidota, apgriežot krāsas oriģinālajā cover_000_round.png.
					using (D.Bitmap bitmap=(D.Bitmap)D.Bitmap.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(this.GetType(), "LogoMask.png"))) {
						var rect=new D.Rectangle(new D.Point(0, 0), bitmap.Size);
						var inBits=srcBitmap.LockBits(rect, DI.ImageLockMode.ReadOnly, DI.PixelFormat.Format32bppArgb);
						var outBits=bitmap.LockBits(rect, DI.ImageLockMode.WriteOnly, DI.PixelFormat.Format32bppArgb);
						unsafe { // Nedrošais kods ar norādēm labākai veiktspējai.
							// Izkrāso caurspīdīguma masku ar logotipa krāsām.
							for (int y = 0; y < bitmap.Height; y++) {
								byte* inRow=(byte*)inBits.Scan0+inBits.Stride*y;
								byte* outRow=(byte*)outBits.Scan0+outBits.Stride*y;
								for (int x = 0; x < bitmap.Width; x++) {
									outRow[4 * x]    =inRow[4 * x];     // zilā
									outRow[4 * x + 1]=inRow[4 * x + 1]; // zaļā
									outRow[4 * x + 2]=inRow[4 * x + 2]; // sarkanā
								}
							}
						}
						srcBitmap.UnlockBits(inBits); bitmap.UnlockBits(outBits);
						logo=System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
							IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
					}
					CacheImage(id.ToString(), logo);

					// {"status":0,"result":{"token":"09270779","playlist":[{"url":"http:\/\/ru2.101.ru:8000\/c14_24?tok=09270779"}]},"errorCode":0,"errorMsg":""}
					string urlPrefix="http://"+Settings.Default.Region; // Iznests mainīgajā, lai nepārrēķinātu katru reizi, meklējot atskaņošanas adresi.
					channel.StreamUrl=client.GetJsonSync("http://101.ru/api/getstationstream.php?station_id="+id).
						Element("result").Element("playlist").Elements("item").First(i => i.Element("url").Value.StartsWith(urlPrefix)).Element("url").Value;
				}

			return new IcyChannel(channel.StreamUrl, logo, timezone, true, brand, new ChannelMenu())
				{ UserAgent="Mozilla/5.0 (Windows NT 6.2; Win64; x64; rv:18.0) Gecko/20130119 Firefox/18.0" }; // Lai 101.ru neteiktu, ka jāklausās pārlūkā.
		}
		public override Guide GetGuide(uint number) {
			return new Ru101Guide(number);
		}
		public override string GetHomepage(uint channelNumber) {
			if (channelNumber == 0) return "http://101.ru/";
			return "http://101.ru/?an=port_channel_mp3&channel="+channelNumber; // Neņem vērā to, ka ētera kanāliem ir savas mājaslapas.
		}
		public override void RemoveChannel(uint id) {
			for (int n=0; n < Settings.Default.Channels.Count; n++)
				if (Settings.Default.Channels[n].Id == id) { Settings.Default.Channels.RemoveAt(n); break; }
			hasSettingsChanges=true;
		}
		public override void SaveSettings() {
			Settings.Default.Save();
		}

		private void settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if (e.PropertyName == "Channels") {
				Channels.Clear();
				SetChannels();
				ChannelsChanged();
			}
			hasSettingsChanges=true;
		}
		private void SetChannels() {
			channels=Settings.Default.Channels;
			if (channels == null || channels.Count == 0)
				Channels.Add(0, "101.ru");
			else foreach (var channel in channels)
				Channels.Add(channel.Id, channel.Caption);
		}

		private Brand brand;
		private List<ChannelItem> channels;
	}
}