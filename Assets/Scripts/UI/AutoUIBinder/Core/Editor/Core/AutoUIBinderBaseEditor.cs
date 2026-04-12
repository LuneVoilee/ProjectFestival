#region

using UnityEditor;

#endregion

namespace AutoUI
{
    [CustomEditor(typeof(AutoUIBinderBase), true)]
    public class AutoUIBinderBaseEditor : Editor
    {
        // 模块化管理器
        private ComponentBindingManager bindingManager;
        private CodeGenerator codeGenerator;
        private PrefabManager prefabManager;

        // UI绘制器
        private InfoSectionDrawer infoDrawer;
        private ActionButtonsDrawer actionDrawer;

        private void OnEnable()
        {
            // 初始化管理器
            bindingManager = new ComponentBindingManager();
            codeGenerator = new CodeGenerator();
            prefabManager = new PrefabManager();

            // 初始化绘制器
            infoDrawer = new InfoSectionDrawer(bindingManager, prefabManager);
            actionDrawer = new ActionButtonsDrawer(bindingManager, codeGenerator);
        }

        public override void OnInspectorGUI()
        {
            var autoUIBinderBase = target as AutoUIBinderBase;
            if (autoUIBinderBase == null) return;

            // 首先绘制默认的Inspector内容
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            // 使用模块化绘制器
            infoDrawer.DrawInfoSection(autoUIBinderBase);

            EditorGUILayout.Space(5);

            actionDrawer.DrawActionButtons(autoUIBinderBase);
        }
    }
}