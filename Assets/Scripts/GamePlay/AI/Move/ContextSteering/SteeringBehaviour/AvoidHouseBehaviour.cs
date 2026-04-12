#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace GamePlay.AI
{
    [RequireComponent(typeof(SeekBehavior))]
    public class AvoidHouseBehaviour : ContextBehavior
    {
        public LayerMask ObstacleMask;
        private const float m_AvoidanceRadius = 30f;
        private const float m_Strength = 1f;
        private SeekBehavior m_Seek;
        private Collider m_CurrentAvoidTarget;
        private const int m_MaxObstacles = 1;

        // 60fps * 5s
        private const int m_MaxAvoidingFrames = 120;
        private bool m_IsAvoiding;
        private List<int> m_AvoidingIndexs;
        private int m_IsAvoidingCounter;

        private void Awake()
        {
            //ObstacleMask = LayerMask.GetMask("Building", "Visitor");
            ////Debug.Log(ObstacleMask.value);
            m_AvoidingIndexs = new List<int>();
            m_Seek = GetComponent<SeekBehavior>();
            m_IsAvoidingCounter = m_MaxAvoidingFrames;
        }


        public override void Evaluate(ContextMap map, Transform agentTransform)
        {
            if (m_IsAvoiding)
            {
                m_IsAvoidingCounter--;

                foreach (var i in m_AvoidingIndexs)
                {
                    HandleMap(map, i);
                }

                if (m_IsAvoidingCounter <= 0)
                {
                    m_IsAvoiding = false;
                    m_AvoidingIndexs.Clear();
                    m_IsAvoidingCounter = m_MaxAvoidingFrames;
                }

                //return;
            }

            var allColliders =
                Physics.OverlapSphere(agentTransform.position, m_AvoidanceRadius, ObstacleMask);

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
            for (var i = 0; i < count; i++)
            {
                var pos = allColliders[i].transform.position;

                if (Vector3.Distance(pos, m_Seek.Target) < 5f)
                {
                    Debug.Log("Detected");
                    continue;
                }

                var closestPoint = allColliders[i].ClosestPoint(agentTransform.position);

                var directionToObstacle = new Vector2(closestPoint.x - agentTransform.position.x,
                    closestPoint.z - agentTransform.position.z).normalized;

                var distance = Vector3.Distance(agentTransform.position, closestPoint);
                if (distance <= 0.001f) distance = 0.001f; // prevent 0
                var danger = (m_AvoidanceRadius - distance) / m_AvoidanceRadius;
                if (danger > 0.6f)
                {
                    danger += 0.15f;
                }

                for (var j = 0; j < ContextMap.GetDirectionCount(); j++)
                {
                    var direction = ContextMap.GetDirection(j).normalized;
                    var dot = Vector2.Dot(directionToObstacle, direction);
                    if (dot > 0)
                    {
                        var temp = danger * dot;
                        if (temp > 0.3f)
                        {
                            temp *= 2f;
                        }

                        if (temp > m_Strength)
                        {
                            temp = m_Strength;
                        }

                        if (temp > 0.8f)
                        {
                            m_IsAvoiding = true;
                            if (!m_AvoidingIndexs.Contains(j))
                            {
                                m_AvoidingIndexs.Add(j);
                            }

                            HandleMap(map, j);
                        }
                    }
                }
            }
        }

        private void HandleMap(ContextMap map, int j)
        {
            map.Disable[j] = true;
            map.Disable[ContextMap.LeftOf(j)] = true;
            map.Disable[ContextMap.RightOf(j)] = true;

            map.Interest[
                ContextMap.BetterOf(
                    ContextMap.LeftOf(ContextMap.LeftOf(j)),
                    ContextMap.RightOf(ContextMap.RightOf(j)),
                    m_Seek.Target)] += 0.2f;
        }
    }
}