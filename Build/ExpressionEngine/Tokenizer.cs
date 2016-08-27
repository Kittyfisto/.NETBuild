using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace Build.ExpressionEngine
{
	public sealed class Tokenizer
	{
		private readonly Dictionary<TokenType, Regex> _tokenMatchers;

		public Tokenizer()
		{
			const RegexOptions options = RegexOptions.Compiled;
			_tokenMatchers = new Dictionary<TokenType, Regex>
				{
					{TokenType.OpenBracket, new Regex(@"\G\(", options)},
					{TokenType.CloseBracket, new Regex(@"\G\)", options)},
					{TokenType.LessOrEquals, new Regex(@"\G<=", options)},
					{TokenType.GreaterOrEquals, new Regex(@"\G>=", options)},
					{TokenType.GreaterThan, new Regex(@"\G>", options)},
					{TokenType.LessThan, new Regex(@"\G<", options)},
					{TokenType.Equals, new Regex(@"\G==", options)},
					{TokenType.NotEquals, new Regex(@"\G!=", options)},
					{TokenType.Not, new Regex(@"\G!", options)},
					{TokenType.And, new Regex(@"\GAnd", options)},
					{TokenType.Or, new Regex(@"\GOr", options)},
					{TokenType.Quotation, new Regex(@"\G'", options)},
					{TokenType.Variable, new Regex(@"\G\$\(([\word]+)\)", options)},
					{TokenType.Literal, new Regex(@"\G([\word|/\\]+)", options)},
					{TokenType.Whitespace, new Regex(@"\G\s*", options)},
				};
		}

		[Pure]
		public List<Token> Tokenize(string expression)
		{
			var tokens = new List<Token>();
			for (int i = 0; i < expression.Length;)
			{
				Token token;
				if (!Match(ref i, expression, out token))
					throw new ParseException(string.Format("Unable to parse: {0}", expression.Substring(i)));

				if (token.Type == TokenType.Whitespace)
					continue;

				tokens.Add(token);
			}
			return tokens;
		}

		private bool Match(ref int startIndex, string expression, out Token token)
		{
			foreach (var pair in _tokenMatchers)
			{
				var match = pair.Value.Match(expression, startIndex);
				if (match.Success)
				{
					string value;
					if (match.Groups.Count > 1)
						value = match.Groups[1].Value;
					else
						value = null;

					startIndex = match.Index + match.Length;
					token = new Token(pair.Key, value);
					return true;
				}
			}

			token = new Token();
			return false;
		}
	}
}