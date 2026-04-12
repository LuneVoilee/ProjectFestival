#region

using Core;
using TMPro;
using Core.Data;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace UI
{
    public class LevelUpPanel : BasePanel<LevelUpData>
    {
        public TMP_Text LevelTitleText;
        public Transform ImageBox;
        public Button CloseButton;
        private int m_CurrentLevel;

        private void OnEnable()
        {
            CloseButton.onClick.AddListener(ClosePanel);
        }

        private void OnDisable()
        {
            CloseButton.onClick.RemoveListener(ClosePanel);
        }

        protected override void OnBind()
        {
            Data.CurrentLevel.Bind(OnLevelUp);
        }

        protected override void OnUnbind()
        {
            Data.CurrentLevel.Unbind(OnLevelUp);
        }


        public void OnLevelUp(int _, int newLevel)
        {
            LevelTitleText.text = $"Level {newLevel} Unlocked! Congratulations!";
            m_CurrentLevel = newLevel;

            AddImages();
        }

        public override void OnShow()
        {
            base.OnShow();
            GameManager.Instance.PauseGame(true);
            UIEvents.OnHideBuildingListAction?.Invoke();
        }

        public override void OnHide()
        {
            base.OnHide();
            GameManager.Instance.PauseGame(false);
            UIEvents.OnShowBuildingListAction?.Invoke();
        }

        private void ClosePanel()
        {
            UIManager.Instance.HidePanel(this);
        }

        private void AddImages()
        {
            if (UIEvents.AskForBuildingImages == null)
            {
                return;
            }

            foreach (Transform t in ImageBox)
            {
                Destroy(t.gameObject);
            }

            var images = UIEvents.AskForBuildingImages(m_CurrentLevel);
            foreach (var image in images)
            {
                var go = new GameObject("Building");
                go.transform.SetParent(ImageBox);
                var imageComponent = go.AddComponent<Image>();
                imageComponent.sprite = image;
            }
        }
    }
}