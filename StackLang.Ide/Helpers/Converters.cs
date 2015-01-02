using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using StackLang.Core;

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

	public class ExecutionSourceToStringConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return "Source: " +
				((ExecutionParameters.ExecutionSource)value == ExecutionParameters.ExecutionSource.Stack ? "Stack" : "Code");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class NullToBoolConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class NullToGridLengthConverter : IValueConverter {
		GridLength lastLength;
		bool lastValueExtended;
		GridLength extendedLength = new GridLength(1, GridUnitType.Star);

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			if (value == null) {
				if (lastValueExtended) {
					extendedLength = lastLength;
				}
				lastLength = new GridLength(0, GridUnitType.Star);
				lastValueExtended = false;
			}
			else {
				lastLength = extendedLength;
				lastValueExtended = true;
			}
			return lastLength;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class NullToVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return value == null ? Visibility.Collapsed : Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}

	public class BoolToHiddenVisibilityConverter : IValueConverter {
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			return (bool) value
				? Visibility.Visible
				: Visibility.Hidden;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			throw new NotImplementedException();
		}
	}
}