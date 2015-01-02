using System;

namespace DrDax.RadioClient {
	/// <summary>Komandu kopa un tās izpildošais kods.</summary>
	/// <typeparam name="TSource">Kanāls vai raidījumu saraksts.</typeparam>
	public abstract class Menu<TSource> : IDisposable {
		public readonly MenuItemList Items;
		public TSource Source;
		public abstract void HandleCommand(int itemIndex);

		protected Menu(MenuItemList items) {
			if (items == null) throw new ArgumentNullException("items");
			Items=items;
		}
		public void HandleCommand(MenuIcon icon) {
			int index=Items.GetIconIndex(icon);
			if (index != -1) HandleCommand(index);
		}

		public void Dispose() {
			Source=default(TSource); // null, noņem cirkulāro atsauci.
		}
	}
}