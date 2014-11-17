namespace StackLang.Core {
	public interface IStackObject {
		int Evaluate();
	}

	public class IntObject : IStackObject {
		readonly int value;

		public IntObject(int newValue) {
			value = newValue;
		}
		
		public int Evaluate() {
			return value;
		}
	}

	public interface IVariableObject : IStackObject {
		IStackObject GetStackObjectValue();

		void SetValue(IStackObject stackObject);
	}

	public class RegisterObject : IVariableObject {
		readonly ProgramMemory memory;

		public RegisterObject(ProgramMemory newMemory) {
			memory = newMemory;
		}

		public int Evaluate() {
			return GetStackObjectValue().Evaluate();
		}

		public IStackObject GetStackObjectValue() {
			return memory.Register;
		}

		public void SetValue(IStackObject stackObject) {
			IVariableObject variableObject = stackObject as IVariableObject;
			if (variableObject != null) {
				stackObject = variableObject.GetStackObjectValue();
			}

			memory.Register = stackObject;
		}
	}

	public class MemoryAreaObject : IVariableObject {
		readonly ProgramMemory memory;

		readonly int index;

		public MemoryAreaObject(ProgramMemory newMemory, int newIndex) {
			memory = newMemory;
			index = newIndex;
		}

		public int Evaluate() {
			return GetStackObjectValue().Evaluate();
		}

		public IStackObject GetStackObjectValue() {
			return memory.MemoryArea[index];
		}

		public void SetValue(IStackObject stackObject) {
			IVariableObject variableObject = stackObject as IVariableObject;
			if (variableObject != null) {
				stackObject = variableObject.GetStackObjectValue();
			}

			memory.MemoryArea[index] = stackObject;
		}
	}

	
}