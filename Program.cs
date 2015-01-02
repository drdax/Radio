using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Registration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel;

namespace DrDax.RadioClient {
	internal partial class RadioApp {
		[STAThread]
		public static void Main(string[] args) {
			MainWindow mainWindow=null;
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
				var registration=new RegistrationBuilder(); registration.ForTypesDerivedFrom<Station>().Export<Station>();
				var container=new CompositionContainer(new AggregateCatalog(new DirectoryCatalog(".", "*.Station.dll", registration), new TypeCatalog(typeof(RadioXmlStation))));
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
				mainWindow=new MainWindow(channel);
				app.Run(mainWindow);

				if (!(mainWindow.Channel is EmptyChannel)) {
		#if !DEBUG
					// Saglabā pēdējo klausīto kanālu.
					settings.ChannelId=mainWindow.Channel.Id;
					settings.Volume=mainWindow.Channel.Volume;
		#endif
					mainWindow.Channel.Dispose();
				}
		#if !DEBUG
				bool saved=false;
				foreach (var station in app.Stations.Values)
					if (station.HasSettingsChanges) {
						station.SaveSettings();
						saved=true;
					}
				if (!saved && settings.HasChanges) settings.Save();
			} catch (Exception ex) { // Visaptverošs kļūdu uztvērējs, lai problēmu gadījumā neparādītos Windows Error Reporting logs.
				string channelId;
				if (mainWindow != null && mainWindow.Channel != null) {
					channelId=mainWindow.Channel.Id;
					try {
						mainWindow.Channel.Stop(); // Fona procesu likvidācijai.
					} catch {}
				} else channelId=null;
				if (!(ex is ChannelNotFoundException)) RadioApp.LogError(ex, channelId);
				RadioApp.ShowError(ex.Message, "Kļūda radio darbībā");
			}
		#endif
		}

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);
	}
}