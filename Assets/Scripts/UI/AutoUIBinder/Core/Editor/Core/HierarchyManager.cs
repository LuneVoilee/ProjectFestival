#region

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

#endregion

namespace AutoUI
{
    /// <summary>
    ///     Hierarchy管理器 - 减少HierarchyComponentIcons的耦合
    /// </summary>
    public static class HierarchyManager
    {
        // 组件高亮状态管理
        private static readonly Dictionary<int, bool> highlightedComponents =
            new Dictionary<int, bool>();

        // 颜色缓存管理
        private static readonly Dictionary<int, Color> handlerColors = new Dictionary<int, Color>();

        /// <summary>
        ///     设置组件高亮状态
        /// </summary>
        public static void SetComponentHighlight(int componentID, bool highlighted)
        {
            if (highlighted)
            {
                highlightedComponents[componentID] = true;
            }
            else
            {
                highlightedComponents.Remove(componentID);
            }
        }

        /// <summary>
        ///     检查组件是否高亮
        /// </summary>
        public static bool IsComponentHighlighted(int componentID)
        {
            return highlightedComponents.ContainsKey(componentID) &&
                   highlightedComponents[componentID];
        }

        /// <summary>
        ///     清空高亮状态
        /// </summary>
        public static void ClearHighlights()
        {
            highlightedComponents.Clear();
        }

        /// <summary>
        ///     验证并清理无效的高亮组件
        /// </summary>
        public static void ValidateAndCleanupHighlights()
        {
            var keysToRemove = new List<int>();

            foreach (var kvp in highlightedComponents.ToList())
            {
                Component comp = EditorUtility.InstanceIDToObject(kvp.Key) as Component;
                if (comp == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                highlightedComponents.Remove(key);
            }
        }

        /// <summary>
        ///     获取所有高亮的组件ID
        /// </summary>
        public static List<int> GetHighlightedComponentIds()
        {
            return new List<int>(highlightedComponents.Keys);
        }

        /// <summary>
        ///     获取Handler颜色
        /// </summary>
        public static Color GetHandlerColor(AutoUIBinderBase handler)
        {
            if (handler == null) return Color.white;

            int handlerID = handler.GetInstanceID();
            if (!handlerColors.ContainsKey(handlerID))
            {
                string uniqueKey = GetHandlerUniqueKey(handler);
                int hash = uniqueKey.GetHashCode();

                Random random = new Random(hash);

                float hue = (float)random.NextDouble();
                float saturation = 0.6f + (float)random.NextDouble() * 0.3f;
                float value = 0.7f + (float)random.NextDouble() * 0.2f;

                Color color = Color.HSVToRGB(hue, saturation, value);
                handlerColors[handlerID] = color;
            }

            return handlerColors[handlerID];
        }

        /// <summary>
        ///     清理Handler颜色缓存
        /// </summary>
        public static void CleanupHandlerColors()
        {
            var keysToRemove = new List<int>();
            foreach (var kvp in handlerColors)
            {
                var handler = EditorUtility.InstanceIDToObject(kvp.Key) as AutoUIBinderBase;
                if (handler == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                handlerColors.Remove(key);
            }
        }

        /// <summary>
        ///     生成节点组件键值 (使用ComponentHelper)
        /// </summary>
        public static string GetNodeComponentKey(Component component)
        {
            return ComponentHelper.GetNodeComponentKey(component);
        }

        /// <summary>
        ///     查找组件对应的AutoUIBinderBase (使用ComponentHelper)
        /// </summary>
        public static AutoUIBinderBase FindIconHandler(Component component)
        {
            return ComponentHelper.FindIconHandler(component);
        }

        /// <summary>
        ///     获取唯一节点名称 (使用ComponentHelper)
        /// </summary>
        public static string GetUniqueNodeName(AutoUIBinderBase handler, string originalName)
        {
            return ComponentHelper.GetUniqueNodeName(handler, originalName);
        }

        private static string GetHandlerUniqueKey(AutoUIBinderBase handler)
        {
            if (handler == null) return "";

            string path = "";
            Transform current = handler.transform;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path + handler.GetType().Name;
        }
    }
}