#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace AutoUI
{
    /// <summary>
    ///     抽象基类：继承此类的GameObject及其子对象将在Hierarchy窗口中显示组件图标
    /// </summary>
    public abstract class AutoUIBinderBase : MonoBehaviour
    {
        /// <summary>
        ///     用于存储组件引用的字典
        /// </summary>
        [SerializeField] [DictionaryDisplayName("UI节点绑定")]
        protected SerializableDictionary<string, Component> m_ComponentRefs = new();

        /// <summary>
        ///     组件引用字典（只读）
        /// </summary>
        public Dictionary<string, Component> ComponentRefs => m_ComponentRefs;

        protected virtual void Awake()
        {
            // UI组件绑定基类，子类可以重写此方法添加初始化逻辑
        }

        /// <summary>
        ///     添加组件引用
        /// </summary>
        public void AddComponentRef(string key, Component component)
        {
            m_ComponentRefs.TryAdd(key, component);
        }

        /// <summary>
        ///     移除组件引用
        /// </summary>
        public void RemoveComponentRef(string key)
        {
            m_ComponentRefs.Remove(key);
        }

        /// <summary>
        ///     获取指定类型的组件
        /// </summary>
        public T GetComponentRef<T>(string key) where T : Component
        {
            if (m_ComponentRefs.TryGetValue(key, out Component component))
            {
                return component as T;
            }

            return null;
        }

        /// <summary>
        ///     检查是否包含指定Key的组件
        /// </summary>
        public bool HasComponentRef(string key)
        {
            return m_ComponentRefs.ContainsKey(key);
        }

        /// <summary>
        ///     获取绑定的组件数量
        /// </summary>
        public int ComponentCount => m_ComponentRefs.Count;
    }
}