using System;

namespace Build.DomainModel.MSBuild
{
	public sealed class Condition
	{
		private readonly string _expression;

		public Condition(string expression)
		{
			if (expression == null)
				throw new ArgumentNullException("expression");

			_expression = expression;
		}

		public string Expression
		{
			get { return _expression; }
		}

		private bool Equals(Condition other)
		{
			return string.Equals(_expression, other._expression);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Condition && Equals((Condition) obj);
		}

		public override int GetHashCode()
		{
			return _expression.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("Condition=\"{0}\"", _expression);
		}
	}
}