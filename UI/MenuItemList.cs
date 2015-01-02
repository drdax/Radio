using System;
using System.Collections;
using System.Collections.Generic;

namespace DrDax.RadioClient {
	public sealed class MenuItemList : IEnumerable {
		/// <summary>Sarakstam pievienoto elementu skaits.</summary>
		public int Count { get { return count; } }
		/// <summary>Informācijas komandas kārtas numurs izvēlnē. -1, ja tāda nav.</summary>
		public int InformationIndex { get { return GetIconIndex(MenuIcon.Information); } }

		public void Add(string caption) {
			Add(MenuIcon.None, caption);
		}
		public void Add(MenuIcon icon, string caption) {
			if (count == 5) throw new IndexOutOfRangeException("Izvēlnē drīkst būt tikai pieci elementi");
			if (caption == null) throw new ArgumentNullException("caption");

			if (!indexes.ContainsKey(icon)) indexes.Add(icon, count);
			items[count++]=Tuple.Create(icon, caption);
		}
		/// <param name="idx">Elementa kārtas numurs sarakstā.</param>
		/// <returns>Saraksta elementa nosaukums.</returns>
		public string this[int idx] {
			get { return items[idx].Item2; }
		}

		internal void AddToMenu(IntPtr menuHandle, int itemsOffset, int commandsOffset) {
			for (int n=0; n < count; n++) {
				uint position=(uint)(itemsOffset+n);
				string caption=items[n].Item2;
				// Tikai pirmajai ikonai pieliek tai atbilstošo īsinājumtaustiņu.
				if (n == indexes[items[n].Item1])
					switch (items[n].Item1) {
						case MenuIcon.Information: caption+="	F1"; break;
						case MenuIcon.Playlist: caption+="	F2"; break;
						case MenuIcon.Video: caption+="	F3"; break;
						case MenuIcon.Settings: caption+="	F4"; break;
					}
				MenuHelper.InsertMenu(menuHandle, position, MenuFlag.ByPosition, new IntPtr(commandsOffset+n), caption);
				if (items[n].Item1 != MenuIcon.None)
					MenuHelper.SetItemIcon(menuHandle, position, new Uri(items[n].Item1.ToString()+".png", UriKind.Relative));
			}
		}

		private Tuple<MenuIcon, string>[] items=new Tuple<MenuIcon,string>[5];
		private readonly Dictionary<MenuIcon, int> indexes=new Dictionary<MenuIcon,int>(5);
		private int count=0;

		/// <summary>
		/// Lai Add metodes darbotos ar kolekciju iniciētājiem, nepieciešams realizēt IEnumerable, bet mums to nevajag.
		/// </summary>
		public IEnumerator GetEnumerator() { throw new NotSupportedException(); }

		internal int GetIconIndex(MenuIcon icon) {
			int index;
			if (indexes.TryGetValue(icon, out index)) return index;
			return -1;
		}
	}
	/// <summary>Ikona blakus izvēlnes elementam.</summary>
	public enum MenuIcon : byte {
		/// <summary>Tukšs.</summary>
		None,
		/// <summary>Informācija par raidījumu.</summary>
		Information,
		/// <summary>Iepriekšējo raidījumu saraksts.</summary>
		Playlist,
		/// <summary>Tiešraide no studijas.</summary>
		Video,
		/// <summary>Iestatījumi, piemēram, kanālu saraksts dinamiskā stacijā.</summary>
		Settings
	}
}