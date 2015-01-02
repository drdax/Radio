using System;
using System.Globalization;
using System.Windows.Media;
using System.Xml;
using System.Xml.Serialization;
using DrDax.RadioClient;

namespace Pieci {
	public class ChannelItem : IXmlSerializable, IComparable<ChannelItem> {
		public uint Id;
		public string Caption { get; set; }
		public string Description { get; set; }
		public string Name;
		public int IconIdx;
		public bool Selected { get; set; }
		public Color Color {
			get { return color; }
			set { color=value; }
		}
		/// <summary>Serializācijas konstruktors.</summary>
		public ChannelItem() {}
		/// <summary>Tīkla datu pilnais konstruktors.</summary>
		public ChannelItem(uint id, string caption, Color color, string description=null, string name=null, bool selected=false) {
			Id=id; Caption=caption; Description=description; Name=name; Selected=selected;
			this.color=color;
			switch (id) {
				case  1: IconIdx=1; break;
				case  5: IconIdx=2; break;
				case  7: IconIdx=3; break;
				case  9: IconIdx=4; break;
				case 10: IconIdx=4; break;
				case 11: IconIdx=5; break;
				case 12: IconIdx=6; break;
				case 13: IconIdx=7; break;
				case 17: IconIdx=8; break;
				case 19: IconIdx=9; break;
				default: IconIdx=0; break;
			}
		}

		#region Iestatījumu saglabāšana un nolasīšana
		public System.Xml.Schema.XmlSchema GetSchema() {
			return null;
		}
		public void ReadXml(XmlReader reader) {
			try {
				Id=uint.Parse(reader["Id"]);
				Caption=reader["Caption"];
				Name=reader["Name"];
				IconIdx=int.Parse(reader["Icon"]);
				SetColor(reader["Color"]);
				reader.Read();
			} catch {}
		}
		public void WriteXml(XmlWriter writer) {
			writer.WriteAttributeString("Id", Id.ToString());
			writer.WriteAttributeString("Caption", Caption);
			writer.WriteAttributeString("Name", Name);
			writer.WriteAttributeString("Icon", IconIdx.ToString());
			// Color.ToString ir #AARRGGBB, mums vajag RRGGBB.
			writer.WriteAttributeString("Color", string.Format("{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B));
		}
		#endregion

		public void SetColor(string colorString) {
			color=int.Parse(colorString, NumberStyles.HexNumber).ToColor();
		}
		/// <summary>Salīdzina pēc kanāla nosaukumiem sakārtošanai sarakstā.</summary>
		public int CompareTo(ChannelItem other) {
			return this.Caption.CompareTo(other.Caption);
		}

		private Color color;
	}
}