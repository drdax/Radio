using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace DrDax.RadioClient {
	internal partial class RadioApp : Application {
		/// <summary>Atdala stacijas nosaukumu un kanāla numuru tajā.</summary>
		public const char ChannelIdSeparator='.';
		/// <summary>Kanālu identifikatoru atdalītājs iestatījumu failā.</summary>
		private const string ChannelSeparator=" ";

		/// <summary>Kanāla pārslēgšanas pakalpojums.</summary>
		private ServiceHost switchService;
		/// <summary>Programmas ikona, kuru lieto, kad stacijai nav savas ikonas.</summary>
		private readonly BitmapSource RadioIcon;
		/// <summary>Kanālu ikonas.</summary>
		private readonly Dictionary<string, BitmapSource> icons;
		/// <summary>Visas pieejamās raidstacijas.</summary>
		public readonly Dictionary<string, Station> Stations;
		private readonly JumpList jumpList;
		public static readonly string ExePath=System.Reflection.Assembly.GetExecutingAssembly().Location;
		private string[] ignoredChannels;

		public RadioApp(Dictionary<string, Station> stations) {
			Stations=stations;
			foreach (var station in Stations.Values) {
				var dyn=station as DynamicStation;
				if (dyn != null) dyn.ChannelsChanged=station_ChannelsChanged;
			}

			RadioIcon=IconLoader.LoadAssemblyIcon(this.GetType().Assembly.Location, 0);
			icons=new Dictionary<string, BitmapSource>(stations.Count);
			ignoredChannels=Settings.Default.IgnoredChannels.Split(new[] { ChannelSeparator }, StringSplitOptions.RemoveEmptyEntries);

			// Aizpilda raidstaciju pārslēgšanas sarakstu.
			jumpList=new JumpList();
			jumpList.JumpItemsRemovedByUser+=jumpList_JumpItemsRemovedByUser;
			FillJumpList();

			// Palaiž raidstaciju pārslēgšanas pakalpojumu.
			switchService=new ServiceHost(typeof(RadioSwitch), new Uri(RadioSwitch.ServiceUrl));
			switchService.AddServiceEndpoint(typeof(IRadioSwitch), new NetNamedPipeBinding(), string.Empty);
			switchService.Open();

			InitializeComponent();
		}

		private void FillJumpList() {
			jumpList.JumpItems.AddRange(
				from s in Stations
				from c in s.Value.Channels
				let channelId=s.Key+ChannelIdSeparator+c.Key
				where !ignoredChannels.Contains(channelId)
				select new JumpTask {
					Title=c.Value.Item1,
					CustomCategory="Raidstacijas",
					ApplicationPath=ExePath,
					Arguments=channelId,
					IconResourcePath=s.Value.IconPath,
					IconResourceIndex=c.Value.Item2
				});
			jumpList.Apply();
			JumpList.SetJumpList(this, jumpList);
		}

		/// <summary>
		/// Pievieno ignorētajiem kanāliem tos, kurus dzēsa lietotājs kopš iepriekšējām kanālu saraksta izmaiņām.
		/// </summary>
		private void jumpList_JumpItemsRemovedByUser(object sender, JumpItemsRemovedEventArgs e) {
			var ignoredList=new List<string>(ignoredChannels.Length+e.RemovedItems.Count); // Visu dzēsto kanālu identifikatori.
			ignoredList.AddRange(ignoredChannels);
			foreach (JumpTask item in e.RemovedItems) {
				string[] id=item.Arguments.Split(ChannelIdSeparator);
				// Dinamiskā raidstacija pati dzēš savus kanālus, ..
				DynamicStation station=Stations[id[0]] as DynamicStation;
				if (station != null) station.RemoveChannel(uint.Parse(id[1]));
				else
					// .. pārējos kanālus iegaumē.
					ignoredList.Add(item.Arguments);
			}
			Settings.Default.IgnoredChannels=String.Join(ChannelSeparator, ignoredList);
			ignoredChannels=ignoredList.ToArray();
		}
		private void station_ChannelsChanged() {
			jumpList.JumpItems.Clear(); // Vieglāk notīrīt visu, nekā dzēst vienu staciju un aizstāt tās kanālus.
			FillJumpList();
		}

		/// <summary>Atgriež kanālu pēc tā identifikatora.</summary>
		/// <param name="id">Kanāla identifikators formā {stacijas nosaukums}.{kanāla numurs}</param>
		public Channel GetChannel(string id) {
			if (string.IsNullOrWhiteSpace(id)) return null;
			var idParts=id.Split(ChannelIdSeparator);
			Station station; uint number;
			if (!Stations.TryGetValue(idParts[0].ToLower(), out station)
				|| !uint.TryParse(idParts[1], out number)) throw new ChannelNotFoundException(id);
			var channel=station.GetChannel(number);
			var channelData=station.Channels[number];
			string iconId=idParts[0].ToLower()+channelData.Item2;
			// Aizpilda kanāla parametrus, kuri zināmi tikai šeit.
			channel.Id=id; channel.Station=station; channel.Number=number;
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
			MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
		}
		/// <summary>Mēģina uz darbagalda saglabāt informāciju par kļūdu radio darbībā.</summary>
		public static void LogError(Exception ex, string channelId) {
			try {
				System.IO.File.WriteAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
					string.Format("Radio Exception {0:yyyy-MM-dd HH-mm-ss}.txt", DateTime.UtcNow)),
					string.Concat(channelId, Environment.NewLine,
						string.Format("{0} {1:yyyy-MM-dd HH:mm}", AboutWindow.Version, AboutWindow.BuildTime), Environment.NewLine,
						ex.ToString()));
			} catch {}
		}
	}
}