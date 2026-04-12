#region

using Tool;

#endregion

namespace GamePlay.AI
{
    public abstract class VisitorBaseState : IState
    {
        protected readonly VisitorController m_Controller;
        protected StateMachine m_StateMachine;

        protected VisitorBaseState(VisitorController controller)
        {
            m_Controller = controller;
        }

        public void SetStateMachine(StateMachine stateMachine)
        {
            m_StateMachine = stateMachine;
        }

        public virtual void OnEnter()
        {
            //Debug.Log($"Entering state: {GetType().Name}");
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnExit()
        {
            //Debug.Log($"Exiting state: {GetType().Name}");
        }
    }
}