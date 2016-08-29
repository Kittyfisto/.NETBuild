namespace Build.ExpressionEngine
{
	public static class Expression
	{
		public static bool IsTrue(object value)
		{
			if (value == null)
				return false;

			if (value is bool)
			{
				var booleanValue = (bool) value;
				return booleanValue;
			}

			var @string = value.ToString();
			if (string.Equals(@string, "true"))
			{
				return true;
			}

			return false;
		}
	}
}