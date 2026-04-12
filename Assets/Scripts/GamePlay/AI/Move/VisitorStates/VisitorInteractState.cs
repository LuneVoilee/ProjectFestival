namespace GamePlay.AI
{
    public class VisitorInteractState : VisitorBaseState
    {
        public VisitorInteractState(VisitorController controller) : base(controller)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            m_Controller.Mover.Stop();
        }

        public override void OnUpdate()
        {
            // NOTE: After the interaction ends, the agent will either wander or search again;
            // this logic is handled by the building.
        }

        public override void OnExit()
        {
            base.OnExit();
            m_Controller.Mover.Restore();
        }
    }
}