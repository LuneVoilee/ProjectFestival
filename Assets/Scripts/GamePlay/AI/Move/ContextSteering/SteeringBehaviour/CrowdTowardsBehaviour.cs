#region

using System;
using UnityEngine;

#endregion

namespace GamePlay.AI
{
    public class CrowdTowardsBehaviour : ContextBehavior
    {
        public LayerMask ObstacleMask;

        private const int m_MaxObstacles = 5;
        private const float m_AvoidanceRadius = 10f;
        private const float m_Strength = 0.3f;

        public override void Evaluate(ContextMap map, Transform agentTransform)
        {
            var allGuys = Physics.OverlapSphere(agentTransform.position,
                m_AvoidanceRadius, ObstacleMask);

            // Sort by distance in ascending order and take the closest m_MaxObstacles
            Array.Sort(allGuys, (a, b) =>
            {
                var distA = Vector3.Distance(agentTransform.position,
                    a.ClosestPoint(agentTransform.position));
                var distB = Vector3.Distance(agentTransform.position,
                    b.ClosestPoint(agentTransform.position));
                return distA.CompareTo(distB);
            });

            var count = Mathf.Min(m_MaxObstacles, allGuys.Length);

            var averageForward = new Vector3();

            for (var j = 0; j < count; j++)
            {
                averageForward += allGuys[j].transform.forward;
            }

            averageForward = averageForward.normalized;

            for (var i = 0; i < ContextMap.GetDirectionCount(); i++)
            {
                var interest = Vector2.Dot(averageForward, ContextMap.GetDirection(i));
                if (interest < 0)
                {
                    interest = 0;
                }

                if (interest > m_Strength)
                {
                    interest = m_Strength;
                }

                map.Interest[i] += interest;
            }
        }
    }
}