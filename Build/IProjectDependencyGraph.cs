using System.Diagnostics.Contracts;
using Build.BuildEngine;

namespace Build
{
	public interface IProjectDependencyGraph
	{
		ProjectAndEnvironment this[string fileName] { get; }

		[Pure]
		ProjectAndEnvironment[] TryGetAll(string[] fileNames);
	}
}