using System.Threading.Tasks;

namespace ModularGodot.Contracts.Abstractions.Services
{
    /// <summary>
    /// 测试服务接口
    /// 用于演示依赖注入和基本功能
    /// </summary>
    public interface ITestService
    {
        /// <summary>
        /// 打印输入内容到控制台
        /// </summary>
        /// <param name="message">要打印的消息</param>
        /// <returns>操作结果</returns>
        Task<string> PrintMessageAsync(string message);

        /// <summary>
        /// 同步版本的打印方法
        /// </summary>
        /// <param name="message">要打印的消息</param>
        /// <returns>操作结果</returns>
        string PrintMessage(string message);
    }
}