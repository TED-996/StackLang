using StackLang.Core.Exceptions;

namespace StackLang.Core.Instructions {
	public class JumpInstruction : Instruction {
		readonly int argument;

		public JumpInstruction(int newArgument) {
			argument = newArgument;
		}


		internal override void Execute(ExecutionParameters parameters) {
			parameters.ChangeLine(argument);
			parameters.CurrentExecutionSource = ExecutionParameters.ExecutionSource.Code;
		}

		public override string ToString() {
			return "k" + (argument + 1);
		}
	}

	public class ExecuteStackInstruction : Instruction {
		internal override bool ForceExecutable { get { return true; } }

		internal override void Execute(ExecutionParameters parameters) {
			parameters.CurrentExecutionSource = ExecutionParameters.ExecutionSource.Stack;
		}

		public override string ToString() {
			return ";";
		}
	}

	public class ExecuteCodeInstruction : Instruction
	{
		internal override void Execute(ExecutionParameters parameters)
		{
			parameters.CurrentExecutionSource = ExecutionParameters.ExecutionSource.Code;
		}

		public override string ToString() {
			return "k";
		}
	}

	public class IfInstruction : Instruction {
		internal override void Execute(ExecutionParameters parameters) {
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

		public override string ToString() {
			return "if";
		}
	}
}