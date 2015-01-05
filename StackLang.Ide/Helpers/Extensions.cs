using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StackLang.Ide.Helpers {
	public static class Extensions {
		public static IEnumerable<T> NullToEmptyCollection<T>(this IEnumerable<T> collection) {
			return collection ?? Enumerable.Empty<T>();
		}

		public static IEnumerable NullToEmptyCollection(this IEnumerable collection) {
			return collection ?? Enumerable.Empty<object>();
		}

		public static MemoryStream ToMemoryStream(this string value) {
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			return new MemoryStream(bytes);
		}
	}
}