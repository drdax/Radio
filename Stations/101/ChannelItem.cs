using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace Ru101 {
	public class ChannelItem : IXmlSerializable, INotifyPropertyChanged, IComparable<ChannelItem> {
		public uint Id; // Visi kanālu identifikatori ir pozitīvi skaitļi, personalizētām stacijām tie ir lieli.
		public string Caption { get { return caption; } }
		/// <summary>Detalizēts kanāla apraksts attēlošanai izvēles logā.</summary>
		public string Description {
			get { return description; }
			set {
				description=value;
				if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Description"));
			}
		}
		public bool Selected { get; set; }
		/// <summary>Pilna logotipa adrese.</summary>
		/// <remarks>Lielai daļai kanālu der formula "http://101.ru/vardata/modules/channel/dynamics/pro/"+Id+".jpg",
		/// bet dažiem strādā tika īpašā adrese, tāpēc to glabā šajā laukā un iestatījumos.</remarks>
		public string LogoUrl { get { return logoUrl; } }
		public string StreamUrl;

		public event PropertyChangedEventHandler PropertyChanged;

		public ChannelItem(uint id, string caption, string logoUrl) {
			this.Id=id;
			this.caption=caption;
			this.logoUrl=logoUrl;
		}
		/// <summary>Serializācijas konstruktors.</summary>
		internal ChannelItem() {}

		#region Iestatījumu saglabāšana un nolasīšana
		public System.Xml.Schema.XmlSchema GetSchema() {
			return null;
		}
		public void ReadXml(XmlReader reader) {
			try {
				Id=uint.Parse(reader["Id"]);
				caption=reader["Caption"];
				logoUrl=reader["Logo"];
				reader.Read();
			} catch {}
		}
		public void WriteXml(XmlWriter writer) {
			writer.WriteAttributeString("Id", Id.ToString());
			writer.WriteAttributeString("Caption", caption);
			writer.WriteAttributeString("Logo", logoUrl);
		}
		#endregion
		/// <summary>Salīdzina pēc kanāla nosaukumiem sakārtošanai sarakstā.</summary>
		public int CompareTo(ChannelItem other) {
			return this.Caption.CompareTo(other.Caption);
		}

		private string caption;
		private string description;
		private string logoUrl;
	}
}