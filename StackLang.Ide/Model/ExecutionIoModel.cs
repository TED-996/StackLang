using System;
using System.Threading;
using StackLang.Core.InputOutput;

namespace StackLang.Ide.Model {
	public class ExecutionIoModel : IInputManager, IOutputManager {
		public event EventHandler<LineEventArgs> LineWriteRequest;
		public event EventHandler AwaitingInput;

		volatile bool isAwaitingInput;
		volatile string inputLine;

		public void ProvideInput(string value) {
			if (!isAwaitingInput) {
				return;
			}
			inputLine = value;
		}

		public void WriteLine(string line) {
			EventHandler<LineEventArgs> ev = LineWriteRequest;
			if (ev != null) {
				ev(this, new LineEventArgs(line));
			}
		}

		public string ReadLine() {
			isAwaitingInput = true;
			inputLine = null;

			EventHandler handler = AwaitingInput;
			if (handler != null) {
				handler(this, EventArgs.Empty);
			}

			while (inputLine == null) {
				Thread.Sleep(20);
			}

			isAwaitingInput = false;
			return inputLine;
		}

		public void Dispose() {
			
		}
	}

	public class LineEventArgs : EventArgs {
		public readonly string Line;

		public LineEventArgs(string newLine) {
			Line = newLine;
		}
	}
}