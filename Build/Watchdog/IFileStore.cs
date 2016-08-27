namespace Build.Watchdog
{
	public interface IFileStore
	{
		void CreateOrUpdate(string filename);
		void Remove(string filename);
	}
}