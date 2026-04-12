#region

using System;
using System.Collections;
using System.Collections.Generic;
using GamePlay.Building;
using UnityEngine;

#endregion

namespace GamePlay.AI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(Navigator))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(SeekBehavior))]
    [RequireComponent(typeof(AvoidBehaviour))]
    [RequireComponent(typeof(WanderBehavior))]
    [RequireComponent(typeof(StayInAreaBehavior))]
    [RequireComponent(typeof(AntiStuckBehaviour))]
    [RequireComponent(typeof(AvoidHouseBehaviour))]
    [RequireComponent(typeof(CrowdTowardsBehaviour))]
    public class ContextController : MonoBehaviour
    {
        [Header("Debug")] public List<DebugLog> DebugLogs = new();
        public bool m_DrawGizmos = true;
        public string CurrentTargetName;

        [Header("Settings")] public float MoveSpeed = 20;

        private Navigator m_Navigator;
        private Animator m_Animator;
        private VisitorController m_Controller;

        private ContextBehavior[] m_Behaviors;
        private WanderBehavior m_WanderBehavior;
        private SeekBehavior m_SeekBehavior;
        private AvoidBehaviour m_AvoidBehaviour;
        private StayInAreaBehavior m_StayInAreaBehavior;
        private AntiStuckBehaviour m_AntiStuckBehaviour;
        private CrowdTowardsBehaviour m_CrowdTowardsBehavior;

        private Vector3 m_CurrentMove;
        private bool m_LastNeedToEnter;

        public Vector3 CurrentMove => m_CurrentMove;
        public ContextMap Map { get; private set; }


        private static readonly int m_IsMovingHash = Animator.StringToHash("isWalking");

        private void Awake()
        {
            Map = new ContextMap();
            m_Navigator = GetComponent<Navigator>();
            m_Animator = GetComponent<Animator>();
            m_Controller = GetComponent<VisitorController>();

            m_Behaviors = GetComponents<ContextBehavior>();
            m_WanderBehavior = GetComponent<WanderBehavior>();
            m_SeekBehavior = GetComponent<SeekBehavior>();
            m_AvoidBehaviour = GetComponent<AvoidBehaviour>();
            m_StayInAreaBehavior = GetComponent<StayInAreaBehavior>();
            m_AntiStuckBehaviour = GetComponent<AntiStuckBehaviour>();
            m_CrowdTowardsBehavior = GetComponent<CrowdTowardsBehaviour>();

            EnableMove();
        }

        private void Update()
        {
            Map.Reset();
            m_CurrentMove = Vector3.zero;

            UpdateBehaviors();

            UpdateMove();

            UpdateAnimation();
        }


        public void Wander()
        {
            m_Navigator.Clear();

            m_WanderBehavior.enabled = true;
            m_SeekBehavior.enabled = false;
        }


        public void Seek(BaseBuilding b)
        {
            //Needed for debugging, so we use b.name
            var target = b.VisitPosition;
            CurrentTargetName = b.Data.ID + "-" + b.name;
            Seek(target);
        }

        public void Seek(Vector3 pos)
        {
            m_WanderBehavior.enabled = false;
            m_SeekBehavior.enabled = true;

            m_SeekBehavior.SetBestTarget(pos);
        }


        public void Stop()
        {
            m_Navigator.Clear();
            m_LastNeedToEnter = m_Controller.SeekingBuilding.Data.NeedToEnterTheBuilding;
            if (m_LastNeedToEnter)
            {
                gameObject.SetActive(false);
            }
            else
            {
                DisableMove();

                var buildingPos = m_Controller.SeekingBuilding.transform.position;
                StartCoroutine(RotateToTarget(buildingPos));
            }
        }

        private IEnumerator RotateToTarget(Vector3 targetPos)
        {
            var dir = targetPos - transform.position;
            dir.y = 0;

            var targetRot = Quaternion.LookRotation(dir);

            while (Quaternion.Angle(transform.rotation, targetRot) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRot,
                    100f * Time.deltaTime
                );
                yield return null;
            }
        }

        public void Restore()
        {
            //NOTICE: SeekingBuilding may change before Restore, so we need to cache it
            if (m_LastNeedToEnter)
            {
                gameObject.SetActive(true);
            }
            else
            {
                EnableMove();
            }
        }

        private void DisableMove()
        {
            m_WanderBehavior.enabled = false;
            m_SeekBehavior.enabled = false;
            m_AvoidBehaviour.enabled = false;
            m_CrowdTowardsBehavior.enabled = false;
            m_StayInAreaBehavior.enabled = false;
            m_AntiStuckBehaviour.enabled = false;
        }

        private void EnableMove()
        {
            m_SeekBehavior.enabled = false;
            m_WanderBehavior.enabled = false;

            m_AvoidBehaviour.enabled = true;
            m_CrowdTowardsBehavior.enabled = true;
            m_StayInAreaBehavior.enabled = true;
            m_AntiStuckBehaviour.enabled = true;
        }

        private void UpdateAnimation()
        {
            m_Animator?.SetBool(m_IsMovingHash, m_CurrentMove.sqrMagnitude > 0.001f);
        }

        private void UpdateMove()
        {
            var finalDir = new Vector2();
            for (var i = 0; i < ContextMap.GetDirectionCount(); i++)
            {
                var interest = Map.Interest[i];
                var danger = Map.Danger[i];
                var disable = Map.Disable[i];

                //Remove the dangerous directions first, then merge the remaining safe ones
                if (disable || danger > 0.8f || danger > interest)
                {
                    continue;
                }

                //The key to behaviours like Avoid is slowing down
                //Because if we require Avoid's danger > 0.8f, the agent will come to a complete stop in dense crowds
                finalDir += ContextMap.GetDirection(i) * (interest - danger);
            }

            var moveDirection = finalDir.normalized;

            m_CurrentMove = new Vector3(moveDirection.x, 0, moveDirection.y) *
                            (MoveSpeed * Time.deltaTime);

            transform.position += m_CurrentMove;
            if (m_CurrentMove != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(m_CurrentMove), Time.deltaTime * 5f);
            }
        }

        private void UpdateBehaviors()
        {
            DebugLogs.Clear();
            foreach (var behavior in m_Behaviors)
            {
                if (!behavior.enabled) continue;

                var interestBefore = new float[ContextMap.GetDirectionCount()];
                var dangerBefore = new float[ContextMap.GetDirectionCount()];
                Array.Copy(Map.Interest, interestBefore, Map.Interest.Length);
                Array.Copy(Map.Danger, dangerBefore, Map.Danger.Length);

                behavior.Evaluate(Map, transform);

                var interestDelta = new float[ContextMap.GetDirectionCount()];
                var dangerDelta = new float[ContextMap.GetDirectionCount()];
                for (var i = 0; i < ContextMap.GetDirectionCount(); i++)
                {
                    interestDelta[i] = Map.Interest[i] - interestBefore[i];
                    dangerDelta[i] = Map.Danger[i] - dangerBefore[i];
                }

                DebugLogs.Add(new DebugLog(behavior.GetType().Name, interestDelta, dangerDelta));
            }
        }


        [Serializable]
        public struct DebugLog
        {
            public string Name;
            public float[] Interest;
            public float[] Danger;

            public DebugLog(string behaviorName, float[] interest, float[] danger)
            {
                Name = behaviorName;
                Interest = interest;
                Danger = danger;
            }
        }

        private void OnDrawGizmos()
        {
            if (!m_DrawGizmos || Map == null || !Application.isPlaying)
            {
                return;
            }

            Gizmos.color = Color.green;
            for (int i = 0; i < ContextMap.GetDirectionCount(); i++)
            {
                Vector3 direction = new Vector3(ContextMap.GetDirection(i).x, 0,
                    ContextMap.GetDirection(i).y);
                Gizmos.DrawRay(transform.position,
                    direction * (Map.Interest[i] > 1f ? 1 : Map.Interest[i]) *
                    2f);
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < ContextMap.GetDirectionCount(); i++)
            {
                Vector3 direction = new Vector3(ContextMap.GetDirection(i).x, 0,
                    ContextMap.GetDirection(i).y);
                if (Map.Disable[i])
                {
                    Gizmos.DrawRay(transform.position, direction * 1 * 2f);
                }
                else
                {
                    Gizmos.DrawRay(transform.position,
                        direction * (Map.Danger[i] > 1f ? 1f : Map.Danger[i]) *
                        2f);
                }
            }

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, m_CurrentMove * 2f);
        }
    }
}