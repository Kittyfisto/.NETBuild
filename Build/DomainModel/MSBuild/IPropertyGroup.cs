using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public interface IPropertyGroup
		: IReadOnlyList<Property>
		, INode
	{
	}
}