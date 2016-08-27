using System;
using System.Reflection;
using Build.ExpressionEngine;

namespace Build
{
	public static class Application
	{
		public static int Main(string[] args)
		{
			try
			{
				Arguments arguments = Arguments.Parse(args);
				if (!arguments.NoLogo)
					PrintLogo();

				using (var engine = new BuildEngine.BuildEngine(arguments))
				{
					engine.Run();
				}

				return 0;
			}
			catch (ParseException e)
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

		private static void PrintLogo()
		{
			Console.WriteLine("Kittyfisto's .NET Build Engine version {0}", Assembly.GetCallingAssembly().GetName().Version);
			Console.WriteLine("[Microsoft .NET Framework, version {0}]", Environment.Version);
		}
	}
}