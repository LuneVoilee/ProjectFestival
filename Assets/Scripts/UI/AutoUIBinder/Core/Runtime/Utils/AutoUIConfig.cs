#region

using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#endregion

namespace AutoUI
{
    [CreateAssetMenu(fileName = "UIPathConfig", menuName = "AutoUIBinder/Create UI Path Config")]
    public class AutoUIConfig : ScriptableObject
    {
        [Header("代码生成路径")] [SerializeField, ReadOnly]
        private string m_Paths = "Assets/Scripts/";

        [Header("命名规则配置")] [SerializeField]
        private NamingConvention m_NamingConvention = NamingConvention.NodeName_ComponentType;

        [SerializeField] [Tooltip("是否允许在Inspector中手动编辑Key")]
        private bool m_AllowKeyEditing = true;

        [Header("UI命名空间")] [SerializeField] private string m_NameSpace = "UI";

        /// <summary>
        ///     代码生成路径
        /// </summary>
        public string Paths => m_Paths;


        /// <summary>
        ///     UI命名空间
        /// </summary>
        public string NameSpace => m_NameSpace;

        /// <summary>
        ///     当前命名规则
        /// </summary>
        public NamingConvention CurrentNamingConvention => m_NamingConvention;

        /// <summary>
        ///     是否允许手动编辑Key
        /// </summary>
        public bool AllowKeyEditing => m_AllowKeyEditing;

        /// <summary>
        ///     获取当前命名策略
        /// </summary>
        public INamingStrategy GetCurrentStrategy()
        {
            return NamingStrategyFactory.CreateStrategy(m_NamingConvention);
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(AutoUIConfig))]
        public class AutoUIConfigEditor : Editor
        {
            private SerializedProperty m_PathsProperty;
            private SerializedProperty m_NameSpaceProperty;

            private SerializedProperty m_NamingConventionProperty;
            private SerializedProperty m_AllowKeyEditingProperty;

            private void OnEnable()
            {
                m_PathsProperty = serializedObject.FindProperty("m_Paths");
                m_NameSpaceProperty = serializedObject.FindProperty("m_NameSpace");
                m_NamingConventionProperty = serializedObject.FindProperty("m_NamingConvention");
                m_AllowKeyEditingProperty = serializedObject.FindProperty("m_AllowKeyEditing");
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                AutoUIConfig config = (AutoUIConfig)target;

                // ========== 路径配置部分 ==========
                EditorGUILayout.LabelField("代码生成路径", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField("路径", config.m_Paths);
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("选择文件夹", GUILayout.Width(100)))
                    {
                        string selectedPath = EditorUtility.OpenFolderPanel("选择文件夹", "Assets", "");
                        if (!string.IsNullOrEmpty(selectedPath))
                        {
                            string relativePath = GetRelativePath(selectedPath);
                            if (!string.IsNullOrEmpty(relativePath))
                            {
                                m_PathsProperty.stringValue = relativePath;
                                serializedObject.ApplyModifiedProperties();
                            }
                        }
                    }
                }

                EditorGUILayout.Space(15);

                // ========== 命名规则配置部分 ==========
                //EditorGUILayout.LabelField("命名规则配置", EditorStyles.boldLabel);

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(m_NamingConventionProperty, new GUIContent("命名规则"));

                // 显示当前规则的示例
                var currentConvention = (NamingConvention)m_NamingConventionProperty.enumValueIndex;
                var strategy = NamingStrategyFactory.CreateStrategy(currentConvention);
                string example = strategy.GetExample("LoginButton", "Button");
                EditorGUILayout.Space(5);

                EditorGUILayout.HelpBox($"示例: {example}", MessageType.None, true);

                EditorGUILayout.Space(5);

                EditorGUILayout.PropertyField(m_AllowKeyEditingProperty,
                    new GUIContent("允许手动编辑Key"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.Space(10);

                // ========== 预览按钮 ==========
                if (GUILayout.Button("预览所有命名规则", GUILayout.Height(25)))
                {
                    ShowNamingConventionPreview();
                }

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.PropertyField(m_NameSpaceProperty, new GUIContent("命名空间"));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }

            private void ShowNamingConventionPreview()
            {
                var examples = NamingStrategyFactory.GetAllExamples("LoginButton", "Button");
                string message = "各命名规则示例 (节点: LoginButton, 组件: Button):\n\n";

                foreach (var kvp in examples)
                {
                    var strategy = NamingStrategyFactory.CreateStrategy(kvp.Key);
                    message += $"• {strategy.DisplayName}:\n   {kvp.Value}\n\n";
                }

                EditorUtility.DisplayDialog("命名规则预览", message, "确定");
            }

            private string GetRelativePath(string absolutePath)
            {
                string projectPath = Path.GetFullPath(Application.dataPath + "/..");
                projectPath = projectPath.Replace("\\", "/");
                absolutePath = absolutePath.Replace("\\", "/");

                if (absolutePath.StartsWith(projectPath))
                {
                    string relativePath = absolutePath.Substring(projectPath.Length + 1);
                    return relativePath;
                }

                EditorUtility.DisplayDialog("错误", "请选择项目内的文件夹！", "确定");
                return "";
            }
        }
#endif
    }

    /// <summary>
    ///     自定义只读特性
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
        }
    }
#endif
}