﻿using System;
using System.Threading;
using StackLang.Core.InputOutput;
using StackLang.Ide.Helpers;

namespace StackLang.Ide.Model {
	public class ExecutionAreaModel : IInputManager, IOutputManager {
		public event EventHandler<LineEventArgs> WriteLineRequest;
		public event EventHandler ClearRequest;

		public event EventHandler AwaitingInput;
		public event EventHandler InputProvided;

		public bool IsAwaitingInput { get; private set; }
		volatile string inputLine;

		public void ProvideInput(string value) {
			if (!IsAwaitingInput) {
				return;
			}
			inputLine = value;
		}

		public void WriteLine(string line) {
			EventHandler<LineEventArgs> ev = WriteLineRequest;
			if (ev != null) {
				ev(this, new LineEventArgs(line));
			}
		}

		public void Clear() {
			EventHandler handler = ClearRequest;
			if (handler != null) {
				handler(this, EventArgs.Empty);
			}
		}

		public string ReadLine() {
			IsAwaitingInput = true;
			inputLine = null;

			EventHandler handler = AwaitingInput;
			if (handler != null) {
				handler(this, EventArgs.Empty);
			}

			while (inputLine == null) {
				Thread.Sleep(20);
			}

			IsAwaitingInput = false;
			handler = InputProvided;
			if (handler != null) {
				handler(this, EventArgs.Empty);
			}

			return inputLine;
		}

		public void Dispose() {
			
		}
	}
}