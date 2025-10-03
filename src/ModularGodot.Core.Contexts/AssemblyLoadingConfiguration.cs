using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModularGodot.Contexts
{
    /// <summary>
    /// 程序集加载配置
    /// </summary>
    public static class AssemblyLoadingConfiguration
    {
        /// <summary>
        /// 获取默认的程序集路径列表（相对于应用程序根目录）
        /// </summary>
        /// <returns>程序集路径列表</returns>
        public static IEnumerable<string> GetDefaultAssemblyPaths()
        {
            var basePath = System.AppDomain.CurrentDomain.BaseDirectory;

            // 默认程序集路径
            var defaultPaths = new List<string>
            {
                Path.Combine(basePath, "ModularGodot.Core.Contracts.dll"),
                Path.Combine(basePath, "ModularGodot.Core.Infrastructure.dll"),
                Path.Combine(basePath, "ModularGodot.Core.Repositories.dll")
            };

            // 如果存在测试目录，添加测试程序集路径
            var testPath = Path.Combine(basePath, "test", "ModularGodot.Core.Tests.dll");
            if (File.Exists(testPath))
            {
                defaultPaths.Add(testPath);
            }

            // 添加XUnit测试程序集路径
            var xunitTestPath = Path.Combine(basePath, "ModularGodot.Core.XUnitTests.dll");
            if (File.Exists(xunitTestPath))
            {
                defaultPaths.Add(xunitTestPath);
            }

            return defaultPaths.Where(path => File.Exists(path));
        }

        /// <summary>
        /// 从环境变量或配置文件获取程序集路径
        /// </summary>
        /// <returns>程序集路径列表</returns>
        public static IEnumerable<string> GetAssemblyPathsFromConfiguration()
        {
            // 尝试从环境变量获取程序集路径
            var assemblyPathsEnv = System.Environment.GetEnvironmentVariable("ASSEMBLY_PATHS");
            if (!string.IsNullOrEmpty(assemblyPathsEnv))
            {
                return assemblyPathsEnv.Split(';')
                    .Select(path => path.Trim())
                    .Where(path => !string.IsNullOrEmpty(path) && File.Exists(path));
            }

            // 返回默认路径
            return GetDefaultAssemblyPaths();
        }
    }
}