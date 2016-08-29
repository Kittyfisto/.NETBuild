namespace Build.DomainModel.MSBuild
{
	public sealed class Error
		: Task
	{
		public string Code { get; set; }
		public string File { get; set; }
		public string HelpKeyword { get; set; }
		public string Text { get; set; }
	}
}