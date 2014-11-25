using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StackLang.Core {
	internal static class Extensions {
		internal static IEnumerable<string> ReadAllLines(this Stream stream, Encoding encoding) {
			using (StreamReader reader = new StreamReader(stream, encoding)) {
				string line = reader.ReadLine();
				while (line != null) {
					yield return line;
					line = reader.ReadLine();
				}
			}
		}

		internal static IEnumerable<string> ReadAllLines(this Stream stream) {
			return ReadAllLines(stream, Encoding.UTF8);
		} 
	}
}