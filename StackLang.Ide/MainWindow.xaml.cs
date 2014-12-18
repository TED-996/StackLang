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
using System.Windows.Media.Imaging;
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
		Core.ExecutionContext executionContext;
		DebugPane debugPane;

		readonly IHighlightingDefinition highlightingDefinition;
		const int XSize = 10;

// ReSharper disable MemberCanBePrivate.Global
		public static readonly RoutedCommand RunCommand = new RoutedCommand();
		public static readonly RoutedCommand DebugCommand = new RoutedCommand();
// ReSharper restore MemberCanBePrivate.Global

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
				Width = 20,
				Height = 20,
				Content = new Image {
					Source = (BitmapSource)Resources["RemoveImageSource"],
					MaxHeight = XSize,
					MaxWidth = XSize
				},
				HorizontalAlignment = HorizontalAlignment.Right,
				Style = (Style) Resources["FlatButtonStyle"]
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
			item.MouseDown += (sender, args) => clickAction(args, MouseButton.Middle);

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

			IdeResultArea.Clear();
			OutputArea.Clear();

			if (!SetExecutionContext()) {
				return;
			}

			executionThread = new Thread(RunCode) {IsBackground = true};
			executionThread.Start();
		}

		void RunCode() {
			IdeResultArea.WriteLine("Executing...");
			try {
				while (!executionContext.ExecutionEnded) {
					executionContext.Tick();
				}
				IdeResultArea.WriteLine("Execution ended.");
			}
			catch (CodeException ex) {
				OutputArea.WriteLine(ex.ToString());
			}

			executionThread = null;
		}

		void OnDebugPressed(object sender, RoutedEventArgs e) {
			OnAbortPressed(sender, e);

			if (!SetExecutionContext()) {
				return;
			}

			ActivateDebug();
			debugPane.Draw();
		}

		void OnStepPressed(object sender, RoutedEventArgs e) {
			if (!debugActivated) {
				OnDebugPressed(sender, e);
			}
			if (!debugActivated) {
				return;
			}

			if (executionThread != null && executionThread.IsAlive) {
				OutputArea.WriteLine("Already running.");
				return;
			}

			StepButton.IsEnabled = false;

			executionThread = new Thread(() => {
				executionContext.Tick();
				Dispatcher.Invoke(() => {
					debugPane.Draw();
					StepButton.IsEnabled = true;
				});
				if (executionContext.ExecutionEnded) {
					Dispatcher.Invoke(() => {
						IdeResultArea.WriteLine("Execution ended");
						DeactivateDebug();
					});
				}
			});
			executionThread.Start();
		}

		bool SetExecutionContext() {
			if (EditorTabs.Items.Count == 0) {
				OutputArea.WriteLine("No tab to run.");
				return false;
			}

			byte[] bytes = Encoding.UTF8.GetBytes(GetCurrentTab().Text);
			Parser parser = new Parser(new MemoryStream(bytes));
			try {
				executionContext = new Core.ExecutionContext(parser.Parse(), IdeResultArea, IdeResultArea);
				OutputArea.WriteLine("Code loaded.");
				IdeResultArea.WriteLine("Execution started.");
			}
			catch (ParseException ex) {
				OutputArea.WriteLine(ex.ToString());
				return false;
			}
			return true;
		}

		void ActivateDebug() {
			if (EditorTabs.Items.Count == 0) {
				return;
			}
			CodeTab tab = GetCurrentTab();
			tab.ReadOnly = true;

			DebugColumnDefinition.Width = debugLength;
			DebugSplitter.Visibility = Visibility.Visible;

			debugPane = new DebugPane(executionContext, tab, OutputArea);
			DebugBorder.Child = debugPane;


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
			StepButton.IsEnabled = true;

			if (EditorTabs.Items.Count != 0) {
				GetCurrentTab().ReadOnly = true;
			}

			debugPane = null;
			DebugBorder.Child = null;
		}

		void OnAbortPressed(object sender, RoutedEventArgs e) {
			if (executionThread != null && executionThread.IsAlive) {
				executionThread.Abort();
				IdeResultArea.WriteLine("Execution aborted.");
			}
			executionThread = null;
			DeactivateDebug();
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
	}
}
