#region

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace AutoUI
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const float LINE_HEIGHT = 20f;
        private const float SPACING = 2f;
        private const float HEADER_HEIGHT = 22f;
        private const float EDIT_BUTTON_WIDTH = 36f;
        private const float CONFIRM_BUTTON_WIDTH = 20f;

        private bool m_IsExpanded = true;

        // 编辑状态追踪
        private static readonly Dictionary<string, bool> s_EditingStates =
            new Dictionary<string, bool>();

        private static readonly Dictionary<string, string> s_PendingKeyChanges =
            new Dictionary<string, string>();

        private static readonly Dictionary<string, string> s_OriginalKeys =
            new Dictionary<string, string>();

        // 配置缓存
        private static AutoUIConfig s_CachedConfig;

        // 自定义颜色
        private static readonly Color HEADER_COLOR = new Color(0.2f, 0.2f, 0.2f, 1f);
        private static readonly Color HEADER_HOVER_COLOR = new Color(0.3f, 0.3f, 0.3f, 1f);
        private static readonly Color HEADER_TEXT_COLOR = new Color(0.9f, 0.9f, 0.9f);
        private static readonly Color ITEM_BACKGROUND_COLOR = new Color(0.85f, 0.85f, 0.85f, 0.1f);
        private static readonly Color HIGHLIGHT_COLOR = new Color(0.2f, 0.4f, 0.8f, 0.2f);
        private static readonly Color LABEL_COLOR = new Color(0.4f, 0.4f, 0.4f, 1f);
        private static readonly Color EDIT_MODE_COLOR = new Color(0.3f, 0.5f, 0.8f, 0.3f);

        private AutoUIConfig GetConfig()
        {
            if (s_CachedConfig == null)
            {
                s_CachedConfig = Resources.Load<AutoUIConfig>("GlobalConfig");
            }

            return s_CachedConfig;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 获取自定义显示名称
            string displayName = label.text;
            var parentObject = property.serializedObject.targetObject;
            var field = parentObject.GetType().GetField(property.name,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                var attr =
                    Attribute.GetCustomAttribute(field, typeof(DictionaryDisplayNameAttribute)) as
                        DictionaryDisplayNameAttribute;
                if (attr != null)
                {
                    displayName = attr.DisplayName;
                }
            }

            EditorGUI.BeginProperty(position, label, property);

            // 绘制标题区域
            DrawHeader(position, displayName);

            if (m_IsExpanded)
            {
                var pairsProperty = property.FindPropertyRelative("m_Pairs");
                float yOffset = HEADER_HEIGHT + SPACING;

                // 获取配置
                var config = GetConfig();
                bool allowEditing = config != null && config.AllowKeyEditing;

                // 绘制列标题
                DrawColumnHeaders(position, ref yOffset, allowEditing);

                // 绘制所有键值对
                for (int i = 0; i < pairsProperty.arraySize; i++)
                {
                    DrawKeyValuePair(position, pairsProperty, i, ref yOffset, allowEditing,
                        property);
                }
            }

            EditorGUI.EndProperty();
        }

        private void DrawHeader(Rect position, string displayName)
        {
            var headerRect = new Rect(position.x, position.y, position.width, HEADER_HEIGHT);
            var headerBorderRect = new Rect(headerRect.x, headerRect.y + headerRect.height - 1,
                headerRect.width, 1);

            // 检查鼠标是否悬停在标题上
            bool headerHovering = headerRect.Contains(Event.current.mousePosition);
            EditorGUI.DrawRect(headerRect, headerHovering ? HEADER_HOVER_COLOR : HEADER_COLOR);
            EditorGUI.DrawRect(headerBorderRect, new Color(0, 0, 0, 0.4f));

            // 自定义折叠箭头
            var arrowRect = new Rect(position.x + 4, position.y + (HEADER_HEIGHT - 13) / 2, 13, 13);
            if (Event.current.type == EventType.Repaint)
            {
                if (headerHovering)
                {
                    EditorGUI.DrawRect(
                        new Rect(arrowRect.x - 2, arrowRect.y - 2, arrowRect.width + 4,
                            arrowRect.height + 4),
                        new Color(1, 1, 1, 0.1f));
                }

                var arrowPath = new Vector3[3];
                if (m_IsExpanded)
                {
                    arrowPath[0] = new Vector3(arrowRect.x + 2, arrowRect.y + 4);
                    arrowPath[1] = new Vector3(arrowRect.x + arrowRect.width - 2, arrowRect.y + 4);
                    arrowPath[2] = new Vector3(arrowRect.x + arrowRect.width / 2,
                        arrowRect.y + arrowRect.height - 2);
                }
                else
                {
                    arrowPath[0] = new Vector3(arrowRect.x + 4, arrowRect.y + 2);
                    arrowPath[1] = new Vector3(arrowRect.x + 4, arrowRect.y + arrowRect.height - 2);
                    arrowPath[2] = new Vector3(arrowRect.x + arrowRect.width - 2,
                        arrowRect.y + arrowRect.height / 2);
                }

                Handles.color = HEADER_TEXT_COLOR;
                Handles.DrawAAConvexPolygon(arrowPath);
            }

            // 处理箭头点击
            if (Event.current.type == EventType.MouseDown &&
                headerRect.Contains(Event.current.mousePosition))
            {
                m_IsExpanded = !m_IsExpanded;
                Event.current.Use();
                GUI.changed = true;
            }

            // 绘制标题文本
            var titleStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = HEADER_TEXT_COLOR },
                fontStyle = FontStyle.Bold,
                fontSize = 12,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };
            var titleRect = new Rect(arrowRect.xMax + 4, position.y + (HEADER_HEIGHT - 16) / 2,
                position.width - arrowRect.xMax - 8, 16);
            EditorGUI.LabelField(titleRect, displayName, titleStyle);
        }

        private void DrawColumnHeaders(Rect position, ref float yOffset, bool allowEditing)
        {
            var columnHeaderRect =
                new Rect(position.x, position.y + yOffset, position.width, LINE_HEIGHT);
            EditorGUI.DrawRect(columnHeaderRect, new Color(0.3f, 0.3f, 0.3f, 0.2f));

            var labelStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = LABEL_COLOR },
                fontStyle = FontStyle.Bold,
                fontSize = 11
            };

            EditorGUI.LabelField(
                new Rect(position.x + 4, columnHeaderRect.y + 2, 20, LINE_HEIGHT),
                "#", labelStyle);

            string keyLabel = allowEditing ? "Key (可编辑)" : "Key (组件名称)";
            EditorGUI.LabelField(
                new Rect(position.x + 28, columnHeaderRect.y + 2, position.width * 0.35f,
                    LINE_HEIGHT),
                keyLabel, labelStyle);

            EditorGUI.LabelField(
                new Rect(position.x + position.width * 0.47f, columnHeaderRect.y + 2,
                    position.width * 0.43f, LINE_HEIGHT),
                "Value (组件引用)", labelStyle);

            EditorGUI.LabelField(
                new Rect(position.x + position.width - 20, columnHeaderRect.y + 2, 16, LINE_HEIGHT),
                "状态", labelStyle);

            yOffset += LINE_HEIGHT + SPACING;
        }

        private void DrawKeyValuePair
        (
            Rect position, SerializedProperty pairsProperty, int index,
            ref float yOffset, bool allowEditing, SerializedProperty rootProperty
        )
        {
            var pairProperty = pairsProperty.GetArrayElementAtIndex(index);
            var keyProperty = pairProperty.FindPropertyRelative("Key");
            var valueProperty = pairProperty.FindPropertyRelative("Value");

            string uniqueId = $"{rootProperty.propertyPath}_{index}";

            // 绘制项背景
            var itemRect = new Rect(position.x, position.y + yOffset, position.width, LINE_HEIGHT);
            EditorGUI.DrawRect(itemRect, index % 2 == 0 ? ITEM_BACKGROUND_COLOR : Color.clear);

            // 检查编辑状态
            bool isEditing = s_EditingStates.ContainsKey(uniqueId) && s_EditingStates[uniqueId];

            // 编辑模式背景
            if (isEditing)
            {
                EditorGUI.DrawRect(itemRect, EDIT_MODE_COLOR);
            }
            else
            {
                // 鼠标悬停效果
                bool isHovering = itemRect.Contains(Event.current.mousePosition);
                if (isHovering)
                {
                    EditorGUI.DrawRect(itemRect, HIGHLIGHT_COLOR);
                }
            }

            // 绘制索引编号
            var indexRect = new Rect(position.x + 4, position.y + yOffset + 2, 20, LINE_HEIGHT - 4);
            var indexStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.6f, 0.6f, 0.6f) },
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUI.LabelField(indexRect, (index + 1).ToString(), indexStyle);

            // 绘制Key字段
            DrawKeyField(position, yOffset, keyProperty, pairsProperty, index, allowEditing,
                uniqueId, rootProperty);

            // 绘制箭头符号
            var arrowStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.5f, 0.5f, 0.5f) },
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            var itemArrowRect = new Rect(position.x + position.width * 0.42f,
                position.y + yOffset + 2, 20, LINE_HEIGHT - 4);
            EditorGUI.LabelField(itemArrowRect, "→", arrowStyle);

            // 绘制值（只读）
            using (new EditorGUI.DisabledScope(true))
            {
                var valueRect = new Rect(position.x + position.width * 0.47f, position.y + yOffset,
                    position.width * 0.48f, LINE_HEIGHT);
                EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);
            }

            // 绘制状态指示器
            DrawStatusIndicator(position, yOffset, valueProperty);

            yOffset += LINE_HEIGHT + SPACING;
        }

        private void DrawKeyField
        (
            Rect position, float yOffset, SerializedProperty keyProperty,
            SerializedProperty pairsProperty, int index, bool allowEditing,
            string uniqueId, SerializedProperty rootProperty
        )
        {
            bool isEditing = s_EditingStates.ContainsKey(uniqueId) && s_EditingStates[uniqueId];

            if (!allowEditing)
            {
                // 不允许编辑时，显示只读字段
                using (new EditorGUI.DisabledScope(true))
                {
                    var keyRect = new Rect(position.x + 28, position.y + yOffset,
                        position.width * 0.35f, LINE_HEIGHT);
                    EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
                }

                return;
            }

            if (isEditing)
            {
                // 编辑模式
                string pendingKey = s_PendingKeyChanges.ContainsKey(uniqueId)
                    ? s_PendingKeyChanges[uniqueId]
                    : keyProperty.stringValue;

                float keyFieldWidth = position.width * 0.35f - CONFIRM_BUTTON_WIDTH * 2 - 8;
                var keyRect = new Rect(position.x + 28, position.y + yOffset, keyFieldWidth,
                    LINE_HEIGHT);

                EditorGUI.BeginChangeCheck();
                string newKey = EditorGUI.TextField(keyRect, pendingKey);
                if (EditorGUI.EndChangeCheck())
                {
                    s_PendingKeyChanges[uniqueId] = newKey;
                }

                // 确认按钮
                var confirmRect = new Rect(keyRect.xMax + 2, position.y + yOffset + 2,
                    CONFIRM_BUTTON_WIDTH, LINE_HEIGHT - 4);
                GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
                if (GUI.Button(confirmRect, "✓", EditorStyles.miniButton))
                {
                    if (ValidateAndApplyKeyChange(keyProperty, newKey, pairsProperty, index,
                            rootProperty))
                    {
                        s_EditingStates[uniqueId] = false;
                        s_PendingKeyChanges.Remove(uniqueId);
                        s_OriginalKeys.Remove(uniqueId);
                    }
                }

                // 取消按钮
                var cancelRect = new Rect(confirmRect.xMax + 2, position.y + yOffset + 2,
                    CONFIRM_BUTTON_WIDTH, LINE_HEIGHT - 4);
                GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
                if (GUI.Button(cancelRect, "✗", EditorStyles.miniButton))
                {
                    s_EditingStates[uniqueId] = false;
                    s_PendingKeyChanges.Remove(uniqueId);
                    s_OriginalKeys.Remove(uniqueId);
                }

                GUI.backgroundColor = Color.white;
            }
            else
            {
                // 显示模式
                float keyFieldWidth = position.width * 0.35f - EDIT_BUTTON_WIDTH - 4;
                var keyRect = new Rect(position.x + 28, position.y + yOffset, keyFieldWidth,
                    LINE_HEIGHT);

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUI.PropertyField(keyRect, keyProperty, GUIContent.none);
                }

                // 编辑按钮
                var editRect = new Rect(keyRect.xMax + 2, position.y + yOffset + 2,
                    EDIT_BUTTON_WIDTH, LINE_HEIGHT - 4);
                GUI.backgroundColor = new Color(0.6f, 0.7f, 0.9f);
                if (GUI.Button(editRect, "编辑", EditorStyles.miniButton))
                {
                    s_EditingStates[uniqueId] = true;
                    s_PendingKeyChanges[uniqueId] = keyProperty.stringValue;
                    s_OriginalKeys[uniqueId] = keyProperty.stringValue;
                }

                GUI.backgroundColor = Color.white;
            }
        }

        private void DrawStatusIndicator
            (Rect position, float yOffset, SerializedProperty valueProperty)
        {
            var statusRect = new Rect(position.x + position.width - 20, position.y + yOffset + 2,
                16, LINE_HEIGHT - 4);
            var component = valueProperty.objectReferenceValue as Component;

            var statusStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };

            if (component != null)
            {
                statusStyle.normal.textColor = Color.green;
                EditorGUI.LabelField(statusRect, "✓", statusStyle);
            }
            else
            {
                statusStyle.normal.textColor = Color.red;
                EditorGUI.LabelField(statusRect, "✗", statusStyle);
            }
        }

        private bool ValidateAndApplyKeyChange
        (
            SerializedProperty keyProperty, string newKey,
            SerializedProperty pairsProperty, int currentIndex,
            SerializedProperty rootProperty
        )
        {
            // 1. 空值检查
            if (string.IsNullOrWhiteSpace(newKey))
            {
                EditorUtility.DisplayDialog("错误", "Key不能为空！", "确定");
                return false;
            }

            // 2. 非法字符检查
            if (newKey.Contains(" "))
            {
                EditorUtility.DisplayDialog("错误", "Key不能包含空格！", "确定");
                return false;
            }

            // 3. 重复检查
            for (int i = 0; i < pairsProperty.arraySize; i++)
            {
                if (i == currentIndex) continue;

                var otherKeyProperty =
                    pairsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("Key");
                if (otherKeyProperty.stringValue == newKey)
                {
                    EditorUtility.DisplayDialog("错误", $"Key '{newKey}' 已存在！", "确定");
                    return false;
                }
            }

            // 4. 获取旧Key
            string oldKey = keyProperty.stringValue;

            // 5. 应用更改到SerializedProperty
            keyProperty.stringValue = newKey;
            rootProperty.serializedObject.ApplyModifiedProperties();

            // 6. 同步底层Dictionary
            SyncDictionaryKey(rootProperty.serializedObject.targetObject, oldKey, newKey);

            return true;
        }

        private void SyncDictionaryKey(Object target, string oldKey, string newKey)
        {
            var handler = target as AutoUIBinderBase;
            if (handler == null) return;

            if (handler.ComponentRefs.TryGetValue(oldKey, out Component component))
            {
                handler.RemoveComponentRef(oldKey);
                handler.AddComponentRef(newKey, component);
                EditorUtility.SetDirty(handler);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!m_IsExpanded)
                return HEADER_HEIGHT;

            var pairsProperty = property.FindPropertyRelative("m_Pairs");
            float height = HEADER_HEIGHT + SPACING; // 标题高度
            height += LINE_HEIGHT + SPACING; // 列标题高度
            height += pairsProperty.arraySize * (LINE_HEIGHT + SPACING); // 所有项的高度
            return height;
        }
    }
}