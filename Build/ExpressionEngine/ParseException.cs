using System;

namespace Build.ExpressionEngine
{
	public sealed class ParseException
		: BuildException
	{
		public ParseException()
		{}

		public ParseException(string message)
			: base(message)
		{
		}

		public ParseException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}