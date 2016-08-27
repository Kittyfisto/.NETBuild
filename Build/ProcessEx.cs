using System.Diagnostics;

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
				process.WaitForExit();
				output = process.StandardOutput.ReadToEnd();
				return process.ExitCode;
			}
		}
	}
}