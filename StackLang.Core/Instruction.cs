using System;

namespace StackLang.Core {
	public abstract class Instruction {
		public abstract InstructionExecutability Executability { get; }

		public abstract void Execute(ExecutionParameters parameters);

		public static Instruction GetInstructionFromString(string instructionString) {
			throw new NotImplementedException();
		}
	}
}