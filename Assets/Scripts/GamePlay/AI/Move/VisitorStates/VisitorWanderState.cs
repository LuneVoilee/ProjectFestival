#region

using UnityEngine;

#endregion

namespace GamePlay.AI
{
    public class VisitorWanderState : VisitorBaseState
    {
        private float m_Timer;
        private int m_WanderCounter;

        private const int m_MaxWanderTimes = 3;
        private const float m_WanderDuration = 10f;

        public VisitorWanderState(VisitorController controller) : base(controller)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            m_Timer = m_WanderDuration;
            m_Controller.SeekingBuilding = null;

            m_WanderCounter++;

            m_Controller.Mover.Wander();
        }

        public override void OnUpdate()
        {
            if (m_WanderCounter >= m_MaxWanderTimes)
            {
                m_Controller.FeelUnhappy();

                m_StateMachine.SwitchState(typeof(VisitorExitState));
                return;
            }

            m_Timer -= Time.deltaTime;

            // After wandering, search for the next target
            if (m_Timer <= 0)
            {
                if (m_Controller.FindBestTarget(out var bestTarget))
                {
                    m_Controller.SeekingBuilding = bestTarget;
                    m_StateMachine.SwitchState(typeof(VisitorMoveToTargetState));
                    return;
                }

                // If no target is found, wander again and accumulate dissatisfaction
                m_Timer += m_WanderDuration;
                m_WanderCounter++;
            }
        }
    }
}