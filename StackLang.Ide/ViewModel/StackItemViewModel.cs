using GalaSoft.MvvmLight;

namespace StackLang.Ide.ViewModel {
	public class StackItemViewModel : ViewModelBase {
		string _text = "";
		public string Text {
			get { return _text; }
			set {
				if (_text == value) {
					return;
				}
				_text = value;
				RaisePropertyChanged();
			}
		}

		bool _isHighlighted;
		public bool IsHighlighted {
			get { return _isHighlighted; }
			set {
				if (_isHighlighted == value) {
					return;
				}
				_isHighlighted = value;
				RaisePropertyChanged();
			}
		}

		public StackItemViewModel(string newText, bool newIsHighlighted = false) {
			Text = newText;
			IsHighlighted = newIsHighlighted;
		}
	}
}