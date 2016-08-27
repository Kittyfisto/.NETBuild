using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public interface IMetadataGroup
		: IEnumerable<Metadata>
	{
		int Count { get; }
	}
}