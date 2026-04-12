#region

using UnityEditor;
using UnityEngine;

#endregion

namespace AutoUI
{
    /// <summary>
    ///     预制体管理器 - 负责预制体相关操作
    /// </summary>
    public class PrefabManager
    {
        /// <summary>
        ///     打开预制体编辑模式
        /// </summary>
        public void OpenPrefabEditMode(AutoUIBinderBase target)
        {
            var gameObject = target.gameObject;

            var prefabType = PrefabUtility.GetPrefabInstanceStatus(gameObject);
            if (prefabType == PrefabInstanceStatus.Connected)
            {
                var prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
                if (prefabAsset != null)
                {
                    AssetDatabase.OpenAsset(prefabAsset);
                    Debug.Log($"[AutoUIBinder] 已打开预制体编辑模式: {prefabAsset.name}");
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "无法找到对应的预制体资源", "确定");
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("创建预制体",
                        "当前对象不是预制体实例。\n是否要将其保存为预制体？",
                        "创建预制体", "取消"))
                {
                    CreatePrefabFromGameObject(gameObject);
                }
            }
        }

        /// <summary>
        ///     从GameObject创建预制体
        /// </summary>
        public void CreatePrefabFromGameObject(GameObject gameObject)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "保存预制体",
                gameObject.name,
                "prefab",
                "请选择保存预制体的位置");

            if (!string.IsNullOrEmpty(path))
            {
                var prefab = PrefabUtility.SaveAsPrefabAsset(gameObject, path);
                if (prefab != null)
                {
                    Selection.activeObject = prefab;
                    EditorGUIUtility.PingObject(prefab);
                    AssetDatabase.OpenAsset(prefab);

                    Debug.Log($"[AutoUIBinder] 已创建预制体并打开编辑模式: {path}");
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "创建预制体失败", "确定");
                }
            }
        }
    }
}