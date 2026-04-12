#region

using Core;
using Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace UI
{
    public class LockAreaPanel : BasePanel<LockAreaData>
    {
        public TMP_Text Text;
        public Button Btn;

        public Vector3 PositionOffset = new(0, 2f, 0);

        protected override void OnBind()
        {
            Text.text = Data.CostText;
            Data.IsUnlocked.Bind(OnUnlockedChanged);
        }

        protected override void OnUnbind()
        {
            Data.IsUnlocked.Unbind(OnUnlockedChanged);
        }


        private void OnEnable()
        {
            Btn.onClick.AddListener(HandleBtnClick);
        }

        private void OnDisable()
        {
            if (Btn)
            {
                Btn.onClick.RemoveListener(HandleBtnClick);
            }
        }

        private void LateUpdate()
        {
            if (Data?.TargetTransform == null)
            {
                return;
            }

            var rect = transform as RectTransform;

            if (!rect)
            {
                return;
            }

            // NOTICE: 没招了，只能用魔法数字。UI大小1460和Terrain的1000不一致，偏移反而一致，令人感慨。
            //var size = new Vector2(1460f, 1460f);
            // rect.sizeDelta = size;

            rect.position = Data.TargetTransform.position + PositionOffset +
                            new Vector3(500f, 0, 500f);
        }

        private void HandleBtnClick()
        {
            if (Data == null) return;

            if (!InputManager.Instance.IsMouseLeftClick)
            {
                return;
            }

            UIEvents.OnUnlockAreaAction?.Invoke(Data.Index);
        }

        private void OnUnlockedChanged(bool _, bool unlocked)
        {
            if (unlocked)
            {
                gameObject.SetActive(false);
            }
        }
    }
}