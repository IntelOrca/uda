using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace uda
{
	internal static class EnumerableExtensions
	{
		public static T[] AsArray<T>(this IEnumerable<T> items)
		{
			T[] result = items as T[];
			if (result == null)
				result = items.ToArray();

			return result;
		}

		public static ImmutableArray<T> AsImmutableArray<T>(this IEnumerable<T> items)
		{
			if (items is ImmutableArray<T>)
				return (ImmutableArray<T>)items;

			return items.ToImmutableArray();
		}

		public static IEnumerable<T> ExceptNulls<T>(this IEnumerable<T> items)
		{
			return items.Where(x => x != null);
		}

		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
		{
			return new HashSet<T>(items);
		}
	}
}
