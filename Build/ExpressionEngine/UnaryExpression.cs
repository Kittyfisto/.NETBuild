using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using Build.DomainModel.MSBuild;
using Build.IO;

namespace Build.ExpressionEngine
{
	public sealed class UnaryExpression
		: IExpression
	{
		public readonly IExpression Expression;
		public readonly UnaryOperation Operation;

		public UnaryExpression(UnaryOperation operation, IExpression expression)
		{
			Operation = operation;
			Expression = expression;
		}

		public override string ToString()
		{
			return string.Format("!{0}", Expression);
		}

		[Pure]
		public object Evaluate(IFileSystem fileSystem, BuildEnvironment environment)
		{
			var value = Expression.Evaluate(fileSystem, environment);
			switch (Operation)
			{
				case UnaryOperation.Not:
					return !Build.ExpressionEngine.Expression.CastToBoolean(this, value);

				default:
					throw new InvalidEnumArgumentException("Operation", (int)Operation, typeof(UnaryOperation));
			}
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment)
		{
			return Evaluate(fileSystem, environment).ToString();
		}

		public string ToString(IFileSystem fileSystem, BuildEnvironment environment, ProjectItem item)
		{
			throw new System.NotImplementedException();
		}

		public void ToItemList(IFileSystem fileSystem, BuildEnvironment environment, List<ProjectItem> items)
		{
			
		}

		private bool Equals(UnaryExpression other)
		{
			return Operation == other.Operation && Expression.Equals(other.Expression);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is UnaryExpression && Equals((UnaryExpression) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int) Operation*397) ^ Expression.GetHashCode();
			}
		}
	}
}