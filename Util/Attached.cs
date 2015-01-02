using System;
using System.Windows;
using System.Windows.Controls;

namespace DrDax.RadioClient {
	/// <summary>Piekabināmās XAML īpašības.</summary>
	public class Attached {
		// http://www.scottlogic.co.uk/blog/colin/2011/01/automatically-showing-tooltips-on-a-trimmed-textblock-silverlight-wpf/
		#region TrimAndShowToolTip
		/// <summary>
		/// Kuro stilu lietot elementa <see cref="TextBlock"/> uzpeldošajam padomam, ja tā teksts pārsniedz redzamo elementa daļu.
		/// Uzpeldošais padoms tiek automātiski pielikts vajadzības gadījumā.
		/// </summary>
		public static readonly DependencyProperty TrimAndShowToolTipProperty=DependencyProperty.RegisterAttached("TrimAndShowToolTip",
			typeof(Style), typeof(Attached), new PropertyMetadata(null, TrimAndShowToolTip_PropertyChanged));

		// Obligātās īpašības iestatīšanas/nolasīšanas metodes.
		public static Style GetTrimAndShowToolTip(DependencyObject obj) {
			return (Style)obj.GetValue(TrimAndShowToolTipProperty);
		}
		public static void SetTrimAndShowToolTip(DependencyObject obj, Style value) {
			obj.SetValue(TrimAndShowToolTipProperty, value);
		}

		private static void TrimAndShowToolTip_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
			TextBlock textBlock=d as TextBlock;
			if (textBlock == null) return; // Ignorē ne teksta elementus.

			if (e.NewValue != null) {
				textBlock.TextTrimming=TextTrimming.WordEllipsis;
				//ComputeTrimAndShowToolTip(textBlock);
				textBlock.TargetUpdated+=textBlock_TargetUpdated; // Lai šis notiktu, Binding.NotifyOnTargetUpdated jābūt true.
				textBlock.SizeChanged+=textBlock_SizeChanged;
			} else {
				textBlock.TargetUpdated-=textBlock_TargetUpdated;
				textBlock.SizeChanged-=textBlock_SizeChanged;
			}
		}

		static void textBlock_SizeChanged(object sender, SizeChangedEventArgs e) {
			ComputeTrimAndShowToolTip(sender as TextBlock);
		}
		private static void textBlock_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e) {
			ComputeTrimAndShowToolTip(sender as TextBlock);
		}

		/// <summary>
		/// Aprēķina vajadzību pēc uzpeldošā padoma un piešķir to teksta elementam <param name="textBlock"/>.
		/// </summary>
		private static void ComputeTrimAndShowToolTip(TextBlock textBlock) {
			if (textBlock.Text == string.Empty) { textBlock.ToolTip=null; return; } // Neesošs teksts mēdz izmērities lielāks, nekā aizpildīts, tāpēc šeit novērš tukšos taisnstūrus.
			textBlock.Measure(new Size(textBlock.TextWrapping == TextWrapping.Wrap ? textBlock.ActualWidth:Double.PositiveInfinity, Double.PositiveInfinity));
			double measuredDimension, actualDimension;
			if (textBlock.TextWrapping == TextWrapping.Wrap) {
				measuredDimension=textBlock.DesiredSize.Height;
				actualDimension=textBlock.ActualHeight;
			} else {
				measuredDimension=textBlock.DesiredSize.Width;
				actualDimension=textBlock.ActualWidth;
			}

			if (actualDimension < measuredDimension) {
				// Garāku tekstu rāda ilgāk, lai paspēj izlasīt.
				ToolTipService.SetShowDuration(textBlock, Math.Min(textBlock.Text.Length*200, 30000));
				textBlock.ToolTip=new ToolTip {
					Content=textBlock.Text, Style=GetTrimAndShowToolTip(textBlock)
				};
			} else {
				ToolTipService.SetShowDuration(textBlock, 5000);
				textBlock.ToolTip=null;
			}
		}
		#endregion
	}
}