using System;

namespace StackLang.Core.InputOutput {
	public interface IOutputManager : IDisposable {
		void WriteLine(string line);
	}
}