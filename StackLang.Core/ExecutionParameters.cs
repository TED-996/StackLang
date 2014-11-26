using System;

namespace StackLang.Core {
	public class ExecutionParameters {
		internal readonly ExecutionStack Stack;
		internal readonly ProgramMemory Memory;

		public int CurrentLine { get; private set; }
		public int CurrentInstruction { get; private set; }
		public ExecutionSource CurrentExecutionSource { get; set; }

		ExecutionSource oldExecutionSource;

		internal bool InstructionEscaped { get { return lineEscaped || oneEscaped; } }

		bool lineEscaped;
		bool oneEscaped;
		bool escapedThisTick;

		bool noInstructionIncrement;
		readonly int[] lineLengths;

		internal ExecutionParameters(int[] newLineLengths) {
			Stack = new ExecutionStack();
			Memory = new ProgramMemory();

			lineLengths = newLineLengths;
			CurrentLine = 0;
			CurrentInstruction = 0;
			CurrentExecutionSource = ExecutionSource.Code;
			noInstructionIncrement = false;
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
	}
}