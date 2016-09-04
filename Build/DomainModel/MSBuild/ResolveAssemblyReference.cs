namespace Build.DomainModel.MSBuild
{
	public sealed class ResolveAssemblyReference
		: Task
	{
		/// <summary>
		///     Specifies the items for which full paths and dependencies must be identified.
		///     These items can have either simple names like "System" or strong names like
		///     "System, Version=2.0.3500.0, Culture=neutral, PublicKeyToken=b77a5c561934e089."
		/// </summary>
		/// <remarks>
		///     Items passed to this parameter may optionally have the following item metadata:
		/// 
		///     - Private: Boolean value. If true, then the item is copied locally. The default value is true.
		/// 
		///     - HintPath: String value. Specifies the path and file name to use as a reference.
		///     This is used when {HintPathFromItem} is specified in the SearchPaths parameter.
		///     The default value is an empty string.
		/// 
		///     - SpecificVersion: Boolean value. If true, then the exact name specified in the Include attribute must match.
		///     If false, then any assembly with the same simple name will work. If SpecificVersion is not specified,
		///     then the task examines the value in the Include attribute of the item.
		///     If the attribute is a simple name, it behaves as if SpecificVersion was false.
		///     If the attribute is a strong name, it behaves as if SpecificVersion was true.
		///     When used with a Reference item type, the Include attribute needs to be the full fusion name
		///     of the assembly to be resolved. The assembly is only resolved if fusion exactly matches the Include attribute.
		///     When a project targets a .NET Framework version and references an assembly compiled for a
		///     higher .NET Framework version, the reference resolves only if it has SpecificVersion set to true.
		///     When a project targets a profile and references an assembly that is not in the profile,
		///     the reference resolves only if it has SpecificVersion set to true.
		/// 
		///     - ExecutableExtension: String value. When present, the resolved assembly must have this extension.
		///     When absent, .dll is considered first, followed by .exe, for each examined directory.
		///     - SubType: String value. Only items with empty SubType metadata will be resolved into full assembly paths.
		///     Items with non-empty SubType metadata are ignored.
		/// 
		///     - AssemblyFolderKey: String value. This metadata is supported for legacy purposes.
		///     It specifies a user-defined registry key, such as "hklm\VendorFolder",
		///     that Assemblies should use to resolve assembly references.
		/// </remarks>
		public string Assemblies { get; set; }
	}
}