using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DrDax.RadioClient {
	internal partial class AboutWindow : ProperWindow {
		public static string Version { get { return AboutWindow.version; } }
		public static DateTime BuildTime { get { return AboutWindow.buildTime; } }
		public BitmapSource ChannelIcon { get { return channel.Icon; } }
		public string ChannelCaption { get { return channel.Caption; } }
		public string Homepage { get { return homepageUrl; } }

		public AboutWindow(Channel channel) {
			InitializeComponent();
			this.channel=channel;
			homepageUrl=channel.HomepageUrl;
			if (homepageUrl == null) {
				homepageIcon.Visibility=Visibility.Collapsed;
				homepageLink.Visibility=Visibility.Collapsed;
			}
			DataContext=this;
		}

		static AboutWindow() {
			var assembly=Assembly.GetExecutingAssembly();
			var v=assembly.GetName().Version;
			version=string.Concat(v.Major.ToString(), ".", v.Minor.ToString());

			// http://www.codinghorror.com/blog/2005/04/determining-build-date-the-hard-way.html
			byte[] header=new byte[2048];
			using (Stream f=File.OpenRead(assembly.Location))
				f.Read(header, 0, 2048);
			DateTime epoch=new DateTime(1970,1,1, 0,0,0);
			buildTime=epoch.AddSeconds(BitConverter.ToInt32(header, BitConverter.ToInt32(header, 60)+8)) // PE header offset + Linker timestamp offset
				.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(epoch).Hours);
		}

		private void Hyperlink_Click(object sender, RoutedEventArgs e) {
			DefaultProgram.OpenPage(((Hyperlink)sender).NavigateUri.AbsoluteUri);
			this.Close();
		}

		protected override void OnSourceInitialized(EventArgs e) {
			base.OnSourceInitialized(e);
			IntPtr handle=new WindowInteropHelper(this).Handle;
			MenuHelper.DeleteSystemSizeCommands(MenuHelper.GetSystemMenu(handle, false), false); // Savādāk izmēra komandas pa reizei kļūst pieejamas.
			SetWindowLong(handle, -16 /*GWL_STYLE*/, 0x00C00000 | 0x00080000); // WS_CAPTION | WS_SYSMENU vai lietot this.ResizeMode=NoResize.
			// http://stackoverflow.com/a/3364875
			// Palūdz noņemt ikonu.
			SendMessage(handle, 0x0080 /*WM_SETICON*/, new IntPtr(0 /*ICON_SMALL*/), IntPtr.Zero);
			SendMessage(handle, 0x0080 /*WM_SETICON*/, new IntPtr(1 /*ICON_BIG*/), IntPtr.Zero);
			// Nomaina loga stilu uz tādu, kuram nav ikonas, bet ir ierastie virsraksta izmēri salīdzinot ar Tool Window stilu.
			SetWindowLong(handle, -20 /*GWL_EXSTYLE*/, 0x0001 /*WS_EX_DLGMODALFRAME*/);
			// Atjaunina loga neklienta daļu, lai izmaiņas uzzīmētos.
			SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0004 | 0x0020); // SWP_NOSIZE | SWP_NOMOVE | SWP_NOZORDER | SWP_FRAMECHANGED
		}
		[DllImport("user32.dll")]
		static extern IntPtr SendMessage(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
		[DllImport("user32.dll")]
		private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(IntPtr hwnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

		private static readonly DateTime buildTime;
		private static readonly string version;
		private readonly Channel channel;
		private readonly string homepageUrl;
	}
}
