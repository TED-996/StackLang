using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace StackLang.Ide {
	/// <summary>
	/// Interaction logic for CodeTab.xaml
	/// </summary>
	public partial class CodeTab {
		public string Filename;

		public bool Changed { get; private set; }

		public string TabName {
			get { return (Filename == null ? "Untitled" : Path.GetFileName(Filename)) + (Changed ? "*" : ""); }
		}

		readonly Action tabNameUpdateAction;

		public CodeTab(Action newUpdateAction) {
			InitializeComponent();
			Filename = null;
			Changed = false;
			tabNameUpdateAction = newUpdateAction;
		}

		public CodeTab(string fileName, Action newUpdateAction) : this(newUpdateAction) {
			Filename = fileName;
			Load();
		}

		void Load() {
			try {
				new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd).Text =
					File.ReadAllText(Filename);
			}
			catch {
				throw new ApplicationException("File " + Filename + " could not be read.");
			}
			tabNameUpdateAction();
		}

		void OnLoaded(object sender, RoutedEventArgs e) {
			UpdateLineNumbers();
		}

		void OnTextBoxChanged(object sender, TextChangedEventArgs e) {
			Changed = true;

			UpdateLineNumbers();
			tabNameUpdateAction();
		}

		void UpdateLineNumbers() {
			int lineCount = TextBox.Text.Count(c => c == '\n');
			StringBuilder builder = new StringBuilder();
			for (int i = 1; i <= lineCount + 1; i++) {
				builder.AppendLine(i.ToString(CultureInfo.InvariantCulture));
			}
			LineNumberBlock.Text = builder.ToString();
		}

		public void SaveAs(string filename) {
			Filename = filename;
			
			Save();
		}

		public void Save() {
			try {
				File.WriteAllText(Filename, TextBox.Text);
			}
			catch {
				throw new ApplicationException("Could not save to file " + Filename + ".");
			}

			Changed = false;
			tabNameUpdateAction();
		}

		
	}
}
