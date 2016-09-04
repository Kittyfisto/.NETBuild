namespace Build.DomainModel.MSBuild
{
	public sealed class ResolveProjectReference
		: Task
	{
		public string ProjectReferences { get; set; }
	}
}