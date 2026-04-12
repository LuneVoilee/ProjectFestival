#region

using Core;
using TMPro;
using Core.Data;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace UI
{
    public class BuildingPanel : BasePanel<BuildingCardData>
    {
        public Image Image;
        public TMP_Text NameText;
        public TMP_Text CostText;
        public Button Btn;

        public Color NormalColor;
        public Color HighlightColor;
        public Vector3 NormalScale;
        public Vector3 HighlightScale;

        protected override void OnBind()
        {
            Image.sprite = Data.Image;
            NameText.text = Data.Name;
            CostText.text = Data.CostText;
            transform.localScale = NormalScale;

            Data.IsSelected.Bind(OnSelectedChanged);
        }

        protected override void OnUnbind()
        {
            Data.IsSelected.Unbind(OnSelectedChanged);
        }

        private void OnEnable()
        {
            Btn.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            Btn.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            if (Data == null) return;

            UIEvents.OnCardClickAction?.Invoke(Data.ID, transform.position);
        }

        private void OnSelectedChanged(bool _, bool selected)
        {
            if (Image != null)
            {
                Image.color = selected ? HighlightColor : NormalColor;
            }

            transform.localScale = selected ? HighlightScale : NormalScale;
        }
    }
}