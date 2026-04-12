#region

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;

#endregion

namespace AutoUI
{
    /// <summary>
    ///     组件绑定管理器 - 负责组件绑定相关的逻辑
    /// </summary>
    public class ComponentBindingManager
    {
        /// <summary>
        ///     验证组件绑定
        /// </summary>
        public ValidationResult ValidateBindings(AutoUIBinderBase target)
        {
            var result = new ValidationResult();
            var invalidKeys = new List<string>();

            foreach (var kvp in target.ComponentRefs)
            {
                if (kvp.Value == null)
                {
                    result.InvalidCount++;
                    invalidKeys.Add(kvp.Key);
                }
                else
                {
                    result.ValidCount++;
                }
            }

            // 清理无效绑定
            foreach (var key in invalidKeys)
            {
                target.RemoveComponentRef(key);
            }

            if (result.InvalidCount > 0)
            {
                EditorUtility.SetDirty(target);
            }

            return result;
        }

        /// <summary>
        ///     清空所有绑定
        /// </summary>
        public void ClearAllBindings(AutoUIBinderBase target)
        {
            target.ComponentRefs.Clear();
            EditorUtility.SetDirty(target);
        }

        /// <summary>
        ///     移除组件绑定
        /// </summary>
        public void RemoveComponentBinding(AutoUIBinderBase target, string componentName)
        {
            if (target.ComponentRefs.ContainsKey(componentName))
            {
                target.RemoveComponentRef(componentName);
                EditorUtility.SetDirty(target);
            }
        }

        /// <summary>
        ///     检查是否在预制体编辑模式
        /// </summary>
        public bool IsInPrefabMode()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            return stage != null;
        }

        /// <summary>
        ///     获取预制体状态文本
        /// </summary>
        public string GetPrefabStatusText()
        {
            return IsInPrefabMode() ? "预制体编辑模式" : "非预制体模式";
        }

        /// <summary>
        ///     验证结果
        /// </summary>
        public class ValidationResult
        {
            public int ValidCount { get; set; }
            public int InvalidCount { get; set; }

            public string GetMessage()
            {
                return $"有效绑定: {ValidCount} 个\n已清理无效绑定: {InvalidCount} 个";
            }
        }
    }
}