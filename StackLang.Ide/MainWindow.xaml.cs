using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using StackLang.Core;
using StackLang.Core.Exceptions;

namespace StackLang.Ide {
	public partial class MainWindow {
		Thread executionThread;

		GridLength debugLength;
		bool debugActivated;

		readonly IHighlightingDefinition highlightingDefinition;

		public MainWindow() {
			InitializeComponent();

			highlightingDefinition = HighlightingLoader.Load(new XmlTextReader(
				new FileStream("StackLangSyntaxHighlighting.xshd", FileMode.Open)), new HighlightingManager());

			AddTab(new CodeTab(UpdateTabNames, highlightingDefinition));
			debugLength = new GridLength(1, GridUnitType.Star);
			debugActivated = false;
		}

		void AddTab(CodeTab codeTab) {
			TabItem item = new TabItem {Content = codeTab};
			CodeTab codeTabCopy = codeTab;

			Action closeAction = () => {
				if (CloseTab(codeTabCopy)) {
					EditorTabs.Items.Remove(item);
				}
			};
			Action<MouseButtonEventArgs, MouseButton> clickAction = (args, button) => {
				if (args.ChangedButton == button && args.ButtonState == MouseButtonState.Pressed) {
					closeAction();
				}
			};

			Button closeButton = new Button {
				Width = 20, Height = 20, Content = "X",
				HorizontalAlignment = HorizontalAlignment.Right,
			};
			closeButton.Click += (sender, args) => closeAction();
			Grid header = new Grid {
				Children = {
					new TextBlock {
						Text = codeTab.TabName, HorizontalAlignment = HorizontalAlignment.Left,
						VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 24, 0)
					},
					closeButton
				}
			};
			header.MouseDown += (sender, args) => clickAction(args, MouseButton.Middle);

			item.Header = header;
			
			EditorTabs.Items.Add(item);
			EditorTabs.SelectedIndex = EditorTabs.Items.Count - 1;
			EditorTabs.Visibility = Visibility.Visible;
		}

		CodeTab GetCurrentTab() {
			TabItem item = (TabItem) EditorTabs.SelectedItem;
			return item.Content as CodeTab;
		}

		IEnumerable<CodeTab> GetCodeTabs() {
			return EditorTabs.Items.Cast<TabItem>().Select(item => (CodeTab) item.Content);
		}

		void OnRunPressed(object sender, RoutedEventArgs e) {
			OnAbortPressed(sender, e);
			if (EditorTabs.Items.Count == 0) {
				OutputArea.WriteLine("No tab to run.");
				return;
			}
			executionThread = new Thread(RunCode) {IsBackground = true};

			IdeResultArea.Clear();
			IdeResultArea.WriteLine("Executing...");
			OutputArea.Clear();

			executionThread.Start(GetCurrentTab().Text);
		}

		void RunCode(object code) {
			byte[] bytes = Encoding.UTF8.GetBytes((string) code);
			Parser parser = new Parser(new MemoryStream(bytes));
			try {
				Core.ExecutionContext context = new Core.ExecutionContext(parser.Parse(), IdeResultArea, IdeResultArea);
				while (!context.ExecutionEnded) {
					context.Tick();
				}
				IdeResultArea.WriteLine("Execution ended.");
			}
			catch (ParseException ex) {
				OutputArea.WriteLine(ex.ToString());
			}
			catch (CodeException ex) {
				OutputArea.WriteLine(ex.ToString());
			}
		}

		void OnDebugPressed(object sender, RoutedEventArgs e) {
			OnAbortPressed(sender, e);
			ActivateDebug();
			OutputArea.WriteLine("Debug not yet implemented.");
		}

		void ActivateDebug() {
			DebugColumnDefinition.Width = debugLength;
			DebugSplitter.Visibility = Visibility.Visible;
			debugActivated = true;
		}

		void DeactivateDebug() {
			if (!debugActivated) {
				return;
			}
			debugLength = DebugColumnDefinition.Width;
			DebugColumnDefinition.Width = new GridLength(0);
			DebugSplitter.Visibility = Visibility.Collapsed;
			debugActivated = false;
		}

		void OnNewPressed(object sender, RoutedEventArgs e) {
			AddTab(new CodeTab(UpdateTabNames, highlightingDefinition));
		}

		void OnOpenPressed(object sender, RoutedEventArgs e) {
			OpenFileDialog dialog = new OpenFileDialog {Filter = "SL Files (*.sl)|*.sl|All Files (*)|*"};
			if (dialog.ShowDialog() == true) {
				try {
					AddTab(new CodeTab(dialog.FileName, UpdateTabNames, highlightingDefinition));
				}
				catch (ApplicationException ex) {
					OutputArea.WriteLine(ex.Message);
				}
			}
		}

		void OnSavePressed(object sender, RoutedEventArgs e) {
			if (EditorTabs.Items.Count == 0) {
				OutputArea.WriteLine("No tab to save.");
				return;
			}
			SaveTab(GetCurrentTab());
		}

		void SaveTab(CodeTab tab) {
			if (tab.Filename == null) {
				SaveAsTab(tab);
			}
			else {
				try {
					tab.Save();
				}
				catch (ApplicationException ex) {
					OutputArea.WriteLine(ex.Message);
				}
			}
		}

		void OnSaveAsPressed(object sender, RoutedEventArgs e) {
			if (EditorTabs.Items.Count == 0) {
				OutputArea.WriteLine("No tab to save.");
				return;
			}
			SaveAsTab(GetCurrentTab());
		}

		void SaveAsTab(CodeTab tab) {
			SaveFileDialog dialog = new SaveFileDialog { Filter = "SL Files (*.sl)|*.sl|All Files (*)|*" };
			if (dialog.ShowDialog() == true) {
				try {
					tab.SaveAs(dialog.FileName);
				}
				catch (ApplicationException ex) {
					OutputArea.WriteLine(ex.Message);
				}
			}
		}

		void UpdateTabNames() {
			foreach (TabItem item in EditorTabs.Items) {
				((TextBlock)(((Grid) item.Header).Children[0])).Text = ((CodeTab)item.Content).TabName;
			}
		}

		void OnClosePressed(object sender, RoutedEventArgs e) {
			if (EditorTabs.Items.Count == 0) {
				OutputArea.WriteLine("No tab to close.");
				return;
			}

			CloseTab(GetCurrentTab());

			EditorTabs.Items.RemoveAt(EditorTabs.SelectedIndex);

			if (EditorTabs.Items.Count == 0) {
				EditorTabs.Visibility = Visibility.Collapsed;
			}
		}

		bool CloseTab(CodeTab tab) {
			if (tab.Changed) {
				MessageBoxResult result = MessageBox.Show("Do you want to save the file you are working on" +
					" before quitting?", "StackLang.Ide", MessageBoxButton.YesNoCancel, MessageBoxImage.Question,
					MessageBoxResult.Yes);
				if (result == MessageBoxResult.Cancel) {
					return false;
				}
				if (result == MessageBoxResult.Yes) {
					SaveTab(tab);
				}
			}
			return true;
		}

		void OnExitPressed(object sender, RoutedEventArgs e) {
			Close();
		}

		void OnAbortPressed(object sender, RoutedEventArgs e) {
			if (executionThread != null) {
				executionThread.Abort();
				IdeResultArea.WriteLine("Execution aborted.");
			}
			executionThread = null;
			DeactivateDebug();
		}

		void OnWindowClosing(object sender, CancelEventArgs e) {
			if (GetCodeTabs().Any(tab => tab.Changed)) {
				MessageBoxResult result = MessageBox.Show("Do you want to save the files you are working on " +
					"before quitting?", "StackLang.Ide", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, 
					MessageBoxResult.Yes);
				if (result == MessageBoxResult.Cancel) {
					e.Cancel = true;
					return;
				}
				if (result == MessageBoxResult.Yes) {
					foreach (CodeTab tab in GetCodeTabs()) {
						SaveTab(tab);
					}
				}
			}
		}

		public static readonly RoutedCommand RunCommand = new RoutedCommand();
		public static readonly RoutedCommand DebugCommand = new RoutedCommand();
	}
}
