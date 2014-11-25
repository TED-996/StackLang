using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using StackLang.Core;
using StackLang.Core.Collections;
using StackLang.Core.Exceptions;

namespace StackLang.Debugger {
	public class Debugger {
		readonly Stream codeSource;

		ExecutionContext executionContext;

		public bool ExecutionEnded { get { return executionContext.ExecutionEnded; } }

		bool pauseRequested;
		public readonly Object ExecutionLock;

		readonly List<int> breakpoints;
		readonly List<int> watches;
		ExecutionSnapshot snapshot;

		public Debugger(Stream newCodeSource) {
			codeSource = newCodeSource;
			breakpoints = new List<int>();
			watches = new List<int>();

			pauseRequested = false;
			ExecutionLock = new Object();
		}

		public void Load() {
			try {
				executionContext = new ExecutionContext(new Parser(codeSource).Parse());
			}
			catch (ParseException ex) {
				Console.WriteLine(ex);
			}

			Console.WriteLine("Code loaded.");
		}

		public void Snapshot() {
			snapshot = executionContext.Parameters.GetSnapshot();
		}

		public void Step() {
			if (executionContext.ExecutionEnded) {
				Console.WriteLine("Execution ended.");
				return;
			}

			try {
				lock (ExecutionLock) {
					executionContext.Tick();
				}
			}
			catch (CodeException ex) {
				Console.WriteLine(ex);
			}
		}

		public void Continue() {
			if (executionContext.ExecutionEnded) {
				Console.WriteLine("Execution ended.");
				return;
			}

			pauseRequested = false;

			while (!executionContext.ExecutionEnded) {
				if (breakpoints.Contains(executionContext.Parameters.CurrentLine)) {
					Console.WriteLine("Breakpoint hit.");
					return;
				}
				if (pauseRequested) {
					Console.WriteLine("Execution paused.");
					pauseRequested = false;
					return;
				}
				Step();
			}
		}

		public void RequestPause() {
			pauseRequested = true;
		}

		public void Print() {
			if (executionContext.ExecutionEnded) {
				Console.WriteLine("Execution ended.");
				return;
			}

			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine("BEGIN SNAPSHOT:");
			Console.ForegroundColor = ConsoleColor.White;

			Console.WriteLine("Execution Source: " +
				(snapshot.CurrentExecutionSource == ExecutionParameters.ExecutionSource.Stack ? "Stack" : "Code")
				+ "\nEscaped: " + snapshot.InstructionEscaped);
			Console.WriteLine("Register: " + (snapshot.Register == null ?
				"null" : snapshot.Register.GetPrintedValue()));
			Console.WriteLine("Watches: " + string.Join("; ", watches.Select(GetStringFromWatch)));
	
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("STACK:");
			Console.ForegroundColor = ConsoleColor.White;

			int limit = snapshot.CurrentExecutionSource == ExecutionParameters.ExecutionSource.Code
				? snapshot.Stack.Count
				: snapshot.Stack.Count - 1;
			for (int i = 0; i < limit; i++) {
				Console.ForegroundColor = Console.ForegroundColor == ConsoleColor.Gray
					? ConsoleColor.DarkGray
					: ConsoleColor.Gray;
				Console.Write(snapshot.Stack[i] + " ");
			}
			if (limit != snapshot.Stack.Count) {
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write(snapshot.Stack[limit]);
			}

			Console.ForegroundColor = ConsoleColor.White;

			Console.WriteLine();

			if (snapshot.CurrentLine >= executionContext.InstructionCollection.Count) {
				Console.WriteLine("Instruction pointer after code end.");
				return;
			}

			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("CODE LINE:");
			Console.ForegroundColor = ConsoleColor.White;

			Console.Write((snapshot.CurrentLine + 1).ToString(CultureInfo.InvariantCulture) + '|');
			InstructionLine line = executionContext.InstructionCollection[snapshot.CurrentLine];

			for (int i = 0; i < line.Count; i++) {
				if (i == snapshot.CurrentInstruction) {
					Console.ForegroundColor = snapshot.CurrentExecutionSource == ExecutionParameters.ExecutionSource.Code
						? ConsoleColor.Yellow
						: ConsoleColor.DarkYellow;
				}
				else {
					Console.ForegroundColor = ConsoleColor.White;
				}
				Console.Write(line.InstructionStrings[i] + ' ');
			}
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine();

		}

		string GetStringFromWatch(int watch) {
			IStackObject stackObject = snapshot.Memory[watch];
			return "m" + watch + ": " + ((stackObject == null) ? "null" : stackObject.GetPrintedValue());
		}

		public void ToggleBreakpoint(int line) {
			line--;
			if (breakpoints.Contains(line)) {
				breakpoints.Remove(line);
				Console.WriteLine("Breakpoint removed.");
			}
			else {
				breakpoints.Add(line);
				Console.WriteLine("Breakpoint added.");
			}
		}

		public void ListBreakpoints() {
			Console.Write("Breakpoints:");
			foreach (int breakpoint in breakpoints) {
				Console.Write(" " + (breakpoint + 1));
			}
			Console.WriteLine();
		}

		public void ToggleWatch(int address) {
			if (watches.Contains(address)) {
				watches.Remove(address);
				Console.Write("Watch removed");
			}
			else {
				watches.Add(address);
				Console.WriteLine("Watch added");
			}
		}

		public void ListWatches() {
			Console.Write("Watches:");
			foreach (int watch in watches) {
				Console.Write(" " + watch);
			}
			Console.WriteLine();
		}
	}
}