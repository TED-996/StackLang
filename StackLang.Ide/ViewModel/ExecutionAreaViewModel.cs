using System;
using GalaSoft.MvvmLight.CommandWpf;
using StackLang.Ide.Helpers;
using StackLang.Ide.Model;
using StackLang.Ide.MVVMEnhancements;

namespace StackLang.Ide.ViewModel {
	public class ExecutionAreaViewModel : ViewModelBaseEnhanced {
		public readonly ExecutionAreaModel Model;

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

		public RelayCommand InputEnterCommand {
			get { return GetRelayCommand(() => Model.ProvideInput(InputText)); }
		}

		public ExecutionAreaViewModel() {
			Model = new ExecutionAreaModel();

			Model.AwaitingInput += OnModelAwaitingInput;
			Model.WriteLineRequest += OnModelWriteLineRequest;
			Model.ClearRequest += OnModelClearRequest;
			Model.InputProvided += OnInputProvided;
		}

		void OnModelWriteLineRequest(object sender, LineEventArgs e) {
			OutputText += e.Line + '\n';
		}

		void OnModelClearRequest(object sender, EventArgs e) {
			Clear();
		}

		void Clear() {
			OutputText = "";
		}

		void OnModelAwaitingInput(object sender, EventArgs e) {
			AwaitingInput = true;
		}

		public void OnInputProvided(object sender, EventArgs e) {
			AwaitingInput = false;
			InputText = "";
		}
	}
}