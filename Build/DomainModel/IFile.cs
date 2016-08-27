using System;

namespace Build.DomainModel
{
	public interface IFile
	{
		DateTime LastModified { get; }
	}
}