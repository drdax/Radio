using System;
using System.Windows;
using System.Linq;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Shell;
using System.Diagnostics;
using System.ServiceModel;
using System.Runtime.InteropServices;

namespace DrDax.RadioClient {
	/// <summary>Raidstacija.</summary>
	public class Channel {
		private string caption;
		private string url;
		private BitmapSource logo;
		/// <summary>Stacijas nosaukums.</summary>
		public string Caption { get { return caption; } }
		/// <summary>Stacijas raidošā interneta adrese.</summary>
		public string Url { get { return url; } }
		/// <summary>Stacijas logotips.</summary>
		public BitmapSource Logo { get { return logo; } }
		private Channel(string caption, string url, BitmapSource logo) {
			this.caption=caption;
			this.url=url;
			this.logo=logo;
		}
		public static Channel GetFromSettings(string url) {
			if (url != null && (url.StartsWith("http://") || url.StartsWith("mms://"))) {
				XElement channelX=Settings.Channels.FirstOrDefault(c => c.Attribute("Url").Value == url);
				if (channelX == null) return null;
				BitmapImage logo;
				try {
					string encodedLogo=Settings.Logos.First(l => l.Attribute("Id").Value == channelX.Attribute("LogoId").Value).Value;
					logo=new BitmapImage();
					logo.BeginInit();
					logo.CacheOption=BitmapCacheOption.OnLoad;
					logo.StreamSource=new MemoryStream(Convert.FromBase64String(encodedLogo));
					logo.EndInit();
				} catch { logo=null; }
				return new Channel(channelX.Attribute("Caption").Value, channelX.Attribute("Url").Value, logo);
			} return null;
		}
	}

	/// <summary>Raidstaciju pārslēgšanas pakalpojuma kontrakts.</summary>
	[ServiceContract]
	public interface IRadioSwitch {
		[OperationContract]
		void SetChannel(string url);
	}
	/// <summary>Raidstaciju pārslēgšanas pakalpojums.</summary>
	[ServiceBehavior(IncludeExceptionDetailInFaults=true)]
	public class RadioSwitch : IRadioSwitch {
		public const string ServiceUrl="net.pipe://localhost/RadioSwitch";
		public void SetChannel(string url) {
			// http://stackoverflow.com/questions/1906416/async-function-callback-using-object-owned-by-main-thread
			Application.Current.Dispatcher.Invoke(new Action(() =>
				((MainWindow)Application.Current.MainWindow).Channel=Channel.GetFromSettings(url)
			));
		}
	}
	public static class Program {
		[STAThread]
		public static void Main(string[] args) {
			try {
				// Pārslēdz raidstaciju citā programmas eksemplārā (ja tāds ir).
				Process thisProcess=Process.GetCurrentProcess();
				foreach (Process otherProcess in Process.GetProcessesByName(thisProcess.ProcessName)) {
					if (otherProcess.Id != thisProcess.Id) {
						if (args.Length == 1) {
							var radioSwitch=new ChannelFactory<IRadioSwitch>(new NetNamedPipeBinding(), RadioSwitch.ServiceUrl);
							radioSwitch.CreateChannel().SetChannel(args[0]);
						}
						SetForegroundWindow(otherProcess.MainWindowHandle);
						return;
					}
				}
				// Palaiž raidstaciju pārslēgšanas pakalpojumu.
				var switchService=new ServiceHost(typeof(RadioSwitch), new Uri(RadioSwitch.ServiceUrl));
				switchService.AddServiceEndpoint(typeof(IRadioSwitch), new NetNamedPipeBinding(), "");
				switchService.Open();

				var app=new Application();
				// Aizpilda raidstaciju pārslēgšanas sarakstu.
				var jumps=new JumpList();
				JumpList.SetJumpList(app, jumps);
				jumps.JumpItems.AddRange(
					from c in Settings.Channels
					select new JumpTask {
						Title=c.Attribute("Caption").Value,
						CustomCategory="Raidstacijas",
						ApplicationPath=Settings.ExePath,
						Arguments=c.Attribute("Url").Value
					});
				jumps.Apply();
				// Palaiž programmu ar izvēlēto raidstaciju.
				var mainWindow=new MainWindow {
					Channel=Channel.GetFromSettings(args.Length == 1 ? args[0]:Settings.LastChannel)
				};
				app.Run(mainWindow);
				// Saglabā pēdējo klausīto raidstaciju.
				if (mainWindow.Channel != null && Settings.LastChannel != mainWindow.Channel.Url) {
					Settings.LastChannel=mainWindow.Channel.Url;
					Settings.Save();
				}
			} catch (Exception ex) {
				MessageBox.Show(ex.Message);
			}
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
	}
}