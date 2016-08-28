using System;

namespace Build
{
	public static class Application
	{
		public static int Main(string[] args)
		{
			try
			{
				Arguments arguments = Arguments.Parse(args);

				using (var engine = new BuildEngine.BuildEngine(arguments))
				{
					engine.Execute();
				}

				return 0;
			}
			catch (BuildException e)
			{
				Console.WriteLine(e.Message);
				return -1;
			}
			catch (Exception e)
			{
				Console.WriteLine("internal error: {0}", e);
				return -2;
			}
		}
	}
}