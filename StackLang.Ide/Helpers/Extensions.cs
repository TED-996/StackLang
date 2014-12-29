using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace StackLang.Ide.Helpers {
	public static class Extensions {
		public static IEnumerable<T> NotNullCollection<T>(this IEnumerable<T> collection) {
			return collection ?? new T[0];
		}

		public static IEnumerable NotNullCollection(this IEnumerable collection) {
			return collection ?? new object[0];
		}

		public static MemoryStream ToMemoryStream(this string value) {
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			return new MemoryStream(bytes);
		}
	}
}