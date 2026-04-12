#region

using System;
using UnityEngine;

#endregion

namespace GamePlay.AI
{
    public class AvoidBehaviour : ContextBehavior
    {
        public LayerMask ObstacleMask = 384;

        private const int m_MaxObstacles = 5;
        private const float m_AvoidanceRadius = 25f;
        private const float m_Strength = 0.8f;

        private void Awake()
        {
            //ObstacleMask = LayerMask.GetMask("Building", "Visitor");
            //Debug.Log(ObstacleMask.value);
        }


        public override void Evaluate(ContextMap map, Transform agentTransform)
        {
            var allColliders = Physics.OverlapSphere(agentTransform.position,
                m_AvoidanceRadius, ObstacleMask);

            if (allColliders.Length == 0)
                return;

            // Sort by distance in ascending order and take the closest m_MaxObstacles
            Array.Sort(allColliders, (a, b) =>
            {
                var distA = Vector3.Distance(agentTransform.position,
                    a.ClosestPoint(agentTransform.position));
                var distB = Vector3.Distance(agentTransform.position,
                    b.ClosestPoint(agentTransform.position));
                return distA.CompareTo(distB);
            });

            var count = Mathf.Min(m_MaxObstacles, allColliders.Length);

            for (var j = 0; j < ContextMap.GetDirectionCount(); j++)
            {
                var direction = ContextMap.GetDirection(j).normalized;
                var dangerOfDir = 0f;

                for (var i = 0; i < count; i++)
                {
                    var obstacle = allColliders[i];
                    var closestPoint = obstacle.ClosestPoint(agentTransform.position);

                    var directionToObstacle = new Vector2(
                        closestPoint.x - agentTransform.position.x,
                        closestPoint.z - agentTransform.position.z
                    ).normalized;

                    var distance = Vector3.Distance(agentTransform.position, closestPoint);
                    if (distance <= 0.001f) distance = 0.001f; // prevent 0

                    var danger = (m_AvoidanceRadius - distance) / m_AvoidanceRadius;
                    var dot = Vector2.Dot(directionToObstacle, direction);

                    if (dot > 0)
                    {
                        dangerOfDir += danger * dot;
                    }
                }

                if (dangerOfDir > m_Strength)
                {
                    dangerOfDir = m_Strength;
                }

                map.Danger[j] += dangerOfDir;
            }
        }
    }
}