namespace Build.DomainModel.MSBuild
{
	public sealed class Delete
		: Task
	{
		public string DeletedFiles { get; set; }

		public string Files { get; set; }
	}
}