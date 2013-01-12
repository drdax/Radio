using System;
using System.ComponentModel;
using System.Globalization;

namespace DrDax.RadioClient {
	/// <summary>
	/// Pārveido <c>enum</c> elementu par attēlojamu teksta vērtību.
	/// </summary>
	/// <seealso cref="Extensions.GetDisplayName"/>
	// http://www.eidias.com/Blog/2012/1/30/localized-enums-with-entity-framework-code-first-41-in-wpf-mvvm-and-aspnet-mvc-3-part-2
	public class EnumToDisplayName : TypeConverter {
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			return (sourceType.Equals(typeof(Enum)));
		}
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			return (destinationType.Equals(typeof(String)));
		}
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			if (!(value is Enum))
				throw new ArgumentException("Var pārveidot tikai no enum.", "value");
			return ((Enum)value).GetDisplayName();
		}
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (!destinationType.Equals(typeof(String)))
				throw new ArgumentException("Var pārveidot tikai uz tekstu.", "destinationType");
			return ConvertFrom(context, culture, value);
		}
	}
}