using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Manager
{
	public class GenericAssemblyLoadContext<T> : AssemblyLoadContext where T : class
	{
		private AssemblyDependencyResolver _resolver;
		private HashSet<string> assembliesToNotLoadIntoContext;

		public GenericAssemblyLoadContext(string pluginPath) : base(isCollectible: true)
		{

			var pluginInterfaceAssembly = typeof(T).Assembly.FullName;
			assembliesToNotLoadIntoContext = GetReferencedAssemblyFullNames(pluginInterfaceAssembly);
			assembliesToNotLoadIntoContext.Add(pluginInterfaceAssembly);

			_resolver = new AssemblyDependencyResolver(pluginPath);
		}
		private HashSet<string> GetReferencedAssemblyFullNames(string ReferencedBy)
		{
			return AppDomain.CurrentDomain
				.GetAssemblies().FirstOrDefault(t => t.FullName == ReferencedBy)
				.GetReferencedAssemblies()
				.Select(t => t.FullName)
				.ToHashSet();
		}
		protected override Assembly Load(AssemblyName assemblyName)
		{
			//Do not load the Plugin Interface DLL into the adapter's context
			//otherwise IsAssignableFrom is false. 
			if (assembliesToNotLoadIntoContext.Contains(assemblyName.FullName))
			{
				return null;
			}

			string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
			if (assemblyPath != null)
			{
				return LoadFromAssemblyPath(assemblyPath);
			}

			return null;
		}
		protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
		{
			string libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
			if (libraryPath != null)
			{
				return LoadUnmanagedDllFromPath(libraryPath);
			}

			return IntPtr.Zero;
		}
	}
}
