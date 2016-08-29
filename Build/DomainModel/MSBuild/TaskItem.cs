using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public sealed class TaskItem
	{
		private readonly Dictionary<string, Metadata> _metadata;

		public TaskItem()
		{
			_metadata = new Dictionary<string, Metadata>();
		}

		public IEnumerable<Metadata> Metadata
		{
			get { return _metadata.Values; }
		}
	}
}