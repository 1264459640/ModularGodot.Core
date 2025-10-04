namespace ModularGodot.Core.Contexts
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

    

            return defaultPaths.Where(path => File.Exists(path));
        }

        /// <summary>
        /// 从环境变量或配置文件获取程序集路径
        /// </summary>
        /// <returns>程序集路径列表</returns>
        public static IEnumerable<string> GetAssemblyPathsFromConfiguration()
        {
            // 尝试从环境变量获取程序集路径
            var assemblyPathsEnv = Environment.GetEnvironmentVariable("ASSEMBLY_PATHS");
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