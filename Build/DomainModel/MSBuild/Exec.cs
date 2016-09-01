namespace Build.DomainModel.MSBuild
{
	public sealed class Exec
		: Node
	{
		public string Command { get; set; }
		public string IgnoreExitCode { get; set; }
		public string IgnoreStandardErrorWarningFormat { get; set; }
		public string WorkingDirectory { get; set; }
	}
}