#region

using System;
using System.Collections;
using MyInput;
using Tool;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

#endregion

namespace Core
{
    public class InputManager : PersistentSingletonMono<InputManager>,
        MyInputActions.IPlayerActions
    {
        private MyInputActions m_InputActions;

        public Vector2 MoveInput { get; private set; }
        public float ScrollInput { get; private set; }

        public float RotateInput { get; private set; }

        public Vector2 MousePosition { get; private set; }

        public Action OnClickAction;
        public Action OnRightClickAction;
        public Action OnESCClickAction;

        protected override void Awake()
        {
            base.Awake();
            m_InputActions = new MyInputActions();
            m_InputActions.Player.AddCallbacks(this);
        }

        private void OnEnable()
        {
            m_InputActions?.Enable();
        }

        private void OnDisable()
        {
            m_InputActions?.Disable();
        }

        private void LateUpdate()
        {
        }

        protected override void OnDestroy()
        {
            m_InputActions?.Dispose();
            base.OnDestroy();
        }

        public void DisableInput()
        {
            m_InputActions.Disable();
        }

        public void EnableInput()
        {
            m_InputActions.Enable();
        }

        public bool IsMouseLeftClick => Mouse.current.leftButton.wasReleasedThisFrame;

        #region Player Actions

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            StartCoroutine(OnClickDelay());
        }

        private IEnumerator OnClickDelay()
        {
            yield return null;

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                yield break;
            }

            OnClickAction?.Invoke();
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            OnRightClickAction?.Invoke();
        }


        public void OnPoint(InputAction.CallbackContext context)
        {
            MousePosition = context.ReadValue<Vector2>();
        }

        public void OnScroll(InputAction.CallbackContext context)
        {
            ScrollInput = context.ReadValue<Vector2>().y;
            //Debug.Log("Input"+ScrollInput);
        }

        public void OnRotate(InputAction.CallbackContext context)
        {
            RotateInput = context.ReadValue<float>();
        }

        public void OnESC(InputAction.CallbackContext context)
        {
            OnESCClickAction?.Invoke();
        }

        #endregion
    }
}