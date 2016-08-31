using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public interface IExpression
	{
		[Pure]
		object Evaluate(IFileSystem fileSystem, BuildEnvironment environment);

		[Pure]
		bool IsTrue(IFileSystem fileSystem, BuildEnvironment environment);

		[Pure]
		string ToString(IFileSystem fileSystem, BuildEnvironment environment);

		void ToItemList(IFileSystem fileSystem,
			BuildEnvironment environment,
			List<ProjectItem> items);
	}
}