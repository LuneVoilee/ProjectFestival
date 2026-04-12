#region

using System;
using System.Collections.Generic;

#endregion

namespace Core.Reactive
{
    public class ReactiveValue<T>
    {
        //current value
        private T m_Value;

        //Two parameters: old value and new value
        private Action<T, T> m_OnValueChanged;

        public T Value
        {
            get => m_Value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(m_Value, value))
                    return;

                var old = m_Value;
                m_Value = value;

                m_OnValueChanged?.Invoke(old, value);
            }
        }

        public ReactiveValue(T initial = default)
        {
            m_Value = initial;
        }

        public void Bind(Action<T, T> onChange)
        {
            m_OnValueChanged += onChange;
        }

        public void Unbind(Action<T, T> onChange)
        {
            m_OnValueChanged -= onChange;
        }

        public static implicit operator T(ReactiveValue<T> prop) => prop.Value;

        public override string ToString() => m_Value?.ToString() ?? "null";
    }
}