using UnityEngine;

namespace AutoUI
{
    /// <summary>
    /// 用于自定义字典在Inspector中的显示名称
    /// </summary>
    public class DictionaryDisplayNameAttribute : PropertyAttribute
    {
        public string DisplayName { get; private set; }
        public DictionaryDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
} 