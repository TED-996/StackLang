using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using StackLang.Core;

namespace StackLang.Ide {
	public partial class DebugPane {
		readonly ExecutionContext context;
		ExecutionSnapshot snapshot;

		readonly List<int> watchAddresses;
		const int XSize = 10;

		readonly CodeTab codeTab;
		readonly OutputArea outputArea;

// ReSharper disable MemberCanBePrivate.Global
		public static readonly RoutedCommand NewWatchCommand = new RoutedCommand();
// ReSharper restore MemberCanBePrivate.Global

		public DebugPane(ExecutionContext newContext, CodeTab newTab, OutputArea newOutputArea) {
			InitializeComponent();
			context = newContext;
			outputArea = newOutputArea;
			codeTab = newTab;

			watchAddresses = new List<int>();
		}

		public void Draw() {
			if (context.ExecutionEnded) {
				return;
			}
			snapshot = context.Parameters.GetSnapshot();

			SourceLabel.Content = "Source: " + (snapshot.CurrentExecutionSource == 
				ExecutionParameters.ExecutionSource.Stack ? "stack" : "code");

			StackVieverStackPanel.Children.Clear();
			foreach (string stackElement in snapshot.Stack.Reverse()) {
				StackVieverStackPanel.Children.Add(GetStackElement(stackElement));
			}

			if (snapshot.CurrentExecutionSource == ExecutionParameters.ExecutionSource.Stack) {
				if (StackVieverStackPanel.Children.Count != 0) {
					((TextBlock) ((Border) StackVieverStackPanel.Children[0]).Child).Background = Brushes.Yellow;
				}
				codeTab.SetHighlight(snapshot.CurrentLine + 1, snapshot.CurrentInstruction, true);
			}
			else {
				codeTab.SetHighlight(snapshot.CurrentLine + 1, snapshot.CurrentInstruction, false);
			}

			MemoryViewerStackPanel.Children.Clear();
			MemoryViewerStackPanel.Children.Add(GetStackElement("Register: " +
				(snapshot.Register == null ? "null" : snapshot.Register.GetPrintedValue())));
			foreach (int address in watchAddresses) {
				MemoryViewerStackPanel.Children.Add(GetWatchElement(address));
			}
		}

		UIElement GetStackElement(string value) {
			return new Border {
				BorderThickness = new Thickness(1, 0, 1, 1),
				BorderBrush = Brushes.Silver,
				Child = new TextBlock { Text = value }
			};
		}

		UIElement GetWatchElement(int address) {
			IStackObject memoryObject = snapshot.Memory[address];
			string value = string.Format("m{0}: {1}", address, 
				memoryObject == null ? "null" : memoryObject.GetPrintedValue());
			TextBlock block = new TextBlock {Text = value};
			Button button = new Button {
				Content = new Image {
					Source = (BitmapSource)Resources["RemoveImageSource"],
					MaxHeight = XSize,
					MaxWidth = XSize
				},
				Style = (Style)Resources["FlatButtonStyle"]
			};
			int addressCopy = address;
			button.Click += (sender, args) => {
				watchAddresses.Remove(addressCopy);
				Draw();
			};
			DockPanel.SetDock(block, Dock.Left);
			DockPanel.SetDock(button, Dock.Right);
			return new Border {
				BorderThickness = new Thickness(1, 0, 1, 1),
				BorderBrush = Brushes.Silver,
				Child = new DockPanel {
					Children = {
						button, block
					}
				}
			};
		}

		void OnWatchTextInputPreview(object sender, TextCompositionEventArgs e) {
			int address;
			if (!int.TryParse(NewWatchTextBox.Text, out address)) {
				e.Handled = false;
			}
		}

		void OnNewWatch(object sender, ExecutedRoutedEventArgs e) {
			int address;
			if (!int.TryParse(NewWatchTextBox.Text, out address)) {
				outputArea.WriteLine("Watch address is not a number.");
				return;
			}
			if (address >= context.Parameters.MemorySize) {
				outputArea.WriteLine("Watch address out of bounds (must be between 0 and " + 
					(context.Parameters.MemorySize - 1) + ", inclusive");
				return;
			}
			if (watchAddresses.Contains(address)) {
				outputArea.WriteLine("Watch already exists.");
				return;
			}
			outputArea.WriteLine("Watch " + address + " added.");
			watchAddresses.Add(address);

			NewWatchTextBox.Text = "";
			Draw();
		}
	}
}
