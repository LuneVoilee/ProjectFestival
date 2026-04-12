#region

using System.Collections;
using Core;
using Core.Reactive;
using GamePlay.Grid;
using UnityEngine;

#endregion

namespace GamePlay.Camera
{
    public class CameraController : MonoBehaviour
    {
        public static UnityEngine.Camera MainCam;

        [Header("Start Position")] public Transform StartTransform;
        public Vector3 StartOffset;
        public float StartRotateAngle = 75f;

        [Header("Camera Height Range")] public float MinHeight = 300f;
        public float MaxHeight = 750f;

        [Header("Rotation Speed")] public float RotateSpeed = 90f;

        private Transform m_CameraTransform;
        private Vector3 m_TargetPos;

        private float m_RotateInput => InputManager.Instance.RotateInput;

        private bool m_FirstFrame = true;

        // 0 - 1
        public ReactiveValue<float> MoveSpeed = new(-1f);

        // 0 - 1
        public ReactiveValue<float> ZoomSpeed = new(-1f);

        private void Awake()
        {
            if (TryGetComponent(out UnityEngine.Camera cam))
            {
                MainCam = cam;
                m_CameraTransform = cam.transform;
            }
        }

        private void OnEnable()
        {
            UIEvents.OnHopeMoveChangeAction += HandleMoveSpeedChange;
            UIEvents.OnHopeRotateChangeAction += HandleZoomSpeedChange;
        }

        private void OnDisable()
        {
            UIEvents.OnHopeMoveChangeAction -= HandleMoveSpeedChange;
            UIEvents.OnHopeRotateChangeAction -= HandleZoomSpeedChange;
        }

        private void HandleMoveSpeedChange(float value) => MoveSpeed.Value = value;
        private void HandleZoomSpeedChange(float value) => ZoomSpeed.Value = value;

        private void Start()
        {
            GPEvents.GetMoveSpeed = () => MoveSpeed;
            GPEvents.GetZoomSpeed = () => ZoomSpeed;

            if (GridManager.Instance)
            {
                InitCamera();
                StartClampTransform();
            }

            InputManager.Instance.DisableInput();
            StartCoroutine(DelayEnable());
        }

        private IEnumerator DelayEnable()
        {
            yield return new WaitForSeconds(1f);
            InputManager.Instance.EnableInput();
        }

        private void LateUpdate()
        {
            if (InputManager.Instance == null)
            {
                return;
            }

            if (m_FirstFrame)
            {
                m_FirstFrame = false;
                MoveSpeed.Value = 0.5f;
                ZoomSpeed.Value = 0.5f;
            }

            m_TargetPos = transform.position;

            UpdateMove();
            UpdateRotate();
            UpdateZoom();

            ClampPosition();

            m_CameraTransform.position = Vector3.Lerp(m_CameraTransform.position, m_TargetPos,
                Time.unscaledDeltaTime * 10f);
        }

        private void ClampPosition()
        {
            //NOTICE:Hardcoding the scene boundaries is not a good practice But It Just Works
            m_TargetPos.x = Mathf.Clamp(m_TargetPos.x, -1000, 1000);
            m_TargetPos.y = Mathf.Clamp(m_TargetPos.y, MinHeight, MaxHeight);
            m_TargetPos.z = Mathf.Clamp(m_TargetPos.z, -1400, 1000);
        }

        private void StartClampTransform()
        {
            //Debug.Log("AwakeClampTransform");

            var pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, -1000, 1000);
            pos.y = Mathf.Clamp(pos.y, MinHeight, MaxHeight);
            pos.z = Mathf.Clamp(pos.z, -1400, 1000);

            transform.position = pos;
        }

        private void InitCamera()
        {
            if (!StartTransform)
            {
                Debug.LogWarning("No StartTransform");
                return;
            }

            m_TargetPos = StartTransform.position + StartOffset;

            m_TargetPos.y = (MinHeight + MaxHeight) * 0.5f;

            m_CameraTransform.position = m_TargetPos;

            m_CameraTransform.rotation = Quaternion.Euler(StartRotateAngle, 0, 0);
        }

        private void UpdateMove()
        {
            var input = InputManager.Instance.MoveInput;

            var forward = m_CameraTransform.forward;
            forward.y = 0f;
            forward.Normalize();

            var right = m_CameraTransform.right;
            right.y = 0f;
            right.Normalize();

            var worldDir = input.x * right + input.y * forward;

            m_TargetPos += worldDir * (MoveSpeed * Time.unscaledDeltaTime * 50000f);
        }

        private void UpdateZoom()
        {
            var scroll = InputManager.Instance.ScrollInput;
            //Debug.Log("Cam" +scroll);
            if (scroll == 0f)
            {
                return;
            }

            var normalized = -Mathf.Sign(scroll);

            m_TargetPos +=
                new Vector3(0, normalized * ZoomSpeed * Time.unscaledDeltaTime * 100000f, 0);
        }

        private void UpdateRotate()
        {
            if (m_RotateInput == 0f)
            {
                return;
            }

            var input = -m_RotateInput;
            var angle = input * RotateSpeed * Time.unscaledDeltaTime;
            var yaw = Quaternion.AngleAxis(angle, Vector3.up);

            m_CameraTransform.rotation = yaw * m_CameraTransform.rotation;
        }
    }
}