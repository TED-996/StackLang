using System;

namespace StackLang.Core.Instructions {
	public class ValueInstruction : Instruction {
		readonly IStackObject stackObject;

		public ValueInstruction(IStackObject newStackObject) {
			stackObject = newStackObject;
		}

		public override InstructionExecutability Executability {
			get { return InstructionExecutability.None; }
		}

		public override void Execute(ExecutionParameters parameters) {
			parameters.Stack.Push(stackObject);
		}
	}

	public class AssignmentInstruction : Instruction {
		public override InstructionExecutability Executability {
			get { return InstructionExecutability.Executable; }
		}

		public override void Execute(ExecutionParameters parameters) {
			try {
				IVariableObject variable = parameters.Stack.Pop() as IVariableObject;
				IStackObject value = parameters.Stack.Pop();

				if (variable == null) {
					throw new CodeException("Location for assignment is not a variable.", parameters);
				}

				variable.SetValue(value);
			}
			catch (IncompleteCodeException ex) {
				throw new CodeException(ex, parameters);
			}
		}
	}
}