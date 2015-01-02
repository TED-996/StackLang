using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using StackLang.Core;
using StackLang.Ide.Helpers;
using StackLang.Ide.Model;

namespace StackLang.Ide.ViewModel {
	public sealed class EditorTabViewModel : ViewModelBase {
		readonly FileModel fileModel;

		public readonly IoSettingsModel IoSettingsModel;

		readonly ObservableCollection<int> breakpoints;
		readonly InstructionHighlighter highlighter;

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

		string text = "";
		public string Text {
			get { return text; }
			set {
				Debug.WriteLine("Text changed: value: " + value);
				if (text == value) {
					return;
				}
				text = value;
				RaisePropertyChanged();
			}
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

		List<DocumentColorizingTransformer> _transformers;
		public List<DocumentColorizingTransformer> Transformers {
			get { return _transformers; }
			set {
				if (_transformers == value) {
					return;
				}
				_transformers = value;
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

		public EditorTabViewModel() : this(new FileModel()) {
			TextChanged += fileModel.OnEditorTabTextChanged;
			fileModel.PropertyChanged += OnFileModelPropertyChanged;
		}

		EditorTabViewModel(FileModel newFileModel) {
			fileModel = newFileModel;
			TextChanged += fileModel.OnEditorTabTextChanged;
			fileModel.PropertyChanged += OnFileModelPropertyChanged;

			Text = fileModel.Text;
			TextModified = false;

			IoSettingsModel = new IoSettingsModel {CodeFileName = fileModel.Filename};

			highlighter = new InstructionHighlighter();
			breakpoints = new ObservableCollection<int>();
			Transformers = new List<DocumentColorizingTransformer> {
				new BreakpointHighlighter(breakpoints),
				highlighter
			};
		}

		void OnFileModelPropertyChanged(object s, PropertyChangedEventArgs e) {
			if (e.PropertyName == "Filename" || e.PropertyName == "DisplayName") {
				RaisePropertyChanged("Name");
				IoSettingsModel.CodeFileName = fileModel.Filename;
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

		public void OnDebugStart(object sender, EventArgs e) {
			DebuggerModel debuggerModel = (DebuggerModel) sender;
			debuggerModel.DebugStart -= OnDebugStart;
			debuggerModel.NewSnapshot += OnNewSnapshot;
			debuggerModel.DebugEnd += OnDebugEnd;
		}

		public void OnNewSnapshot(object sender, NewSnapshotEventArgs e) {
			SnapshotWrapper snapshot = e.SnapshotWrapper;
			highlighter.Enabled = true;
			highlighter.Line = snapshot.CurrentLine + 1;
			highlighter.Instruction = snapshot.CurrentInstruction;
			highlighter.DimHighlight = snapshot.CurrentExecutionSource == ExecutionParameters.ExecutionSource.Stack;

			highlighter.RequestRedraw();
		}

		public void OnDebugEnd(object sender, EventArgs e) {
			highlighter.Enabled = false;

			highlighter.RequestRedraw();

			DebuggerModel debuggerModel = (DebuggerModel)sender;
			debuggerModel.NewSnapshot -= OnNewSnapshot;
			debuggerModel.DebugEnd -= OnDebugEnd;

		}

		public event EventHandler RequestRemove;
		public void RaiseRequestRemove() {
			if (TextModified) {
				bool? result = FileDialogHelpers.PromptSave(Name);
				if (result == null) {
					return;
				}
				if (result == true) {
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
}