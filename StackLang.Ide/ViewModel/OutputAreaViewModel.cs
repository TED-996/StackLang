using System;
using GalaSoft.MvvmLight.CommandWpf;
using StackLang.Ide.Helpers;
using StackLang.Ide.Model;
using StackLang.Ide.MVVMEnhancements;

namespace StackLang.Ide.ViewModel {
	public class OutputAreaViewModel : ViewModelBaseEnhanced {
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

		public RelayCommand ClearCommand {
			get { return GetRelayCommand(Clear); }
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