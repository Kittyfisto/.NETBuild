using Build.DomainModel.MSBuild;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.DomainModel
{
	[TestFixture]
	public sealed class ConditionTest
	{
		[Test]
		public void TestEquality1()
		{
			var condition = new Condition("");
			condition.Equals(condition).Should().BeTrue();
			condition.Equals(new Condition("")).Should().BeTrue();
			condition.Equals(new Condition(" ")).Should().BeFalse();
		}
	}
}