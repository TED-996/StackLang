namespace StackLang.Core {
	public class ProgramMemory {
		internal IStackObject Register { get; set; }
		internal readonly IStackObject[] MemoryArea;

		internal ProgramMemory(int memorySize = 1024) {
			IStackObject defaultValue = new IntObject(0);

			MemoryArea = new IStackObject[memorySize];
			for (int i = 0; i < MemoryArea.Length; i++) {
				MemoryArea[i] = defaultValue;
			}

			Register = defaultValue;
		}
	}
}