using System;

namespace ModularGodot.Core.Test.Models
{
    /// <summary>
    /// 表示单个集成测试场景，对应一个Godot场景文件
    /// </summary>
    public class TestScene
    {
        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName { get; set; }

        /// <summary>
        /// 测试场景描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 测试状态 (Pending, Running, Passed, Failed)
        /// </summary>
        public TestStatus Status { get; set; }
    }

    /// <summary>
    /// 测试状态枚举
    /// </summary>
    public enum TestStatus
    {
        Pending,
        Running,
        Passed,
        Failed
    }
}