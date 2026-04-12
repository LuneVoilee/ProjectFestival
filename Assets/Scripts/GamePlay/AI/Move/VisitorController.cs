#region

using System;
using System.Collections.Generic;
using Core;
using GamePlay.Building;
using GamePlay.Level;
using Tool;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace GamePlay.AI
{
    [RequireComponent(typeof(Navigator))]
    [RequireComponent(typeof(ContextController))]
    public class VisitorController : MonoBehaviour
    {
        public Navigator Navigator { get; private set; }
        public ContextController Mover { get; private set; } // Movement Controller

        public StateMachine StateMachine { get; private set; }
        public BaseBuilding SeekingBuilding { get; set; }

        //NOTICE:[SerializeField] private use for Debug
        [SerializeField] private string m_CurrentStateDisplayName;

        [SerializeField] private float m_HappinessGet;
        [SerializeField] private float m_UnhappinessGet;

        private List<BaseBuilding> m_VisitedBuildings;

        private void Awake()
        {
            Navigator = GetComponent<Navigator>();
            Mover = GetComponent<ContextController>();

            m_VisitedBuildings = new List<BaseBuilding>();

            var states = new VisitorBaseState[]
            {
                new VisitorWanderState(this),
                new VisitorMoveToTargetState(this),
                new VisitorInteractState(this),
                new VisitorExitState(this)
            };

            Type startState = typeof(VisitorWanderState);
            if (FindBestTarget(out var bestBuilding))
            {
                SeekingBuilding = bestBuilding;
                startState = typeof(VisitorMoveToTargetState);
            }

            StateMachine = new StateMachine(startState, states);

            foreach (var state in states)
            {
                state.SetStateMachine(StateMachine);
            }
        }

        private void Update()
        {
            StateMachine.OnUpdate();
            m_CurrentStateDisplayName = StateMachine.CurrentStateType.Name;
        }

        public void OnInteractionComplete()
        {
            if (SeekingBuilding != null)
            {
                m_VisitedBuildings.Add(SeekingBuilding);
            }

            if (FindBestTarget(out var building))
            {
                SeekingBuilding = building;
                StateMachine.SwitchState(typeof(VisitorMoveToTargetState));
            }
            else
            {
                StateMachine.SwitchState(typeof(VisitorWanderState));
            }
        }

        public bool FindBestTarget(out BaseBuilding bestBuilding)
        {
            bestBuilding = FindBestTargetInternal();
            return bestBuilding != null;
        }

        public void FeelHappy(float benefit = 0f)
        {
            if (m_HappinessGet >= 100f)
            {
                return;
            }

            GPEvents.OnFeelHappyAction?.Invoke(transform);

            var range = ValueManager.Instance.HappinessGetRange;
            var happyValue = benefit +
                             Random.Range(range.RandomHappinessMinGet, range.RandomHappinessMaxGet);

            ValueManager.Instance.AddHappiness(happyValue);

            m_HappinessGet += happyValue;
        }

        public void FeelUnhappy()
        {
            if (m_UnhappinessGet >= 100f)
            {
                return;
            }

            GPEvents.OnFeelUnhappyAction?.Invoke(transform);

            var range = ValueManager.Instance.HappinessGetRange;
            var unhappyValue =
                Random.Range(range.RandomUnhappinessMinGet, range.RandomUnhappinessMaxGet);

            ValueManager.Instance.AddHappiness(-unhappyValue);

            m_HappinessGet += unhappyValue;
        }

        private BaseBuilding FindBestTargetInternal()
        {
            // already optimized to the maximum — tough 

            var m_AllBuildings = BuildingManager.Instance.AllBuildings;

            if (m_AllBuildings == null || m_AllBuildings.Count == 0)
            {
                return null;
            }

            BaseBuilding b0 = null, b1 = null, b2 = null;
            float s0 = float.MinValue, s1 = float.MinValue, s2 = float.MinValue;

            foreach (var building in m_AllBuildings)
            {
                if (m_VisitedBuildings.Contains(building))
                {
                    continue;
                }

                var distance = Vector3.Distance(transform.position, building.VisitPosition);
                if (distance < 1f) distance = 1f;

                // NOTICE : Scoring mechanism
                // Current score = Satisfaction gain / Distance
                var score = building.Data.HappinessBenefit * building.Data.HappinessBenefit /
                            distance;

                if (score > s0)
                {
                    s2 = s1;
                    s1 = s0;
                    s0 = score;

                    b2 = b1;
                    b1 = b0;
                    b0 = building;
                }
                else if (score > s1)
                {
                    s2 = s1;
                    s1 = score;

                    b2 = b1;
                    b1 = building;
                }
                else if (score > s2)
                {
                    s2 = score;
                    b2 = building;
                }
            }

            var count = 0;
            if (b0) count++;
            if (b1) count++;
            if (b2) count++;

            if (count == 0)
            {
                return null;
            }

            var index = Random.Range(0, count);

            return index switch
            {
                0 => b0,
                1 => b1,
                2 => b2,
                _ => null
            };
        }
    }
}