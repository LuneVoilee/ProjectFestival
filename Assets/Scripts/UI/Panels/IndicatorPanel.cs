#region

using Core;
using UnityEngine;

#endregion

namespace UI
{
    public class IndicatorPanel : BasePanel
    {
        public RectTransform m_LineRect;
        public RectTransform m_ArrowRect;

        private Vector2 m_StartPos;

        private void Awake()
        {
            if (m_ArrowRect == null || m_LineRect == null)
            {
                Debug.LogWarning("缺失引用");
            }

            HideIndicator();
        }

        private void Update()
        {
            UpdateIndicator();
        }


        public void ShowIndicator(Vector2 start)
        {
            //Debug.Log(start);
            m_StartPos = start;
            gameObject.SetActive(true);
        }

        public void HideIndicator()
        {
            gameObject.SetActive(false);
        }

        private void UpdateIndicator()
        {
            var mousePos = InputManager.Instance.MousePosition;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Canvas, m_StartPos, null, out var localStart);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Canvas, mousePos, null, out var localMouse);

            var direction = localMouse - localStart;
            var distance = direction.magnitude;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            m_LineRect.anchoredPosition = localStart;
            m_LineRect.sizeDelta = new Vector2(distance, m_LineRect.sizeDelta.y);
            m_LineRect.localRotation = Quaternion.Euler(0f, 0f, angle);


            m_ArrowRect.anchoredPosition = localMouse;
            m_ArrowRect.localRotation = Quaternion.Euler(0f, 0f, angle + 45f);
        }

        protected override void OnDestroy()
        {
            Destroy(gameObject);

            base.OnDestroy();
        }
    }
}