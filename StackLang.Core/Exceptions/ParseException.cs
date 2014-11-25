using System;

namespace StackLang.Core.Exceptions {
	public class ParseException : Exception {
		public readonly int Line;
		public readonly int Instruction;

		internal ParseException(string message, int newLine, int newInstruction)
			: base(message) {
			Line = newLine;
			Instruction = newInstruction;
		}

		public override string ToString() {
			return Message + "\nAt line " + (Line + 1) + ", instruction " + (Instruction + 1);
		}

	}

	public class IncompleteParseException : ApplicationException {
		readonly int? line;
		readonly int? instruction;

		internal IncompleteParseException(string message)
			: base(message) {
			line = null;
			instruction = null;
		}

		internal IncompleteParseException(IncompleteParseException exception, int? newLine, int? newInstruction)
			: base(exception.Message) {
			line = exception.line ?? newLine;
			instruction = exception.instruction ?? newInstruction;
		}

		internal ParseException GetParseException() {
			if (line == null || instruction == null) {
				throw new ApplicationException("Incomplete parse exception to end of parse chain");
			}
			return new ParseException(Message, line.Value, instruction.Value);
		}
	}
}