using System.Collections.Generic;

namespace AutoUI
{
    /// <summary>
    /// 命名策略工厂
    /// </summary>
    public static class NamingStrategyFactory
    {
        private static readonly Dictionary<NamingConvention, INamingStrategy> s_StrategyCache
            = new Dictionary<NamingConvention, INamingStrategy>();

        /// <summary>
        /// 根据命名规则创建策略
        /// </summary>
        public static INamingStrategy CreateStrategy(NamingConvention convention)
        {
            if (s_StrategyCache.TryGetValue(convention, out INamingStrategy cachedStrategy))
            {
                return cachedStrategy;
            }

            INamingStrategy strategy;

            switch (convention)
            {
                case NamingConvention.NodeName_ComponentType:
                    strategy = new NodeNameComponentTypeStrategy();
                    break;
                case NamingConvention.ComponentType_NodeName:
                    strategy = new ComponentTypeNodeNameStrategy();
                    break;
                case NamingConvention.NodeNameOnly:
                    strategy = new NodeNameOnlyStrategy();
                    break;
                case NamingConvention.PascalCase:
                    strategy = new PascalCaseStrategy();
                    break;
                case NamingConvention.m_PascalCase:
                    strategy = new PrivateFieldPascalCaseStrategy();
                    break;
                case NamingConvention.CamelCase:
                    strategy = new CamelCaseStrategy();
                    break;
                default:
                    strategy = new NodeNameComponentTypeStrategy();
                    break;
            }

            s_StrategyCache[convention] = strategy;
            return strategy;
        }

        /// <summary>
        /// 获取所有策略的预览示例
        /// </summary>
        public static Dictionary<NamingConvention, string> GetAllExamples(string nodeName, string componentType)
        {
            var examples = new Dictionary<NamingConvention, string>();

            foreach (NamingConvention convention in System.Enum.GetValues(typeof(NamingConvention)))
            {
                var strategy = CreateStrategy(convention);
                examples[convention] = strategy.GetExample(nodeName, componentType);
            }

            return examples;
        }

        /// <summary>
        /// 清理策略缓存
        /// </summary>
        public static void ClearCache()
        {
            s_StrategyCache.Clear();
        }
    }
}
