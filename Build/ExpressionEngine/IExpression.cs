using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	public interface IExpression
	{
		[Pure]
		object Evaluate(IFileSystem fileSystem, BuildEnvironment environment);

		[Pure]
		string ToString(IFileSystem fileSystem, BuildEnvironment environment);

		[Pure]
		string ToString(IFileSystem fileSystem, BuildEnvironment environment, ProjectItem item);

		void ToItemList(IFileSystem fileSystem,
			BuildEnvironment environment,
			List<ProjectItem> items);
	}
}