using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Shell;
using System.Windows.Interop;

namespace DrDax.RadioClient {
	public partial class MainWindow : Window {
		#region Resursi
		private ImageSource mutedIcon;
		private ImageSource unmutedIcon;
		private Brush activeBarBack;
		private Brush inactiveBarBack;
		#endregion
		private ThumbButtonInfo muteTaskBtn;
		private Channel channel;
		/// <summary>Atskaņojamā raidstacija.</summary>
		public Channel Channel {
			get { return channel; }
			set {
				channel=value;
				if (channel != null) {
					Title=channel.Caption;
					MuteBtn.IsEnabled=true;
					VolumeSlider.IsEnabled=true;
					if (channel.Logo != null) ChannelLogo.Source=channel.Logo;
					else ChannelLogo.Source=(BitmapImage)this.Resources["NoRadioLogo"];
					Player.Source=new Uri(channel.Url);
				} else {
					Title="Nav raidstacijas";
					MuteBtn.IsEnabled=false;
					VolumeSlider.IsEnabled=false;
					ChannelLogo.Source=(BitmapImage)this.Resources["NoRadioLogo"];
					Player.Source=null;
				}
			}
		}

		public MainWindow() {
			InitializeComponent();
			// http://blogs.msdn.com/nickkramer/archive/2006/03/18/554235.aspx
			this.Loaded+=(object sender, RoutedEventArgs e) => {
				HwndSource.FromHwnd(new WindowInteropHelper(this).Handle).AddHook(new HwndSourceHook(WndProc)); // Lai varētu uztvert loga fokusu.
			};
			activeBarBack=(Brush)this.Resources["ActiveBarBack"];
			inactiveBarBack=(Brush)this.Resources["InactiveBarBack"];
			mutedIcon=(BitmapImage)this.Resources["MutedIcon"];
			unmutedIcon=(BitmapImage)this.Resources["UnmutedIcon"];
			this.CommandBindings.Add(new CommandBinding(MediaCommands.MuteVolume, MuteCmdExecuted, VolumeCmdCanExecute));
			this.CommandBindings.Add(new CommandBinding(MediaCommands.IncreaseVolume, IncreaseCmdExecuted, VolumeCmdCanExecute));
			this.CommandBindings.Add(new CommandBinding(MediaCommands.DecreaseVolume, DecreaseCmdExecuted, VolumeCmdCanExecute));
			// Pieliek skaņas atslēgšanas pogu zem programmas sīktēla uzdevumu joslā.
			muteTaskBtn=new ThumbButtonInfo {
				Command=MediaCommands.MuteVolume,
				CommandTarget=this,
				ImageSource=unmutedIcon,
				Description="Izslēgt skaņu"
			};
			this.TaskbarItemInfo.ThumbButtonInfos.Add(muteTaskBtn);
			// Nodrošina izslēgtas skaņas ikonu uzdevumjoslā.
			// http://blogs.msdn.com/llobo/archive/2007/03/05/listening-to-dependencyproperty-changes.aspx
			DependencyPropertyDescriptor dpd=DependencyPropertyDescriptor.FromProperty(MediaElement.IsMutedProperty, typeof(MediaElement));
			if (dpd != null)
				dpd.AddValueChanged(Player, delegate {
					this.TaskbarItemInfo.Overlay=Player.IsMuted ? mutedIcon:null;
				});
		}

		private void VolumeCmdCanExecute(object sender, CanExecuteRoutedEventArgs e) {
			e.CanExecute=MuteBtn.IsEnabled;
		}
		/// <summary>Ieslēdz/izslēdz skaņu.</summary>
		private void MuteCmdExecuted(object sender, ExecutedRoutedEventArgs e) {
			Player.IsMuted=!Player.IsMuted;
			if (Player.IsMuted) {
				muteTaskBtn.ImageSource=mutedIcon;
				muteTaskBtn.Description="Ieslēgt skaņu";
			} else {
				muteTaskBtn.ImageSource=unmutedIcon;
				muteTaskBtn.Description="Izslēgt skaņu";
			}
		}
		/// <summary>Palielina skaļumu.</summary>
		private void IncreaseCmdExecuted(object sender, ExecutedRoutedEventArgs e) {
			if (Player.Volume != 1) Player.Volume+=0.1;
		}
		/// <summary>Pamazina skaļumu.</summary>
		private void DecreaseCmdExecuted(object sender, ExecutedRoutedEventArgs e) {
			if (Player.Volume != 0) Player.Volume-=0.1;
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
			if (msg == 0x86 /*WM_NCACTIVATE*/) {
				// http://stackoverflow.com/questions/813745/keep-window-inactive-in-appearance-even-when-activated
				if (wParam == (IntPtr)0 /*FALSE*/) // Logs pazaudēja fokusu.
					TopBar.Background=inactiveBarBack;
				else if (wParam == (IntPtr)1 /*TRUE*/) // Logs ieguva fokusu.
					TopBar.Background=activeBarBack;
				handled=true; // Bez šī WM_NCACTIVATE netiks izsaukts pēc loga atgriešanas no minimizēta stāvokļa.
			} else if (msg == 0x84 /*WM_NCHITTEST*/) { // Neļauj mainīt loga izmēru.
				// http://stackoverflow.com/questions/853533/aero-glass-borders-on-popup-windows-in-c
				handled=true;
				return (IntPtr)1;
			} 
			return IntPtr.Zero;
		}
	}
}