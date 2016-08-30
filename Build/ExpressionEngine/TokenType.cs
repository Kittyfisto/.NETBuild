namespace Build.ExpressionEngine
{
	public enum TokenType
	{
		Invalid = 0,

		Whitespace,

		OpenBracket,
		CloseBracket,

		#region Binary Operators
		Equals,
		NotEquals,
		LessThan,
		LessOrEquals,
		GreaterThan,
		GreaterOrEquals,
		And,
		Or,
		#endregion

		#region Unary Operators
		Not,
		HasTrailingSlash,
		#endregion

		Quotation,
		Dollar,
		At,
		Literal,
	}
}