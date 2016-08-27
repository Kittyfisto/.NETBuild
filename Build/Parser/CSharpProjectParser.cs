using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml;
using Build.DomainModel.MSBuild;
using Build.Watchdog;

namespace Build.Parser
{
	/// <summary>
	///     Is responsible for parsing the contents visual studio c# projects and creating
	///     <see cref="CSharpProject" /> objects, representing them.
	/// </summary>
	public sealed class CSharpProjectParser
		: IFileParser<CSharpProject>
	{
		public static readonly CSharpProjectParser Instance;

		static CSharpProjectParser()
		{
			Instance = new CSharpProjectParser();
		}

		private CSharpProjectParser()
		{}

		[Pure]
		public CSharpProject Parse(string filePath)
		{
			using (var stream = File.OpenRead(filePath))
			using (var reader = XmlReader.Create(stream))
			{
				var itemGroups = new List<ItemGroup>();
				var propertyGroups = new List<IPropertyGroup>();

				while (reader.Read())
				{
					if (reader.NodeType == XmlNodeType.Element)
					{
						switch (reader.Name)
						{
							case "Project":
								ReadProject(reader.ReadSubtree(),
									propertyGroups,
									itemGroups);
								break;
						}

						reader.Skip();
					}
				}

				var properties = new PropertyGroups(propertyGroups);
				var items = new ItemGroups(itemGroups);
				var project = new CSharpProject(filePath, properties, items);
				return project;
			}
		}

		private static void ReadProject(XmlReader reader,
			List<IPropertyGroup> propertyGroups,
			List<ItemGroup> itemGroups)
		{
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
								propertyGroups.Add(new PropertyGroup(properties, condition));
							break;

						case "ItemGroup":
							var itemGroup = ReadItemGroup(reader.ReadSubtree());
							itemGroups.Add(itemGroup);
							break;
					}
				}
			}
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