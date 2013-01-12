using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DrDax.RadioClient {
	/// <summary>Radio signāla (atskaņošanas plūsmas) stāvoklis.</summary>
	[TypeConverter(typeof(EnumToDisplayName))]
	public enum PlaybackState : byte {
		/// <summary>Nav signāla (plūsmošana nenotiek).</summary>
		[Display(Name="Atvienojies")]
		Stopped=0,
		/// <summary>Veido savienojumu ar staciju (plūsmošana nenotiek).</summary>
		[Display(Name="Pieslēdzas")]
		Connecting=1,
		/// <summary>Ir signāls, bet vēl nespēj atskaņot (plūsmošana notiek).</summary>
		[Display(Name="Nolasa")]
		Buffering=2,
		/// <summary>Ir signāls un tas tiek atskaņots.</summary>
		[Display(Name="Atskaņo")]
		Playing=3
	}
}