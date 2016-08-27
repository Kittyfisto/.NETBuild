using System.Diagnostics.Contracts;
using Build.BuildEngine;

namespace Build.ExpressionEngine
{
	public interface IExpression
	{
		[Pure]
		object Evaluate(BuildEnvironment environment);
	}
}