namespace Build.DomainModel.MSBuild
{
	public static class Properties
	{
		#region Reserved MSBuild Properties (see https://msdn.microsoft.com/en-us/library/ms164309.aspx)

		/// <summary>
		/// The absolute path of the folder where the MSBuild binaries that are currently being used are 
		/// located (for example, C:\Windows\Microsoft.Net\Framework\versionNumber).
		/// This property is useful if you have to refer to files in the MSBuild directory.
		/// </summary>
		/// <remarks>
		/// Do not include the final backslash on this property.
		/// </remarks>
		public const string MSBuildBinPath = "MSBuildBinPath";

		/// <summary>
		/// Introduced in the .NET Framework 4: there is no difference between the default values of
		/// MSBuildExtensionsPath and MSBuildExtensionsPath32. You can set the environment variable
		/// MSBUILDLEGACYEXTENSIONSPATH to a non-null value to enable the behavior of the default value
		/// of MSBuildExtensionsPath in earlier versions.
		/// In the .NET Framework 3.5 and earlier, the default value of MSBuildExtensionsPath points to the
		/// path of the MSBuild subfolder under the \Program Files\ or \Program Files (x86) folder,
		/// depending on the bitness of the current process. For example, for a 32-bit process on a 64-bit machine,
		/// this property points to the \Program Files (x86) folder. For a 64-bit process on a 64-bit machine,
		/// this property points to the \Program Files folder.
		/// Do not include the final backslash on this property.
		/// This location is a useful place to put custom target files. For example, your target files could be
		/// installed at \Program Files\MSBuild\MyFiles\Northwind.targets and then imported in project files
		/// by using this XML code:
		/// 'Import Project="$(MSBuildExtensionsPath)\MyFiles\Northwind.targets"/'
		/// </summary>
		public const string MSBuildExtensionsPath = "MSBuildExtensionsPath";

		/// <summary>
		/// The path of the MSBuild subfolder under the \Program Files or \Program Files (x86) folder.
		/// This path always points to the 32-bit \Program Files folder on a 32-bit machine and
		/// \Program Files (x86) on a 64-bit machine. See also MSBuildExtensionsPath and MSBuildExtensionsPath64.
		/// </summary>
		/// <remarks>
		/// Do not include the final backslash on this property.
		/// </remarks>
		public const string MSBuildExtensionsPath32 = "MSBuildExtensionsPath32";

		/// <summary>
		/// The path of the MSBuild subfolder under the \Program Files folder.
		/// For a 64-bit machine, this path always points to the \Program Files folder.
		/// For a 32-bit machine, this path is blank.
		/// See also MSBuildExtensionsPath and MSBuildExtensionsPath32.
		/// </summary>
		/// <remarks>
		/// Do not include the final backslash on this property.
		/// </remarks>
		public const string MSBuildExtensionsPath64 = "MSBuildExtensionsPath64";

		/// <summary>
		/// true if the previous task completed without any errors (even if there were warnings),
		/// or false if the previous task had errors. Typically, when an error occurs in a task, 
		/// the error is the last thing that happens in that project. Therefore, the value of this
		/// property is never false, except in these scenarios:
		/// When the ContinueOnError attribute of the Task Element (MSBuild) is set to WarnAndContinue
		/// (or true) or ErrorAndContinue.
		/// When the Target has an OnError Element (MSBuild) as a child element.
		/// </summary>
		public const string MSBuildLastTaskResult = "MSBuildLastTaskResult";

		/// <summary>
		/// The maximum number of concurrent processes that are used when building.
		/// This is the value that you specified for /maxcpucount on the command line.
		/// If you specified /maxcpucount without specifying a value, then MSBuildNodeCount
		/// specifies the number of processors in the computer. For more information,
		/// see MSBuild Command-Line Reference and Building Multiple Projects in Parallel with MSBuild.
		/// </summary>
		public const string MSBuildNodeCount = "MSBuildNodeCount";

		/// <summary>
		/// The location of the 32-bit program folder; for example, C:\Program Files (x86).
		/// </summary>
		/// <remarks>
		/// Do not include the final backslash on this property.
		/// </remarks>
		public const string MSBuildProgramFiles32 = "MSBuildProgramFiles32";

		/// <summary>
		/// The complete list of targets that are specified in the DefaultTargets attribute of the Project element.
		/// For example, the following Project element would have an MSBuildDefaultTargets property value of A;B;C:
		/// 'Project DefaultTargets="A;B;C" '
		/// </summary>
		public const string MSBuildProjectDefaultTargets = "MSBuildProjectDefaultTargets";

		/// <summary>
		/// The absolute path of the directory where the project file is located, for example C:\MyCompany\MyProduct.
		/// </summary>
		/// <remarks>
		/// Do not include the final backslash on this property.
		/// </remarks>
		public const string MSBuildProjectDirectory = "MSBuildProjectDirectory";

		/// <summary>
		/// The value of the MSBuildProjectDirectory property, excluding the root drive.
		/// </summary>
		/// <remarks>
		/// Do not include the final backslash on this property.
		/// </remarks>
		public const string MSBuildProjectDirectoryNoRoot = "MSBuildProjectDirectoryNoRoot";

		/// <summary>
		/// The file name extension of the project file, including the period; for example, .proj.
		/// </summary>
		public const string MSBuildProjectExtension = "MSBuildProjectExtension";

		/// <summary>
		/// The complete file name of the project file, including the file name extension; for example, MyApp.proj.
		/// </summary>
		public const string MSBuildProjectFile = "MSBuildProjectFile";

		/// <summary>
		/// The absolute path and complete file name of the project file, including the file name extension;
		/// for example, C:\MyCompany\MyProduct\MyApp.proj.
		/// </summary>
		public const string MSBuildProjectFullPath = "MSBuildProjectFullPath";

		/// <summary>
		/// The file name of the project file without the file name extension; for example, MyApp.
		/// </summary>
		public const string MSBuildProjectName = "MSBuildProjectName";

		/// <summary>
		/// The absolute path of the folder where MSBuild is called. By using this property,
		/// you can build everything below a specific point in a project tree without creating dirs.proj
		/// files in every directory. Instead, you have just one project—for example, c:\traversal.proj,
		/// as shown here:
		/// </summary>
		public const string MSBuildStartupDirectory = "MSBuildStartupDirectory";

		/// <summary>
		/// The file name and file extension portion of MSBuildThisFileFullPath.
		/// </summary>
		public const string MSBuildThisFile = "MSBuildThisFile";

		/// <summary>
		/// The directory portion of MSBuildThisFileFullPath.
		/// </summary>
		/// <remarks>
		/// Include the final backslash in the path.
		/// </remarks>
		public const string MSBuildThisFileDirectory = "MSBuildThisFileDirectory";

		/// <summary>
		/// The directory portion of MSBuildThisFileFullPath, excluding the root drive.
		/// </summary>
		/// <remarks>
		/// Include the final backslash in the path.
		/// </remarks>
		public const string MSBuildThisFileDirectoryNoRoot = "MSBuildThisFileDirectoryNoRoot";

		/// <summary>
		/// The file name extension portion of MSBuildThisFileFullPath.
		/// </summary>
		public const string MSBuildThisFileExtension = "MSBuildThisFileExtension";

		/// <summary>
		/// The absolute path of the project or targets file that contains the target that is running.
		/// </summary>
		/// <remarks>
		/// You can specify a relative path in a targets file that's relative to the targets file and not relative to the original project file.
		/// </remarks>
		public const string MSBuildThisFileFullPath = "MSBuildThisFileFullPath";

		/// <summary>
		/// The file name portion of MSBuildThisFileFullPath, without the file name extension.
		/// </summary>
		public const string MSBuildThisFileName = "MSBuildThisFileName";

		/// <summary>
		/// The installation path of the MSBuild version that's associated with the value of MSBuildToolsVersion.
		/// This property cannot be overridden.
		/// </summary>
		/// <remarks>
		/// Do not include the final backslash in the path.
		/// </remarks>
		public const string MSBuildToolsPath = "MSBuildToolsPath";

		/// <summary>
		/// The version of the MSBuild Toolset that is used to build the project.
		/// </summary>
		/// <remarks>
		/// An MSBuild Toolset consists of tasks, targets, and tools that are used to build an application.
		/// The tools include compilers such as csc.exe and vbc.exe. For more information,
		/// see MSBuild Toolset (ToolsVersion), and Standard and Custom Toolset Configurations.
		/// </remarks>
		public const string MSBuildToolsVersion = "MSBuildToolsVersion";

		#endregion

		#region Common Properties

		/// <summary>
		/// Specifies additional folders in which compilers should look for reference assemblies.
		/// </summary>
		public const string AdditionalLibPaths = "AdditionalLibPaths";

		/// <summary>
		/// Causes the compiler to make all type information from the specified files available to the project you are compiling.
		/// This property is equivalent to the /addModules compiler switch.
		/// </summary>
		public const string AddModules = "AddModules";

		/// <summary>
		/// The path where AL.exe can be found. This property overrides the current version of AL.exe to enable use of a different version.
		/// </summary>
		public const string ALToolPath = "ALToolPath";

		/// <summary>
		/// The .ico icon file to pass to the compiler for embedding as a Win32 icon. The property is equivalent to the /win32icon compiler switch.
		/// </summary>
		public const string ApplicationIcon = "ApplicationIcon";

		/// <summary>
		/// Specifies the path of the file that is used to generate external User Account Control (UAC) manifest information. Applies only to Visual Studio projects targeting Windows Vista.
		/// In most cases, the manifest is embedded. However, if you use Registration Free COM or ClickOnce deployment,
		/// then the manifest can be an external file that is installed together with your application assemblies.
		/// For more information, see the NoWin32Manifest property in this topic.
		/// </summary>
		public const string ApplicationManifest = "ApplicationManifest";

		/// <summary>
		/// Specifies the file that's used to sign the assembly (.snk or .pfx) and that's passed to the ResolveKeySource Task to generate the actual key that's used to sign the assembly.
		/// </summary>
		public const string AssemblyOriginatorKeyFile = "AssemblyOriginatorKeyFile";

		/// <summary>
		/// A list of locations to search during build-time reference assembly resolution. The order in which paths appear in this list is meaningful because paths listed earlier takes precedence over later entries.
		/// </summary>
		public const string AssemblySearchPaths = "AssemblySearchPaths";

		/// <summary>
		/// The name of the final output assembly after the project is built.
		/// </summary>
		public const string AssemblyName = "AssemblyName";

		/// <summary>
		/// Specifies the base address of the main output assembly. This property is equivalent to the /baseaddress compiler switch.
		/// </summary>
		public const string BaseAddress = "BaseAddress";

		/// <summary>
		/// Specifies the base path for the output file. If it is set, MSBuild will use OutputPath = $(BaseOutputPath)\$(Configuration)\. Example syntax: <BaseOutputPath>c:\xyz\bin\</BaseOutputPath>
		/// </summary>
		public const string BaseOutputPath = "BaseOutputPath";

		/// <summary>
		/// The top-level folder where all configuration-specific intermediate output folders are created. The default value is obj\. The following code is an example: <BaseIntermediateOutputPath>c:\xyz\obj\</BaseIntermediateOutputPath>
		/// </summary>
		public const string BaseIntermediateOutputPath = "BaseIntermediateOutputPath";

		/// <summary>
		/// A boolean value that indicates whether project references are built or cleaned in parallel when Multi-Proc MSBuild is used. The default value is true, which means that projects will be built in parallel if the system has multiple cores or processors.
		/// </summary>
		public const string BuildInParallel = "BuildInParallel";

		/// <summary>
		/// A boolean value that indicates whether project references are built by MSBuild. Set false if you are building your project in the Visual Studio integrated development environment (IDE), true if otherwise.
		/// </summary>
		public const string BuildProjectReferences = "BuildProjectReferences";

		/// <summary>
		/// The name of the file that will be used as the "clean cache." The clean cache is a list of generated files to be deleted during the cleaning operation. The file is put in the intermediate output path by the build process.
		/// This property specifies only file names that do not have path information.
		/// </summary>
		public const string CleanFile = "CleanFile";

		/// <summary>
		/// Specifies the code page to use for all source-code files in the compilation. This property is equivalent to the /codepage compiler switch.
		/// </summary>
		public const string CodePage = "CodePage";

		/// <summary>
		/// An optional response file that can be passed to the compiler tasks.
		/// </summary>
		public const string CompilerResponseFile = "CompilerResponseFile";

		/// <summary>
		/// The configuration that you are building, either "Debug" or "Release."
		/// </summary>
		public const string Configuration = "Configuration";

		/// <summary>
		/// The path of csc.exe, the Visual C# compiler.
		/// </summary>
		public const string CscToolPath = "CscToolPath";

		/// <summary>
		/// The name of a project file or targets file that is to be imported automatically before the common targets import.
		/// </summary>
		public const string CustomBeforeMicrosoftCommonTargets = "CustomBeforeMicrosoftCommonTargets";

		/// <summary>
		/// A boolean value that indicates whether symbols are generated by the build.
		/// Setting /p:DebugSymbols=false on the command line disables generation of program database (.pdb) symbol files.
		/// </summary>
		public const string DebugSymbols = "DebugSymbols";

		/// <summary>
		/// Defines conditional compiler constants. Symbol/value pairs are separated by semicolons and are specified by using the following syntax:
		/// symbol1 = value1 ; symbol2 = value2
		/// The property is equivalent to the /define compiler switch.
		/// </summary>
		public const string DefineConstants = "DefineConstants";

		/// <summary>
		/// A boolean value that indicates whether you want the DEBUG constant defined.
		/// </summary>
		public const string DefineDebug = "DefineDebug";

		/// <summary>
		/// A boolean value that indicates whether you want the TRACE constant defined.
		/// </summary>
		public const string DefineTrace = "DefineTrace";

		/// <summary>
		/// Defines the level of debug information that you want generated. Valid values are "full," "pdbonly," and "none."
		/// </summary>
		public const string DebugType = "DebugType";

		/// <summary>
		/// A boolean value that indicates whether you want to delay-sign the assembly rather than full-sign it.
		/// </summary>
		public const string DelaySign = "DelaySign";

		/// <summary>
		/// Suppresses the specified warnings. Only the numeric part of the warning identifier must be specified.
		/// Multiple warnings are separated by semicolons.
		/// This parameter corresponds to the /nowarn switch of the vbc.exe compiler.
		/// </summary>
		public const string DisabledWarnings = "DisabledWarnings";

		/// <summary>
		/// A boolean value that applies to Visual Studio only.
		/// The Visual Studio build manager uses a process called FastUpToDateCheck to determine whether a project
		/// must be rebuilt to be up to date.
		/// This process is faster than using MSBuild to determine this.
		/// Setting the DisableFastUpToDateCheck property to true lets you bypass the Visual Studio build manager
		/// and forse it to use MSBuild to determine whether the project is up to date.
		/// </summary>
		public const string DisableFastUpToDateCheck = "DisableFastUpToDateCheck";

		/// <summary>
		/// The name of the file that is generated as the XML documentation file.
		/// This name includes only the file name and has no path information.
		/// </summary>
		public const string DocumentationFile = "DocumentationFile";

		/// <summary>
		/// Specifies how the compiler task should report internal compiler errors.
		/// Valid values are "prompt," "send," or "none."
		/// This property is equivalent to the /errorreport compiler switch.
		/// </summary>
		public const string ErrorReport = "ErrorReport";

		/// <summary>
		///     The GenerateDeploymentManifest Task adds a deploymentProvider tag to the deployment manifest
		///     if the project file includes any of the following elements:
		///     UpdateUrl
		///     InstallUrl
		///     PublishUrl
		///     Using ExcludeDeploymentUrl, however, you can prevent the deploymentProvider tag from being added
		///     to the deployment manifest even if any of the above URLs are specified. To do this,
		///     add the following property to your project file:
		///     <ExcludeDeploymentUrl>true</ExcludeDeploymentUrl>
		/// </summary>
		/// <remarks>
		///     ExcludeDeploymentUrl is not exposed in the Visual Studio IDE and can be set only by manually
		///     editing the project file. Setting this property does not affect publishing within Visual Studio;
		///     that is, the deploymentProvider tag will still be added to the URL specified by PublishUrl.
		/// </remarks>
		public const string ExcludeDeploymentUrl = "ExcludeDeploymentUrl";

		/// <summary>
		/// Specifies, in bytes, where to align the sections of the output file. Valid values are 512, 1024, 2048, 4096, 8192.
		/// This property is equivalent to the /filealignment compiler switch.
		/// </summary>
		public const string FileAlignment = "FileAlignment";

		/// <summary>
		/// Specifies the location of mscorlib.dll and microsoft.visualbasic.dll.
		/// This parameter is equivalent to the /sdkpath switch of the vbc.exe compiler.
		/// </summary>
		public const string FrameworkPathOverride = "FrameworkPathOverride";

		/// <summary>
		/// A boolean parameter that indicates whether documentation is generated by the build.
		/// If true, the build generates documentation information and puts it in an .xml file
		/// together with the name of the executable file or library that the build task created.
		/// </summary>
		public const string GenerateDocumentation = "GenerateDocumentation";

		/// <summary>
		/// The full intermediate output path as derived from BaseIntermediateOutputPath, if no path is specified.
		/// For example, \obj\debug\. If this property is overridden, then setting BaseIntermediateOutputPath
		/// has no effect.
		/// </summary>
		public const string IntermediateOutputPath = "IntermediateOutputPath";

		/// <summary>
		/// The name of the strong-name key container.
		/// </summary>
		public const string KeyContainerName = "KeyContainerName";

		/// <summary>
		/// The name of the strong-name key file.
		/// </summary>
		public const string KeyOriginatorFile = "KeyOriginatorFile";

		/// <summary>
		/// The name of the assembly that the compiled module is to be incorporated into.
		/// The property is equivalent to the /moduleassemblyname compiler switch.
		/// </summary>
		public const string ModuleAssemblyName = "ModuleAssemblyName";

		/// <summary>
		/// A boolean value that indicates whether you want compiler logo to be turned off.
		/// This property is equivalent to the /nologo compiler switch.
		/// </summary>
		public const string NoLogo = "NoLogo";

		/// <summary>
		/// A boolean value that indicates whether to avoid referencing the standard library (mscorlib.dll). 
		/// The default value is false.
		/// </summary>
		public const string NoStdLib = "NoStdLib";

		/// <summary>
		/// A boolean value that indicates whether the Visual Basic runtime (Microsoft.VisualBasic.dll)
		/// should be included as a reference in the project.
		/// </summary>
		public const string NoVBRuntimeReference = "NoVBRuntimeReference";

		/// <summary>
		/// Determines whether the compiler generates the default Win32 manifest into the output assembly.
		/// The default value of false means that the default Win32 manifest is generated for all applications.
		/// This property is equivalent to the /nowin32manifest compiler switch of vbc.exe.
		/// </summary>
		/// <remarks>
		/// A boolean value that indicates whether User Account Control (UAC) manifest information will be
		/// embedded in the application's executable. Applies only to Visual Studio projects targeting Windows Vista.
		/// In projects deployed using ClickOnce and Registration-Free COM, this element is ignored.
		/// False (the default value) specifies that User Account Control (UAC) manifest information be embedded
		/// in the application's executable. True specifies that UAC manifest information not be embedded.
		/// This property applies only to Visual Studio projects targeting Windows Vista. In projects deployed
		/// using ClickOnce and Registration-Free COM, this property is ignored.
		/// You should add NoWin32Manifest only if you do not want Visual Studio to embed any manifest information
		/// in the application's executable; this process is called virtualization. To use virtualization,
		/// set 'ApplicationManifest' in conjunction with 'NoWin32Manifest' as follows:
		/// For Visual Basic projects, remove the 'ApplicationManifest' node. (In Visual Basic projects,
		/// 'NoWin32Manifest' is ignored when an 'ApplicationManifest' node exists.)
		/// For Visual C# projects, set ApplicationManifest' to False and 'NoWin32Manifest' to True.
		/// (In Visual C# projects, 'ApplicationManifest' overrides 'NoWin32Manifest'.)
		/// </remarks>
		public const string NoWin32Manifest = "NoWin32Manifest";

		/// <summary>
		/// 	A boolean value that when set to true, enables compiler optimizations. 
		/// This property is equivalent to the /optimize compiler switch.
		/// </summary>
		public const string Optimize = "Optimize";

		/// <summary>
		/// Specifies how string comparisons are made. Valid values are "binary" or "text."
		/// This property is equivalent to the /optioncompare compiler switch of vbc.exe.
		/// </summary>
		public const string OptionCompare = "OptionCompare";

		/// <summary>
		/// A boolean value that when set to true, requires explicit declaration of variables in the source code.
		/// This property is equivalent to the /optionexplicit compiler switch.
		/// </summary>
		public const string OptionExplicit = "OptionExplicit";

		/// <summary>
		/// 	A boolean value that when set to true, enables type inference of variables.
		/// This property is equivalent to the /optioninfer compiler switch.
		/// </summary>
		public const string OptionInfer = "OptionInfer";

		/// <summary>
		/// A boolean value that when set to true, causes the build task to enforce strict type semantics to
		/// restrict implicit type conversions. This property is equivalent to the /optionstrict switch of
		/// the vbc.exe compiler.
		/// </summary>
		public const string OptionStrict = "OptionStrict";

		/// <summary>
		/// Specifies the path to the output directory, relative to the project directory, for example, "bin\Debug".
		/// </summary>
		public const string OutputPath = "OutputPath";

		/// <summary>
		/// Specifies the file format of the output file. This parameter can have one of the following values:
		/// Library. Creates a code library. (Default value.)
		/// Exe. Creates a console application.
		/// Module. Creates a module.
		/// Winexe. Creates a Windows-based program.
		/// This property is equivalent to the /target switch of the vbc.exe compiler.
		/// </summary>
		public const string OutputType = "OutputType";

		/// <summary>
		/// A boolean value that indicates whether you want to enable the build to overwrite read-only files
		/// or trigger an error.
		/// </summary>
		public const string OverwriteReadOnlyFiles = "OverwriteReadOnlyFiles";

		/// <summary>
		/// The file name of the .pdb file that you are emitting. This property is equivalent to the /pdb switch of the csc.exe compiler.
		/// </summary>
		public const string PdbFile = "PdbFile";

		/// <summary>
		/// 	The operating system you are building for. Valid values are "Any CPU", "x86", and "x64".
		/// </summary>
		public const string Platform = "Platform";

		/// <summary>
		/// A boolean value that indicates whether to disable integer overflow error checks. 
		/// The default value is false.
		/// This property is equivalent to the /removeintchecks switch of the vbc.exe compiler.
		/// </summary>
		public const string RemoveIntegerChecks = "RemoveIntegerChecks";

		/// <summary>
		/// A boolean value that indicates whether proxy types should be generated by SGen.exe.
		/// The SGen target uses this property to set the UseProxyTypes flag.
		/// This property defaults to true, and there is no UI to change this.
		/// To generate the serialization assembly for non-webservice types, add this property to the project file
		/// and set it to false before importing the Microsoft.Common.Targets or the C#/VB.targets.
		/// </summary>
		public const string SGenUseProxyTypes = "SGenUseProxyTypes";

		/// <summary>
		/// An optional tool path that indicates where to obtain SGen.exe when the current version of SGen.exe is overridden.
		/// </summary>
		public const string SGenToolPath = "SGenToolPath";

		/// <summary>
		/// Specifies the class or module that contains the Main method or Sub Main procedure. 
		/// This property is equivalent to the /main compiler switch.
		/// </summary>
		public const string StartupObject = "StartupObject";

		/// <summary>
		/// The processor architecture that is used when assembly references are resolved.
		/// Valid values are "msil," "x86," "amd64," or "ia64."
		/// </summary>
		public const string ProcessorArchitecture = "ProcessorArchitecture";

		/// <summary>
		/// The root namespace to use when you name an embedded resource.
		/// This namespace is part of the embedded resource manifest name.
		/// </summary>
		public const string RootNamespace = "RootNamespace";

		/// <summary>
		/// The ID of the AL.exe hashing algorithm to use when satellite assemblies are created.
		/// </summary>
		public const string Satellite_AlgorithmId = "Satellite_AlgorithmId";

		/// <summary>
		/// The base address to use when culture-specific satellite assemblies are built by using the CreateSatelliteAssemblies target.
		/// </summary>
		public const string Satellite_BaseAddress = "Satellite_BaseAddress";

		/// <summary>
		/// The company name to pass into AL.exe during satellite assembly generation.
		/// </summary>
		public const string Satellite_CompanyName = "Satellite_CompanyName";

		/// <summary>
		/// The configuration name to pass into AL.exe during satellite assembly generation.
		/// </summary>
		public const string Satellite_Configuration = "Satellite_Configuration";

		/// <summary>
		/// The description text to pass into AL.exe during satellite assembly generation.
		/// </summary>
		public const string Satellite_Description = "Satellite_Description";

		/// <summary>
		/// Embeds the specified file in the satellite assembly that has the resource name "Security.Evidence."
		/// </summary>
		public const string Satellite_EvidenceFile = "Satellite_EvidenceFile";

		/// <summary>
		/// Specifies a string for the File Version field in the satellite assembly.
		/// </summary>
		public const string Satellite_FileVersion = "Satellite_FileVersion";

		/// <summary>
		/// Specifies a value for the Flags field in the satellite assembly.
		/// </summary>
		public const string Satellite_Flags = "Satellite_Flags";

		/// <summary>
		/// Causes the build task to use absolute paths for any files reported in an error message.
		/// </summary>
		public const string Satellite_GenerateFullPaths = "Satellite_GenerateFullPaths";

		/// <summary>
		/// Links the specified resource files to a satellite assembly.
		/// </summary>
		public const string Satellite_LinkResource = "Satellite_LinkResource";

		/// <summary>
		/// Specifies the fully-qualified name (that is, class.method) of the method to use as an entry point
		/// when a module is converted to an executable file during satellite assembly generation.
		/// </summary>
		public const string Satellite_MainEntryPoint = "Satellite_MainEntryPoint";

		/// <summary>
		/// Specifies a string for the Product field in the satellite assembly.
		/// </summary>
		public const string Satellite_ProductName = "Satellite_ProductName";

		/// <summary>
		/// Specifies a string for the ProductVersion field in the satellite assembly.
		/// </summary>
		public const string Satellite_ProductVersion = "Satellite_ProductVersion";

		/// <summary>
		/// Specifies the file format of the satellite assembly output file as "library," "exe," or "win." 
		/// The default value is "library."
		/// </summary>
		public const string Satellite_TargetType = "Satellite_TargetType";

		/// <summary>
		/// Specifies a string for the Title field in the satellite assembly.
		/// </summary>
		public const string Satellite_Title = "Satellite_Title";

		/// <summary>
		/// Specifies a string for the Trademark field in the satellite assembly.
		/// </summary>
		public const string Satellite_Trademark = "Satellite_Trademark";

		/// <summary>
		/// Specifies the version information for the satellite assembly.
		/// </summary>
		public const string Satellite_Version = "Satellite_Version";

		/// <summary>
		/// Inserts an .ico icon file in the satellite assembly.
		/// </summary>
		public const string Satellite_Win32Icon = "Satellite_Win32Icon";

		/// <summary>
		/// Inserts a Win32 resource (.res file) into the satellite assembly.
		/// </summary>
		public const string Satellite_Win32Resource = "Satellite_Win32Resource";

		/// <summary>
		/// Specifies the minimum version of the subsystem that the generated executable file can use.
		/// This property is equivalent to the /subsystemversion compiler switch.
		/// For information about the default value of this property, see /subsystemversion (Visual Basic)
		/// or /subsystemversion (C# Compiler Options).
		/// </summary>
		public const string SubsystemVersion = "SubsystemVersion";

		/// <summary>
		/// The version of the .NET Compact Framework that is required to run the application that you are building.
		/// Specifying this lets you reference certain framework assemblies that you may not be able to reference otherwise.
		/// </summary>
		public const string TargetCompactFramework = "TargetCompactFramework";

		/// <summary>
		/// The version of the .NET Framework that is required to run the application that you are building.
		/// Specifying this lets you reference certain framework assemblies that you may not be able to
		/// reference otherwise.
		/// </summary>
		public const string TargetFrameworkVersion = "TargetFrameworkVersion";

		/// <summary>
		/// A boolean parameter that, if true, causes all warnings to be treated as errors.
		/// This parameter is equivalent to the /nowarn compiler switch.
		/// </summary>
		public const string TreatWarningsAsErrors = "TreatWarningsAsErrors";

		/// <summary>
		/// A boolean parameter that, if true, causes the build task to use the in-process compiler object,
		/// if it is available. This parameter is used only by Visual Studio.
		/// </summary>
		public const string UseHostCompilerIfAvailable = "UseHostCompilerIfAvailable";

		/// <summary>
		/// A boolean parameter that, if true, logs compiler output by using UTF-8 encoding.
		/// This parameter is equivalent to the /utf8Output compiler switch.
		/// </summary>
		public const string Utf8Output = " Utf8Output";

		/// <summary>
		/// An optional path that indicates another location for vbc.exe when the current version of vbc.exe is overridden.
		/// </summary>
		public const string VbcToolPath = "VbcToolPath";

		/// <summary>
		/// Specifies the verbosity of the Visual Basic compiler’s output. 
		/// Valid values are "Quiet," "Normal" (the default value), or "Verbose."
		/// </summary>
		public const string VbcVerbosity = "VbcVerbosity";

		/// <summary>
		/// Specifies the version of Visual Studio under which this project should be considered to be running.
		/// If this property isn't specified, MSBuild sets it to a reasonable default value.
		/// This property is used in several project types to specify the set of targets that are used for the build.
		/// If ToolsVersion is set to 4.0 or higher for a project, VisualStudioVersion is used to specify which sub-toolset
		/// to use. For more information, see MSBuild Toolset (ToolsVersion).
		/// </summary>
		public const string VisualStudioVersion = "VisualStudioVersion";

		/// <summary>
		/// Specifies a list of warnings to treat as errors.
		/// This parameter is equivalent to the /warnaserror compiler switch.
		/// </summary>
		public const string WarningsAsErrors = "WarningsAsErrors";

		/// <summary>
		/// Specifies a list of warnings that are not treated as errors.
		/// This parameter is equivalent to the /warnaserror compiler switch.
		/// </summary>
		public const string WarningsNotAsErrors = "WarningsNotAsErrors";

		/// <summary>
		/// The name of the manifest file that should be embedded in the final assembly.
		/// This parameter is equivalent to the /win32Manifest compiler switch.
		/// </summary>
		public const string Win32Manifest = "Win32Manifest";

		/// <summary>
		/// The file name of the Win32 resource to be embedded in the final assembly.
		/// This parameter is equivalent to the /win32resource compiler switch.
		/// </summary>
		public const string Win32Resource = "Win32Resource";

		#endregion

		#region Project Properties

		public const string Prefer32Bit = "Prefer32Bit";
		public const string AppDesignerFolder = "AppDesignerFolder";
		public const string ProjectGuid = "ProjectGuid";
		public const string WarningLevel = "WarningLevel";
		public const string AllowUnsafeBlocks = "AllowUnsafeBlocks";
		public const string PlatformTarget = "PlatformTarget";

		#endregion

		#region Reserved Custom Build Properties

		/// <summary>
		/// The build target to build in this step.
		/// <see cref="Targets.Build"/> or <see cref="Targets.Clean"/>.
		/// </summary>
		public const string DotNetBuildTarget = "DotNetBuildTarget";

		#endregion
	}
}