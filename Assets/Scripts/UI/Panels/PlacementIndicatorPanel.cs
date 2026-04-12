#region

using Core.Data;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace UI.Panels
{
    public class PlacementIndicatorPanel : BasePanel<PlacementIndicatorData>
    {
        [SerializeField] private RectTransform m_Indicator;
        [SerializeField] private Image m_IndicatorImage;

        protected override void OnBind()
        {
            Data.IsVisible.Bind(OnVisibilityChanged);
            Data.Position.Bind(OnPositionChanged);
            Data.Size.Bind(OnSizeChanged);
            Data.IsPlaceable.Bind(OnPlaceableChanged);
        }

        protected override void OnUnbind()
        {
            Data.IsVisible.Unbind(OnVisibilityChanged);
            Data.Position.Unbind(OnPositionChanged);
            Data.Size.Unbind(OnSizeChanged);
            Data.IsPlaceable.Unbind(OnPlaceableChanged);
        }

        private void OnVisibilityChanged(bool old, bool now)
        {
            gameObject.SetActive(now);
        }

        private void OnPositionChanged(Vector2 old, Vector2 now)
        {
            if (m_Indicator == null) return;

            m_Indicator.anchoredPosition = new Vector2(now.x, now.y);
        }

        private void OnSizeChanged(Vector2 old, Vector2 now)
        {
            if (m_Indicator == null) return;

            m_Indicator.sizeDelta = new Vector2(
                now.x * Data.GridSize,
                now.y * Data.GridSize
            );
        }

        private void OnPlaceableChanged(bool old, bool now)
        {
            if (m_IndicatorImage == null) return;

            m_IndicatorImage.color = now ? Data.ValidColor : Data.InvalidColor;
        }
    }
}