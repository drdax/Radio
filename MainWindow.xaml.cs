using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DrDax.RadioClient.Properties;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Data;

namespace DrDax.RadioClient {
	internal partial class MainWindow : Window {
		#region Resursi
		private ImageSource mutedIcon;
		private ImageSource unmutedIcon;
		private Thickness thumbnailWithGuideMargin;
		private Thickness thumbnailWithoutGuideMargin;
		private double widthWithGuide;
		private double widthWithoutGuide;
		#endregion

		private Channel channel;
		/// <summary>
		/// Vai skaņa bija izslēgta pirms piekļuve pie datora tika noslēgta.
		/// </summary>
		private bool wasMuted=true; // Pēc noklusējuma patiess, lai neieslēgtu skaņu nelaikā.
		/// <summary>
		/// Taimeris, pēc kura katru minūti tiek atjaunots laiks studijā.
		/// </summary>
		private readonly Timer clockTimer;
		/// <summary>
		/// Taimers, pēc kura regulāri tiek attēlots atskaņotais raidījuma ilgums.
		/// </summary>
		private readonly Timer progressTimer;
		/// <summary>
		/// Raidījuma atskaņošanas progresa joslas lielākais augstums DIPikseļos.
		/// </summary>
		/// <remarks>Atbilst <code>progressBar.ActualWidth</code>.
		/// Iestatīts konstanti, jo tā vienkāršāk laicīgi (pirms kanāla piešķiršanas) tikt pie vērtības.</remarks>
		private const double progressMaxWidth=240;

		/// <summary>Uztveramais kanāls.</summary>
		public Channel Channel {
			get { return channel; }
			set {
				if (channel != null && !(channel is EmptyChannel)) {
					if (value != null) {
						value.IsMuted=channel.IsMuted;
						value.Volume=channel.Volume;
					}
					channel.Stop();
					if (channel.HasGuide) {
						channel.Guide.CurrentBroadcastChanged-=guide_BroadcastChanged;
						progressTimer.Stop();
					}
					channel.Dispose();
				}
				channel=value ?? new EmptyChannel();

				Title=channel.Caption;
				if (channel.Timezone == null) {
					clock.Visibility=Visibility.Collapsed;
					clockTimer.Stop();
				} else {
					clock.Visibility=Visibility.Visible;
					StartClockTimer();
				}
				if (channel.HasGuide) {
					guidePanel.Visibility=Visibility.Visible;
					TaskbarItemInfo.ThumbnailClipMargin=thumbnailWithGuideMargin;
					this.Width=widthWithGuide;
					channel.Guide.CurrentBroadcastChanged+=guide_BroadcastChanged;

					if (channel.Guide.HasKnownDuration) {
						// Parāda atskaņotā ilguma joslu.
						progressBar.Visibility=Visibility.Visible;
						progressBarBack.Visibility=Visibility.Visible;
						var margin=currentDescription.Margin;
						margin.Top=4; margin.Bottom=2;
						currentDescription.Margin=margin;
						// Ja zināms ilgums, tad vajadzētu jau zināt raidījumu pirms Play izaukuma.
						guide_BroadcastChanged(channel.Guide.CurrentBroadcast);
					} else {
						// Paslēpj atskaņotā ilguma joslu.
						progressBar.Visibility=Visibility.Collapsed;
						progressBarBack.Visibility=Visibility.Collapsed;
						var margin=currentDescription.Margin;
						margin.Top=0; margin.Bottom=6;
						currentDescription.Margin=margin;
					}
					if (channel.Guide.CurrentBroadcast != null)
						SetWindowTitle(channel.Guide.CurrentBroadcast.Caption);
				} else {
					guidePanel.Visibility=Visibility.Collapsed;
					TaskbarItemInfo.ThumbnailClipMargin=thumbnailWithoutGuideMargin;
					this.Width=widthWithoutGuide;
				}
				System.Windows.Controls.Grid.SetColumnSpan(logoBackground, channel.Brand.HasSharedBackground ? 2:1);
				channel.Play();
				ShowMuteStateInTaskBar(channel.IsMuted);
				DataContext=channel;
			}
		}

		public MainWindow() {
			InitializeComponent();
			this.Loaded+=Window_Loaded;
			this.Closing+=Window_Closing;
			mutedIcon=(BitmapImage)Resources["MutedIcon"];
			unmutedIcon=(BitmapImage)Resources["UnmutedIcon"];
			thumbnailWithGuideMargin=(Thickness)Resources["ThumbnailWithGuide"];
			thumbnailWithoutGuideMargin=(Thickness)Resources["ThumbnailWithoutGuide"];
			// "Apmales papildinājums", kurš var traucēt pilnībā attēlot loga saturu.
			byte borderPadding=(byte)(Convert.ToInt16(Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "PaddedBorderWidth", 0))/-15);
			this.Width+=borderPadding*2; this.Height+=borderPadding*2;
			widthWithGuide=this.Width;
			widthWithoutGuide=this.Width-thumbnailWithGuideMargin.Right;
			ShowMuteStateInTaskBar(true);
			this.CommandBindings.Add(new CommandBinding(MediaCommands.MuteVolume, MuteCmd_Executed/*, VolumeCmd_CanExecute*/));
			this.CommandBindings.Add(new CommandBinding(MediaCommands.IncreaseVolume, IncreaseCmd_Executed/*, VolumeCmd_CanExecute*/));
			this.CommandBindings.Add(new CommandBinding(MediaCommands.DecreaseVolume, DecreaseCmd_Executed/*, VolumeCmd_CanExecute*/));
			clockTimer=new Timer();
			clockTimer.Elapsed+=clockTimer_Elapsed;
			if (Station.UseGuide) {
				progressTimer=new Timer();
				progressTimer.Elapsed+=progressTimer_Elapsed;
			} else progressTimer=null;
		}

		/// <summary>Nomaina attēloto laiku studijā.</summary>
		private void UpdateClock() {
			clock.Text=TimeZoneInfo.ConvertTime(DateTimeOffset.Now, channel.Timezone).ToString("H:mm");
		}
		/// <summary>Sāk studijas pulksteņa taimeri.</summary>
		private void StartClockTimer() {
			clockTimer.Interval=(60-DateTime.UtcNow.Second)*1000; // Milisekundes līdz nākamās minūtes sākumam.
			UpdateClock();
			clockTimer.Start();
		}
		/// <summary>Parāda ieslēgtas/izslēgtas skaņas stāvokli uzdevumu joslā.</summary>
		private void ShowMuteStateInTaskBar(bool isMuted) {
			if (isMuted) {
				muteTaskBtn.ImageSource=mutedIcon;
				muteTaskBtn.Description="Ieslēgt skaņu";
				this.TaskbarItemInfo.Overlay=mutedIcon;
			} else {
				muteTaskBtn.ImageSource=unmutedIcon;
				muteTaskBtn.Description="Izslēgt skaņu";
				this.TaskbarItemInfo.Overlay=channel.Icon;
			}
		}
		/// <summary>Iestata loga virsrakstu no raidījuma un kanāla nosaukumiem.</summary>
		private void SetWindowTitle(string broadcastCaption) {
			Title=broadcastCaption+" - "+channel.Caption;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
			IntPtr handle=new WindowInteropHelper(this).Handle;
			// Ja nesagaidīja loga parādīšanos pēc programmas palaišanas.
			if (GetForegroundWindow() != handle) channel.Brand.Focused=false;
			// http://blogs.msdn.com/nickkramer/archive/2006/03/18/554235.aspx
			HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WndProc)); // Lai varētu uztvert loga fokusu. GotFocus un LostFocus nelīdz.
			SystemEvents.SessionSwitch+=SystemEvents_SessionSwitch;
			// Aizvāc izmēra maiņas loga izvēlnes punktus.
			IntPtr hMenu=GetSystemMenu(handle, 0/*false*/);
			DeleteMenu(hMenu, 0xF000 /*SC_SIZE*/, 0 /*MF_BYCOMMAND*/);
			DeleteMenu(hMenu, 0xF030 /*SC_MAXIMIZE*/, 0 /*MF_BYCOMMAND*/);
			DeleteMenu(hMenu, 0xF120 /*SC_RESTORE*/, 0 /*MF_BYCOMMAND*/);
		}
		private void Window_Closing(object sender, CancelEventArgs e) {
			// Apstādina kanālu tai skaitā arī, lai nepaliktu fona procesi.
			if (channel != null) channel.Stop();
		}
		/// <summary>Nodrošina skaņas izslēgšanu, kad lietotājs bloķē datoru.</summary>
		private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e) {
			switch (e.Reason) {
				case SessionSwitchReason.SessionLock:
					if (Settings.Default.MuteOnLock) {
						wasMuted=channel.IsMuted;
						if (!wasMuted) {
							channel.IsMuted=true;
							ShowMuteStateInTaskBar(true);
						}
					}
					clockTimer.Stop();
					break;
				case SessionSwitchReason.SessionUnlock:
					if (Settings.Default.MuteOnLock && !wasMuted) {
						channel.IsMuted=false;
						ShowMuteStateInTaskBar(false);
					}
					StartClockTimer();
					break;
			}
		}
		private void guide_BroadcastChanged(Broadcast broadcast) {
			progressTimer.Stop();
			if (broadcast == null) return;
			if (channel.Guide.HasKnownDuration) {
				double duration=(broadcast.EndTime-broadcast.StartTime).TotalSeconds;
				Dispatcher.Invoke((Action<double, string>)(
					(double newWidth, string broadcastCaption) => {
						progressBar.Width=newWidth; // Jau atskaņotais ilgums.
						SetWindowTitle(broadcastCaption);
					}), Math.Round(progressMaxWidth*(DateTime.Now-broadcast.StartTime).TotalSeconds/duration), broadcast.Caption);
				// Intervāls milisekundēs, pēc kurām ilguma rādītājs mainīsies par vienu pikseli.
				progressTimer.Interval=Math.Ceiling(1000*duration/progressMaxWidth);
				progressTimer.Start();
			} else {
				Dispatcher.Invoke((Action<string>)SetWindowTitle, broadcast.Caption);
			}
		}
		private void clockTimer_Elapsed(object sender, ElapsedEventArgs e) {
			// Iestata regulāro minūtes intervālu pēc pirmās reizes.
			if (clockTimer.Interval != 60000) clockTimer.Interval=60000;
			Dispatcher.BeginInvoke((Action)UpdateClock);
		}
		private void progressTimer_Elapsed(object sender, ElapsedEventArgs e) {
			Dispatcher.Invoke((Action)(() => {
				progressBar.Width++;
				// Ja nenomainījās raidījums, neļaujam progresa joslai pārsniegt lielāko lielumu.
				if (progressBar.Width == progressMaxWidth) progressTimer.Stop();
			}));
		}
		/*private void VolumeCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute=true;// Player.HasAudio;
		}*/
		/// <summary>Ieslēdz/izslēdz skaņu.</summary>
		private void MuteCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
			channel.IsMuted=!channel.IsMuted;
			ShowMuteStateInTaskBar(channel.IsMuted);
		}
		/// <summary>Palielina skaļumu.</summary>
		private void IncreaseCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
			channel.Volume+=0.1;
			if (channel.Volume > 1) channel.Volume=1;
		}
		/// <summary>Pamazina skaļumu.</summary>
		private void DecreaseCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
			channel.Volume-=0.1;
			if (channel.Volume < 0) channel.Volume=0;
		}
		private void MinimizeWindow(object sender, RoutedEventArgs e) {
			this.WindowState=WindowState.Minimized;
		}
		private void CloseWindow(object sender, RoutedEventArgs e) {
			this.Close();
		}
		private void DragStart(object sender, MouseButtonEventArgs e) {
			this.DragMove();
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			switch (msg) {
				case 0x86: //WM_NCACTIVATE
					// http://stackoverflow.com/questions/813745/keep-window-inactive-in-appearance-even-when-activated
					if (wParam == (IntPtr)0 /*FALSE*/) // Logs pazaudēja fokusu.
						channel.Brand.Focused=false;
					else if (wParam == (IntPtr)1 /*TRUE*/) // Logs ieguva fokusu.
						channel.Brand.Focused=true;
					handled=!RadioApp.ShowingDialog; // Bez šī WM_NCACTIVATE netiks izsaukts pēc loga atgriešanas no minimizēta stāvokļa.
					break;
				case 0x84: //WM_NCHITTEST // Neļauj mainīt loga izmēru.
					// http://stackoverflow.com/questions/853533/aero-glass-borders-on-popup-windows-in-c
					handled=true;
					return (IntPtr)1;
			}
			return IntPtr.Zero;
		}
		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();
		[DllImport("user32.dll")]
		private static extern IntPtr GetSystemMenu(IntPtr hwnd, int bReset);
		[DllImport("user32.dll")]
		static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);
	}

	public class PlaybackStateToOpacityConverter : IValueConverter {
		/// <summary>Pārveido signāla līmeni indikatora caurspīdīgumā.</summary>
		/// <param name="value">(<see cref="PlaybackState"/>) signāla līmenis.</param>
		/// <param name="targetType">double</param>
		/// <param name="parameter">Signāla līmenis, kuru attēlo indikators. <see cref="PlaybackState"/> kā skaitlis.</param>
		/// <returns>Caurspīdīgums, kurš atbilst ieslēgtam (1), vai izslēgtam signāla indikatoram (0.3).</returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			byte state=(byte)value, targetState=byte.Parse((string)parameter); // XAML saistīšanas gadījumā parameter ir string.
			return state < targetState ? 0.3d:1d;
		}
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}