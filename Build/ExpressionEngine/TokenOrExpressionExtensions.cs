using System.Collections.Generic;

namespace Build.ExpressionEngine
{
	public static class TokenOrExpressionExtensions
	{
		public static void Trim(this List<TokenOrExpression> that)
		{
			that.Trim(TokenType.Whitespace);
		}

		public static void Trim(this List<TokenOrExpression> that, TokenType trimTypes)
		{
			int start = that.FindFirstNon(trimTypes);
			if (start == -1)
			{
				that.Clear();
				return;
			}

			if (start > 0)
				that.RemoveRange(0, start);

			int end = that.FindLastNon(trimTypes);
			if (end < that.Count - 1)
				that.RemoveRange(end + 1, that.Count - end - 1);
		}

		private static int FindFirstNon(this List<TokenOrExpression> that, TokenType expectedType)
		{
			for (int i = 0; i < that.Count; ++i)
			{
				if (that[i].Token.Type != expectedType)
					return i;
			}

			return -1;
		}

		private static int FindLastNon(this List<TokenOrExpression> that, TokenType expectedType)
		{
			for (int i = that.Count -1; i >= 0; --i)
			{
				if (that[i].Token.Type != expectedType)
					return i;
			}

			return -1;
		}
	}
}