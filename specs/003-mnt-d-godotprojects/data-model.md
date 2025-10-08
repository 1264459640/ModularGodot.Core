# 数据模型：生产基础设施的集成测试

## 简化实体

### 测试场景 (TestScene)
表示单个集成测试场景，对应一个Godot场景文件。

**属性**:
- SceneName: 场景名称
- Description: 测试场景描述
- Status: 测试状态 (Pending, Running, Passed, Failed)

### 测试结果 (TestResult)
表示单个测试场景的执行结果。

**属性**:
- SceneName: 关联的场景名称
- Status: 执行状态 (Passed, Failed, Skipped)
- Message: 测试消息或错误信息