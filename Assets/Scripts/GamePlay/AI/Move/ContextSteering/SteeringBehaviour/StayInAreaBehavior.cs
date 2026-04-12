#region

using GamePlay.Grid;
using UnityEngine;

#endregion

namespace GamePlay.AI
{
    public class StayInAreaBehavior : ContextBehavior
    {
        private float m_MoveSpeed;

        private const float m_Weight = 1f;

        private void Awake()
        {
            if (!TryGetComponent(out ContextController controller))
            {
                Debug.LogWarning("no ContextSteeringController");
            }

            m_MoveSpeed = controller.MoveSpeed;
        }

        public override void Evaluate(ContextMap map, Transform agentTransform)
        {
            for (var i = 0; i < ContextMap.GetDirectionCount(); i++)
            {
                var dir = ContextMap.GetDirection(i);

                var futurePos = agentTransform.position +
                                new Vector3(dir.x, 0, dir.y) * (m_MoveSpeed * Time.deltaTime * 5);

                //Debug.Log("《StayInArea》：" + AreaManager.Instance.IsPositionLegal(futurePos));

                if (!AreaManager.Instance.IsPositionLegal(futurePos))
                {
                    map.Danger[i] += m_Weight;
                    //map.Interest[ContextMap.ReverseOf(i)] += m_Weight;
                }
            }
        }
    }
}