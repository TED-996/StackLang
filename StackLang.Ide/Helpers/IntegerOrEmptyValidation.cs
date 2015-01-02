using System.Globalization;
using System.Windows.Controls;

namespace StackLang.Ide.Helpers {
	public class IntegerOrEmptyValidation : ValidationRule {
		public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
			if (value == null) {
				return new ValidationResult(false, "Value must not be null.");
			}
			if (value.ToString() == "") {
				return new ValidationResult(true, "Empty values are allowed.");
			}
			int intValue;
			if (!int.TryParse(value.ToString(), out intValue)) {
				return new ValidationResult(false, "Value is not a number.");
			}
			return new ValidationResult(true, null);
		}
	}
}