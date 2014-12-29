using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using StackLang.Ide.Helpers;
using StackLang.Ide.Model;

namespace StackLang.Ide.ViewModel {
	public sealed class EditorTabViewModel : ViewModelBase {
		readonly FileModel fileModel;

		public string Name {
			get { return fileModel.DisplayName + (TextModified ? " *" : ""); }
		}

		bool _textModified;
		bool TextModified {
			get { return _textModified; }
			set {
				if (_textModified == value) {
					return;
				}
				_textModified = value;

				RaisePropertyChanged("Name");
			}
		}

		TextDocument _document = new TextDocument();
		public TextDocument Document {
			get { return _document; }
			set {
				if (_document == value) {
					return;
				}
				_document = value;
				RaisePropertyChanged();
			}
		}

		public string Text {
			get { return Document.Text; }
			set { Document.Text = value; }
		}

		IHighlightingDefinition _highlighting = HighlightingManager.Instance.HighlightingDefinitions.First(
				definition => definition.Name == "C#");
		public IHighlightingDefinition Highlighting {
			get { return _highlighting; }
			set {
				if (_highlighting == value) {
					return;
				}
				_highlighting = value;
				RaisePropertyChanged();
			}
		}

		RelayCommand textChangedCommand;
		public RelayCommand TextChangedCommand {
			get {
				return textChangedCommand
				       ?? (textChangedCommand = new RelayCommand(OnTextChanged));
			}
		}

		RelayCommand removeCommand;
		public RelayCommand RemoveCommand {
			get {
				return removeCommand
				       ?? (removeCommand = new RelayCommand(RaiseRequestRemove));
			}
		}

		public EditorTabViewModel() {
			fileModel = new FileModel();
			TextChanged += fileModel.OnEditorTabTextChanged;
			fileModel.PropertyChanged += OnFileModelPropertyChanged;
		}

		EditorTabViewModel(FileModel newFileModel) {
			fileModel = newFileModel;
			TextChanged += fileModel.OnEditorTabTextChanged;
			fileModel.PropertyChanged += OnFileModelPropertyChanged;

			Text = fileModel.Text;
			TextModified = false;
		}

		void OnFileModelPropertyChanged(object s, PropertyChangedEventArgs e) {
			if (e.PropertyName == "Filename" || e.PropertyName == "DisplayName") {
				RaisePropertyChanged("Name");
			}
		}

		public bool Save(bool saveAs = false) {
			string filename = fileModel.Filename;

			if (saveAs || filename == null) {
				filename = FileDialogHelpers.ShowSaveFileDialog(filename);
				if (filename == null) {
					return false;
				}
			}

			fileModel.Save(filename);

			TextModified = false;
			return true;
		}

		public event EventHandler RequestRemove;
		public void RaiseRequestRemove() {
			if (TextModified) {
				MessageBoxResult result = FileDialogHelpers.PromptSave(Name);
				if (result == MessageBoxResult.Cancel) {
					return;
				}
				if (result == MessageBoxResult.Yes) {
					Save();
				}
			}
			EventHandler ev = RequestRemove;
			if (ev != null) {
				ev(this, EventArgs.Empty);
			}
		}

		void OnTextChanged() {
			TextModified = true;
			RaiseTextChanged();
		}

		EventHandler<TextChangedEventArgs> TextChanged;
		void RaiseTextChanged() {
			EventHandler<TextChangedEventArgs> ev = TextChanged;
			if (ev != null) {
				ev(this, new TextChangedEventArgs(Text));
			}
		}

		public static EditorTabViewModel Open() {
			string filename = FileDialogHelpers.ShowOpenFileDialog();
			if (filename == null) {
				return null;
			}
			return new EditorTabViewModel(FileModel.LoadFromFile(filename));
		}
	}

	public class TextChangedEventArgs : EventArgs {
		public readonly string Text;

		public TextChangedEventArgs(string newText) {
			Text = newText;
		}
	}
}