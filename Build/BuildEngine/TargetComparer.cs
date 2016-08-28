using System;
using System.Collections.Generic;
using Build.DomainModel.MSBuild;

namespace Build.BuildEngine
{
	internal sealed class TargetComparer
		: IComparer<string>
	{
		private static readonly string[] Order;

		static TargetComparer()
		{
			Order = new[]
				{
					Targets.Avenge,
					Targets.Clean,
					Targets.Build,
				};
		}

		public int Compare(string x, string y)
		{
			int xIndex = Array.IndexOf(Order, x);
			int yIndex = Array.IndexOf(Order, y);

			return xIndex.CompareTo(yIndex);
		}
	}
}