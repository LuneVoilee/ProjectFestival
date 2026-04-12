#region

using UnityEngine;

#endregion

namespace GamePlay.AI
{
    public class VisitorExitState : VisitorBaseState
    {
        public VisitorExitState(VisitorController controller) : base(controller)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if (VisitorManager.Instance != null && VisitorManager.Instance.SpawnPoint != null)
            {
                m_Controller.Mover.Seek(VisitorManager.Instance.SpawnPoint.position);
            }
        }

        public override void OnUpdate()
        {
            if (VisitorManager.Instance == null || VisitorManager.Instance.SpawnPoint == null)
            {
                return;
            }

            var distance = Vector3.Distance(m_Controller.transform.position,
                VisitorManager.Instance.SpawnPoint.position);

            if (distance <= 50f)
            {
                VisitorManager.Instance.KillVisitor(m_Controller.gameObject);
            }
        }
    }
}