using Build.DomainModel.MSBuild;

namespace Build.TaskEngine
{
	internal interface ITaskRunner
	{
		void Run(Node task);
	}
}