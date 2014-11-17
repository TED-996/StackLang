namespace StackLang.Core {
	public class ProgramMemory {
		public IStackObject Register { get; set; }
		public readonly IStackObject[] MemoryArea;

		public ProgramMemory(int memorySize = 1024) {
			MemoryArea = new IStackObject[memorySize];
			Register = null;
		}
	}
}