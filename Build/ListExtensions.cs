using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Build
{
	public static class ListExtensions
	{
		public static List<T> Cut<T>(this List<T> values, int startIndex, int count)
		{
			var ret = Splice(values, startIndex, count);
			values.RemoveRange(startIndex, count);
			return ret;
		}

		[Pure]
		public static List<T> Splice<T>(this List<T> values, int startIndex, int count)
		{
			var ret = new List<T>(count);
			for (int i = 0; i < count; ++i)
			{
				ret.Add(values[i + startIndex]);
			}
			return ret;
		}
	}
}