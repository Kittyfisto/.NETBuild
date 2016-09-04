namespace Build.DomainModel.MSBuild
{
	public sealed class Output
		: Task
	{
		public string TaskParameter { get; set; }
		public string PropertyName { get; set; }
	}
}