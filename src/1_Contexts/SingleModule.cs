using Autofac;
using Microsoft.Extensions.Caching.Memory;
using ModularGodot.Contracts.Attributes;
// Add this using
using System.Reflection;

namespace ModularGodot.Contexts
{
    public class SingleModule : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MemoryCache>().As<IMemoryCache>().SingleInstance();

            // 先主动加载所有引用的程序集
            LoadAllReferencedAssemblies();

            // 然后获取所有已加载的程序集进行扫描
            var assembliesToScan = AppDomain.CurrentDomain.GetAssemblies().ToList();

            builder.RegisterAssemblyTypes(assembliesToScan.Distinct().ToArray())
                .Where(t => t.GetCustomAttribute<InjectableAttribute>() != null)
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
        }

        private void LoadAllReferencedAssemblies()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
            var assembliesToCheck = new List<Assembly>(loadedAssemblies);
            
            foreach (Assembly assembly in assembliesToCheck)
            {
                foreach (AssemblyName refName in assembly.GetReferencedAssemblies())
                {
                    try
                    {
                        var referencedAssembly = Assembly.Load(refName);
                        if (!loadedAssemblies.Contains(referencedAssembly))
                        {
                            loadedAssemblies.Add(referencedAssembly);
                        }
                    }
                    catch { /* 忽略加载失败的程序集 */ }
                }
            }
        }
    }
}