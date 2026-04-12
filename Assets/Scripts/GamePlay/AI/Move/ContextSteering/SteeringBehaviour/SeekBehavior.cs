#region

using UnityEngine;

#endregion

namespace GamePlay.AI
{
    [RequireComponent(typeof(Navigator))]
    public class SeekBehavior : ContextBehavior
    {
        public Vector3 Target;
        private Navigator m_Navigator;

        [Range(0, 1)] public float Weight = 1.0f;

        private void Awake()
        {
            m_Navigator = GetComponent<Navigator>();
        }

        public void SetBestTarget(Vector3 pos)
        {
            Target = pos;
            m_Navigator.SetDestination(Target);
        }

        public override void Evaluate(ContextMap map, Transform agentTransform)
        {
            m_Navigator.SetDestination(Target);

            if (m_Navigator.CurrentWaypoint == null)
            {
                return;
            }

            Vector3 targetPosition = m_Navigator.CurrentWaypoint.Value;

            Vector3 directionToTarget3D = (targetPosition - agentTransform.position);
            Vector2 directionToTarget2D =
                new Vector2(directionToTarget3D.x, directionToTarget3D.z).normalized;

            for (int i = 0; i < ContextMap.GetDirectionCount(); i++)
            {
                var direction = ContextMap.GetDirection(i);

                var dot = Vector2.Dot(directionToTarget2D, direction);

                if (dot > 0)
                {
                    map.Interest[i] += dot * Weight;
                }
            }
        }
    }
}