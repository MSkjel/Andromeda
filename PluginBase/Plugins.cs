using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace InfinityScript
{
    public static partial class Script
    {
        internal static List<Plugin> LoadedPlugins = new List<Plugin>();

        public static readonly string PluginPath = @"scripts";

        internal static void LoadPlugins()
        {
            var assemblies = Directory.GetFiles(PluginPath, "*.dll", SearchOption.TopDirectoryOnly)
                .ToDictionary(ass => AssemblyName.GetAssemblyName(ass), ass => ass);

            Assembly resolve(object sender, ResolveEventArgs args)
            {
                var name = new AssemblyName(args.Name);

                foreach (var entry in assemblies)
                    if (name.Name == entry.Key.Name && name.Version <= entry.Key.Version)
                        return LoadAssembly(entry.Value);

                return null;
            }

            AppDomain.CurrentDomain.AssemblyResolve += resolve;

            foreach (var file in Directory.GetFiles(PluginPath, @"*.plugin.dll", SearchOption.TopDirectoryOnly))
            {
                // make sure we don't load an assembly twice (possible due to dependencies)
                if (AppDomain.CurrentDomain.GetAssemblies().Any(ass =>
                        ass.GetName().Name == AssemblyName.GetAssemblyName(file).Name))
                    continue;

                LoadAssembly(file);
            }

            foreach (var plugin in LoadedPlugins)
            {
                plugin.RunEntryPoint();
            }
        }

        internal static Assembly LoadAssembly(string path)
        {
            var plugin = new Plugin(path);

            if (plugin.IsLibrary)
                Log.Info($"Found library {plugin.Name}");
            else
                Log.Info($"Found plugin {plugin.Name} with {plugin.EntryPointCount} entry points.");

            LoadedPlugins.Add(plugin);

            return plugin.Assembly;
        }
    }

    internal class Plugin
    {
        public readonly Assembly Assembly;

        public int EntryPointCount { get; private set; } = 0;

        private readonly Action EntryPoint;
        private readonly Action Cleanups;

        public bool IsLibrary
            => EntryPointCount == 0;

        public readonly string Name;

        public Plugin(string path)
        {
            Assembly = Assembly.Load(File.ReadAllBytes(path));
            Name = AssemblyName.GetAssemblyName(path).Name;

            foreach (Type type in Assembly.GetExportedTypes())
            {
                try
                {
                    if (type.GetCustomAttributes(typeof(PluginAttribute), false).FirstOrDefault() is PluginAttribute attr)
                    {
                        try
                        {
                            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Error running {type.FullName}'s static constructor:");
                            Log.Error(ex);
                        }

                        foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                        {
                            if (method.GetCustomAttributes(typeof(EntryPointAttribute), false).FirstOrDefault() != null)
                            {
                                EntryPoint += Delegate.CreateDelegate(typeof(Action), method) as Action;
                                EntryPointCount++;
                            }

                            if (method.GetCustomAttributes(typeof(CleanupAttribute), false).FirstOrDefault() != null)
                            {
                                Cleanups += Delegate.CreateDelegate(typeof(Action), method) as Action;
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    //Log.Info(ex.ToString());
                    break;
                }
            }
        }

        public void RunEntryPoint()
        {
            try
            {
                EntryPoint?.Invoke();
            }
            catch (Exception ex)
            {
                Log.Error($"Error running one of plugin {Assembly.GetName().Name}'s entry points:");
                Log.Error(ex);
            }
        }

        public void RunCleanups()
        {
            foreach (Action action in Cleanups.GetInvocationList())
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Log.Error($"Error running one of plugin {Assembly.GetName().Name}'s cleanups:");
                    Log.Error(ex);
                }
            }
        }
    }
}
