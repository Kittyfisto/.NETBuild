using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using Build.DomainModel.MSBuild;

namespace Build.ExpressionEngine
{
	/// <summary>
	///     Responsible for evaluating expressions.
	/// </summary>
	public sealed class ExpressionEngine
	{
		private readonly IFileSystem _fileSystem;
		private readonly ExpressionParser _parser;

		public ExpressionEngine(IFileSystem fileSystem)
		{
			if (fileSystem == null)
				throw new ArgumentNullException("fileSystem");

			_fileSystem = fileSystem;
			_parser =new ExpressionParser();
		}

		/// <summary>
		///     Evaluates the given project using the given environment.
		/// </summary>
		/// <param name="project"></param>
		/// <param name="environment"></param>
		/// <returns>A project that contains only those groups, properties and items that match the current environment</returns>
		public Project Evaluate(Project project, BuildEnvironment environment)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			if (environment == null)
				throw new ArgumentNullException("environment");

			var relativeOrAbsoluteProjectFilePath = project.Filename;
			if (relativeOrAbsoluteProjectFilePath == null)
				throw new ArgumentException("The project is required to have a filename");

			environment.Properties[Properties.MSBuildProjectExtension] = Path.GetExtension(relativeOrAbsoluteProjectFilePath);
			environment.Properties[Properties.MSBuildProjectFile] = Path.GetFilename(relativeOrAbsoluteProjectFilePath);
			environment.Properties[Properties.MSBuildProjectName] = Path.GetFilenameWithoutExtension(relativeOrAbsoluteProjectFilePath);
			environment.Properties[Properties.MSBuildProjectFullPath] = Path.MakeAbsolute(Directory.GetCurrentDirectory(), relativeOrAbsoluteProjectFilePath);
			environment.Properties[Properties.MSBuildProjectDirectory] = Path.GetDirectory(environment.Properties[Properties.MSBuildProjectFullPath]);
			environment.Properties[Properties.MSBuildProjectDirectoryNoRoot] = Path.GetDirectoryWithoutRoot(environment.Properties[Properties.MSBuildProjectFullPath], Slash.Exclude);

			// First we shall evaluate the property groups of the project using the given environment.
			// We can then evaluate all item groups.
			// TODO: Maybe, probably, we need to do this in the order the property- and item groups
			//       appear in the csproject file
			Evaluate(project.Properties, environment);
			Evaluate(project.ItemGroups, environment);

			var evaluated =  new Project
				{
					Filename = project.Filename,
				};

			foreach (var target in project.Targets)
			{
				evaluated.Targets.Add(target);
			}

			return evaluated;
		}

		/// <summary>
		/// Evaluates the given property groups using the given environment and places all evaluated property values
		/// back into the environment.
		/// </summary>
		/// <param name="groups"></param>
		/// <param name="environment"></param>
		/// <returns></returns>
		public void Evaluate(IEnumerable<IPropertyGroup> groups, BuildEnvironment environment)
		{
			foreach (var group in groups)
			{
				Evaluate(group, environment);
			}
		}

		/// <summary>
		/// Evaluates the given property group using the given environment and places all evaluated property values
		/// back into the environment.
		/// </summary>
		/// <param name="group"></param>
		/// <param name="environment"></param>
		/// <returns></returns>
		public void Evaluate(IPropertyGroup group, BuildEnvironment environment)
		{
			if (group.Condition != null && !EvaluateCondition(group.Condition, environment))
				return;

			foreach (var property in group)
			{
				Evaluate(property, environment);
			}
		}

		/// <summary>
		/// Evaluates the given property using the given environment and places
		/// the value back into the environment.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="environment"></param>
		public void Evaluate(Property property, BuildEnvironment environment)
		{
			if (property == null)
				return;

			if (property.Condition != null && !EvaluateCondition(property.Condition, environment))
				return;

			var value = property.Value;
			var expanded = Expand(value, environment);
			environment.Properties.Add(property.Name, expanded);
		}

		public void Evaluate(IEnumerable<ItemGroup> groups, BuildEnvironment environment)
		{
			foreach (var group in groups)
			{
				Evaluate(group, environment);
			}
		}

		public void Evaluate(ItemGroup group, BuildEnvironment environment)
		{
			if (group.Condition != null && !EvaluateCondition(group.Condition, environment))
				return;

			foreach (var item in group)
			{
				Evaluate(item, environment);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="item"></param>
		/// <param name="environment"></param>
		/// <returns>The fully evaluated item(s)</returns>
		/// <remarks>
		/// Returns an enumeration of items where each item represents ONE file only.
		/// </remarks>
		public void Evaluate(ProjectItem item, BuildEnvironment environment)
		{
			if (item.Condition != null && !EvaluateCondition(item.Condition, environment))
				return;

			var items = EvaluateItemList(item.Include, environment);
			foreach (var actualItem in items)
			{
				actualItem.Type = item.Type;
				actualItem[Metadatas.Identity] = item.Include;
			}
			environment.Items.AddRange(items);
		}

		[Pure]
		public string EvaluateExpression(string expression, BuildEnvironment environment)
		{
			var expr = _parser.ParseConcatenation(expression);
			object value = expr.Evaluate(_fileSystem, environment);
			return value != null ? value.ToString() : string.Empty;
		}

		[Pure]
		public string EvaluateConcatenation(string expression, BuildEnvironment environment)
		{
			if (string.IsNullOrWhiteSpace(expression))
				return expression;

			var concatenation = _parser.ParseConcatenation(expression);
			object value = concatenation.Evaluate(_fileSystem, environment);
			return value != null ? value.ToString() : string.Empty;
		}

		[Pure]
		public string Expand(string value, BuildEnvironment environment)
		{
			var evaluated = EvaluateConcatenation(value, environment);
			// TODO: replace properties with values, evaluate wildcards, perform projections, etc...
			return evaluated;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="environment"></param>
		/// <returns></returns>
		[Pure]
		public ProjectItem[] EvaluateItemList(string expression, BuildEnvironment environment)
		{
			var exp = _parser.ParseItemList(expression);
			var files = new List<ProjectItem>();
			exp.ToItemList(_fileSystem, environment, files);
			return files.ToArray();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="environment"></param>
		/// <returns></returns>
		[Pure]
		public bool EvaluateCondition(string expression, BuildEnvironment environment)
		{
			var expr = _parser.ParseCondition(expression);
			if (expr == null) //< empty expression given, this evaluates to true as per MSBuild implementation...
				return true;

			var value = expr.Evaluate(_fileSystem, environment);
			return Expression.CastToBoolean(expr, value);
		}
	}
}