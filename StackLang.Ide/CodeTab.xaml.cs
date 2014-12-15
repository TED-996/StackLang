using System;
using System.IO;
using ICSharpCode.AvalonEdit.Highlighting;

namespace StackLang.Ide {
	/// <summary>
	/// Interaction logic for CodeTab.xaml
	/// </summary>
	public partial class CodeTab {
		public string Filename;

		public bool Changed { get; private set; }

		public string Text {
			get { return TextBox.Text; }
			private set { TextBox.Text = value; }
		}

		public bool ReadOnly {
			get { return TextBox.IsReadOnly; }
			set { TextBox.IsReadOnly = value; }
		}

		public string TabName {
			get { return (Filename == null ? "Untitled" : Path.GetFileName(Filename)) + (Changed ? "*" : ""); }
		}

		readonly Action tabNameUpdateAction;

		public CodeTab(Action newUpdateAction, IHighlightingDefinition highlightingDefinition) {
			InitializeComponent();
			Filename = null;
			Changed = false;
			tabNameUpdateAction = newUpdateAction;

			TextBox.SyntaxHighlighting = highlightingDefinition;
		}

		public CodeTab(string fileName, Action newUpdateAction, IHighlightingDefinition highlightingDefinition)
			: this(newUpdateAction, highlightingDefinition) {
			Filename = fileName;
			Load();
		}

		void Load() {
			try {
				Text = File.ReadAllText(Filename);
			}
			catch {
				throw new ApplicationException("File " + Filename + " could not be read.");
			}
			tabNameUpdateAction();
		}

		public void SaveAs(string filename) {
			Filename = filename;
			
			Save();
		}

		public void Save() {
			try {
				File.WriteAllText(Filename, Text);
			}
			catch {
				throw new ApplicationException("Could not save to file " + Filename + ".");
			}

			Changed = false;
			tabNameUpdateAction();
		}

		void OnTextChanged(object sender, EventArgs e) {
			Changed = true;
			tabNameUpdateAction();
		}
	}
}
