using System;
using System.Collections.Generic;

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

	public class BinaryOperatorInstruction : Instruction {
		static readonly Dictionary<string, Func<int, int, int>> Operations = new Dictionary<string, Func<int, int, int>> {
			{"+", (a, b) => a + b},
			{"-", (a, b) => a - b},
			{"*", (a, b) => a * b},
			{"/", (a, b) => a / b},
			{"%", (a, b) => a % b},
			{">", (a, b) => a > b ? 1 : 0},
			{">=", (a, b) => a >= b ? 1 : 0},
			{"<", (a, b) => a < b ? 1 : 0},
			{"<=", (a, b) => a <= b ? 1 : 0},
			{"==", (a, b) => a == b ? 1 : 0},
			{"!=", (a, b) => a != b ? 1 : 0},
			{"&&", (a, b) => (a != 0) && (b != 0) ? 1 : 0},
			{"||", (a, b) => (a != 0) || (b != 0) ? 1 : 0},
			{"&", (a, b) => a & b},
			{"|", (a, b) => a | b},
			{"^", (a, b) => a ^ b},
		};

		public static bool IsOperatorDefined(string op) {
			return Operations.ContainsKey(op);
		}

		readonly string operatorAsString;

		public BinaryOperatorInstruction(string newOperatorAsString) {
			operatorAsString = newOperatorAsString;
			if (!IsOperatorDefined(operatorAsString)) {
				throw new ApplicationException("Binary operator doesn't exist; developer bug.");
			}
		}

		public override InstructionExecutability Executability {
			get { return InstructionExecutability.None; }
		}

		public override void Execute(ExecutionParameters parameters) {
			try {
				int value1 = parameters.Stack.Pop().Evaluate();
				int value2 = parameters.Stack.Pop().Evaluate();

				//This is reversed because an earlier operand will be retrieved later than a later operand.
				parameters.Stack.Push(new IntObject(Operations[operatorAsString](value2, value1)));
			}
			catch (IncompleteCodeException ex) {
				throw new CodeException(ex, parameters);
			}
		}
	}

	public class UnaryOperatorInstruction : Instruction {
		static readonly Dictionary<string, Func<int, int>> Operations = new Dictionary<string, Func<int, int>> {
			{"~", v => ~v},
			{"!", v => v == 0 ? 1 : 0}
		};

		public static bool IsOperatorDefined(string op) {
			return Operations.ContainsKey(op);
		}

		readonly string operatorAsString;

		public UnaryOperatorInstruction(string newOperatorAsString) {
			operatorAsString = newOperatorAsString;
			if (!IsOperatorDefined(operatorAsString)) {
				throw new ApplicationException("Binary operator doesn't exist; developer bug.");
			}
		}

		public override InstructionExecutability Executability {
			get { return InstructionExecutability.None; }
		}

		public override void Execute(ExecutionParameters parameters) {
			try {
				int value = parameters.Stack.Pop().Evaluate();

				parameters.Stack.Push(new IntObject(Operations[operatorAsString](value)));
			}
			catch (IncompleteCodeException ex) {
				throw new CodeException(ex, parameters);
			}
		}
	}
}