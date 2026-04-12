namespace AutoUI
{
    /// <summary>
    /// 命名策略基类
    /// </summary>
    public abstract class NamingStrategyBase : INamingStrategy
    {
        public abstract string DisplayName { get; }

        public abstract string GenerateKey(string nodeName, string componentTypeName);

        public virtual string GetExample(string nodeName, string componentTypeName)
        {
            return GenerateKey(nodeName, componentTypeName);
        }

        /// <summary>
        /// 清理节点名称中的无效字符
        /// </summary>
        protected string CleanNodeName(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName)) return string.Empty;

            return nodeName.Replace(" ", "_")
                          .Replace("(", "")
                          .Replace(")", "")
                          .Replace("-", "_");
        }

        /// <summary>
        /// 转换为大驼峰命名
        /// </summary>
        protected string ToPascalCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            // 先清理特殊字符
            string cleaned = text.Replace(" ", "")
                                .Replace("_", "")
                                .Replace("(", "")
                                .Replace(")", "")
                                .Replace("-", "");

            if (cleaned.Length == 0) return string.Empty;

            return char.ToUpper(cleaned[0]) + (cleaned.Length > 1 ? cleaned.Substring(1) : "");
        }

        /// <summary>
        /// 转换为小驼峰命名
        /// </summary>
        protected string ToCamelCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            string cleaned = text.Replace(" ", "")
                                .Replace("_", "")
                                .Replace("(", "")
                                .Replace(")", "")
                                .Replace("-", "");

            if (cleaned.Length == 0) return string.Empty;

            return char.ToLower(cleaned[0]) + (cleaned.Length > 1 ? cleaned.Substring(1) : "");
        }
    }

    /// <summary>
    /// 节点名_组件类型策略 (默认)
    /// </summary>
    public class NodeNameComponentTypeStrategy : NamingStrategyBase
    {
        public override string DisplayName => "节点名_组件类型";

        public override string GenerateKey(string nodeName, string componentTypeName)
        {
            string cleanName = CleanNodeName(nodeName);
            return $"{cleanName}_{componentTypeName}";
        }
    }

    /// <summary>
    /// 组件类型_节点名策略
    /// </summary>
    public class ComponentTypeNodeNameStrategy : NamingStrategyBase
    {
        public override string DisplayName => "组件类型_节点名";

        public override string GenerateKey(string nodeName, string componentTypeName)
        {
            string cleanName = CleanNodeName(nodeName);
            return $"{componentTypeName}_{cleanName}";
        }
    }

    /// <summary>
    /// 仅节点名策略
    /// </summary>
    public class NodeNameOnlyStrategy : NamingStrategyBase
    {
        public override string DisplayName => "仅节点名";

        public override string GenerateKey(string nodeName, string componentTypeName)
        {
            return CleanNodeName(nodeName);
        }
    }

    /// <summary>
    /// 大驼峰策略 (PascalCase)
    /// </summary>
    public class PascalCaseStrategy : NamingStrategyBase
    {
        public override string DisplayName => "大驼峰 (PascalCase)";

        public override string GenerateKey(string nodeName, string componentTypeName)
        {
            return ToPascalCase(nodeName) + componentTypeName;
        }
    }

    /// <summary>
    /// m_大驼峰策略 (私有字段命名)
    /// </summary>
    public class PrivateFieldPascalCaseStrategy : NamingStrategyBase
    {
        public override string DisplayName => "m_大驼峰 (Private)";

        public override string GenerateKey(string nodeName, string componentTypeName)
        {
            return "m_" + ToPascalCase(nodeName) + componentTypeName;
        }
    }

    /// <summary>
    /// 小驼峰策略 (camelCase)
    /// </summary>
    public class CamelCaseStrategy : NamingStrategyBase
    {
        public override string DisplayName => "小驼峰 (camelCase)";

        public override string GenerateKey(string nodeName, string componentTypeName)
        {
            return ToCamelCase(nodeName) + componentTypeName;
        }
    }
}
