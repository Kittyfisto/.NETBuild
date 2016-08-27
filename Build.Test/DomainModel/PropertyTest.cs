using Build.DomainModel.MSBuild;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test.DomainModel
{
	[TestFixture]
	public sealed class PropertyTest
	{
		[Test]
		public void TestEquality1()
		{
			var property = new Property("Foo", "Bar");
			property.Equals(property).Should().BeTrue();
			property.Equals(new Property("Foo", "Bar")).Should().BeTrue();
			property.Equals(new Property("Foo", "bar")).Should().BeFalse();
		}

		[Test]
		public void TestEquality2()
		{
			var property = new Property("Foo", "Bar");
			property.Equals(new Property("foo", "Bar")).Should().BeFalse();
		}

		[Test]
		public void TestEquality3()
		{
			var property = new Property("Foo", "Bar", new Condition(""));
			property.Equals(new Property("Foo", "Bar")).Should().BeFalse();

			property = new Property("Foo", "Bar", new Condition(""));
			property.Equals(new Property("Foo", "Bar", new Condition(""))).Should().BeTrue();
		}
	}
}