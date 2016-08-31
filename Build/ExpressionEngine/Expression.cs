using System;
using System.Globalization;

namespace Build.ExpressionEngine
{
	public static class Expression
	{
		public static bool CastToBoolean(IExpression expression, object value)
		{
			if (value is bool)
			{
				var result = (bool)value;
				return result;
			}
			var stringValue = value as string;
			if (stringValue != null)
			{
				if (string.Equals(stringValue, "true", StringComparison.InvariantCultureIgnoreCase))
					return true;
				if (string.Equals(stringValue, "false", StringComparison.InvariantCultureIgnoreCase))
					return false;
			}

			throw new EvaluationException(string.Format("Specified condition \"{0}\" evaluates to \"{1}\" instead of a boolean.",
			                                            expression,
			                                            value));
		}

		public static decimal CastToNumber(IExpression expression, object value)
		{
			try
			{
				var numeric = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
				return numeric;
			}
			catch (FormatException e)
			{
				throw new EvaluationException(
					string.Format("A numeric comparison was attempted on \"{0}\" that evaluates to \"{1}\" instead of a number.",
					              expression,
					              value), e);
			}
			catch (InvalidCastException e)
			{
				throw new EvaluationException(
					string.Format("A numeric comparison was attempted on \"{0}\" that evaluates to \"{1}\" instead of a number.",
								  expression,
								  value), e);
			}
			catch (OverflowException e)
			{
				throw new EvaluationException(
					string.Format("A numeric comparison was attempted on \"{0}\" that evaluates to \"{1}\" instead of a number.",
								  expression,
								  value), e);
			}
		}
	}
}