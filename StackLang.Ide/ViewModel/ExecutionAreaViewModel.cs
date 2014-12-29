using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using StackLang.Ide.Model;

namespace StackLang.Ide.ViewModel {
	public class ExecutionAreaViewModel : ViewModelBase {
		readonly ExecutionIoModel model;

		string _outputText = "";
		public string OutputText {
			get { return _outputText; }
			set {
				if (_outputText == value) {
					return;
				}
				_outputText = value;
				RaisePropertyChanged();
			}
		}

		string _inputText = "";
		public string InputText {
			get { return _inputText; }
			set {
				if (_inputText == value) {
					return;
				}
				_inputText = value;
				RaisePropertyChanged();
			}
		}

		bool _awaitingInput;
		public bool AwaitingInput {
			get { return _awaitingInput; }
			set {
				if (_awaitingInput == value) {
					return;
				}
				_awaitingInput = value;
				RaisePropertyChanged();
			}
		}

		RelayCommand inputEnterCommand;
		public RelayCommand InputEnterCommand {
			get {
				return inputEnterCommand ?? (inputEnterCommand = new RelayCommand(() => {
					model.ProvideInput(InputText);
					AwaitingInput = false;
					InputText = "";
				}));
			}
		}

		public ExecutionAreaViewModel(ExecutionIoModel newModel) {
			model = newModel;
			model.AwaitingInput += OnModelAwaitingInput;
			model.LineWriteRequest += OnModelLineWriteRequest;
		}

		void OnModelLineWriteRequest(object sender, LineEventArgs e) {
			OutputText += e.Line + '\n';
		}

		void OnModelAwaitingInput(object sender, EventArgs e) {
			_awaitingInput = true;
		}
	}
}