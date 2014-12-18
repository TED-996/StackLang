using System;
using System.Collections.Generic;
using StackLang.Core.Exceptions;

namespace StackLang.Core.Instructions {
	public class ValueInstruction : Instruction {
		readonly IStackObject stackObject;

		public ValueInstruction(IStackObject newStackObject) {
			stackObject = newStackObject;
		}

		internal override bool ForceExecutable { get { return true; } }

		internal override void Execute(ExecutionParameters parameters) {
			IVariableObject variableObject = stackObject as IVariableObject;
			if (variableObject != null) {
				variableObject.SetProgramMemory(parameters.Memory);
			}

			parameters.Stack.Push(stackObject);
		}

		public override string ToString() {
			return stackObject.ToString();
		}
	}

	public class VariableCreatingInstruction : Instruction {
		internal override void Execute(ExecutionParameters parameters) {
			int value;
			try {
				value = parameters.Stack.Pop().Evaluate();
			}
			catch (IncompleteCodeException ex) {
				throw new CodeException(ex, parameters);
			}
			if (value < 0 || value >= parameters.MemorySize) {
				throw new CodeException("Memory address out of bounds", parameters);
			}
			MemoryAreaObject memoryObject = new MemoryAreaObject(value);
			memoryObject.SetProgramMemory(parameters.Memory);

			parameters.Stack.Push(memoryObject);
		}

		public override string ToString() {
			return "m";
		}
	}

	public class AssignmentInstruction : Instruction {
		internal override void Execute(ExecutionParameters parameters) {
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

		public override string ToString() {
			return "=";
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

		internal override void Execute(ExecutionParameters parameters) {
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

		public override string ToString() {
			return operatorAsString;
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

		internal override void Execute(ExecutionParameters parameters) {
			try {
				int value = parameters.Stack.Pop().Evaluate();

				parameters.Stack.Push(new IntObject(Operations[operatorAsString](value)));
			}
			catch (IncompleteCodeException ex) {
				throw new CodeException(ex, parameters);
			}
		}

		public override string ToString() {
			return operatorAsString;
		}
	}
}