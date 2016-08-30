using System;
using System.Collections.Generic;

namespace Build.BuildEngine
{
	public sealed class FilenameComparer : IEqualityComparer<string>
	{
		public bool Equals(string x, string y)
		{
			return string.Equals(x, y, StringComparison.CurrentCultureIgnoreCase);
		}

		public int GetHashCode(string filename)
		{
			return filename.ToLower().GetHashCode();
		}
	}
}