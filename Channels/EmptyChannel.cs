#if DEBUG
using System;
using System.ComponentModel;
#endif

namespace DrDax.RadioClient {
	/// <summary>"Tukšs kanāls", kuru attēlo, kad pirmo reizi palaiž programmu.</summary>
	/// <remarks>Izstrādes vidē to izmanto, lai veidotu programmas loga noformējumu.</remarks>
	internal class EmptyChannel : Channel {
		public override double Volume {
			get { return Properties.Settings.Default.Volume; }
			set {}
		}

		public EmptyChannel() :
			#if DEBUG
			base(null, null, null, isDesignMode ? new DemoGuide():null, null) {
			#else
			base(null, null, null, null, null) {
			#endif
			Caption="Izvēlieties raidstaciju";
			#if DEBUG
			if (isDesignMode) PlaybackState=PlaybackState.Buffering;
			#endif
		}

		protected override bool GetIsMuted() { return true; }
		protected override void SetIsMuted(bool value) {}
		public override void Play() {}
		public override void Stop() {}

		#if DEBUG
		private static readonly bool isDesignMode=
			(bool)DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(System.Windows.FrameworkElement)).Metadata.DefaultValue;
		/// <summary>Raidījumu saraksts attēlošanai izstrādes vides dizaina līdzeklī.</summary>
		/// <remarks>Pēc izmaiņām programma jāpārkompilē, lai tās ieraudzītu.</remarks>
		private class DemoGuide : Guide {
			public DemoGuide() {
				DateTime now=DateTime.Now;
				// Reidījumi no Latvijas un Ukrainas valsts radio attēlošanai izstrādes vidē.
				PreviousBroadcast=new Broadcast(now.AddHours(-1), now, "Польова пошта");
				CurrentBroadcast=new Broadcast(now, now.AddHours(1), "Kustoņu pasaule",
					"Šoreiz „Kustoņu pasaulē divi temati: no sākuma dzirdēsim - kā vienā ģimenē sadzīvo izbijis patversmes iemītnieks- takšveidīgais Pipariņš ar tīršķirnes franču buldogu Krisu,");
				NextBroadcast=new Broadcast(now.AddHours(1), now.AddHours(2), "Zināmais nezināmajā",
					"reportāža no Baldones observatorijas - vērojot zvaigznes. Kā radās pirmās Galaktikas");
			}
			public override bool HasKnownDuration {
				get { return true; }
			}
		}
		#endif
	}
}