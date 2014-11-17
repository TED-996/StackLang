using System.Security.AccessControl;

namespace StackLang.Core.Instructions {
	public class JumpInstruction : Instruction {
		readonly int argument;

		public override InstructionExecutability Executability {
			get { return InstructionExecutability.Executable; }
		}

		public JumpInstruction(int newArgument) {
			argument = newArgument;
		}


		public override void Execute(ExecutionParameters parameters) {
			parameters.ChangeLine(argument);
		}
	}

	public class ExecuteStackInstruction : Instruction {
		public override InstructionExecutability Executability {
			get { return InstructionExecutability.ForceExecutable; }
		}

		public override void Execute(ExecutionParameters parameters) {
			parameters.CurrentExecutionSource = ExecutionParameters.ExecutionSource.Stack;
		}
	}

	public class ExecuteCodeInstruction : Instruction
	{
		public override InstructionExecutability Executability
		{
			get { return InstructionExecutability.Executable; }
		}

		public override void Execute(ExecutionParameters parameters)
		{
			parameters.CurrentExecutionSource = ExecutionParameters.ExecutionSource.Code;
		}
	}

	public class IfInstruction : Instruction {
		public override InstructionExecutability Executability {
			get { return InstructionExecutability.Executable; }
		}

		public override void Execute(ExecutionParameters parameters) {
			try {
				if (parameters.Stack.Pop().Evaluate() == 0) {
					parameters.Stack.Pop();
					parameters.Stack.Pop();
				}
			}
			catch (IncompleteCodeException ex) {
				throw new CodeException(ex, parameters);
			}
		}
	}
}