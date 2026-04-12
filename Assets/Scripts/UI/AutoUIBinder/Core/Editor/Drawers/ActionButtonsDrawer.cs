#region

using System;
using UnityEditor;
using UnityEngine;

#endregion

namespace AutoUI
{
    /// <summary>
    ///     操作按钮绘制器
    /// </summary>
    public class ActionButtonsDrawer
    {
        private readonly ComponentBindingManager bindingManager;
        private readonly CodeGenerator codeGenerator;

        public ActionButtonsDrawer
            (ComponentBindingManager bindingManager, CodeGenerator codeGenerator)
        {
            this.bindingManager = bindingManager;
            this.codeGenerator = codeGenerator;
        }

        /// <summary>
        ///     绘制操作按钮区域
        /// </summary>
        public void DrawActionButtons(AutoUIBinderBase target)
        {
            if (!bindingManager.IsInPrefabMode()) return;

            EditorGUILayout.BeginHorizontal();

            DrawClearBindingsButton(target);
            DrawValidateBindingsButton(target);
            DrawGenerateUICodeButton(target);

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawClearBindingsButton(AutoUIBinderBase target)
        {
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("清空绑定", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("确认清空", "确定要清空所有组件绑定吗？", "确定", "取消"))
                {
                    bindingManager.ClearAllBindings(target);
                }
            }
        }

        private void DrawValidateBindingsButton(AutoUIBinderBase target)
        {
            GUI.backgroundColor = new Color(0.8f, 0.6f, 0.4f);
            if (GUILayout.Button("验证绑定", GUILayout.Height(30)))
            {
                var result = bindingManager.ValidateBindings(target);
                Debug.Log(
                    $"[AutoUIBinder] 验证完成 - 有效: {result.ValidCount}, 已清理: {result.InvalidCount}");
                EditorUtility.DisplayDialog("验证完成", result.GetMessage(), "确定");
            }
        }


        private void DrawGenerateUICodeButton(AutoUIBinderBase target)
        {
            GUI.backgroundColor = new Color(0.4f, 0.6f, 0.8f);
            if (GUILayout.Button("生成 UI 代码", GUILayout.Height(30)))
            {
                try
                {
                    // 生成UI代码
                    codeGenerator.GenerateUICode(target);

                    string className = target.GetType().Name;
                    EditorUtility.DisplayDialog("生成完成",
                        $"UI代码生成完成\n类名: {className}\n组件数: {target.ComponentRefs.Count}",
                        "确定");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[AutoUIBinder] UI代码生成失败: {ex.Message}");
                    EditorUtility.DisplayDialog("错误", $"UI代码生成失败:\n{ex.Message}", "确定");
                }
            }
        }
    }
}