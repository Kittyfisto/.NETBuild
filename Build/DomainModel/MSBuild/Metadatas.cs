﻿namespace Build.DomainModel.MSBuild
{
	public sealed class Metadatas
	{
		#region Common Metadata (see https://msdn.microsoft.com/en-us/library/ms164313.aspx)

		/// <summary>
		///     Contains the full path of the item. For example:
		///     C:\MyProject\Source\Program.cs
		/// </summary>
		public const string FullPath = "FullPath";

		/// <summary>
		///     Contains the root directory of the item. For example:
		///     C:\
		/// </summary>
		public const string RootDir = "RootDir";

		/// <summary>
		///     Contains the file name of the item, without the extension. For example:
		///     Program
		/// </summary>
		public const string Filename = "Filename";

		/// <summary>
		///     Contains the file name extension of the item. For example:
		///     .cs
		/// </summary>
		public const string Extension = "Extension";

		/// <summary>
		///     Contains the boolean value of whether or not the file exists on file system.
		/// </summary>
		public const string Exists = "Exists";

		/// <summary>
		///     Contains the path specified in the Include attribute, up to the final backslash (\). For example:
		///     Source\
		/// </summary>
		public const string RelativeDir = "RelativeDir";

		/// <summary>
		///     Contains the directory of the item, without the root directory. For example:
		///     MyProject\Source\
		/// </summary>
		public const string Directory = "Directory";

		/// <summary>
		///     If the Include attribute contains the wildcard **, this metadata specifies the part of the path
		///     that replaces the wildcard. For more information on wildcards, see How to: Select the Files to Build.
		///     If the folder C:\MySolution\MyProject\Source\ contains the file Program.cs, and if the project file
		///     contains this item:
		/// </summary>
		/// <ItemGroup>
		///     <MyItem Include="C:\**\Program.cs" />
		/// </ItemGroup>
		/// <remarks>
		///     then the value of %(MyItem.RecursiveDir) would be MySolution\MyProject\Source\.
		/// </remarks>
		public const string RecursiveDir = "RecursiveDir";

		/// <summary>
		///     The item specified in the Include attribute.. For example:
		///     Source\Program.cs
		/// </summary>
		public const string Identity = "Identity";

		/// <summary>
		///     Contains the timestamp from the last time the item was modified. For example:
		///     2004-07-01 00:21:31.5073316
		/// </summary>
		public const string ModifiedTime = "ModifiedTime";

		/// <summary>
		///     Contains the timestamp from when the item was created. For example:
		///     2004-06-25 09:26:45.8237425
		/// </summary>
		public const string CreatedTime = "CreatedTime";

		/// <summary>
		///     Contains the timestamp from the last time the item was accessed.
		///     2004-08-14 16:52:36.3168743
		/// </summary>
		public const string AccessedTime = "AccessedTime";

		/// <summary>
		/// </summary>
		public const string HintPath = "HintPath";

		#endregion
	}
}