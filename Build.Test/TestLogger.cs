using System;
using Build.BuildEngine;

namespace Build.Test
{
	public sealed class TestLogger
		: ILogger
	{
		public void WriteLine(Verbosity verbosity, string format, params object[] arguments)
		{
			Console.WriteLine(format, arguments);
		}

		public void WriteMultiLine(Verbosity verbosity, string message)
		{
			var lines = message.Split(new[] {"\n", "\r\n"}, StringSplitOptions.RemoveEmptyEntries);
			foreach (var line in lines)
			{
				Console.WriteLine(line);
			}
		}

		public void WriteWarning(string format, params  object[] arguments)
		{
			throw new NotImplementedException();
		}

		public void WriteError(string format, params  object[] arguments)
		{
			throw new NotImplementedException();
		}
	}
}