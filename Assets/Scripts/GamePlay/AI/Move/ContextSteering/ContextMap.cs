#region

using UnityEngine;

#endregion

namespace GamePlay.AI
{
    public class ContextMap
    {
        public readonly float[] Interest = new float[m_DirectionCount];
        public readonly float[] Danger = new float[m_DirectionCount];
        public readonly bool[] Disable = new bool[m_DirectionCount];

        private const int m_DirectionCount = 8;
        private static readonly Vector2[] m_Directions;

        static ContextMap()
        {
            m_Directions = new Vector2[m_DirectionCount];
            for (var i = 0; i < m_DirectionCount; i++)
            {
                var angle = i * 360f / m_DirectionCount;
                m_Directions[i] = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad));
            }
        }


        public void Reset()
        {
            for (var i = 0; i < m_DirectionCount; i++)
            {
                Interest[i] = 0.1f;
                Danger[i] = 0;
                Disable[i] = false;
            }
        }


        public static Vector2 GetDirection
            (int index) => index is < 0 or >= m_DirectionCount ? Vector2.zero : m_Directions[index];

        public static int LeftOf(int i) => (i + 1) % m_DirectionCount;

        public static int RightOf(int i) => (i + m_DirectionCount - 1) % m_DirectionCount;

        //Between the two directions, choose the one that is closer to the desired direction
        public static int BetterOf(int i, int j, Vector3 target)
        {
            var dotI = Vector3.Dot(target, m_Directions[i]);
            var dotJ = Vector3.Dot(target, m_Directions[j]);
            return dotI > dotJ ? i : j;
        }

        public static int GetDirectionCount()
        {
            return m_DirectionCount;
        }
    }
}