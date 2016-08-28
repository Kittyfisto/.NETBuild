using System;
using Build.ExpressionEngine;
using FluentAssertions;
using NUnit.Framework;

namespace Build.Test
{
	[TestFixture]
	public sealed class ArgumentsTest
	{
		[Test]
		public void TestCtor()
		{
			var arguments = new Arguments();
			arguments.Verbosity.Should().Be(Verbosity.Normal);
			arguments.Properties.Should().NotBeNull();
			arguments.Properties.Should().BeEmpty();
			arguments.Targets.Should().NotBeNull();
			arguments.Targets.Should().BeEmpty();
		}

		[Test]
		public void TestParse1()
		{
			new Action(() => Arguments.Parse("foo.sln", "adwda"))
				.ShouldThrow<ParseException>()
				.WithMessage("error MSB1008: Only one project can be specified.\r\nSwitch: adwda");
		}

		[Test]
		public void TestParse2()
		{
			new Action(() => Arguments.Parse("/v:t"))
				.ShouldThrow<ParseException>()
				.WithMessage("error MSB1018: Verbosity level is not valid.\r\nSwitch: t");
		}

		[Test]
		public void TestParse3()
		{
			new Action(() => Arguments.Parse("/foo"))
				.ShouldThrow<ParseException>()
				.WithMessage("error MSB1001: Unknown switch.\r\nSwitch: /foo");
		}
	}
}