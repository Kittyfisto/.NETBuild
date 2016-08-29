using System.Collections.Generic;

namespace Build.DomainModel.MSBuild
{
	public sealed class Delete
		: Task
	{
		private readonly List<TaskItem> _deletedFiles;
		private readonly List<TaskItem> _files;

		public Delete(Condition condition = null) : base(condition)
		{
			_deletedFiles = new List<TaskItem>();
			_files = new List<TaskItem>();
		}

		public List<TaskItem> DeletedFiles
		{
			get { return _deletedFiles; }
		}

		public List<TaskItem> Files
		{
			get { return _files; }
		}
	}
}