using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using StackLang.Ide.Helpers;
using StackLang.Ide.Model;

namespace StackLang.Ide.ViewModel {
	public class SettingsViewModel : ViewModelBase {
		IoSettingsModel _ioModel;
		public IoSettingsModel IoModel {
			get { return _ioModel; }
			set {
				if (_ioModel == value) {
					return;
				}
				_ioModel = value;
				RaisePropertyChanged();
			}
		}

		RelayCommand browseInputCommand;
		public RelayCommand BrowseInputCommand {
			get {
				return browseInputCommand ?? (browseInputCommand = new RelayCommand(BrowseInput));
			}
		}

		RelayCommand browseOutputCommand;
		public RelayCommand BrowseOutputCommand {
			get {
				return browseOutputCommand ?? (browseOutputCommand = new RelayCommand(BrowseOutput));
			}
		}

		void BrowseInput() {
			string result = FileDialogHelpers.ShowInputDialog(IoModel.InputStartName);
			if (result != null) {
				IoModel.InputFilename = result;
			}
		}

		void BrowseOutput() {
			string result = FileDialogHelpers.ShowOutputDialog(IoModel.OutputStartName);
			if (result != null) {
				IoModel.OutputFilename = result;
			}
		}
	}
}