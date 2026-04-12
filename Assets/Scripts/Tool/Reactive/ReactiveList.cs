#region

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Core.Reactive
{
    public class ReactiveList<T> : IEnumerable<T>
    {
        private readonly List<T> m_List = new();

        private Action<int, T> m_OnItemAdded;
        private Action<int, T> m_OnItemRemoved;
        private Action m_OnCleared;

        public int Count => m_List.Count;

        public T this[int index]
        {
            get => m_List[index];
            set => m_List[index] = value;
        }

        #region Bind/Unbind

        public void BindAdd(Action<int, T> handler) => m_OnItemAdded += handler;
        public void UnbindAdd(Action<int, T> handler) => m_OnItemAdded -= handler;

        public void BindRemove(Action<int, T> handler) => m_OnItemRemoved += handler;
        public void UnbindRemove(Action<int, T> handler) => m_OnItemRemoved -= handler;

        public void BindClear(Action handler) => m_OnCleared += handler;
        public void UnbindClear(Action handler) => m_OnCleared -= handler;

        #endregion

        #region List Operations

        public void Add(T item)
        {
            m_List.Add(item);
            m_OnItemAdded?.Invoke(m_List.Count - 1, item);
        }


        public void Clear()
        {
            m_List.Clear();
            m_OnCleared?.Invoke();
        }

        public IEnumerator<T> GetEnumerator() => m_List.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}