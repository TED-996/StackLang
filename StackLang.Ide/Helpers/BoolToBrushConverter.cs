using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StackLang.Ide.Helpers {
	public class BoolToBrushConverter : IValueConverter {
		public Brush TrueBrush { get; set; }
		public Brush FalseBrush { get; set; }

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return System.Convert.ToBoolean(value) ? TrueBrush : FalseBrush;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return Equals(value, TrueBrush);
		}
	}
}