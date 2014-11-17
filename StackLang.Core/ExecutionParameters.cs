using System;

namespace StackLang.Core {
	public class ExecutionParameters {
		public readonly ExecutionStack Stack;

		public int CurrentLine { get; private set; }
		public int CurrentInstruction { get; private set; }
		public ExecutionSource CurrentExecutionSource { get; set; }

		public bool InstructionEscaped { get { return lineEscaped || oneEscaped; } }

		bool lineEscaped;
		bool oneEscaped;
		bool escapedThisTick;

		public IStackObject Register { get; set; }

		readonly int[] lineLengths;

		public ExecutionParameters(int[] newLineLengths) {
			Stack = new ExecutionStack();

			lineLengths = newLineLengths;
			CurrentLine = 0;
			CurrentInstruction = 0;
			CurrentExecutionSource = ExecutionSource.Code;
		}

		public void TickStart() {
			if (CurrentLine >= lineLengths.Length){
				throw new InvalidOperationException("Execution already ended.");
			}
			escapedThisTick = false;
		}

		public void TickEnd() {
			if (CurrentExecutionSource != ExecutionSource.Code) {
				return;
			}
			if (CurrentLine >= lineLengths.Length) {
				throw new InvalidOperationException("Execution already ended.");
			}
			CurrentInstruction++;
			if (CurrentInstruction > lineLengths[CurrentLine]) {
				ChangeLine(CurrentLine + 1);
			}
			if (oneEscaped && !escapedThisTick) {
				oneEscaped = false;
			}
		}

		public void ChangeLine(int newLine) {
			CurrentLine = newLine;
			CurrentInstruction = 0;
			lineEscaped = false;
		}

		public void SetEscaping(Escaping escaping) {
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
	}
}