using System;
using System.Collections.Generic;
using System.Text;
using Build.BuildEngine;

namespace Build.ExpressionEngine
{
	public sealed class ConcatExpression
		: IExpression
	{
		public readonly IExpression[] Arguments;

		public ConcatExpression(params IExpression[] arguments)
		{
			if (arguments == null)
				throw new ArgumentNullException("arguments");

			Arguments = arguments;
		}

		public override string ToString()
		{
			return string.Format("'{0}'", string.Join("", (IEnumerable<IExpression>)Arguments));
		}

		private bool Equals(ConcatExpression other)
		{
			if (Arguments.Length != other.Arguments.Length)
				return false;

			for (int i = 0; i < Arguments.Length; ++i)
			{
				var lhs = Arguments[i];
				var rhs = other.Arguments[i];

				if (!Equals(lhs, rhs))
					return false;
			}

			return true;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is ConcatExpression && Equals((ConcatExpression) obj);
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public object Evaluate(BuildEnvironment environment)
		{
			var builder = new StringBuilder();
			for (int i = 0; i < Arguments.Length; ++i)
			{
				var value = Arguments[i].Evaluate(environment);
				builder.Append(value);
			}
			return builder.ToString();
		}
	}
}