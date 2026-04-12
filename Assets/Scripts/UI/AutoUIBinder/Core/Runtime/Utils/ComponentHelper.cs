#region

using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace AutoUI
{
    /// <summary>
    ///     组件辅助工具类 - 提供组件查找、类型检查等通用功能
    /// </summary>
    public static class ComponentHelper
    {
        #region 策略缓存

        private static AutoUIConfig m_CachedConfig;
        private static INamingStrategy m_CachedStrategy;
        private static NamingConvention m_LastConvention = NamingConvention.NodeName_ComponentType;

        /// <summary>
        ///     获取当前命名策略
        /// </summary>
        private static INamingStrategy GetCurrentStrategy()
        {
#if UNITY_EDITOR
            if (m_CachedConfig == null)
            {
                m_CachedConfig = Resources.Load<AutoUIConfig>("GlobalConfig");
            }

            if (m_CachedConfig != null)
            {
                if (m_CachedStrategy == null ||
                    m_LastConvention != m_CachedConfig.CurrentNamingConvention)
                {
                    m_LastConvention = m_CachedConfig.CurrentNamingConvention;
                    m_CachedStrategy = m_CachedConfig.GetCurrentStrategy();
                }

                return m_CachedStrategy;
            }
#endif
            // 默认策略
            if (m_CachedStrategy == null)
            {
                m_CachedStrategy = new NodeNameComponentTypeStrategy();
            }

            return m_CachedStrategy;
        }

        /// <summary>
        ///     刷新策略缓存 (配置更改后调用)
        /// </summary>
        public static void RefreshStrategyCache()
        {
            m_CachedConfig = null;
            m_CachedStrategy = null;
        }

        #endregion

        #region 组件查找

        /// <summary>
        ///     查找组件的最近的AutoUIBinderBase父级
        /// </summary>
        public static AutoUIBinderBase FindIconHandler(Component component)
        {
            if (component == null) return null;

            // 首先检查组件自身是否是AutoUIBinderBase
            if (component is AutoUIBinderBase)
            {
                // 如果是AutoUIBinderBase，查找父级
                var parent = component.transform.parent;
                while (parent != null)
                {
                    var parentHandler = parent.GetComponent<AutoUIBinderBase>();
                    if (parentHandler != null)
                        return parentHandler;
                    parent = parent.parent;
                }

                return null; // AutoUIBinderBase组件不绑定到自己
            }

            // 如果不是AutoUIBinderBase，先查找自身，然后查找父级
            var handler = component.GetComponent<AutoUIBinderBase>();
            if (handler != null)
                return handler;

            // 向上查找父级
            var currentParent = component.transform.parent;
            while (currentParent != null)
            {
                handler = currentParent.GetComponent<AutoUIBinderBase>();
                if (handler != null)
                    return handler;
                currentParent = currentParent.parent;
            }

            return null;
        }

        /// <summary>
        ///     检查是否应该为指定的GameObject显示图标
        /// </summary>
        public static bool ShouldShowIcons(GameObject gameObject)
        {
#if UNITY_EDITOR
            // 检查是否在预制体编辑模式
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null)
            {
                return false; // 不在预制体编辑模式中
            }

            // 检查自身或任意父级对象是否继承了AutoUIBinderBase
            Transform current = gameObject.transform;
            while (current != null)
            {
                if (current.GetComponent<AutoUIBinderBase>() != null)
                    return true;
                current = current.parent;
            }
#endif
            return false;
        }

        /// <summary>
        ///     获取GameObject上的所有UI组件
        /// </summary>
        public static List<Component> GetUIComponents(GameObject gameObject)
        {
            if (gameObject == null) return new List<Component>();

            var uiComponents = new List<Component>(4); // 预分配容量

            // 检查常见UI组件（按使用频率排序）
            CheckAndAddComponent<Button>(gameObject, uiComponents);
            CheckAndAddComponent<Image>(gameObject, uiComponents);
            CheckAndAddComponent<Text>(gameObject, uiComponents);
            CheckAndAddComponent<Toggle>(gameObject, uiComponents);
            CheckAndAddComponent<Slider>(gameObject, uiComponents);
            CheckAndAddComponent<InputField>(gameObject, uiComponents);
            CheckAndAddComponent<Dropdown>(gameObject, uiComponents);
            CheckAndAddComponent<ScrollRect>(gameObject, uiComponents);

            // TextMeshPro组件
            CheckAndAddComponent<TextMeshProUGUI>(gameObject, uiComponents);
            CheckAndAddComponent<TMP_InputField>(gameObject, uiComponents);
            CheckAndAddComponent<TMP_Dropdown>(gameObject, uiComponents);

            return uiComponents;
        }

        /// <summary>
        ///     检查并添加特定类型的组件
        /// </summary>
        private static void CheckAndAddComponent<T>
            (GameObject gameObject, List<Component> components) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                components.Add(component);
            }
        }

        #endregion

        #region 组件引用管理

        /// <summary>
        ///     生成节点组件的Key - 使用可配置的命名规则
        /// </summary>
        public static string GetNodeComponentKey(Component component)
        {
            if (component == null) return string.Empty;

            string nodeName = component.gameObject.name;
            string componentType = component.GetType().Name;

            var strategy = GetCurrentStrategy();
            return strategy.GenerateKey(nodeName, componentType);
        }

        /// <summary>
        ///     使用指定策略生成Key (用于批量转换)
        /// </summary>
        public static string GetNodeComponentKey(Component component, INamingStrategy strategy)
        {
            if (component == null || strategy == null) return string.Empty;

            string nodeName = component.gameObject.name;
            string componentType = component.GetType().Name;

            return strategy.GenerateKey(nodeName, componentType);
        }

        /// <summary>
        ///     生成唯一的节点名称
        /// </summary>
        public static string GetUniqueNodeName(AutoUIBinderBase handler, string baseName)
        {
            if (handler?.ComponentRefs == null) return baseName;

            var existingNames = new HashSet<string>();
            foreach (var kvp in handler.ComponentRefs)
            {
                if (kvp.Value != null)
                {
                    existingNames.Add(kvp.Value.gameObject.name);
                }
            }

            string uniqueName = baseName;
            int counter = 1;
            while (existingNames.Contains(uniqueName))
            {
                uniqueName = $"{baseName}_{counter}";
                counter++;
            }

            return uniqueName;
        }

        /// <summary>
        ///     生成唯一的Key (避免Key冲突)
        /// </summary>
        public static string GetUniqueKey(AutoUIBinderBase handler, string baseKey)
        {
            if (handler?.ComponentRefs == null) return baseKey;

            string uniqueKey = baseKey;
            int counter = 1;
            while (handler.ComponentRefs.ContainsKey(uniqueKey))
            {
                uniqueKey = $"{baseKey}_{counter}";
                counter++;
            }

            return uniqueKey;
        }

        /// <summary>
        ///     检查名称是否有冲突
        /// </summary>
        public static bool HasNameConflict(AutoUIBinderBase handler, GameObject targetObject)
        {
            if (handler?.ComponentRefs == null) return false;

            foreach (var kvp in handler.ComponentRefs)
            {
                if (kvp.Value != null &&
                    kvp.Value.gameObject != targetObject && // 不是自己
                    kvp.Value.gameObject.name == targetObject.name) // 名字相同
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     检查Key是否有冲突
        /// </summary>
        public static bool HasKeyConflict
            (AutoUIBinderBase handler, string key, Component excludeComponent = null)
        {
            if (handler?.ComponentRefs == null || string.IsNullOrEmpty(key)) return false;

            if (handler.ComponentRefs.TryGetValue(key, out Component existingComponent))
            {
                // 如果是同一个组件则不算冲突
                return existingComponent != excludeComponent;
            }

            return false;
        }

        #endregion

        #region 层级关系检查

        /// <summary>
        ///     检查是否为父子关系
        /// </summary>
        public static bool IsParentChildRelation(Transform parent, Transform child)
        {
            if (parent == null || child == null) return false;

            Transform current = child.parent;
            while (current != null)
            {
                if (current == parent)
                    return true;
                current = current.parent;
            }

            return false;
        }

        /// <summary>
        ///     检查组件是否可以绑定到指定的handler
        /// </summary>
        public static bool CanBindToHandler(Component component, AutoUIBinderBase handler)
        {
            if (component == null || handler == null) return false;

            // 如果是AutoUIBinderBase组件，只允许绑定到父级，且不能绑定自己
            if (component is AutoUIBinderBase)
            {
                if (handler.gameObject == component.gameObject)
                {
                    return false; // 不能绑定到自己
                }

                // 检查是否是父级关系
                return IsParentChildRelation(handler.transform, component.transform);
            }

            return true; // 普通组件可以绑定
        }

        #endregion

        #region 字符串处理

        /// <summary>
        ///     清理节点名称中的无效字符
        /// </summary>
        public static string CleanNodeName(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName)) return nodeName;

            return nodeName.Replace(" ", "_")
                .Replace("(", "")
                .Replace(")", "");
        }

        /// <summary>
        ///     检查节点名称是否需要重命名
        /// </summary>
        public static bool NeedsRename(string nodeName)
        {
            return !string.IsNullOrEmpty(nodeName) &&
                   (nodeName.Contains("(") || nodeName.Contains(")") || nodeName.Contains(" "));
        }

        #endregion

        #region 调试信息

        /// <summary>
        ///     获取组件的完整路径
        /// </summary>
        public static string GetFullPath(GameObject obj)
        {
            if (obj == null) return string.Empty;

            var path = new StringBuilder(obj.name);
            var current = obj.transform.parent;

            // 向上遍历直到找到AutoUIBinderBase或到达根节点
            while (current != null)
            {
                // 如果找到AutoUIBinderBase，停止
                if (current.GetComponent<AutoUIBinderBase>() != null)
                    break;

                path.Insert(0, current.name + "/");
                current = current.parent;
            }

            return path.ToString().Replace(" ", "_");
        }

        #endregion
    }
}