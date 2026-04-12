#region

using UnityEngine;

#endregion

namespace Tool
{
    public abstract class PersistentSingletonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T m_Instance;

        // ReSharper disable once StaticMemberInGenericType
        private static bool m_IsShuttingDown;

        public static T Instance
        {
            get
            {
                if (m_IsShuttingDown)
                {
                    return null;
                }

                if (m_Instance == null)
                {
                    m_Instance = FindFirstObjectByType<T>();

                    if (!m_Instance)
                    {
                        GameObject singletonObject = new GameObject();
                        m_Instance = singletonObject.AddComponent<T>();
                        singletonObject.name = $"{typeof(T).Name} (Singleton)";
                        DontDestroyOnLoad(singletonObject);
                    }
                }

                return m_Instance;
            }
        }

        protected virtual void Awake()
        {
            if (m_Instance == null)
            {
                m_Instance = this as T;

                DontDestroyOnLoad(gameObject);
            }
            else if (m_Instance != this)
            {
                Debug.LogWarning($" {typeof(T).Name} is defined more than once. Removing {gameObject.name}");
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_Instance == this)
            {
                m_IsShuttingDown = true;
                m_Instance = null;
            }
        }

        public static void DestroyInstance()
        {
            if (m_Instance != null)
            {
                Destroy(m_Instance.gameObject);
                m_IsShuttingDown = true;
                m_Instance = null;
            }
        }
    }
}