#region

using GamePlay.Grid;
using UnityEngine;

#endregion

namespace GamePlay.AI
{

    public class WanderBehavior : ContextBehavior
    {
        private const float m_WanderStrength = 2f; 
        private const float m_NoiseSpeed = 0.2f; 

        private float m_NoiseOffsetX1;
        private float m_NoiseOffsetX2;
        private float m_NoiseOffsetY1;
        private float m_NoiseOffsetY2;
        private float m_MoveSpeed;

        private Vector2 m_DesiredDirectionInUpdate = Vector2.zero;

        private void Start()
        {
            float baseSeed = Random.Range(-10000f, 10000f);
            m_NoiseOffsetX1 = baseSeed + 10f;
            m_NoiseOffsetX2 = baseSeed + 20f;
            m_NoiseOffsetY1 = baseSeed + 30f;
            m_NoiseOffsetY2 = baseSeed + 40f;

            if (!TryGetComponent(out ContextController controller))
            {
                Debug.LogWarning("no ContextSteeringController");
            }

            m_MoveSpeed = controller.MoveSpeed;
        }

        public override void Evaluate(ContextMap map, Transform agentTransform)
        {
            bool shouldLoop = true;
            int attempts = 0;
            const int maxAttempts = 10;

            while (shouldLoop)
            {
                var timeInput = Time.time * m_NoiseSpeed;

                var p1 = Mathf.PerlinNoise(timeInput, m_NoiseOffsetX1);
                var p2 = Mathf.PerlinNoise(timeInput, m_NoiseOffsetX2);

                var p3 = Mathf.PerlinNoise(m_NoiseOffsetY1, timeInput);
                var p4 = Mathf.PerlinNoise(m_NoiseOffsetY2, timeInput);


                var x = p1 - p2;
                var y = p3 - p4;
                m_DesiredDirectionInUpdate.x = x;
                m_DesiredDirectionInUpdate.y = y;
                m_DesiredDirectionInUpdate.Normalize();

                var futurePos = agentTransform.position +
                                new Vector3(m_DesiredDirectionInUpdate.x, 0,
                                    m_DesiredDirectionInUpdate.y) *
                                (m_MoveSpeed * Time.deltaTime * 10);

                if (AreaManager.Instance.IsPositionLegal(futurePos))
                {
                    shouldLoop = false;
                }
                else
                {
                    float newSeed = Random.Range(-10000f, 10000f);
                    m_NoiseOffsetX1 = newSeed + 10f;
                    m_NoiseOffsetX2 = newSeed + 20f;
                    m_NoiseOffsetY1 = newSeed + 30f;
                    m_NoiseOffsetY2 = newSeed + 40f;

                    attempts++;
                    if (attempts >= maxAttempts)
                    {
                        shouldLoop = false;

                        Debug.LogWarning("WanderBehavior: cannot find valid position.");
                    }
                }
            }

            for (var i = 0; i < ContextMap.GetDirectionCount(); i++)
            {
                var direction = ContextMap.GetDirection(i);
                var dot = Vector2.Dot(m_DesiredDirectionInUpdate, direction);

                if (dot > 0)
                {
                    map.Interest[i] += dot * m_WanderStrength;
                }
            }
        }
    }
}