using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using DrDax.RadioClient.Properties;

namespace DrDax.RadioClient {
	internal partial class RadioApp : Application {
		/// <summary>Atdala stacijas nosaukumu un kanāla numuru tajā.</summary>
		public const char ChannelIdSeparator='.';
		/// <summary>Kanālu identifikatoru atdalītājs iestatījumu failā.</summary>
		private const char ChannelSeparator=' ';

		/// <summary>Kanāla pārslēgšanas pakalpojums.</summary>
		private ServiceHost switchService;
		/// <summary>Programmas ikona, kuru lieto, kad stacijai nav savas ikonas.</summary>
		private readonly BitmapSource RadioIcon;
		/// <summary>Kanālu ikonas.</summary>
		private readonly Dictionary<string, BitmapSource> icons;
		/// <summary>Visas pieejamās raidstacijas.</summary>
		public readonly Dictionary<string, Station> Stations;
		/// <summary>Vai pašlaik rāda dialoglogu pa virsu programmas logam.</summary>
		/// <remarks>Ieviests, lai kļūdas paziņojumi varētu saņemt fokusu.</remarks>
		public static bool ShowingDialog=false;

		public RadioApp(Dictionary<string, Station> stations) {
			Stations=stations;
			string exePath=exePath=System.Reflection.Assembly.GetExecutingAssembly().Location;

			RadioIcon=IconLoader.LoadAssemblyIcon(this.GetType().Assembly.Location, 0);
			icons=new Dictionary<string, BitmapSource>(stations.Count);
			string[] ignoredChannels=Settings.Default.IgnoredChannels.Split(new[] { ChannelSeparator }, StringSplitOptions.RemoveEmptyEntries);

			// Aizpilda raidstaciju pārslēgšanas sarakstu.
			var jumps=new JumpList();
			jumps.JumpItemsRemovedByUser+=jumps_JumpItemsRemovedByUser;
			JumpList.SetJumpList(this, jumps);
			jumps.JumpItems.AddRange(
				from s in stations
				from c in s.Value.Channels
				let channelId=s.Key+ChannelIdSeparator+c.Key
				where !ignoredChannels.Contains(channelId)
				select new JumpTask {
					Title=c.Value.Item1,
					CustomCategory="Raidstacijas",
					ApplicationPath=exePath,
					Arguments=channelId,
					IconResourcePath=s.Value.IconPath,
					IconResourceIndex=c.Value.Item2
				});
			jumps.Apply();

			// Palaiž raidstaciju pārslēgšanas pakalpojumu.
			switchService=new ServiceHost(typeof(RadioSwitch), new Uri(RadioSwitch.ServiceUrl));
			switchService.AddServiceEndpoint(typeof(IRadioSwitch), new NetNamedPipeBinding(), string.Empty);
			switchService.Open();
			InitializeComponent();
		}

		/// <summary>
		/// Pievieno ignorētajiem kanāliem tos, kurus dzēsa lietotājs kopš iepriekšējās programmas palaišanas.
		/// </summary>
		private void jumps_JumpItemsRemovedByUser(object sender, JumpItemsRemovedEventArgs e) {
			string removedChannels=string.Join(" ", e.RemovedItems.Cast<JumpTask>().Select(t => t.Arguments));
			if (Settings.Default.IgnoredChannels.Length == 0)
				Settings.Default.IgnoredChannels=removedChannels;
			else Settings.Default.IgnoredChannels+=ChannelSeparator+removedChannels;
		}

		/// <summary>Atgriež kanālu pēc tā identifikatora.</summary>
		/// <param name="id">Kanāla identifikators formā {stacijas nosaukums}.{kanāla numurs}</param>
		public Channel GetChannel(string id) {
			if (string.IsNullOrWhiteSpace(id)) return null;
			var idParts=id.Split(ChannelIdSeparator);
			Station station; byte number;
			if (!Stations.TryGetValue(idParts[0].ToLower(), out station)
				|| !byte.TryParse(idParts[1], out number)) throw new ChannelNotFoundException(id);
			var channel=station.GetChannel(number);
			var channelData=station.Channels[number];
			string iconId=idParts[0].ToLower()+channelData.Item2;
			// Aizpilda kanāla parametrus, kuri zināmi tikai šeit.
			channel.Id=id;
			channel.Caption=channelData.Item1;
			// Ikona attēlošanai uz programmas ikonas uzdevumu joslā.
			// Formāts Windows Icon, izmērs 16x16 pikseļi.
			BitmapSource icon;
			if (!icons.TryGetValue(iconId, out icon)) {
				try {
					icon=IconLoader.LoadAssemblyIcon(station.GetType().Assembly.Location, channelData.Item2);
				} catch {
					icon=RadioIcon;
				}
				icons.Add(iconId, icon);
			}
			channel.Icon=icon;
			return channel;
		}

		/// <summary>Parāda kļūdas paziņojumu lietotājam.</summary>
		/// <param name="message">Paziņojuma teksts.</param>
		/// <param name="caption">Paziņojuma loga virsraksts.</param>
		public static void ShowError(string message, string caption) {
			ShowingDialog=true;
			MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
			ShowingDialog=false;
		}
	}
}