#region

using Core;
using Core.Data;
using UnityEngine.UI;

#endregion

namespace UI
{
    // 这个UI 本身没有任何状态 （This UI has no state , So it can not be Data-Driven）
    // 继承BasePanel<MainMenuData>只是为了统一UI框架的写法
    public class MainMenuPanel : BasePanel<MainMenuData>
    {
        public Button StartGameBtn;
        public Button EndGameBtn;

        private void Start()
        {
            if (GPEvents.GetMainMenuData != null)
            {
                var (s, e) = GPEvents.GetMainMenuData();
                var data = new MainMenuData(s, e);
                Bind(data);
            }
        }

        private void OnEnable()
        {
            StartGameBtn.onClick.AddListener(() => UIEvents.OnHopeStartGameAction?.Invoke());
            EndGameBtn.onClick.AddListener(() => UIEvents.OnHopeEndGameAction?.Invoke());
        }


        private void OnDisable()
        {
            StartGameBtn.onClick.RemoveAllListeners();
            EndGameBtn.onClick.RemoveAllListeners();
        }

        protected override void OnBind()
        {
        }

        protected override void OnUnbind()
        {
        }

        protected override void OnDestroy()
        {
            Destroy(gameObject);
        }
    }
}