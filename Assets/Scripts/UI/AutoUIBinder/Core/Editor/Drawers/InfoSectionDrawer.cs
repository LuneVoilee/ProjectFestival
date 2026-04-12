#region

using UnityEditor;
using UnityEngine;

#endregion

namespace AutoUI
{
    /// <summary>
    ///     信息区域绘制器
    /// </summary>
    public class InfoSectionDrawer
    {
        private bool showInfoFoldout = true;
        private readonly ComponentBindingManager bindingManager;
        private readonly PrefabManager prefabManager;

        public InfoSectionDrawer
            (ComponentBindingManager bindingManager, PrefabManager prefabManager)
        {
            this.bindingManager = bindingManager;
            this.prefabManager = prefabManager;
        }

        /// <summary>
        ///     绘制信息区域
        /// </summary>
        public void DrawInfoSection(AutoUIBinderBase target)
        {
            showInfoFoldout = EditorGUILayout.Foldout(showInfoFoldout, "数据信息", true);

            if (showInfoFoldout)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                int componentCount = target.ComponentRefs.Count;
                EditorGUILayout.LabelField($"已绑定组件: {componentCount} 个");

                string className = target.GetType().Name;
                EditorGUILayout.LabelField($"类名: {className}");

                bool inPrefabMode = bindingManager.IsInPrefabMode();
                EditorGUILayout.LabelField($"预制体状态: {bindingManager.GetPrefabStatusText()}");

                if (!inPrefabMode)
                {
                    EditorGUILayout.Space(5);
                    GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                    if (GUILayout.Button("打开预制体编辑", GUILayout.Height(25)))
                    {
                        prefabManager.OpenPrefabEditMode(target);
                    }

                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.EndVertical();
            }
        }
    }
}