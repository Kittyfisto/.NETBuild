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
	}
}