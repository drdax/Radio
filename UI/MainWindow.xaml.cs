using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace DrDax.RadioClient {
	internal partial class MainWindow : Window {
		#region Resursi
		private readonly ImageSource mutedIcon;
		private readonly ImageSource unmutedIcon;
		private readonly Thickness thumbnailWithGuideMargin=new Thickness(3, 28, 243, 28);
		private readonly Thickness thumbnailWithoutGuideMargin=new Thickness(3, 28, 3, 28);

		/// <summary>Izvēlnes pogas puscaurspīdīgā daļa.</summary>
		private static readonly Color menuMaskColor=Color.FromArgb(80, 0, 0, 0);
		/// <summary>Pogas caurspīdīgums, kad ir tikai programmas izvēlne.</summary>
		private readonly SolidColorBrush appMenuMask=new SolidColorBrush(menuMaskColor);
		/// <summary>Pogas caurspīdīgums, kad pieejams raidījumu saraksts.</summary>
		private readonly LinearGradientBrush guidedMenuMask=new LinearGradientBrush(new GradientStopCollection(3) {
			new GradientStop(Colors.Black, 0), new GradientStop(Colors.Black, 0.35), new GradientStop(menuMaskColor, 0.35)
		}, 90);
		/// <summary>Pogas caurspīdīgums, kad ir programmas un kanāla izvēlnes, bet nav raidījumu saraksta.</summary>
		private readonly LinearGradientBrush channelMenuMask=new LinearGradientBrush(new GradientStopCollection(5) {
			new GradientStop(menuMaskColor, 0), new GradientStop(menuMaskColor, 0.35),
			new GradientStop(Colors.Black, 0.35), new GradientStop(Colors.Black, 0.65),
			new GradientStop(menuMaskColor, 0.65)
		}, 90);
		/// <summary>Pogas caurspīdīgums, kad ir programmas un kanāla izvēlnes, bet raidījumu sarakstam nav izvēlnes.</summary>
		private readonly LinearGradientBrush guidedChannelMenuMask=new LinearGradientBrush(new GradientStopCollection(3) {
			new GradientStop(Colors.Black, 0), new GradientStop(Colors.Black, 0.65), new GradientStop(menuMaskColor, 0.65)
		}, 90);
		/// <summary>Pogas caurspīdīgums, kad ir programmas un raidījumu saraksta izvēlnes.</summary>
		private readonly LinearGradientBrush guideMenuMask=new LinearGradientBrush(new GradientStopCollection(5) {
			new GradientStop(Colors.Black, 0), new GradientStop(Colors.Black, 0.35),
			new GradientStop(menuMaskColor, 0.35), new GradientStop(menuMaskColor, 0.65),
			new GradientStop(Colors.Black, 0.65)
		}, 90);
		#endregion

		private Channel channel;
		/// <summary>Vai <see cref="channel"/> vērtība nav pirmā programmas darbības laikā.</summary>
		private bool notFirstChannel=false;
		/// <summary>
		/// Vai skaņa bija izslēgta pirms piekļuve pie datora tika noslēgta.
		/// </summary>
		private bool wasMuted=true; // Pēc noklusējuma patiess, lai neieslēgtu skaņu nelaikā.
		/// <summary>
		/// Taimeris, pēc kura katru minūti tiek atjaunots laiks studijā.
		/// </summary>
		private Timer clockTimer;
		/// <summary>
		/// Taimers, pēc kura regulāri tiek attēlots atskaņotais raidījuma ilgums.
		/// </summary>
		private Timer progressTimer;
		/// <summary>
		/// Vai notiek programmas stāvokļa maiņa, piemēram, raidījumu saraksta ieslēgšana.
		/// </summary>
		private bool stateChanging=false;
		/// <summary>
		/// Raidījuma atskaņošanas progresa joslas lielākais augstums DIPikseļos.
		/// </summary>
		/// <remarks>Atbilst <code>progressBar.ActualWidth</code>.
		/// Iestatīts konstanti, jo tā vienkāršāk laicīgi (pirms kanāla piešķiršanas) tikt pie vērtības.</remarks>
		private const double ProgressMaxWidth=240;

		/// <summary>Uztveramais kanāls.</summary>
		public Channel Channel { get { return channel; } }

		#if DEBUG
		public MainWindow() : this(null) {} // Konstruktors Visual Studio dizainerim.
		#endif
		public MainWindow(Channel firstChannel) {
			InitializeComponent();
			this.channel=firstChannel;
			this.Loaded+=Window_Loaded; this.Closing+=Window_Closing;
			mutedIcon=(BitmapImage)Resources["MutedIcon"];
			unmutedIcon=(BitmapImage)Resources["UnmutedIcon"];
			ShowMuteStateInTaskBar(true);
			this.CommandBindings.Add(new CommandBinding(MediaCommands.MuteVolume, MuteCmd_Executed));
			this.CommandBindings.Add(new CommandBinding(MediaCommands.IncreaseVolume, IncreaseCmd_Executed));
			this.CommandBindings.Add(new CommandBinding(MediaCommands.DecreaseVolume, DecreaseCmd_Executed));
			appMenuMask.Freeze(); channelMenuMask.Freeze(); guideMenuMask.Freeze(); guidedMenuMask.Freeze(); guidedChannelMenuMask.Freeze();
		}

		public async Task SetChannel(Channel newChannel) {
			if (stateChanging) return;
			stateChanging=true;
			if (newChannel == null) newChannel=new EmptyChannel();
			bool hadGuide=false;
			if (channel != null && notFirstChannel) {
				if (!(channel is EmptyChannel)) {
					newChannel.IsMuted=channel.IsMuted;
					newChannel.Volume=channel.Volume;
					hadGuide=channel.Guide != null;
					if (hadGuide) {
						if (guidePanel.Visibility == Visibility.Visible) StopGuide();
						channel.Guide.CurrentBroadcastChanged-=guide_BroadcastChanged;
						if (channel.Guide.Menu != null)
							for (int n=0; n < channel.Guide.Menu.Items.Count; n++)
								MenuHelper.Delete((uint)(MenuHelper.GuideIdOffset+n));
					}
				}
				if (channel.Menu != null)
					for (int n=0; n < channel.Menu.Items.Count; n++)
						MenuHelper.Delete((uint)(MenuHelper.ChannelIdOffset+n));
				ProperWindow.CloseAll();
				channel.Stop(); channel.Dispose();
			}

			Title=newChannel.Caption; // Šiem jāsakrīt, jo to izmanto EmptyChannel loga pogas atrašanai.

			if (Settings.Default.ShowClock) {
				// channel piešķiršanas secību nosaka UpdateClock nepieciešamā instance.
				if (newChannel.Timezone == null) {
					HideClock();
					channel=newChannel;
				} else {
					channel=newChannel;
					ShowClock();
				}
			} else channel=newChannel;

			if (newChannel.Menu != null)
				newChannel.Menu.Items.AddToMenu(MenuHelper.SystemMenuHandle, 1, MenuHelper.ChannelIdOffset);
			// Tā kā raidījumu saraksta ielāde var prasīt laiku, nomaina izskatu pirms tā.
			await Dispatcher.InvokeAsync(() => {
				System.Windows.Controls.Grid.SetColumnSpan(logoBackground, channel.Brand.HasSharedBackground ? 2:1);
				channel.Brand.Focused=this.IsActive;
				DataContext=channel; // Parāda kanālu pirms raidījumu saraksta ielādes, lai tas nebremzē pārslēgšanas uztveri.
			});
			if (newChannel.HasGuide && Settings.Default.UseGuide) {
				InitGuide();
				if (!hadGuide) ShowGuide();
				await StartGuide(true).ConfigureAwait(false);
			} else HideGuide();
			await Dispatcher.InvokeAsync(() => {
				ShowMuteStateInTaskBar(channel.IsMuted);
				UpdateMenuButtonMask(); // Atkarīgs no raidījum saraksta pieejamības un notiek saskarnes pavedienā.
			});
			channel.Play();
			stateChanging=false;
		}

		#region Pulksteņa vadība
		private void ShowClock() {
			clock.Visibility=Visibility.Visible;
			if (clockTimer == null) {
				clockTimer=new Timer();
				clockTimer.Elapsed+=clockTimer_Elapsed;
			}
			StartClockTimer();
		}
		private void HideClock() {
			clock.Visibility=Visibility.Collapsed;
			if (clockTimer != null) { // Ja pirmajam kanālam nav laika zonas, tad taimeris nav inicializēts.
				clockTimer.Elapsed-=clockTimer_Elapsed;
				clockTimer.Dispose();
				clockTimer=null;
			}
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
		private void SwitchClock() {
			stateChanging=true;
			bool newStatus=!Settings.Default.ShowClock;
			MenuHelper.SetIsChecked(2, Settings.Default.ShowClock=newStatus);
			if (newStatus && channel.Timezone != null) ShowClock();
			else if (!newStatus && clockTimer != null) HideClock();
			stateChanging=false;
		}
		#endregion

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
		private void UpdateMenuButtonMask() {
			byte menuConfig=000;
			if (channel.HasGuide) menuConfig+=100;
			if (channel.Menu != null) menuConfig+=010;
			if (channel.Guide != null && channel.Guide.Menu != null) menuConfig+=001;
			switch (menuConfig) {
				case 000: menuBtn.Background=appMenuMask; break;
				case 100: menuBtn.Background=guidedMenuMask; break;
				case 010: menuBtn.Background=channelMenuMask; break;
				case 110: menuBtn.Background=guidedChannelMenuMask; break;
				case 101: menuBtn.Background=guideMenuMask; break;
				case 111: menuBtn.Background=Brushes.Black; break;
			}
		}

		#region Raidījumu saraksta vadība
		private void InitGuide() {
			if (channel.Guide == null) channel.SetGuide();
			channel.Guide.CurrentBroadcastChanged+=guide_BroadcastChanged;
			if (channel.Guide.HasKnownDuration) {
				if (progressTimer == null) {
					progressTimer=new Timer();
					progressTimer.Elapsed+=progressTimer_Elapsed;
				}
				// Parāda atskaņotā ilguma joslu.
				progressBar.Visibility=Visibility.Visible;
				progressBarBack.Visibility=Visibility.Visible;
				currentRow.Height=new GridLength(22, GridUnitType.Pixel);
			} else {
				// Paslēpj atskaņotā ilguma joslu.
				progressBar.Visibility=Visibility.Collapsed;
				progressBarBack.Visibility=Visibility.Collapsed;
				currentRow.Height=new GridLength(20, GridUnitType.Pixel);
			}
			AddGuideMenu();
		}
		private void ShowGuide() {
			guidePanel.Visibility=Visibility.Visible;
			TaskbarItemInfo.ThumbnailClipMargin=thumbnailWithGuideMargin;
			this.Width=446;
		}
		private void HideGuide() {
			guidePanel.Visibility=Visibility.Collapsed;
			TaskbarItemInfo.ThumbnailClipMargin=thumbnailWithoutGuideMargin;
			this.Width=206;
			infoTaskBtn.IsEnabled=false;
		}
		private async Task StartGuide(bool restart) {
			progressBar.Width=0; // Lai nerāda nepatiesu ilgumu, kamēr ielādējas raidījumi.
			#if !DEBUG
			try {
			#endif
				await channel.Guide.Start(restart);
			#if !DEBUG
			} catch (Exception ex) {
				RadioApp.LogError(ex, channel.Id);
				RadioApp.ShowError(channel.Caption+" raidījumu sarakstu nevar ieslēgt", "Kļūda ieslēdzot raidījumu sarakstu");
				return;
			}
			#endif
				if (channel.Guide.Menu != null) {
				for (uint n=0; n < channel.Guide.Menu.Items.Count; n++)
					MenuHelper.SetIsEnabledGuide(n, true);
				infoTaskBtn.IsEnabled=channel.Guide.HasInfoCommand;
			}
		}
		private void StopGuide() {
			channel.Guide.Stop();
			if (progressTimer != null) progressTimer.Stop();
			if (channel.Guide.Menu != null) {
				for (uint n=0; n < channel.Guide.Menu.Items.Count; n++)
					MenuHelper.SetIsEnabledGuide(n, false);
				infoTaskBtn.IsEnabled=false;
			}
		}
		private void AddGuideMenu() {
			if (channel.Guide.Menu != null) {
				channel.Guide.Menu.Items.AddToMenu(MenuHelper.SystemMenuHandle, channel.Menu != null ? channel.Menu.Items.Count+1:1, MenuHelper.GuideIdOffset);
				if (channel.Guide.HasInfoCommand) {
					infoTaskBtn.Description=channel.Guide.Menu.Items[channel.Guide.Menu.Items.InformationIndex];
					infoTaskBtn.IsEnabled=true;
				} else infoTaskBtn.IsEnabled=false;
			}
		}
		private async void SwitchGuide() {
			if (stateChanging) return;
			stateChanging=true;
			bool newStatus=!Settings.Default.UseGuide;
			MenuHelper.SetIsChecked(1, Settings.Default.UseGuide=newStatus);
			if (channel.HasGuide) {
				if (newStatus) {
					bool restartGuide=false;
					if (channel.Guide == null) {
						InitGuide(); UpdateMenuButtonMask();
						restartGuide=true;
					} else if (channel.Guide.Menu != null)
						for (uint n=0; n < channel.Guide.Menu.Items.Count; n++)
							MenuHelper.SetIsEnabledGuide(n, true);
					ShowGuide(); await StartGuide(restartGuide).ConfigureAwait(false);
				} else {
					HideGuide(); StopGuide();
				}
			}
			stateChanging=false;
		}
		#endregion

		private async void Window_Loaded(object sender, RoutedEventArgs e) {
			this.Loaded-=Window_Loaded;
			this.Activated+=Window_Activated; this.Deactivated+=Window_Deactivated;
			IntPtr handle=new WindowInteropHelper(this).Handle;
			// Subclassing Window's WndProc http://blogs.msdn.com/nickkramer/archive/2006/03/18/554235.aspx
			HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WndProc)); // Loga izvēlnes komandu notveršanai.
			SystemEvents.SessionSwitch+=SystemEvents_SessionSwitch;

			MenuHelper.SystemMenuHandle=MenuHelper.GetSystemMenu(handle, false);
			MenuHelper.DeleteSystemSizeCommands(MenuHelper.SystemMenuHandle, true);
			// Pieliek programmas punktus.
			MenuHelper.SettingsMenuHandle=MenuHelper.CreatePopupMenu();
			// Ar tabulācijas zīmi atdalīts īsinājumtaustiņš.
			MenuHelper.InsertMenu(MenuHelper.SettingsMenuHandle, 0, MenuFlag.ByPosition | (Settings.Default.UseGuide ? MenuFlag.Checked:MenuFlag.Unchecked), new IntPtr(MenuHelper.GuideCommandId), "Rādīt raidījumu sarakstu	G");
			MenuHelper.InsertMenu(MenuHelper.SettingsMenuHandle, 1, MenuFlag.ByPosition | (Settings.Default.ShowClock ? MenuFlag.Checked:MenuFlag.Unchecked), new IntPtr(MenuHelper.ClockCommandId), "Rādīt laiku studijā	T");
			MenuHelper.InsertMenu(MenuHelper.SettingsMenuHandle, 2, MenuFlag.ByPosition | (Settings.Default.MuteOnLock ? MenuFlag.Checked:MenuFlag.Unchecked), new IntPtr(MenuHelper.LockCommandId), "Bloķējot izslēgt skaņu");
			MenuHelper.InsertMenu(MenuHelper.SettingsMenuHandle, 3, MenuFlag.ByPosition | (Settings.Default.UseSystemProxy ? MenuFlag.Checked:MenuFlag.Unchecked), new IntPtr(MenuHelper.ProxyCommandId), "Lietot sistēmas starpniekserveri");
			MenuHelper.InsertMenu(MenuHelper.SystemMenuHandle, 0, MenuFlag.ByPosition | MenuFlag.Popup, MenuHelper.SettingsMenuHandle, "Iestatījumi");
			MenuHelper.InsertMenu(MenuHelper.SystemMenuHandle, 1, MenuFlag.ByPosition | MenuFlag.String, new IntPtr(MenuHelper.AboutCommandId), "Par Radio");
			MenuHelper.InsertMenu(MenuHelper.SystemMenuHandle, 2, MenuFlag.ByPosition | MenuFlag.Separator, IntPtr.Zero, null);

			// Iestata pirmo kanālu šeit, lai Dispatcher.Invoke neuzkar programmu. BeginInvoke nav risinājums, jo tad paraleli izpildās kods, kuram jānotiek secīgi.
			await SetChannel(channel).ConfigureAwait(false);
			notFirstChannel=true;
		}
		private void Window_Closing(object sender, CancelEventArgs e) {
			// Apstādina kanālu tai skaitā arī, lai nepaliktu fona procesi.
			ProperWindow.CloseAll(); channel.Stop();
		}
		private void Window_Activated(object sender, EventArgs e) {
			channel.Brand.Focused=true;
		}
		private void Window_Deactivated(object sender, EventArgs e) {
			channel.Brand.Focused=false;
		}
		protected override void OnKeyUp(KeyEventArgs e) {
			switch (e.Key) {
				case Key.C: if (channel.Guide != null) CopyCurrentBroadcastData(this, e); break;
				case Key.G: SwitchGuide(); break;
				case Key.H: string url=channel.HomepageUrl; if (url != null) DefaultProgram.OpenPage(url); break;
				case Key.M: MuteCmd_Executed(this, e); break;
				case Key.T: SwitchClock(); break;
				case Key.X: if (channel.Guide != null) CopyPreviousBroadcastData(this, e); break;
				case Key.F1: if (infoTaskBtn.IsEnabled) channel.Guide.Menu.HandleCommand(MenuIcon.Information); break;
				case Key.F2: if (channel.Menu != null) channel.Menu.HandleCommand(MenuIcon.Playlist); break;
				case Key.F3: if (channel.Menu != null) channel.Menu.HandleCommand(MenuIcon.Video); break;
				case Key.F4: if (channel.Menu != null) channel.Menu.HandleCommand(MenuIcon.Settings); break;
				case Key.Down: DecreaseCmd_Executed(this, null); break;
				case Key.Up: IncreaseCmd_Executed(this, null); break;
				default: base.OnKeyUp(e); break;
			}
		}
		/// <summary>Nodrošina skaņas izslēgšanu, kad lietotājs bloķē datoru.</summary>
		private async void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e) {
			switch (e.Reason) {
				case SessionSwitchReason.SessionLock:
					if (Settings.Default.MuteOnLock) {
						wasMuted=channel.IsMuted;
						if (!wasMuted) {
							channel.IsMuted=true;
							ShowMuteStateInTaskBar(true);
						}
					}
					if (clockTimer != null) clockTimer.Stop();
					if (Settings.Default.UseGuide && channel.Guide != null && !channel.Guide.IsStreamDependent) StopGuide();
					break;
				case SessionSwitchReason.SessionUnlock:
					if (Settings.Default.MuteOnLock && !wasMuted) {
						channel.IsMuted=false;
						ShowMuteStateInTaskBar(false);
					}
					if (clockTimer != null) StartClockTimer();
					if (Settings.Default.UseGuide && channel.Guide != null && !channel.Guide.IsStreamDependent) await StartGuide(false).ConfigureAwait(false);
					break;
			}
		}
		private void guide_BroadcastChanged(Broadcast broadcast) {
			if (progressTimer != null) progressTimer.Stop();
			if (broadcast == null) {
				Dispatcher.Invoke((Action<string>)((caption) => Title=caption), channel.Caption);
				return;
			}
			if (channel.Guide.HasKnownDuration) {
				double duration=(broadcast.EndTime-broadcast.StartTime).TotalSeconds;
				Dispatcher.Invoke((Action<double, string>)(
					(double newWidth, string broadcastCaption) => {
						progressBar.Width=newWidth; // Jau atskaņotais ilgums.
						SetWindowTitle(broadcastCaption);
					}), Math.Round(ProgressMaxWidth*(DateTime.Now-broadcast.StartTime).TotalSeconds/duration), broadcast.Caption);
				// Intervāls milisekundēs, pēc kurām ilguma rādītājs mainīsies par vienu pikseli.
				progressTimer.Interval=Math.Ceiling(1000*duration/ProgressMaxWidth);
				progressTimer.Start();
			} else Dispatcher.Invoke((Action<string>)SetWindowTitle, broadcast.Caption);
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
				if (progressBar.Width >= ProgressMaxWidth) progressTimer.Stop();
			}));
		}
		/// <summary>Ieslēdz/izslēdz skaņu.</summary>
		private void MuteCmd_Executed(object sender, EventArgs e) {
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
		private void InformationCmd_Executed(object sender, EventArgs e) {
			channel.Guide.Menu.HandleCommand(MenuIcon.Information);
		}
		/// <summary>
		/// Parāda programmas izvēlni (viens klikšķis) vai aizver logu (dubultklikšķis).
		/// </summary>
		private void ShowMenuFromButton(object sender, MouseButtonEventArgs e) {
			if (e.ClickCount == 1)
				SystemCommands.ShowSystemMenu(this, PointToScreen(new Point(0, 20)));
			else if (e.ClickCount == 2)
				this.Close();
		}
		/// <summary>Parāda programmas izvēlni.</summary>
		private void ShowMenuFromHeader(object sender, MouseButtonEventArgs e) {
			SystemCommands.ShowSystemMenu(this, PointToScreen(e.GetPosition(this)));
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
		private void CopyPreviousBroadcastData(object sender, RoutedEventArgs e) {
			CopyBroadcastData(channel.Guide.PreviousBroadcast);
		}
		private void CopyCurrentBroadcastData(object sender, RoutedEventArgs e) {
			CopyBroadcastData(channel.Guide.CurrentBroadcast);
		}
		private void CopyBroadcastData(Broadcast broadcast) {
			if (broadcast == null) return;
			Clipboard.SetText(string.Concat(broadcast.Caption, Environment.NewLine, broadcast.Description));
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
			if (msg == 0x112) { // WM_SYSCOMMAND
				int commandId=wParam.ToInt32();
				if ((commandId & 0xFF00) == 0) { // Ja nav sistēmas komanda, piemēram, minimizēšana.
					switch (commandId) {
						case MenuHelper.GuideCommandId: SwitchGuide(); break;
						case MenuHelper.ClockCommandId: SwitchClock(); break;
						case MenuHelper.LockCommandId:
							MenuHelper.SetIsChecked(3, Settings.Default.MuteOnLock=!Settings.Default.MuteOnLock);
							break;
						case MenuHelper.ProxyCommandId:
							MenuHelper.SetIsChecked(4, Settings.Default.UseSystemProxy=!Settings.Default.UseSystemProxy);
							channel.Stop(); channel.Play();
							break;
						case MenuHelper.AboutCommandId:
							var aboutWindow=new AboutWindow(channel);
							aboutWindow.ShowDialog();
							break;
						default:
							if (commandId >= MenuHelper.ChannelIdOffset) channel.Menu.HandleCommand(commandId-MenuHelper.ChannelIdOffset);
							else if (commandId >= MenuHelper.GuideIdOffset) channel.Guide.Menu.HandleCommand(commandId-MenuHelper.GuideIdOffset);
							break;
					}
					handled=true;
				}
			}
			return IntPtr.Zero;
		}
	}
}