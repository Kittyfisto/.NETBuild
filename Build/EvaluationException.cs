using System;

namespace Build
{
	public class EvaluationException : BuildException
	{
		public EvaluationException(string message)
			: base(message)
		{
		}

		public EvaluationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}