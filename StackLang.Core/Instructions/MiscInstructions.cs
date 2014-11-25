using System;
using StackLang.Core.Exceptions;

namespace StackLang.Core.Instructions {
	public class NoOpInstruction : Instruction {
		internal override void Execute(ExecutionParameters parameters) {
		}

		public override string ToString() {
			return ".";
		}
	}

	public class EscapeOneInstruction : Instruction {
		internal override void Execute(ExecutionParameters parameters) {
			parameters.SetEscaping(ExecutionParameters.Escaping.One);
		}

		public override string ToString() {
			return @"\";
		}
	}

	public class EscapeLineInstruction : Instruction {
		internal override void Execute(ExecutionParameters parameters) {
			parameters.SetEscaping(ExecutionParameters.Escaping.Line);
		}

		public override string ToString() {
			return @"\\";
		}
	}

	public class PopInstruction : Instruction {
		internal override void Execute(ExecutionParameters parameters) {
			try {
				if (parameters.Stack.Count != 0) {
					parameters.Stack.Pop();
				}
			}
			catch (IncompleteCodeException ex) {
				throw new CodeException(ex, parameters);
			}
		}

		public override string ToString() {
			return "p";
		}
	}

	public class ReadInstruction : Instruction {
		internal override void Execute(ExecutionParameters parameters) {
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("<<: ");
			Console.ForegroundColor = ConsoleColor.White;
			
			string line = Console.ReadLine();
			if (line == null) {
				throw new CodeException("Received incomplete input", parameters);
			}
			int value;
			if (!int.TryParse(line, out value)) {
				throw new CodeException("Value received not a number", parameters);
			}
			parameters.Stack.Push(new IntObject(value));
		}

		public override string ToString() {
			return "<<";
		}
	}

	public class WriteInstruction : Instruction {
		internal override void Execute(ExecutionParameters parameters) {
			if (parameters.Stack.Count == 0) {
				Console.WriteLine("Stack empty.");
			}
			else {
				Console.WriteLine(parameters.Stack.Pop().GetPrintedValue());
			}
		}

		public override string ToString() {
			return ">>";
		}
	}
}