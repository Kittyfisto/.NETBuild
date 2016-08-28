using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Build.DomainModel.MSBuild;
using Build.ExpressionEngine;

namespace Build
{
	/// <summary>
	/// </summary>
	public sealed class Arguments
	{
		private readonly List<string> _ignoreProjectExtensions;
		private readonly List<Property> _properties;
		private readonly List<string> _targets;

		public Arguments()
		{
			_targets = new List<string>();
			_ignoreProjectExtensions = new List<string>();
			_properties = new List<Property>();
			Verbosity = Verbosity.Normal;
			MaxCpuCount = Environment.ProcessorCount;
		}

		public Verbosity Verbosity
		{
			get; set; }

		public List<Property> Properties
		{
			get { return _properties; }
		}

		public List<string> IgnoreProjectExtensions
		{
			get { return _ignoreProjectExtensions; }
		}

		public bool NoAutoResponse { get; set; }

		public bool NoConsoleLogger { get; set; }

		public bool DetailedSummary { get; set; }

		public int MaxCpuCount { get; set; }

		public string InputFile { get; set; }

		public List<string> Targets
		{
			get { return _targets; }
		}

		public bool Help { get; set; }

		public bool NoLogo { get; set; }

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.Append("build ");
			if (InputFile != null)
			{
				builder.Append(InputFile);
				builder.Append(' ');
			}

			if (_properties.Count > 0)
			{
				string properties = string.Join(";", _properties.Select(x => string.Format("{0}={1}", x.Name, x.Value)));
				builder.AppendFormat("/p:{0}", properties);
			}

			return builder.ToString();
		}

		public static Arguments Parse(params string[] args)
		{
			var arguments = new Arguments();
			foreach (string argument in args)
			{
				if (argument.Length > 0 && argument[0] == '/')
				{
					EvaluateSwitch(arguments, argument);
				}
				else
				{
					if (string.IsNullOrEmpty(arguments.InputFile))
					{
						arguments.InputFile = argument;
					}
					else
					{
						throw new ParseException(string.Format("error MSB1008: Only one project can be specified.\r\nSwitch: {0}",
						                                       argument));
					}
				}
			}

			return arguments;
		}

		private static void EvaluateSwitch(Arguments arguments, string argument)
		{
			KeyValuePair<string, string> pair = ParseArgument(argument);
			switch (pair.Key)
			{
				case "/t":
				case "/target":
					arguments._targets.AddRange(ParseList(pair.Value));
					break;

				case "/m":
				case "/maxcpucount":
					arguments.MaxCpuCount = int.Parse(pair.Value);
					break;

				case "/ds":
				case "/detailedsummary":
					arguments.DetailedSummary = true;
					break;

				case "/ignore":
				case "/ignoreprojectextensions":
					arguments._ignoreProjectExtensions.AddRange(ParseList(pair.Value));
					break;

				case "/noautorsp":
				case "/noautoresponse":
					arguments.NoAutoResponse = true;
					break;

				case "/nr":
				case "/nodeReuse":
					break;

				case "/nologo":
					arguments.NoLogo = true;
					break;

				case "/pp":
				case "/preprocess":
					break;

				case "/p":
				case "/property":
					arguments._properties.AddRange(ParseProperties(pair.Value));
					break;

				case "/tv":
				case "/toolsversion":
					break;

				case "/val":
				case "/validate":
					break;

				case "/v":
				case "/verbosity":
					arguments.Verbosity = ParseVerbosity(pair.Value);
					break;

				case "/h":
				case "/help":
					arguments.Help = true;
					break;

				case "/noconlog":
				case "/noconsolelogger":
					arguments.NoConsoleLogger = true;
					break;

				default:
					throw new ParseException(string.Format("error MSB1001: Unknown switch.\r\nSwitch: {0}", pair.Key));
			}
		}

		private static Verbosity ParseVerbosity(string value)
		{
			switch (value)
			{
				case "q":
				case "quiet":
					return Verbosity.Quiet;

				case "m":
				case "minimal":
					return Verbosity.Minimal;

				case "n":
				case "normal":
					return Verbosity.Normal;

				case "d":
				case "detailed":
					return Verbosity.Detailed;

				case "diag":
				case "diagnostic":
					return Verbosity.Diagnostic;

				default:
					throw new ParseException(string.Format("error MSB1018: Verbosity level is not valid.\r\nSwitch: {0}", value));
			}
		}

		private static IEnumerable<Property> ParseProperties(string value)
		{
			List<string> values = ParseList(value);
			var properties = new Property[values.Count];
			for (int i = 0; i < properties.Length; ++i)
			{
				string propertyNameAndValue = values[i];
				string[] tmp = propertyNameAndValue.Split('=');
				properties[i] = new Property(tmp[0], tmp[1]);
			}
			return properties;
		}

		/// <summary>
		/// </summary>
		/// <param name="argument"></param>
		/// <returns></returns>
		private static KeyValuePair<string, string> ParseArgument(string argument)
		{
			int index = argument.IndexOf(':');
			if (index == -1)
			{
				return new KeyValuePair<string, string>(argument, string.Empty);
			}

			string name = argument.Substring(0, index);
			string value = argument.Substring(index + 1);
			return new KeyValuePair<string, string>(name, value);
		}

		/// <summary>
		/// </summary>
		/// <param name="argumentValue"></param>
		/// <returns></returns>
		private static List<string> ParseList(string argumentValue)
		{
			string[] values = argumentValue.Split(';');
			return values.ToList();
		}
	}
}