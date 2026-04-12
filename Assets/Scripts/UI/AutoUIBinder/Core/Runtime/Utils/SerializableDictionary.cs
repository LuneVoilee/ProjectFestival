using UnityEngine;
using System;
using System.Collections.Generic;

namespace AutoUI
{
    /// <summary>
    /// 可序列化的键值对
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    [Serializable]
    public class SerializableKeyValuePair<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;

        public SerializableKeyValuePair()
        {
        }

        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// 可序列化的字典类
    /// Unity原生的Dictionary无法序列化，此类通过ISerializationCallbackReceiver实现序列化
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<SerializableKeyValuePair<TKey, TValue>> m_Pairs = new List<SerializableKeyValuePair<TKey, TValue>>();

        /// <summary>
        /// 序列化前回调：将Dictionary数据转换为List
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_Pairs.Clear();
            foreach (KeyValuePair<TKey, TValue> kvp in this)
            {
                m_Pairs.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
            }
        }

        /// <summary>
        /// 反序列化后回调：将List数据恢复到Dictionary
        /// </summary>
        public void OnAfterDeserialize()
        {
            this.Clear();

            foreach (var pair in m_Pairs)
            {
                if (pair.Key != null && !ContainsKey(pair.Key))
                {
                    this.Add(pair.Key, pair.Value);
                }
            }
        }
    }
}
