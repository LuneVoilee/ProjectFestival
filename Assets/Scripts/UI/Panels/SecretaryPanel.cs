#region

using Core;
using TMPro;
using Core.Data;
using UnityEngine.EventSystems;

#endregion

namespace UI
{
    public class SecretaryPanel : BasePanel<SecretaryData>, IPointerClickHandler
    {
        public TMP_Text Conversation;

        protected override void OnBind()
        {
            Data.CurrentIndex.Bind(OnIndexChanged);

            Data.CurrentIndex.Value = 0;
        }

        protected override void OnUnbind()
        {
            Data.CurrentIndex.Unbind(OnIndexChanged);
        }

        public override void OnShow()
        {
            base.OnShow();

            //Debug.Log("Show Secretary Panel");
            UIEvents.OnHideBuildingListAction?.Invoke();

            GameManager.Instance.PauseGame(true);
        }

        public override void OnHide()
        {
            base.OnHide();

            UIEvents.OnShowBuildingListAction?.Invoke();

            GameManager.Instance.PauseGame(false);
        }

        private void OnIndexChanged(int _, int index)
        {
            if (Data == null) return;

            if (Data.IsFinished)
            {
                EndConversation();
                return;
            }

            Conversation.text = Data.CurrentLine;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //Debug.Log("Clicked");
            Data?.Next();
        }

        private void EndConversation()
        {
            if (Data != null && Data.ID != -1)
            {
                UIEvents.OnConversationEndAction?.Invoke(Data.ID);
            }

            UIManager.Instance.HidePanel(this);
        }
    }
}