#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Core
{
    public static class UIEvents
    {
        public static Action OnAfterBindSettingsAction;
        public static Action<float> OnHopeVolumeChangeAction;
        public static Action<float> OnHopeDifficultyChangeAction;
        public static Action<float> OnHopeRotateChangeAction;
        public static Action<float> OnHopeMoveChangeAction;
        public static Action OnGoBackToMainMenuAction;


        public static Action OnTrafficModeAction;
        public static Action OnBuildingModeAction;
        public static Action OnDefaultModeAction;

        public static Action<int, Sprite, string, string> OnCreateCardUIAction;
        public static Action<int, Vector3> OnCardClickAction;
        public static Action<int> OnCompleteCardClickAction;

        public static Action<int, string, GameObject, float> OnCreateLockAreaUIAction;
        public static Action<int> OnUnlockAreaAction;
        public static Func<int, List<Sprite>> AskForBuildingImages;

        public static Action OnShowBuildingListAction;
        public static Action OnHideBuildingListAction;

        public static Action<int> OnConversationEndAction;

        public static Action OnHopeStartGameAction;
        public static Action OnHopeEndGameAction;

        public static Action<float> OnHopeChangeTimeScaleAction;
    }
}