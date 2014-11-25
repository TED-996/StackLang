using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

			Console.WriteLine("Execution ended.");
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
			if (snapshot.CurrentExecutionSource == ExecutionParameters.ExecutionSource.Stack) {
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.Write(snapshot.Stack[snapshot.Stack.Count - 1]);
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

			string lineNumber = (snapshot.CurrentLine + 1).ToString(CultureInfo.InvariantCulture) + "|";
			InstructionLine line = executionContext.InstructionCollection[snapshot.CurrentLine];
			string lineString = lineNumber + String.Join(" ", line.InstructionStrings);

			Console.WriteLine(lineString);
			Console.WriteLine(GetCaretString(line, snapshot.CurrentInstruction, lineNumber.Length));
		}

		static string GetCaretString(InstructionLine line, int instruction, int padding) {
			int spaceCount = padding;
			for (int i = 0; i < instruction; i++) {
				spaceCount += line.InstructionStrings[i].Length + 1;
			}
			return new string(' ', spaceCount) + '^';
		}

		public void ToggleBreakpoint(int line) {
			line--;
			if (breakpoints.Contains(line)) {
				breakpoints.Remove(line);
				Console.Write("Breakpoint removed.");
			}
			else {
				breakpoints.Add(line);
				Console.Write("Breakpoint added.");
			}
		}

		public void ListBreakpoints() {
			Console.Write("Breakpoints:");
			foreach (int breakpoint in breakpoints) {
				Console.Write(" " + (breakpoint + 1));
			}
			Console.WriteLine();
		}
	}
}