using System;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;
using DrDax.RadioClient.Properties;

namespace DrDax.RadioClient {
	public static class Program {
		[STAThread]
		public static void Main(string[] args) {
		#if !DEBUG
			try {
		#endif
				// Pārslēdz kanālu citā programmas eksemplārā, ja tāds ir.
				Process thisProcess=Process.GetCurrentProcess();
				foreach (Process otherProcess in Process.GetProcessesByName(thisProcess.ProcessName)) {
					if (otherProcess.Id != thisProcess.Id) {
						if (args.Length == 1) {
							var radioSwitch=new ChannelFactory<IRadioSwitch>(
								new NetNamedPipeBinding() { OpenTimeout=TimeSpan.FromSeconds(10), SendTimeout=TimeSpan.FromSeconds(5) },
								RadioSwitch.ServiceUrl);
							try { radioSwitch.CreateChannel().SetChannel(args[0]); }
							catch {}
						} else SetForegroundWindow(otherProcess.MainWindowHandle);
						return;
					}
				}

				// Ielādē visas stacijas no programmas mapes un XML faila staciju.
				var container=new CompositionContainer(new AggregateCatalog(new DirectoryCatalog(".", "*.Station.dll"), new TypeCatalog(typeof(RadioXmlStation))));
				// ! Pašlaik drīkst būt tikai viena stacijas klase katrā DLLā.
				var app=new RadioApp(container.GetExportedValues<Station>().ToDictionary(s => {
					string name=s.GetType().Assembly.GetName().Name.ToLower();
					if (name.Contains(RadioApp.ChannelIdSeparator)) return name.Substring(0, name.Length-8).Replace(RadioApp.ChannelIdSeparator, '_'); // -8 noņem vārdu "station".
					else return name;
				}));

				Settings settings=Settings.Default;
				Channel channel=null;
				// Palaiž programmu ar izvēlēto kanālu.
				if (args.Length == 1) channel=app.GetChannel(args[0]);
				else {
					try {
						channel=app.GetChannel(settings.ChannelId);
					} catch (ChannelNotFoundException) {
						// Pēc staciju nomaiņas saglabātais kanāls var būt nepareizs.
						settings.ChannelId=null;
					}
				}
				var mainWindow=new MainWindow {
					Channel=channel // Jāiestata pat ja null.
				};
				app.Run(mainWindow);

		#if !DEBUG
				// Saglabā pēdējo klausīto kanālu.
				if (!(mainWindow.Channel is EmptyChannel)) {
					settings.ChannelId=mainWindow.Channel.Id;
					settings.Volume=mainWindow.Channel.Volume;
					settings.Save();
				}
			} catch (Exception ex) { // Visaptverošs kļūdu uztvērējs, lai problēmu gadījumā neparādītos Windows Error Reporting logs.
			   RadioApp.ShowError(ex.Message, "Kļūda radio darbībā");
			}
		#endif
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
	}
}