using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Build
{
	public sealed class ProcessEx
	{
		public static int Run(string filename,
		                      string workingDirectory,
		                      ArgumentBuilder arguments,
		                      out string output)
		{
			var info = new ProcessStartInfo(filename)
				{
					Arguments = arguments.ToString(),
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					WorkingDirectory = workingDirectory
				};
			using (var process = new Process
				{
					StartInfo = info
				})
			{
				process.Start();

				var stream = process.StandardOutput;
				var task = stream.ReadToEndAsync();
				process.WaitForExit();
				task.Wait();
				output = task.Result;

				return process.ExitCode;
			}
		}
	}
}