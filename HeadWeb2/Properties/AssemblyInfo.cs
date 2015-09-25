using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MediaPortal.Common.Utils;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("HeadWeb for MediaPortal")]
[assembly: AssemblyDescription("HeadWeb Configuration")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Team HeadWeb")]
[assembly: AssemblyProduct("HeadWeb for MediaPortal")]
[assembly: AssemblyCopyright("Copyright © 2012-2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9b3e7865-7e32-4c37-b5ca-9d793225b576")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("0.0.1.164")]
[assembly: AssemblyFileVersion("0.0.1.164")]
//[assembly: CompatibleVersion("1.3.100.0", "1.3.100.0")]

// Define that our plugin is designed for MediaPortal 1.7 - the MP dlls have been slightly restructured and it's using .net 4 now, so not backward compatible
[assembly: CompatibleVersion("1.6.100.0", "1.6.100.0")]

// Tell MediaPortal which subsystems this plugin will use, so it can check for compatiblity
[assembly: UsesSubsystem("MP.SkinEngine")]
[assembly: UsesSubsystem("MP.Config")]

