using System.Xml.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace DrDax.RadioClient {
	/// <summary>Programmas iestatījumi.</summary>
	public static class Settings {
		private static XDocument radioDoc;
		private static string exePath;
		private static string lastChannel;

		/// <summary>Pilns ceļš līdz iestatījumu dokumentam.</summary>
		private static string FilePath {
			get { return Path.Combine(Path.GetDirectoryName(ExePath), "Radio.xml"); }
		}
		/// <summary>Iestatījumu dokuments.</summary>
		private static XDocument RadioDoc {
			get {
				if (radioDoc == null) radioDoc=XDocument.Load(FilePath);
				return radioDoc;
			}
		}
		/// <summary>Pilns ceļš līdz programmai.</summary>
		public static string ExePath {
			get {
				if (exePath == null) exePath=Assembly.GetExecutingAssembly().Location;
				return exePath;
			}
		}
		/// <summary>Iestatījumos minētās raidstacijas.</summary>
		public static IEnumerable<XElement> Channels {
			get { return RadioDoc.Descendants("Channel"); }
		}
		/// <summary>Iestatījumos minētie staciju logotipi.</summary>
		public static IEnumerable<XElement> Logos {
			get { return RadioDoc.Descendants("Logo"); }
		}
		/// <summary>Pēdējās klausītās stacijas raidošā interneta adrese.</summary>
		public static string LastChannel {
			get {
				if (lastChannel == null)
					return RadioDoc.Root.Element("LastChannel") == null ? null : RadioDoc.Root.Element("LastChannel").Value;
				return lastChannel;
			}
			set { lastChannel=value; }
		}

		/// <summary>Saglabā iestatījumus.</summary>
		public static void Save() {
			if (lastChannel != null) {
				XElement lastChannelX=RadioDoc.Root.Element("LastChannel");
				if (lastChannelX == null) {
					lastChannelX=new XElement("LastChannel");
					lastChannelX.Value=lastChannel;
					RadioDoc.Root.AddFirst(lastChannelX);
				} else lastChannelX.Value=lastChannel;
			}
			RadioDoc.Save(FilePath);
		}
	}
}