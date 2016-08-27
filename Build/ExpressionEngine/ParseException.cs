using System;

namespace Build.ExpressionEngine
{
	public sealed class ParseException
		: Exception
	{
		public ParseException()
		{}

		public ParseException(string message)
			: base(message)
		{
		}
	}
}