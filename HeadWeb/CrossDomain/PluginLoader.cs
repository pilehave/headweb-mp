using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using System.ComponentModel;

namespace HeadWeb
{
	internal class PluginLoader : MarshalByRefObject
	{
		static string onlineVideosMainDllName = Assembly.GetExecutingAssembly().GetName().Name;
		Dictionary<String, Type> utils = new Dictionary<String, Type>();

		public PluginLoader()
		{
			AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve;
		}

		Assembly AssemblyResolve(object sender, ResolveEventArgs args)
		{
			// this should only be called to resolve OnlineVideos.dll -> return it regardless of the version, only the name "OnlineVideos"
			AssemblyName an = new AssemblyName(args.Name);
			var asm = (sender as AppDomain).GetAssemblies().FirstOrDefault(a => a.GetName().Name == an.Name);
			return asm;
		}


		#region MarshalByRefObject overrides
		public override object InitializeLifetimeService()
		{
			// In order to have the lease across appdomains live forever, we return null.
			return null;
		}
		#endregion
	}
}
