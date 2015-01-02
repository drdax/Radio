using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DrDax.RadioClient;

namespace Pieci {
	public class PieciStation : DynamicStation {
		public const uint
			/// <summary>Kanāla identifikators, kuram plūsmas adresē nav skaitlis.</summary>
			EmptyId=1,
			/// <summary>Ētera kanāla identifikators.</summary>
			FmId=19;
		public PieciStation() : base("E. Europe Standard Time") { // Latvijas laika josla.
			channels=Settings.Default.Channels;
			// Pēc noklusējuma atstāj ētera kanālu.
			if (channels == null || channels.Count == 0) {
				var fmChannel=new ChannelItem(FmId, "5 FM", 0x2D3191.ToColor());
				if (channels == null)
					channels=new List<ChannelItem>(1) { fmChannel };
				else channels.Add(fmChannel);
				Settings.Default.Channels=channels;
				hasSettingsChanges=true;
			}

			SetChannels();
			Settings.Default.PropertyChanged+=settings_PropertyChanged;
		}

		public override Channel GetChannel(uint id) {
			var channel=channels.FirstOrDefault(c => c.Id == id);
			if (channel == null) throw new ChannelNotFoundException(id);
			// Virsraksta un logo fona krāsa ņemta no staciju pārslēga logotipiem, raidījumu saraksta fona krāsa ņemta no atskaņošanas pogas apakšējā stūra, bet statusa krāsa no labā stūra.

			RenderTargetBitmap logo=(RenderTargetBitmap)GetCachedImage(id.ToString());
			if (logo == null) {
				logo=new RenderTargetBitmap(150, 150, 96, 96, PixelFormats.Pbgra32);
				var fill=new SolidColorBrush(channel.Color);
				var canvas=new Canvas() {
					Children= {
						new Path() {
							Fill=fill,
							Data=Geometry.Parse("M30.08,14.11 L51.98,2.97 36.75,55.69 0.00,86.88 2.23,59.03 30.08,14.11Z")
						},
						new Path() {
							Fill=fill,
							Data=Geometry.Parse("M39.73,62.74 L0.38,96.17 32.67,140.71 89.85,150.38 109.53,106.93 39.73,62.74Z")
						},
						new Path() {
							Fill=fill,
							Data=Geometry.Parse("M117.33,107.68 L102.85,139.61 113.99,129.21 142.57,86.52 145.92,75.74 117.33,107.68Z")
						},
						new Path() {
							Fill=fill,
							Data=new PathGeometry(new[] {
								new PathFigure(new Point(113.61,100.99), new[] {
									new PolyLineSegment(new[] {
										new Point(43.81,56.44), new Point(60.51,0.00), new Point(118.45,8.55), new Point(150.00,60.15), new Point(113.61,100.99) },
										false)
								}, true),
								new PathFigure(new Point(80.94,33.42), new PathSegment[] {
									new PolyBezierSegment(new[] {
										new Point(81.71,27.87), new Point(77.62,24.02), new Point(72.41,24.87) },
									false),
									new PolyLineSegment(new[] {
										new Point(73.51,21.54), new Point(82.06,21.54), new Point(83.17,15.96), new Point(69.06,15.96), new Point(65.35,28.96), new Point(67.57,30.81) },
									false),
									new PolyBezierSegment(new[] {
										new Point(68.50,30.34), new Point(69.27,29.91), new Point(70.20,29.86), new Point(72.85,29.72), new Point(74.36,31.21), new Point(74.26,33.42), new Point(74.17,35.34), new Point(72.24,36.69), new Point(69.64,36.69), new Point(66.91,36.69), new Point(65.01,36.21), new Point(62.75,35.27) },
									false),
									new PolyLineSegment(new[] {
										new Point(63.12,40.48) },
									false),
									new PolyBezierSegment(new[] {
										new Point(65.78,41.77), new Point(67.80,41.92), new Point(70.18,41.95), new Point(76.55,42.07), new Point(80.14,39.29), new Point(80.94,33.42) },
									false),
								}, true)
							})
						}
					}
				};
				// Ļauj pārrēķināt. Bez šī izsaukuma neko nezīmē.
				canvas.Arrange(new Rect(0, 0, logo.Width, logo.Height));
				logo.Render(canvas);
				CacheImage(id.ToString(), logo);
			}

			return new IcyChannel(string.Format("http://live.pieci.lv/live{0}-hq.mp3", id == EmptyId ? string.Empty:id.ToString()),
				logo, timezone, true,
				new Brand(Colors.White, channel.Color, 0x99A2A8.ToColor(), channel.Color, Colors.Black, 0x141414.ToColor(), 0x141414.ToColor()),
				new ChannelMenu());
		}

		public override Guide GetGuide(uint channelNumber) {
			// Ir dziesmas nosaukums un izpildītājs ICY datos, bet tad nav zināms ilgums. Tāpēc lieto JSON pakalpojumu, kurš tiek izmantotas arī kanāla lapā.
			return new PieciGuide(channelNumber, timezone);
		}
		public override string GetHomepage(uint channelId) {
			if (channelId == FmId) return "http://www.pieci.lv/"; // fm.* ir atskaņotājs kā citi, bet www.* ir kopējā mājaslapa.
			var channel=channels.FirstOrDefault(c => c.Id == channelId);
			return string.Concat("http://", channel.Name, ".pieci.lv/");
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
				channels=Settings.Default.Channels;
				SetChannels();
				ChannelsChanged();
			}
			hasSettingsChanges=true;
		}
		private void SetChannels() {
			foreach (var channel in channels)
				Channels.Add(channel.Id, channel.Caption, channel.IconIdx);
		}
		private List<ChannelItem> channels;
	}
}