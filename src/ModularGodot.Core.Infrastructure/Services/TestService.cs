using ModularGodot.Core.Contracts.Abstractions.Services;
using ModularGodot.Core.Contracts.Attributes;

namespace ModularGodot.Core.Infrastructure.Services
{
    /// <summary>
    /// 测试服务实现
    /// 用于演示依赖注入和基本功能
    /// </summary>
    [Injectable(Lifetime.Transient)]
    public class TestService : ITestService
    {
        /// <summary>
        /// 打印输入内容到控制台
        /// </summary>
        /// <param name="message">要打印的消息</param>
        /// <returns>操作结果</returns>
        public async Task<string> PrintMessageAsync(string message)
        {
            return await Task.Run(() =>
            {
                Console.WriteLine($"[Async] {message}");
                return $"已异步打印: {message}";
            });
        }

        /// <summary>
        /// 同步版本的打印方法
        /// </summary>
        /// <param name="message">要打印的消息</param>
        /// <returns>操作结果</returns>
        public string PrintMessage(string message)
        {
            Console.WriteLine($"[Sync] {message}");
            return $"已同步打印: {message}";
        }
    }
}