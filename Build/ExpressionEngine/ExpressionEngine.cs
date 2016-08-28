using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Build.BuildEngine;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	/// <summary>
	///     Responsible for evaluating expressions.
	/// </summary>
	public sealed class ExpressionEngine
	{
		private readonly Tokenizer _tokenizer;

		public ExpressionEngine()
		{
			_tokenizer = new Tokenizer();
		}

		/// <summary>
		///     Evaluates the given project using the given environment.
		/// </summary>
		/// <param name="project"></param>
		/// <param name="environment"></param>
		/// <returns>A project that contains only those groups, properties and items that match the current environment</returns>
		public CSharpProject Evaluate(CSharpProject project, BuildEnvironment environment)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (environment == null)
				throw new ArgumentNullException("environment");

			var relativeOrAbsoluteProjectFilePath = project.Filename;
			environment[Properties.MSBuildProjectExtension] = Path.GetExtension(relativeOrAbsoluteProjectFilePath);
			environment[Properties.MSBuildProjectFile] = Path.GetFilename(relativeOrAbsoluteProjectFilePath);
			environment[Properties.MSBuildProjectName] = Path.GetFilenameWithoutExtension(relativeOrAbsoluteProjectFilePath);
			environment[Properties.MSBuildProjectFullPath] = Path.MakeAbsolute(Directory.GetCurrentDirectory(), relativeOrAbsoluteProjectFilePath);
			environment[Properties.MSBuildProjectDirectory] = Path.GetDirectory(environment[Properties.MSBuildProjectFullPath]);
			environment[Properties.MSBuildProjectDirectoryNoRoot] = Path.GetDirectoryWithoutRoot(environment[Properties.MSBuildProjectFullPath], Slash.Exclude);

			// First we shall evaluate the property groups of the project using the given environment.
			// We can then evaluate all item groups.
			// TODO: Maybe, probably, we need to do this in the order the property- and item groups
			//       appear in the csproject file
			Evaluate(project.Properties, environment);

			var itemGroups = Evaluate(project.ItemGroups, environment);
			return new CSharpProject(project.Filename,
			                         ReadOnlyPropertyGroups.Instance,
			                         itemGroups);
		}

		/// <summary>
		/// Evaluates the given property groups using the given environment and places all evaluated property values
		/// back into the environment.
		/// </summary>
		/// <param name="groups"></param>
		/// <param name="environment"></param>
		/// <returns></returns>
		public void Evaluate(IPropertyGroups groups, BuildEnvironment environment)
		{
			foreach (var group in groups)
			{
				Evaluate(group, environment);
			}
		}

		/// <summary>
		/// Evaluates the given property group using the given environment and places all evaluated property values
		/// back into the environment.
		/// </summary>
		/// <param name="group"></param>
		/// <param name="environment"></param>
		/// <returns></returns>
		public void Evaluate(IPropertyGroup group, BuildEnvironment environment)
		{
			if (group.Condition != null && !IsTrue(group.Condition, environment))
				return;

			foreach (var property in group)
			{
				Evaluate(property, environment);
			}
		}

		/// <summary>
		/// Evaluates the given property using the given environment and places
		/// the value back into the environment.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="environment"></param>
		public void Evaluate(Property property, BuildEnvironment environment)
		{
			if (property == null)
				return;

			if (property.Condition == null || IsTrue(property.Condition, environment))
			{
				var value = property.Value;
				var expanded = Expand(value, environment);
				environment.Add(property.Name, expanded);
			}
		}

		public IItemGroups Evaluate(IItemGroups groups, BuildEnvironment environment)
		{
			var evaluated = new List<IItemGroup>();

			foreach (var group in groups)
			{
				var evaluatedGroup = Evaluate(group, environment);
				if (evaluatedGroup.Count > 0)
				{
					evaluated.Add(evaluatedGroup);
				}
			}

			return new ItemGroups(evaluated);
		}

		public IItemGroup Evaluate(IItemGroup group, BuildEnvironment environment)
		{
			if (group.Condition != null && !IsTrue(group.Condition, environment))
				return ReadOnlyItemGroup.Instance;

			var evaluated = new List<ProjectItem>();
			foreach (var item in group)
			{
				var evaluatedItems = Evaluate(item, environment);
				evaluated.AddRange(evaluatedItems);
			}

			return new ItemGroup(evaluated);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <param name="environment"></param>
		/// <returns>The fully evaluated item(s)</returns>
		/// <remarks>
		/// Returns an enumeration of items where each item represents ONE file only.
		/// </remarks>
		public IEnumerable<ProjectItem> Evaluate(ProjectItem item, BuildEnvironment environment)
		{
			if (item.Condition != null && !IsTrue(item.Condition, environment))
				return Enumerable.Empty<ProjectItem>();

			var expandedInclude = Expand(item.Include, environment);
			var expandedExclude = Expand(item.Exclude, environment);
			var expandedRemove = Expand(item.Remove, environment);

			// TODO: Split each string into lists of filenames (; as separator) and build a complete list of files represented by this item
			var fullPath = Path.MakeAbsolute(environment[Properties.MSBuildProjectDirectory], expandedInclude);
			var info = new FileInfo(fullPath);
			var metadata = new List<Metadata>
				{
					new Metadata(Metadatas.FullPath, fullPath),
					new Metadata(Metadatas.RootDir, Path.GetRootDir(fullPath)),
					new Metadata(Metadatas.Filename, Path.GetFilenameWithoutExtension(fullPath)),
					new Metadata(Metadatas.Extension, Path.GetExtension(fullPath)),
					new Metadata(Metadatas.RelativeDir, Path.GetRelativeDir(expandedInclude)),
					new Metadata(Metadatas.Directory, Path.GetDirectoryWithoutRoot(fullPath, Slash.Include)),
					new Metadata(Metadatas.Identity, expandedInclude),
					new Metadata(Metadatas.ModifiedTime, FormatTime(info.LastWriteTime)),
					new Metadata(Metadatas.CreatedTime, FormatTime(info.CreationTime)),
					new Metadata(Metadatas.AccessedTime, FormatTime(info.LastAccessTime))
				};

			metadata.AddRange(item.Metadata);

			var evaluated = new ProjectItem(item.Type,
			                         expandedInclude,
			                         expandedExclude,
			                         expandedRemove,
			                         null,
			                         metadata);
			return new[] {evaluated};
		}

		private static string FormatTime(DateTime lastWriteTime)
		{
			var value = lastWriteTime.ToString("yyyy-mm-dd hh:mm:ss.fffffff");
			return value;
		}

		public string EvaluateExpression(string expression, BuildEnvironment environment)
		{
			IExpression exp = Parse(expression);
			object value = exp.Evaluate(environment);
			return value != null ? value.ToString() : string.Empty;
		}

		public string Expand(string value, BuildEnvironment environment)
		{
			// TODO: replace properties with values, evaluate wildcards, perform projections, etc...
			return value;
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
					return 3;

				case TokenType.Equals:
				case TokenType.NotEquals:
				case TokenType.LessThan:
				case TokenType.LessOrEquals:
				case TokenType.GreaterThan:
				case TokenType.GreaterOrEquals:
					return 1;

				default:
					return 0;
			}
		}

		public IExpression Parse(string expression)
		{
			List<Token> tokens = _tokenizer.Tokenize(expression);
			return Parse(tokens);
		}

		private IExpression Parse(IEnumerable<Token> tokens)
		{
			var stack = new List<TokenOrExpression>();
			int highestPrecedence = 0;
			foreach (Token token in tokens)
			{
				if (IsOperator(token.Type))
				{
					int precedence = Precedence(token.Type);
					if (precedence < highestPrecedence)
					{
						ParseExpression(stack);
					}

					stack.Add(token);
					highestPrecedence = precedence;
				}
				else
				{
					stack.Add(token);
				}
			}

			if (stack.Count > 0)
			{
				ParseExpression(stack);
			}
			else
			{
				throw new ParseException("Empty input given");
			}

			if (stack.Count != 1)
			{
				throw new ParseException();
			}
			TokenOrExpression tok = stack[0];
			return tok.Expression;
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
		private void ParseExpression(List<TokenOrExpression> tokens)
		{
			UnaryOperation unaryOperation;
			BinaryOperation binaryOperation;
			if (tokens.Count >= 3 && IsBinaryOperator(tokens[1].Token.Type, out binaryOperation))
			{
				TokenOrExpression leftHandSide = tokens[0];
				tokens.RemoveRange(0, 2);
				ParseExpression(tokens);
				if (tokens.Count != 1)
					throw new ParseException();

				IExpression rightHandSide = tokens[0].Expression;
				var expression = new BinaryExpression(Parse(leftHandSide), binaryOperation, rightHandSide);
				tokens.RemoveAt(0);
				tokens.Insert(0, new TokenOrExpression(expression));
			}
			else if (tokens.Count >= 2 && tokens[0].Token.Type == TokenType.Quotation)
			{
				int endIndex = FindQuote(tokens, 1);
				if (endIndex == -1)
					throw new ParseException("Expected closing quote");

				var arguments = new IExpression[endIndex - 1];
				for (int i = 0; i < arguments.Length; ++i)
				{
					arguments[i] = Parse(tokens[i + 1]);
				}
				var expression = new ConcatExpression(arguments);
				tokens.RemoveRange(0, endIndex + 1);
				tokens.Insert(0, new TokenOrExpression(expression));
				// But we're not done yet!
				// There might be further expressions to the right, hence let's go one deeper
				ParseExpression(tokens);
			}
			else if (tokens.Count >= 2 && IsUnaryOperator(tokens[0].Token.Type, out unaryOperation))
			{
				tokens.RemoveAt(0);
				ParseExpression(tokens);
				if (tokens.Count != 1)
					throw new ParseException();

				IExpression rightHandSide = tokens[0].Expression;
				var expression = new UnaryExpression(unaryOperation, rightHandSide);
				tokens.RemoveAt(0);
				tokens.Insert(0, new TokenOrExpression(expression));
			}
			else if (tokens.Count == 1)
			{
				TokenOrExpression token = tokens[0];
				if (tokens[0].Expression == null)
				{
					tokens[0] = new TokenOrExpression(Parse(token));
				}
				// Otherwise we don't need to do anything because there's already an expression
			}
			else
			{
				throw new ParseException();
			}
		}

		private IExpression Parse(Token token)
		{
			switch (token.Type)
			{
				case TokenType.Variable:
					return new Variable(token.Value);

				case TokenType.Literal:
					return new Literal(token.Value);

				default:
					throw new ParseException(string.Format("Expected token or literal but found: {0}", token));
			}
		}

		private static int FindQuote(List<TokenOrExpression> stack, int startIndex)
		{
			for (int i = startIndex; i < stack.Count; ++i)
			{
				if (stack[i].Token.Type == TokenType.Quotation)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		///     Evaluates the given condition using the currently available variables.
		/// </summary>
		/// <param name="condition"></param>
		/// <param name="environment"></param>
		/// <returns></returns>
		[Pure]
		public bool IsTrue(Condition condition, BuildEnvironment environment)
		{
			IExpression expression = Parse(condition.Expression);
			object result = expression.Evaluate(environment);
			if (result is bool)
			{
				return (bool) result;
			}

			throw new NotImplementedException();
		}
	}
}