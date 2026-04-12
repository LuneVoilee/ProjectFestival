#region

using Core;
using Core.Music;
using Core.Reactive;
using UnityEngine;

#endregion

public class MusicEventsHandler : MonoBehaviour
{
    private void OnEnable()
    {
        GPEvents.OnCompeletePlacementAction += HandlePlaceBuilding;
        GPEvents.OnLevelChangeAction += HandleLevelChange;
    }

    private void OnDisable()
    {
        GPEvents.OnCompeletePlacementAction -= HandlePlaceBuilding;
        GPEvents.OnLevelChangeAction -= HandleLevelChange;
    }

    private void HandlePlaceBuilding(Vector3 _)
    {
        MusicManager.Instance.PlaySFX("SFX_PlaceBuilding");
    }

    private void HandleLevelChange(ReactiveValue<int> level, float arg2, float arg3)
    {
        if (level == 1)
        {
            return;
        }

        MusicManager.Instance.PlaySFX("SFX_Success");
    }
}