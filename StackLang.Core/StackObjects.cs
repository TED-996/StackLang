using System;
using System.Globalization;
using StackLang.Core.Exceptions;

namespace StackLang.Core {
	public interface IStackObject {
		int Evaluate();

		string GetPrintedValue();
	}

	public class IntObject : IStackObject {
		readonly int value;

		internal IntObject(int newValue) {
			value = newValue;
		}
		
		public int Evaluate() {
			return value;
		}

		public string GetPrintedValue() {
			return value.ToString(CultureInfo.InvariantCulture);
		}

		public override string ToString() {
			return value.ToString(CultureInfo.InvariantCulture);
		}
	}

	public class InstructionObject : IStackObject {
		public readonly Instruction Instruction;

		public InstructionObject(Instruction newInstruction) {
			Instruction = newInstruction;
		}

		public int Evaluate() {
			return 1;
		}

		public string GetPrintedValue() {
			return Instruction.ToString();
		}

		public override string ToString() {
			return Instruction.ToString();
		}
	}

	public interface IVariableObject : IStackObject {
		void SetProgramMemory(ProgramMemory newMemory);

		IStackObject GetStackObjectValue();

		void SetValue(IStackObject stackObject);
	}

	public class RegisterObject : IVariableObject {
		ProgramMemory memory;

		public int Evaluate() {
			IStackObject stackObject = GetStackObjectValue();
			return stackObject == null ? 0 : stackObject.Evaluate();
		}

		public void SetProgramMemory(ProgramMemory newMemory) {
			memory = newMemory;
		}

		public IStackObject GetStackObjectValue() {
			if (memory == null) {
				throw new ApplicationException("Memory not set.");
			}

			return memory.Register;
		}

		public void SetValue(IStackObject stackObject) {
			if (memory == null) {
				throw new ApplicationException("Memory not set.");
			}

			IVariableObject variableObject = stackObject as IVariableObject;
			if (variableObject != null) {
				stackObject = variableObject.GetStackObjectValue();
			}

			memory.Register = stackObject;
		}

		public string GetPrintedValue() {
			if (memory == null) {
				return "r";
			}
			IStackObject stackObject = MemoryAreaObject.SolveReferences(this);
			return stackObject == null ? "null" : stackObject.Evaluate().ToString(CultureInfo.InvariantCulture);
		}

		public override string ToString() {
			return "r: " + GetPrintedValue();
		}
	}

	public class MemoryAreaObject : IVariableObject {
		ProgramMemory memory;

		readonly int index;

		public MemoryAreaObject(int newIndex) {
			index = newIndex;
		}

		public int Evaluate() {
			IStackObject stackObject = GetStackObjectValue();
			return stackObject == null ? 0 : stackObject.Evaluate();
		}

		public void SetProgramMemory(ProgramMemory newMemory) {
			memory = newMemory;
		}

		public IStackObject GetStackObjectValue() {
			if (memory == null) {
				throw new ApplicationException("Memory not set.");
			}
			if (index >= memory.MemoryArea.Length) {
				throw new IncompleteCodeException("Index out of memory.");
			}

			return memory.MemoryArea[index];
		}

		public void SetValue(IStackObject stackObject) {
			if (memory == null) {
				throw new ApplicationException("Memory not set.");
			}

			IVariableObject variableObject = stackObject as IVariableObject;
			if (variableObject != null) {
				stackObject = variableObject.GetStackObjectValue();
			}

			if (index >= memory.MemoryArea.Length) {
				throw new IncompleteCodeException("Index out of memory.");
			}

			memory.MemoryArea[index] = stackObject;
		}

		internal static IStackObject SolveReferences(IStackObject stackObject) {
			while (stackObject != null) {
				IVariableObject objectAsVariableObject = stackObject as IVariableObject;
				if (objectAsVariableObject == null) {
					break;
				}
				stackObject = objectAsVariableObject.GetStackObjectValue();
			}
			return stackObject;
		}

		public string GetPrintedValue() {
			if (memory == null) {
				return "m" + index;
			}
			IStackObject stackObject = SolveReferences(this);
			return stackObject == null ? "null" : stackObject.Evaluate().ToString(CultureInfo.InvariantCulture);
		}

		public override string ToString() {
			return "m" + index + ": " + GetPrintedValue();
		}
	}

	
}