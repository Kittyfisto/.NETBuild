using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml;
using Build.DomainModel.MSBuild;
using Build.ExpressionEngine;

namespace Build.Parser
{
	/// <summary>
	///     Is responsible for parsing the contents visual studio c# projects and creating
	///     <see cref="Project" /> objects, representing them.
	/// </summary>
	public sealed class CSharpProjectParser
		: IFileParser<Project>
	{
		public static readonly CSharpProjectParser Instance;

		static CSharpProjectParser()
		{
			Instance = new CSharpProjectParser();
		}

		private CSharpProjectParser()
		{}

		[Pure]
		public Project Parse(string filePath)
		{
			using (var stream = File.OpenRead(filePath))
			using (var reader = XmlReader.Create(stream))
			{
				if (!reader.ReadToDescendant("Project"))
					throw new NotImplementedException();

				var project = new Project
					{
						Filename = filePath
					};
				ReadProject(reader.ReadSubtree(), project);
				return project;
			}
		}

		private static void ReadProject(XmlReader reader, Project project)
		{
			reader.MoveToContent();
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					switch (reader.Name)
					{
						case "PropertyGroup":
							var condition = reader.TryReadCondition();
							var properties = ReadPropertyGroup(reader.ReadSubtree());
							if (properties.Count > 0)
								project.Properties.Add(new PropertyGroup(properties, condition));
							break;

						case "ItemGroup":
							var itemGroup = ReadItemGroup(reader.ReadSubtree());
							project.ItemGroups.Add(itemGroup);
							break;

						case "Target":
							var target = ReadTarget(reader);
							project.Targets.Add(target);
							break;
					}

					reader.Skip();
				}
			}
		}

		private static Target ReadTarget(XmlReader reader)
		{
			var target = new Target();

			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "Name":
						target.Name = reader.Value;
						break;

					case "Condition":
						target.Condition = new Condition(reader.Value);
						break;

					case "DependsOnTargets":
						target.DependsOnTargets = reader.Value;
						break;

					case "Inputs":
						target.Inputs = reader.Value;
						break;

					case "Output":
						target.Output = reader.Value;
						break;

					default:
						throw new ParseException(string.Format("Unknown attribute '{0}' on type 'Target'",
							reader.Name));
				}
			}

			reader.MoveToContent();
			var innerReader = reader.ReadSubtree();
			innerReader.MoveToContent();
			while (innerReader.Read())
			{
				if (innerReader.NodeType == XmlNodeType.Element)
				{
					switch (reader.Name)
					{
						case "PropertyGroup":
							var condition = innerReader.TryReadCondition();
							var properties = ReadPropertyGroup(innerReader.ReadSubtree());
							if (properties.Count > 0)
							{
								var group = new PropertyGroup(properties)
									{
										Condition = condition
									};
								target.Children.Add(group);
							}
							break;

						case "Message":
							target.Children.Add(ReadMessage(innerReader));
							break;

						case "Copy":
							target.Children.Add(ReadCopy(innerReader));
							break;

						default:
							throw new ParseException(string.Format("Unsupported node: '{0}'", innerReader.Name));
					}

					innerReader.Skip();
				}
			}

			return target;
		}

		private static Copy ReadCopy(XmlReader reader)
		{
			var task = new Copy();
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "SourceFiles":
						task.SourceFiles = reader.Value;
						break;

					case "DestinationFiles":
						task.DestinationFiles = reader.Value;
						break;

					default:
						throw new ParseException(string.Format("Unsupported attribute: {0}", reader.Name));
				}
			}
			return task;
		}

		private static Message ReadMessage(XmlReader reader)
		{
			var value = reader.GetAttribute("Importance");
			var importance = Importance.Normal;
			switch (value)
			{
				case "High":
					importance = Importance.High;
					break;

				case "Low":
					importance = Importance.Low;
					break;
			}

			var message = new Message
				{
					Condition = reader.TryReadCondition(),
					Importance = importance,
					Text = reader.GetAttribute("Text")
				};
			return message;
		}

		private static ItemGroup ReadItemGroup(XmlReader reader)
		{
			reader.MoveToContent();
			var condition = reader.TryReadCondition();
			var items = new ItemGroup(condition);
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					var include = reader.GetAttribute("Include");
					var condition2 = reader.TryReadCondition();
					var metadata = ReadMetadata(reader.ReadSubtree());
					var item = new ProjectItem(reader.Name,
					                           include, null, null, condition2,
					                           metadata);
					items.Add(item);
					reader.Skip();
				}
			}

			return items;
		}

		private static List<Metadata> ReadMetadata(XmlReader reader)
		{
			reader.MoveToContent();
			List<Metadata> metadatas = null;
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					var name = reader.Name;
					var value = reader.ReadElementContentAsString();
					var metadata = new Metadata(name, value);

					if (metadatas == null)
						metadatas = new List<Metadata>();
					metadatas.Add(metadata);
				}
			}

			return metadatas;
		}

		private static List<Property> ReadPropertyGroup(XmlReader reader)
		{
			reader.MoveToContent();
			var properties = new List<Property>();
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					var name = reader.Name;
					var condition2 = reader.TryReadCondition();
					var value = reader.ReadElementContentAsString();
					var property = new Property(name, value, condition2);

					properties.Add(property);
				}
			}

			return properties;
		}
	}
}