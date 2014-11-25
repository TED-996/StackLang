namespace StackLang.Core {
	public class ProgramMemory {
		internal IStackObject Register { get; set; }
		internal readonly IStackObject[] MemoryArea;

		internal ProgramMemory(int memorySize = 1024) {
			MemoryArea = new IStackObject[memorySize];
			Register = null;
		}
	}
}