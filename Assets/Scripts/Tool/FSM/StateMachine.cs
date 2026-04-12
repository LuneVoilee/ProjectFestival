#region

using System;
using System.Collections.Generic;

#endregion

namespace Tool
{
    public class StateMachine
    {
        public Type CurrentStateType { get; private set; }

        private IState m_CurrentState;

        private readonly Dictionary<Type, IState> m_States = new();

        public StateMachine(Type startState, IEnumerable<IState> states)
        {
            foreach (var state in states)
            {
                m_States.Add(state.GetType(), state);
            }

            SwitchState(startState);
        }

        public void OnUpdate()
        {
            m_CurrentState?.OnUpdate();
        }

        public void SwitchState(Type newStateType)
        {
            m_CurrentState?.OnExit();
            m_CurrentState = m_States[newStateType];
            CurrentStateType = newStateType;
            m_CurrentState?.OnEnter();
        }
    }
}