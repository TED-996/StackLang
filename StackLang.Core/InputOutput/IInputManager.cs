using System;

namespace StackLang.Core.InputOutput {
	public interface IInputManager : IDisposable {
		string ReadLine();
	}
}