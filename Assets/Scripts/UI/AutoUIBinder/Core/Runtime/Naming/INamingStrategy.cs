namespace AutoUI
{
    /// <summary>
    ///     命名策略接口
    /// </summary>
    public interface INamingStrategy
    {
        /// <summary>
        ///     策略显示名称
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        ///     生成组件Key
        /// </summary>
        string GenerateKey(string nodeName, string componentTypeName);

        /// <summary>
        ///     获取示例
        /// </summary>
        string GetExample(string nodeName, string componentTypeName);
    }
}