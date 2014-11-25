using StackLang.Core.Exceptions;
using StackLang.Core.Instructions;

namespace StackLang.Core {
	public abstract class Instruction {
		internal virtual bool ForceExecutable { get { return false; } }

		internal abstract void Execute(ExecutionParameters parameters);

		public override string ToString() {
			return GetType().Name;
		}

		internal static Instruction GetInstructionFromString(string str) {
			if (str == ".") {
				return new NoOpInstruction();
			}
			if (str == "k") {
				return new ExecuteCodeInstruction(); 
			}
			if (str.StartsWith("k+") || str.StartsWith("k-")) {
				int amount;
				if (!int.TryParse(str.Substring(1), out amount)) {
					throw new IncompleteParseException("Amount to increment line number not a number.");
				}
				return new IncrementalJumpInstruction(amount);
			}
			if (str.StartsWith("k")) {
				int line;
				if (!int.TryParse(str.Substring(1), out line)) {
					throw new IncompleteParseException("Code reference not a number.");
				}
				return new JumpInstruction(line - 1);
			}
			if (str == ";") {
				return new ExecuteStackInstruction();
			}
			if (str == @"\") {
				return new EscapeOneInstruction();
			}
			if (str == @"\\") {
				return new EscapeLineInstruction();
			}
			if (str == ">>") {
				return new WriteInstruction();
			}
			if (str == "<<") {
				return new ReadInstruction();
			}
			if (str == "p") {
				return new PopInstruction();
			}
			if (str == "=") {
				return new AssignmentInstruction();
			}
			if (str == "if") {
				return new IfInstruction();
			}
			if (str == "r") {
				return new ValueInstruction(new RegisterObject());
			}
			if (str == "m") {
				return new VariableCreatingInstruction(); 
			}
			if (str.StartsWith("m")) {
				int address;
				if (!int.TryParse(str.Substring(1), out address)) {
					throw new IncompleteParseException("Memory reference not a number.");
				}
				return new ValueInstruction(new MemoryAreaObject(address));
			}
			if (BinaryOperatorInstruction.IsOperatorDefined(str)) {
				return new BinaryOperatorInstruction(str);
			}
			if (UnaryOperatorInstruction.IsOperatorDefined(str)) {
				return new UnaryOperatorInstruction(str);
			}

			int intValue;
			if (int.TryParse(str, out intValue)) {
				return new ValueInstruction(new IntObject(intValue));
			}

			throw new IncompleteParseException("Instruction " + str + "doesn't exist.");
		}
	}
}