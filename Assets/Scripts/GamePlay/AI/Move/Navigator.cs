#region

using UnityEngine;
using UnityEngine.AI;

#endregion

namespace GamePlay.AI
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class Navigator : MonoBehaviour
    {
        private NavMeshAgent m_Agent;
        private Vector3[] m_Corners;
        private int m_CurrentCornerIndex;
        private bool m_HasPath;

        // The current waypoint to move toward
        public Vector3? CurrentWaypoint => m_HasPath && m_Corners != null &&
                                           m_CurrentCornerIndex < m_Corners.Length
            ? m_Corners[m_CurrentCornerIndex]
            : null;

        // Whether the agent has reached the final destination
        public bool HasReachedDestination => !m_HasPath;

        [SerializeField] private float m_WaypointReachedDistance = 1.0f;

        private void Awake()
        {
            m_Agent = GetComponent<NavMeshAgent>();

            m_Agent.updatePosition = false;
            m_Agent.updateRotation = false;
        }

        private void Update()
        {
            m_Agent.nextPosition = transform.position;

            if (!m_HasPath) return;

            if (CurrentWaypoint.HasValue &&
                Vector3.Distance(transform.position, CurrentWaypoint.Value) <
                m_WaypointReachedDistance)
            {
                m_CurrentCornerIndex++;
                if (m_CurrentCornerIndex >= m_Corners.Length)
                {
                    m_HasPath = false;
                }
            }
        }

        public void SetDestination(Vector3 destination)
        {
            if (!m_Agent.enabled || !gameObject.activeSelf)
            {
                return;
            }

            var path = new NavMeshPath();

            if (m_Agent.CalculatePath(destination, path) &&
                path.status == NavMeshPathStatus.PathComplete)
            {
                m_Corners = path.corners;
                m_CurrentCornerIndex = 1; // The 0th point is the agent's current position, so we start from the 1st waypoint
                m_HasPath = m_Corners.Length > 1;
            }
            else
            {
                // Path calculation failed
                m_HasPath = false;
            }
        }


        public void Clear()
        {
            m_HasPath = false;
            m_CurrentCornerIndex = 0;
            m_Corners = null;
        }

        public Vector3[] GetPathCorners()
        {
            return m_HasPath ? m_Corners : null;
        }

        private void OnDrawGizmosSelected()
        {
            if (m_HasPath && m_Corners != null)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < m_Corners.Length - 1; i++)
                {
                    Gizmos.DrawLine(m_Corners[i], m_Corners[i + 1]);
                    Gizmos.DrawSphere(m_Corners[i + 1], 0.1f);
                }
            }
        }
    }
}