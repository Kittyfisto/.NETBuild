using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Xml;
using Build.DomainModel.MSBuild;
using Build.ExpressionEngine;
using Build.IO;

namespace Build.Parser
{
	/// <summary>
	///     Is responsible for parsing the contents visual studio c# projects and creating
	///     <see cref="Project" /> objects, representing them.
	/// </summary>
	public sealed class ProjectParser
		: IFileParser<Project>
	{
		private readonly IFileSystem _fileSystem;

		public ProjectParser(IFileSystem fileSystem)
		{
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");

			_fileSystem = fileSystem;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		[Pure]
		public Project Parse(string filePath)
		{
			using (var stream = _fileSystem.OpenRead(filePath))
			{
				return Parse(stream, filePath);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public Project Parse(Stream stream, string filePath)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");
			if (filePath == null)
				throw new ArgumentNullException("filePath");

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
						case "Import":
							// Not yet supported
							break;

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
						target.Condition = reader.Value;
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

						case "ItemGroup":
							var itemGroup = ReadItemGroup(reader.ReadSubtree());
							target.Children.Add(itemGroup);
							break;

						case "Message":
							target.Children.Add(ReadMessage(innerReader));
							break;

						case "Copy":
							target.Children.Add(ReadCopy(innerReader));
							break;

						case "Csc":
							target.Children.Add(ReadCsc(innerReader));
							break;

						case "Delete":
							target.Children.Add(ReadDelete(innerReader));
							break;

						case "Exec":
							target.Children.Add(ReadExec(innerReader));
							break;

						case "ResolveAssemblyReference":
							target.Children.Add(ReadResolveAssemblyReference(innerReader));
							break;

						case "ResolveProjectReference":
							target.Children.Add(ReadResolveProjectReference(innerReader));
							break;

						case "Output":
							target.Children.Add(ReadOutput(innerReader));
							break;

						default:
							throw new ParseException(string.Format("Unsupported node: '{0}'", innerReader.Name));
					}

					innerReader.Skip();
				}
			}

			return target;
		}

		private static Node ReadResolveProjectReference(XmlReader reader)
		{
			var task = new ResolveProjectReference();
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "Condition":
						task.Condition = reader.Value;
						break;
					case "ProjectReferences":
						task.ProjectReferences = reader.Value;
						break;
					default:
						throw new ParseException(string.Format("Unknown attribute \"{0}\" on ResolveProjectReference element",
															   reader.Name));
				}
			}
			return task;
		}

		private static Node ReadResolveAssemblyReference(XmlReader reader)
		{
			var task = new ResolveAssemblyReference();
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "Condition":
						task.Condition = reader.Value;
						break;
					case "Assemblies":
						task.Assemblies = reader.Value;
						break;
					default:
						throw new ParseException(string.Format("Unknown attribute \"{0}\" on ResolveAssemblyReference element",
															   reader.Name));
				}
			}
			return task;
		}

		private static Node ReadOutput(XmlReader reader)
		{
			var task = new Output();
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "Condition":
						task.Condition = reader.Value;
						break;
					case "TaskParameter":
						task.TaskParameter = reader.Value;
						break;
					case "PropertyName":
						task.PropertyName = reader.Value;
						break;
					default:
						throw new ParseException(string.Format("Unknown attribute \"{0}\" on Output element",
															   reader.Name));
				}
			}
			return task;
		}

		private static Node ReadDelete(XmlReader reader)
		{
			var task = new Delete();
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "Condition":
						task.Condition = reader.Value;
						break;
					case "Files":
						task.Files = reader.Value;
						break;
					default:
						throw new ParseException(string.Format("Unknown attribute \"{0}\" on Delete element",
															   reader.Name));
				}
			}
			return task;
		}

		private static Node ReadExec(XmlReader reader)
		{
			var task = new Exec();
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "Condition":
						task.Condition = reader.Value;
						break;
					case "Command":
						task.Command = reader.Value;
						break;
					case "IgnoreExitCode":
						task.IgnoreExitCode = reader.Value;
						break;
					case "IgnoreStandardErrorWarningFormat":
						task.IgnoreStandardErrorWarningFormat = reader.Value;
						break;
					case "WorkingDirectory":
						task.WorkingDirectory = reader.Value;
						break;
					default:
						throw new ParseException(string.Format("Unknown attribute \"{0}\" on Exec element",
															   reader.Name));
				}
			}

			return task;
		}

		private static Node ReadCsc(XmlReader reader)
		{
			var task = new Csc();
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "Condition":
						task.Condition = reader.Value;
						break;
					case "AllowUnsafeBlocks":
						task.AllowUnsafeBlocks = reader.Value;
						break;
					case "BaseAddress":
						task.BaseAddress = reader.Value;
						break;
					case "CheckForOverflowUnderflow":
						task.CheckForOverflowUnderflow = reader.Value;
						break;
					case "DebugType":
						task.DebugType = reader.Value;
						break;
					case "DefineConstants":
						task.DefineConstants = reader.Value;
						break;
					case "DelaySign":
						task.DelaySign = reader.Value;
						break;
					case "DisabledWarnings":
						task.DisabledWarnings = reader.Value;
						break;
					case "EmitDebugInformation":
						task.EmitDebugInformation = reader.Value;
						break;
					case "ErrorEndLocation":
						task.ErrorEndLocation = reader.Value;
						break;
					case "ErrorReport":
						task.ErrorReport = reader.Value;
						break;
					case "FileAlignment":
						task.FileAlignment = reader.Value;
						break;
					case "HighEntropyVA":
						task.HighEntropyVA = reader.Value;
						break;
					case "MainEntryPoint":
						task.MainEntryPoint = reader.Value;
						break;
					case "ModuleAssemblyName":
						task.ModuleAssemblyName = reader.Value;
						break;
					case "NoConfig":
						task.NoConfig = reader.Value;
						break;
					case "NoLogo":
						task.NoLogo = reader.Value;
						break;
					case "Optimize":
						task.Optimize = reader.Value;
						break;
					case "OutputAssembly":
						task.OutputAssembly = reader.Value;
						break;
					case "PdbFile":
						task.PdbFile = reader.Value;
						break;
					case "Platform":
						task.Platform = reader.Value;
						break;
					case "Prefer32Bit":
						task.Prefer32Bit = reader.Value;
						break;
					case "PreferredUILang":
						task.PreferredUILang = reader.Value;
						break;
					case "References":
						task.References = reader.Value;
						break;
					case "TargetType":
						task.TargetType = reader.Value;
						break;
					case "TreatWarningsAsErrors":
						task.TreatWarningsAsErrors = reader.Value;
						break;
					case "Utf8Output":
						task.Utf8Output = reader.Value;
						break;
					case "WarningLevel":
						task.WarningLevel = reader.Value;
						break;
					case "WarningsAsErrors":
						task.WarningsAsErrors = reader.Value;
						break;
					case "Win32Icon":
						task.Win32Icon = reader.Value;
						break;
					case "Resources":
						task.Resources = reader.Value;
						break;
					case "Sources":
						task.Sources = reader.Value;
						break;
					case "SubsystemVersion":
						task.SubsystemVersion = reader.Value;
						break;
					default:
						throw new ParseException(string.Format("Unknown attribute \"{0}\" on Csc element",
						                                       reader.Name));
				}
			}

			return task;
		}

		private static Copy ReadCopy(XmlReader reader)
		{
			var task = new Copy();
			for (int i = 0; i < reader.AttributeCount; ++i)
			{
				reader.MoveToAttribute(i);
				switch (reader.Name)
				{
					case "Condition":
						task.Condition = reader.Value;
						break;

					case "SourceFiles":
						task.SourceFiles = reader.Value;
						break;

					case "DestinationFiles":
						task.DestinationFiles = reader.Value;
						break;

					default:
						throw new ParseException(string.Format("Unsupported attribute \"{0}\" on Copy element", reader.Name));
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