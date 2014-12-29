using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using StackLang.Ide.Helpers;
using StackLang.Ide.Model;

namespace StackLang.Ide.ViewModel {
	public class OutputAreaViewModel : ViewModelBase {
		public readonly OutputAreaModel Model;

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

		RelayCommand clearCommand;
		public RelayCommand ClearCommand {
			get {
				return clearCommand ?? (clearCommand = new RelayCommand(Clear));
			}
		}

		public OutputAreaViewModel() {
			Model = new OutputAreaModel();
			Model.WriteLineRequest += OnModelWriteLineRequest;
			Model.ClearRequest += OnModelClearRequest;
		}

		void OnModelWriteLineRequest(object sender, LineEventArgs lineEventArgs) {
			Text += lineEventArgs.Line + '\n';
		}

		void OnModelClearRequest(object sender, EventArgs e) {
			Clear();
		}

		void Clear() {
			Text = "";
		}
	}
}