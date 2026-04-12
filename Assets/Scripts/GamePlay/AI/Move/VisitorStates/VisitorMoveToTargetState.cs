#region

using UnityEngine;

#endregion

namespace GamePlay.AI
{
    public class VisitorMoveToTargetState : VisitorBaseState
    {
        public VisitorMoveToTargetState(VisitorController controller) : base(controller)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            if (m_Controller.SeekingBuilding != null)
            {
                //Debug.Log($"Heading to target: {m_Controller.SeekingBuilding.VisitPosition}");
                m_Controller.Mover.Seek(m_Controller.SeekingBuilding);
            }
            else
            {
                // If no valid target exists, return to the Wander state
                m_StateMachine.SwitchState(typeof(VisitorWanderState));
            }
        }

        public override void OnUpdate()
        {
            if (m_Controller.SeekingBuilding != null)
            {
                var distance = Vector3.Distance(m_Controller.transform.position,
                    m_Controller.SeekingBuilding.VisitPosition);

                if (distance <= 5f)
                {
                    var success = m_Controller.SeekingBuilding.OnVisitorArrived(m_Controller);

                    if (success)
                    {
                        m_StateMachine.SwitchState(typeof(VisitorInteractState));
                    }
                    else
                    {
                        m_StateMachine.SwitchState(typeof(VisitorWanderState));
                    }
                }
            }
            else
            {
                m_StateMachine.SwitchState(typeof(VisitorWanderState));
            }
        }
    }
}