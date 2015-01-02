#if DEBUG
using System;
using System.ComponentModel;
using System.Threading.Tasks;
#endif
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using SWF=System.Windows.Forms; // Iesauka, jo ir cita kursora klase System.Windows.Input.

namespace DrDax.RadioClient {
	/// <summary>"Tukšs kanāls", kuru attēlo, kad pirmo reizi palaiž programmu.</summary>
	/// <remarks>Izstrādes vidē to izmanto, lai veidotu programmas loga noformējumu.</remarks>
	public class EmptyChannel : Channel {
		public override double Volume {
			get { return Settings.Default.Volume; }
			set {}
		}

		#if DEBUG
		public EmptyChannel() : base(null, null, isDesignMode, null, null) { // Publisks, savādāk Visual Studio to neinicializē.
		#else
		internal EmptyChannel() : base(null, null, false, null, null) { // Iekšējs, jo to drīkst izmantot tikai Radio programma.
		#endif
			Caption="Izvēlieties raidstaciju";
			#if DEBUG
			if (isDesignMode) { PlaybackState=PlaybackState.Buffering; Guide=new DemoGuide(); }
			#endif
			openJumplist=true;
		}
		public EmptyChannel(string caption, BitmapSource logo, Brand brand, Menu<Channel> menu) : base(logo, null, false, brand, menu) {
			Caption=caption;
			openJumplist=false;
		}

		protected override bool GetIsMuted() { return true; }
		protected override void SetIsMuted(bool value) {}
		public override void Play() {
			// Ja lietotājam nospiesta peles poga, tad netraucē tās darbību.
			if (!openJumplist
				|| Mouse.LeftButton == MouseButtonState.Pressed || Mouse.RightButton == MouseButtonState.Pressed
				|| Mouse.MiddleButton == MouseButtonState.Pressed || Mouse.XButton1 == MouseButtonState.Pressed || Mouse.XButton2 == MouseButtonState.Pressed) return;
			// Ja prasa izvēlēties kanālu, tad jāparāda to saraksts (kurš gan var būt tukšs) uzdevumu joslas izvēlnē.
			AutomationElement button=null;
			try {
				// Rīkjolsa rūtī "Palaistās lietojumprogrammas".
				button=TreeWalker.ControlViewWalker.GetFirstChild(
					// Uzdevumu josla uz darbagalda. Parasti pirmā sarakstā.
					AutomationElement.RootElement.FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "Shell_TrayWnd")).
					// Galvenā Uzdevumu joslas daļa.
					FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.ClassNameProperty, "ReBarWindow32"))
				).
				// Palaistās programmas poga, kuru atrod pēc loga nosaukuma.
				FindFirst(TreeScope.Children, new PropertyCondition(AutomationElement.NameProperty, Caption));
			} catch { return; } // Lai nepārbaudītu elementu uz null katru reizi.
			if (button == null) return;

			var startPositon=SWF.Cursor.Position;
			var buttonBounds=button.Current.BoundingRectangle;
			// Pārvieto kursoru uz pogas viduspunktu.
			SWF.Cursor.Position=new System.Drawing.Point((int)(buttonBounds.Left+buttonBounds.Width/2), (int)(buttonBounds.Top+buttonBounds.Height/2));
			if (SWF.SystemInformation.MouseButtonsSwapped)
				MouseHelper.LeftClick();
			else MouseHelper.RightClick();
			// Atgriež kursoru vietā.
			SWF.Cursor.Position=startPositon;
		}
		public override void Stop() {}

		/// <summary>Vai jāmēģina atvērt kanālu saraksts.</summary>
		private readonly bool openJumplist;

		#if DEBUG
		private static readonly bool isDesignMode=
			(bool)DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(System.Windows.FrameworkElement)).Metadata.DefaultValue;
		/// <summary>Raidījumu saraksts attēlošanai izstrādes vides dizaina līdzeklī.</summary>
		/// <remarks>Pēc izmaiņām programma jāpārkompilē, lai tās ieraudzītu.</remarks>
		private class DemoGuide : Guide {
			public DemoGuide() : base(null, true, false) {
				DateTime now=DateTime.Now;
				// Reidījumi no Latvijas un Ukrainas valsts radio attēlošanai izstrādes vidē.
				PreviousBroadcast=new Broadcast(now.AddHours(-1), now, "Польова пошта");
				CurrentBroadcast=new Broadcast(now.AddMinutes(-5), now.AddHours(1), "Kustoņu pasaule",
					"Šoreiz „Kustoņu pasaulē divi temati: no sākuma dzirdēsim - kā vienā ģimenē sadzīvo izbijis patversmes iemītnieks- takšveidīgais Pipariņš ar tīršķirnes franču buldogu Krisu,");
				NextBroadcast=new Broadcast(now.AddHours(1), now.AddHours(2), "Zināmais nezināmajā",
					"reportāža no Baldones observatorijas - vērojot zvaigznes. Kā radās pirmās Galaktikas");
			}
			public override Task Start(bool initialize) { return null; }
		}
		#endif
	}
}