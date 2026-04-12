#region

using Core;
using Core.Data;
using TMPro;
using UnityEngine.UI;

#endregion

namespace UI
{
    public class SettingsPanel : BasePanel<SettingsData>
    {
        public Slider CharacterMovementSld;
        public TMP_Text CharacterMovementTxt;
        public Slider CharacterZoomSld;
        public TMP_Text CharacterZoomTxt;
        public Slider DifficultySld;
        public TMP_Text DifficultyTxt;
        public Slider VolumeSld;
        public TMP_Text VolumeTxt;
        public Button GoBackToMainMenuBtn;
        public Button CloseBtn;

        private void OnEnable()
        {
            UIEvents.OnHideBuildingListAction?.Invoke();

            GoBackToMainMenuBtn.onClick.AddListener(() =>
            {
                UIEvents.OnGoBackToMainMenuAction?.Invoke();
            });

            CloseBtn.onClick.AddListener(() => { UIManager.Instance.HidePanel(this); });

            CharacterMovementSld.onValueChanged.AddListener(value =>
                UIEvents.OnHopeMoveChangeAction?.Invoke(value));
            CharacterZoomSld.onValueChanged.AddListener(value =>
                UIEvents.OnHopeRotateChangeAction?.Invoke(value));
            DifficultySld.onValueChanged.AddListener(value =>
                UIEvents.OnHopeDifficultyChangeAction
                    ?.Invoke(value));
            VolumeSld.onValueChanged.AddListener(value =>
                UIEvents.OnHopeVolumeChangeAction?.Invoke(value));
        }

        private void OnDisable()
        {
            UIEvents.OnShowBuildingListAction?.Invoke();

            GoBackToMainMenuBtn.onClick.RemoveAllListeners();
            CloseBtn.onClick.RemoveAllListeners();
            CharacterMovementSld.onValueChanged.RemoveAllListeners();
            CharacterZoomSld.onValueChanged.RemoveAllListeners();
            DifficultySld.onValueChanged.RemoveAllListeners();
            VolumeSld.onValueChanged.RemoveAllListeners();
        }

        protected override void OnBind()
        {
            Data.CharacterMovement.Bind(HandleMoveChange);
            Data.CharacterZoom.Bind(HandleRotateChange);
            Data.Difficulty.Bind(HandleDifficultyChange);
            Data.Volume.Bind(HandleVolumeChange);
        }


        protected override void OnUnbind()
        {
            Data.CharacterMovement.Unbind(HandleMoveChange);
            Data.CharacterZoom.Unbind(HandleRotateChange);
            Data.Difficulty.Unbind(HandleDifficultyChange);
            Data.Volume.Unbind(HandleVolumeChange);
        }

        private void HandleMoveChange(float oldV, float newV)
        {
            CharacterMovementSld.value = newV;
            CharacterMovementTxt.text = newV.ToString("F1");
        }

        private void HandleRotateChange(float oldV, float newV)
        {
            CharacterZoomSld.value = newV;
            CharacterZoomTxt.text = newV.ToString("F1");
        }

        private void HandleDifficultyChange(float oldV, float newV)
        {
            DifficultySld.value = newV;
            DifficultyTxt.text = newV.ToString("F1");
        }

        private void HandleVolumeChange(float oldV, float newV)
        {
            VolumeSld.value = newV;
            VolumeTxt.text = newV.ToString("F1");
        }
    }
}