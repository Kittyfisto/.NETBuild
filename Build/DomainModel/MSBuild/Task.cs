namespace Build.DomainModel.MSBuild
{
	public abstract class Task
		: Node
	{
		protected Task(Condition condition) : base(condition)
		{
		}
	}
}