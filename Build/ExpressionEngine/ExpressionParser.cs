using System;
using System.Collections.Generic;

namespace Build.ExpressionEngine
{
	public sealed class ExpressionParser
	{
		private readonly Tokenizer _tokenizer;

		public ExpressionParser()
		{
			_tokenizer = new Tokenizer();
		}

		private bool IsFunctionName(Token token, out FunctionOperation operation)
		{
			switch (token.Value)
			{
				case "Exists":
					operation = FunctionOperation.Exists;
					return true;

				case "HasTrailingSlash":
					operation = FunctionOperation.HasTrailingSlash;
					return true;
			}

			operation = (FunctionOperation) (-1);
			return false;
		}

		private static bool IsBinaryOperator(TokenType type, out BinaryOperation operation)
		{
			switch (type)
			{
				case TokenType.Equals:
					operation = BinaryOperation.Equals;
					return true;

				case TokenType.NotEquals:
					operation = BinaryOperation.EqualsNot;
					return true;
				case TokenType.LessThan:
					operation = BinaryOperation.LessThan;
					return true;

				case TokenType.LessOrEquals:
					operation = BinaryOperation.LessOrEquals;
					return true;

				case TokenType.GreaterThan:
					operation = BinaryOperation.GreaterThan;
					return true;

				case TokenType.GreaterOrEquals:
					operation = BinaryOperation.GreaterOrEquals;
					return true;

				case TokenType.And:
					operation = BinaryOperation.And;
					return true;

				case TokenType.Or:
					operation = BinaryOperation.Or;
					return true;

				default:
					operation = (BinaryOperation) (-1);
					return false;
			}
		}

		private static bool IsOperator(TokenType type)
		{
			BinaryOperation unused1;
			if (IsBinaryOperator(type, out unused1))
				return true;

			UnaryOperation unused2;
			if (IsUnaryOperator(type, out unused2))
				return true;

			return false;
		}

		private static bool IsUnaryOperator(TokenType type, out UnaryOperation operation)
		{
			switch (type)
			{
				case TokenType.Not:
					operation = UnaryOperation.Not;
					return true;

				default:
					operation = (UnaryOperation) (-1);
					return false;
			}
		}

		private static int Precedence(TokenType type)
		{
			switch (type)
			{
				case TokenType.Not:
					return 5;

				case TokenType.LessThan:
				case TokenType.LessOrEquals:
				case TokenType.GreaterThan:
				case TokenType.GreaterOrEquals:
					return 4;

				case TokenType.Equals:
				case TokenType.NotEquals:
					return 3;

				case TokenType.And:
					return 2;

				case TokenType.Or:
					return 1;

				default:
					return 0;
			}
		}

		public IExpression ParseExpression(string expression)
		{
			List<Token> tokens = _tokenizer.Tokenize(expression);
			return Parse(tokens);
		}

		private IExpression Parse(IEnumerable<Token> tokens)
		{
			var stack = new List<TokenOrExpression>();
			int highestPrecedence = 0;
			IEnumerator<Token> iterator = tokens.GetEnumerator();
			while (iterator.MoveNext())
			{
				Token token = iterator.Current;
				if (token.Type == TokenType.Whitespace)
					continue;

				if (IsOperator(token.Type))
				{
					int precedence = Precedence(token.Type);
					if (precedence < highestPrecedence)
					{
						TryParseLeftToRight(stack);
					}

					stack.Add(token);
					highestPrecedence = precedence;
				}
				else if (token.Type == TokenType.Quotation)
				{
					// Consume everything until the quote closes again...
					var content = new List<TokenOrExpression>();
					var arguments = new List<IExpression>();
					while (iterator.MoveNext() &&
					       iterator.Current.Type != TokenType.Quotation)
					{
						content.Add(iterator.Current);
						if (TryParseLeftToRight(content))
						{
							arguments.Add(content[0].Expression);
							content.Clear();
						}
					}

					if (content.Count != 0)
						throw new ParseException();

					stack.Add(new TokenOrExpression(new ConcatExpression(arguments)));
				}
				else
				{
					stack.Add(token);
				}
			}

			if (!TryParseLeftToRight(stack))
				throw new ParseException();

			TokenOrExpression tok = stack[0];
			return tok.Expression;
		}

		public ConcatExpression ParseConcatenation(string expression)
		{
			// This is much simpler since we don't interpret general expressions
			// in string concatenation: We only replace property values..
			List<Token> tokens = _tokenizer.Tokenize(expression);
			tokens = _tokenizer.GroupWhiteSpaceAndLiteral(tokens);
			var stack = new List<TokenOrExpression>(tokens.Count);
			var arguments = new List<IExpression>();
			foreach (Token token in tokens)
			{
				stack.Add(token);
				if (TryParseOne(stack))
				{
					arguments.Add(stack[0].Expression);
					stack.Clear();
				}
			}

			if (stack.Count != 0)
				throw new ParseException();

			return new ConcatExpression(arguments);
		}

		private IExpression Parse(TokenOrExpression tokenOrExpression)
		{
			if (tokenOrExpression.Expression != null)
				return tokenOrExpression.Expression;

			return Parse(tokenOrExpression.Token);
		}

		/// <summary>
		///     Parses an expression from the given stack in left-to-right order.
		///     Operator precedences are ignored.
		/// </summary>
		/// <param name="tokens"></param>
		private bool TryParseLeftToRight(List<TokenOrExpression> tokens)
		{
			int beginTokens;
			do
			{
				beginTokens = tokens.Count;
				if (!TryParseOne(tokens))
					return false;

				if (tokens.Count > beginTokens)
					throw new Exception("Parser encountered an error: There shouldn't be more tokens than we started with!");

			} while (tokens.Count < beginTokens);

			if (tokens.Count == 1 &&
			    tokens[0].Expression != null)
				return true;

			return false;
		}

		private bool TryParseOne(List<TokenOrExpression> tokens)
		{
			if (tokens.Count == 1)
			{
				if (tokens[0].Expression != null)
					return true;

				if (tokens[0].Token.Type == TokenType.Literal)
				{
					tokens[0] = new TokenOrExpression(new Literal(tokens[0].Token.Value));
					return true;
				}
			}

			if (TryParseVariable(tokens))
				return true;

			if (TryParseItemList(tokens))
				return true;

			if (TryParseFunctionCall(tokens))
				return true;

			if (TryParseBinaryOperation(tokens))
				return true;

			if (TryParseUnaryOperation(tokens))
				return true;

			return false;
		}

		private bool TryParseVariable(List<TokenOrExpression> tokens)
		{
			if (tokens.Count >= 4 &&
			    tokens[0].Token.Type == TokenType.Dollar &&
			    tokens[1].Token.Type == TokenType.OpenBracket &&
			    tokens[2].Token.Type == TokenType.Literal &&
			    tokens[3].Token.Type == TokenType.CloseBracket)
			{
				TokenOrExpression name = tokens[2];
				tokens.RemoveRange(0, 4);
				tokens.Insert(0, new TokenOrExpression(new Variable(name.Token.Value)));
				return true;
			}

			return false;
		}

		private bool TryParseItemList(List<TokenOrExpression> tokens)
		{
			if (tokens.Count >= 4 &&
			    tokens[0].Token.Type == TokenType.At &&
			    tokens[1].Token.Type == TokenType.OpenBracket &&
			    tokens[2].Token.Type == TokenType.Literal &&
			    tokens[3].Token.Type == TokenType.CloseBracket)
			{
				TokenOrExpression name = tokens[2];
				tokens.RemoveRange(0, 4);
				tokens.Insert(0, new TokenOrExpression(new ItemList(name.Token.Value)));
				return true;
			}

			return false;
		}

		private bool TryParseFunctionCall(List<TokenOrExpression> tokens)
		{
			FunctionOperation operation;
			if (tokens.Count >= 3 && IsFunctionName(tokens[0].Token, out operation) &&
			    tokens[1].Token.Type == TokenType.OpenBracket &&
			    tokens[tokens.Count - 1].Token.Type == TokenType.CloseBracket)
			{
				// Function call fn(...)
				// to obtain the parameter we need to remove the function name and paranetheses, then
				// parse the sub-expression
				tokens.RemoveRange(0, 2);
				tokens.RemoveAt(tokens.Count - 1);

				TryParseLeftToRight(tokens);
				if (tokens.Count != 1)
					throw new ParseException();

				// Currently all function calls have one parameter
				IExpression parameter = tokens[0].Expression;
				var expression = new FunctionExpression(operation, parameter);
				tokens.RemoveAt(0);
				tokens.Insert(0, new TokenOrExpression(expression));
				return true;
			}

			return false;
		}

		private bool TryParseBinaryOperation(List<TokenOrExpression> tokens)
		{
			BinaryOperation binaryOperation;
			if (tokens.Count >= 3 && IsBinaryOperator(tokens[1].Token.Type, out binaryOperation))
			{
				// Binary expression
				TokenOrExpression leftHandSide = tokens[0];
				tokens.RemoveRange(0, 2);
				TryParseLeftToRight(tokens);
				if (tokens.Count != 1)
					throw new ParseException();

				IExpression rightHandSide = tokens[0].Expression;
				var expression = new BinaryExpression(Parse(leftHandSide), binaryOperation, rightHandSide);
				tokens.RemoveAt(0);
				tokens.Insert(0, new TokenOrExpression(expression));
				return true;
			}

			return false;
		}

		private bool TryParseUnaryOperation(List<TokenOrExpression> tokens)
		{
			UnaryOperation unaryOperation;
			if (tokens.Count >= 2 && IsUnaryOperator(tokens[0].Token.Type, out unaryOperation))
			{
				tokens.RemoveAt(0);
				if (!TryParseLeftToRight(tokens))
					throw new ParseException();
				if (tokens.Count != 1)
					throw new ParseException();

				IExpression rightHandSide = tokens[0].Expression;
				var expression = new UnaryExpression(unaryOperation, rightHandSide);
				tokens.RemoveAt(0);
				tokens.Insert(0, new TokenOrExpression(expression));
				return true;
			}

			return false;
		}

		private IExpression Parse(Token token)
		{
			switch (token.Type)
			{
				case TokenType.Literal:
					return new Literal(token.Value);

				default:
					throw new ParseException(string.Format("Expected token or literal but found: {0}", token));
			}
		}
	}
}