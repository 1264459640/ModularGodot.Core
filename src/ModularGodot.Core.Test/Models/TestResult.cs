using System;

namespace ModularGodot.Core.Test.Models
{
    /// <summary>
    /// 表示单个测试场景的执行结果
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// 关联的场景名称
        /// </summary>
        public string SceneName { get; set; }

        /// <summary>
        /// 执行状态 (Passed, Failed, Skipped)
        /// </summary>
        public TestExecutionStatus Status { get; set; }

        /// <summary>
        /// 测试消息或错误信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 测试执行时间（毫秒）
        /// </summary>
        public long ExecutionTimeMs { get; set; }

        /// <summary>
        /// 测试完成时间
        /// </summary>
        public DateTime CompletedAt { get; set; }
    }

    /// <summary>
    /// 测试执行状态枚举
    /// </summary>
    public enum TestExecutionStatus
    {
        Passed,
        Failed,
        Skipped
    }
}