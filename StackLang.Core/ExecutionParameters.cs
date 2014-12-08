using System;
using System.Linq;
using StackLang.Core.Exceptions;
using StackLang.Core.InputOutput;

namespace StackLang.Core {
	public class ExecutionParameters : IDisposable {
		internal readonly ExecutionStack Stack;
		internal readonly ProgramMemory Memory;

		public int CurrentLine { get; private set; }
		public int CurrentInstruction { get; private set; }
		public ExecutionSource CurrentExecutionSource { get; set; }

		internal readonly IInputManager InputManager;
		internal readonly IOutputManager OutputManager;

		ExecutionSource oldExecutionSource;

		internal bool InstructionEscaped { get { return lineEscaped || oneEscaped; } }

		bool lineEscaped;
		bool oneEscaped;
		bool escapedThisTick;

		bool noInstructionIncrement;
		readonly int[] lineLengths;

		internal ExecutionParameters(int[] newLineLengths, IInputManager newInputManager, IOutputManager newOutputManager) {
			Stack = new ExecutionStack();
			Memory = new ProgramMemory();

			lineLengths = newLineLengths;
			if (lineLengths.All(v => v == 0)) {
				throw new ParseException("Code empty.", 0, 0);
			}

			CurrentLine = 0;
			while (lineLengths[CurrentLine] == 0) {
				CurrentLine++;
			}

			CurrentInstruction = 0;
			CurrentExecutionSource = ExecutionSource.Code;
			noInstructionIncrement = false;

			InputManager = newInputManager;
			OutputManager = newOutputManager;
		}

		internal void TickStart() {
			if (CurrentLine >= lineLengths.Length){
				throw new InvalidOperationException("Execution already ended.");
			}
			escapedThisTick = false;
			oldExecutionSource = CurrentExecutionSource;
			noInstructionIncrement = false;
		}

		internal void TickEnd() {
			if (CurrentExecutionSource == ExecutionSource.Code) {
				while (CurrentLine < lineLengths.Length && lineLengths[CurrentLine] == 0) {
					CurrentLine++;
				}
			}

			if (oldExecutionSource != ExecutionSource.Code) {
				return;
			}
			if (CurrentLine >= lineLengths.Length) {
				throw new InvalidOperationException("Execution already ended.");
			}
			if (!noInstructionIncrement) {
				CurrentInstruction++;
			}
			if (CurrentInstruction >= lineLengths[CurrentLine]) {
				CurrentLine++;
				CurrentInstruction = 0;
				lineEscaped = false;
			}
			if (oneEscaped && !escapedThisTick) {
				oneEscaped = false;
			}

			while (CurrentLine < lineLengths.Length && lineLengths[CurrentLine] == 0) {
				CurrentLine++;
			}
		}

		internal void ChangeLine(int newLine) {
			CurrentLine = newLine;
			CurrentInstruction = 0;
			lineEscaped = false;
			noInstructionIncrement = true;
		}

		internal void SetEscaping(Escaping escaping) {
			if (escaping == Escaping.One) {
				oneEscaped = true;
			}
			else if (escaping == Escaping.Line) {
				lineEscaped = true;
			}
			escapedThisTick = true;
		}

		public enum ExecutionSource {
			Code,
			Stack
		};

		public enum Escaping {
			One,
			Line
		};

		public ExecutionSnapshot GetSnapshot() {
			return new ExecutionSnapshot(this);
		}

		public void Dispose() {
			InputManager.Dispose();
			OutputManager.Dispose();
		}
	}
}