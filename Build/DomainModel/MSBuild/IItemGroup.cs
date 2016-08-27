using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public interface IItemGroup
		: INode
		, IReadOnlyList<ProjectItem>
	{
	}
}