#region

using System;
using System.Collections.Generic;
using Core.Data;
using Core.Reactive;
using UnityEngine;

#endregion

namespace Core
{
    //GP means GamePlay
    public static class GPEvents
    {
        public static Action<Vector3> OnCompeletePlacementAction;
        public static Action<int> OnAreaUnlockedAction;

        public static Action<ReactiveValue<int>, float, float> OnLevelChangeAction;

        public static Action<Transform> OnFeelUnhappyAction;
        public static Action<Transform> OnFeelHappyAction;

        public static Action<int, List<string>> OnBeginConversationAction;

        public static Func<(ReactiveValue<float> money, ReactiveValue<float> happiness)>
            GetEconomyData;

        public static Func<ReactiveValue<float>> GetTimeScale;
        public static Func<ReactiveValue<int>> GetViewMode;

        public static Func<GridViewData> GetGridViewData;
        public static Func<OccupiedViewData> GetOccupiedViewData;
        public static Func<TrafficViewData> GetTrafficViewData;
        public static Func<PlacementIndicatorData> GetPlacementIndicatorData;

        public static Func<(ReactiveValue<bool> start, ReactiveValue<bool> end)> GetMainMenuData;

        public static Func<ReactiveValue<float>> GetMoveSpeed;
        public static Func<ReactiveValue<float>> GetZoomSpeed;
        public static Func<ReactiveValue<float>> GetDifficulty;
        public static Func<ReactiveValue<float>> GetVolume;
    }
}