using System;

namespace StackLang.Core.Instructions {
	public class NoOpInstruction : Instruction {
		public override InstructionExecutability Executability {
			get { return InstructionExecutability.None; }
		}

		public override void Execute(ExecutionParameters parameters) {
		}
	}

	public class EscapeOneInstruction : Instruction {
		public override InstructionExecutability Executability {
			get { return InstructionExecutability.Executable; }
		}

		public override void Execute(ExecutionParameters parameters) {
			parameters.SetEscaping(ExecutionParameters.Escaping.One);
		}
	}

	public class EscapeLineInstruction : Instruction {
		public override InstructionExecutability Executability {
			get { return InstructionExecutability.Executable; }
		}

		public override void Execute(ExecutionParameters parameters) {
			parameters.SetEscaping(ExecutionParameters.Escaping.Line);
		}
	}

	public class PopInstruction : Instruction {
		public override InstructionExecutability Executability {
			get { return InstructionExecutability.Executable; }
		}

		public override void Execute(ExecutionParameters parameters) {
			if (parameters.Stack.Count != 0) {
				parameters.Stack.Pop();
			}
		}
	}

	public class ReadInstruction : Instruction {
		public override InstructionExecutability Executability {
			get { return InstructionExecutability.Executable; }
		}

		public override void Execute(ExecutionParameters parameters) {
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
	}

	public class WriteInstruction : Instruction {
		public override InstructionExecutability Executability {
			get { return InstructionExecutability.Executable; }
		}

		public override void Execute(ExecutionParameters parameters) {
			if (parameters.Stack.Count == 0) {
				Console.Write("Stack empty.");
			}
			Console.Write(parameters.Stack.Pop());
		}
	}
}