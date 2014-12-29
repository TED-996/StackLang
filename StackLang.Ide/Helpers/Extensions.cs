using System.Collections;
using System.Collections.Generic;

namespace TestingMvvmLight.Helpers {
	public static class Extensions {
		public static IEnumerable<T> NotNullCollection<T>(this IEnumerable<T> collection) {
			return collection ?? new T[0];
		}

		public static IEnumerable NotNullCollection(this IEnumerable collection) {
			return collection ?? new object[0];
		}
	}
}