using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


/*
https://makolyte.com/csharp-generic-plugin-loader/
*/
namespace Plugin.Manager
{
	public class GenericPluginLoader<T> where T : class
	{
		private readonly List<GenericAssemblyLoadContext<T>> loadContexts = new List<GenericAssemblyLoadContext<T>>();
		public List<T> LoadAll(params object[] constructorArgs)
		{
			string pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\plugins\\";
			string filter = "*.dll";
			List<T> plugins = new List<T>();

			foreach (var filePath in Directory.EnumerateFiles(pluginPath, filter, SearchOption.AllDirectories))
			{
                foreach (var plugin in Load(filePath, constructorArgs))
                {
                    if (plugin != null)
                    {
                        plugins.Add(plugin);
                    }
                }
			}

			return plugins;
		}
		private IEnumerable<T> Load(string pluginPath, params object[] constructorArgs)
		{
			var loadContext = new GenericAssemblyLoadContext<T>(pluginPath);

			loadContexts.Add(loadContext);

			var assembly = loadContext.LoadFromAssemblyPath(pluginPath);
			var types = assembly.GetTypes().Where(t => typeof(T).IsAssignableFrom(t));
            foreach (Type type in types)
            {
				if (type != null)
				{
					yield return (T)Activator.CreateInstance(type, constructorArgs);
				}
			}
		}

		public void UnloadAll()
		{
			foreach (var loadContext in loadContexts)
			{
				loadContext.Unload();
			}
		}
	}
}
