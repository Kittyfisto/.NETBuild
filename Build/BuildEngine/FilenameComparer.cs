using System.Collections.Generic;

namespace Build.BuildEngine
{
	public sealed class FilenameComparer : IEqualityComparer<string>
	{
		private static string Normalize(string x)
		{
			return x.ToLower().Replace('/', '\\');
		}

		public bool Equals(string x, string y)
		{
			var left = Normalize(x);
			var right = Normalize(y);

			return string.Equals(left, right);
		}

		public int GetHashCode(string filename)
		{
			return Normalize(filename).GetHashCode();
		}
	}
}