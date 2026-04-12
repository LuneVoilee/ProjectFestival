#region

using Core.Reactive;
using UnityEngine;

#endregion

namespace Core.Data
{
    public class LockAreaData : BasePanelData
    {
        public int Index { get; }
        public float Cost { get; }
        public Transform TargetTransform { get; }

        public ReactiveValue<bool> IsUnlocked { get; } = new();

        public string CostText => $"Need {Cost} To Unlock";

        public LockAreaData(int index, float cost, Transform target)
        {
            Index = index;
            Cost = cost;
            TargetTransform = target;
        }
    }
}