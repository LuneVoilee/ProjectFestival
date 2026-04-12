#region

using UnityEngine;

#endregion

namespace GamePlay.Building
{
    [CreateAssetMenu(fileName = "BuildingData", menuName = "AScriptableObject/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [Header("Unique digital mark")] public int ID;
        [Header("Name")] public string Name;
        [Header("Unlock level")] public int UnlockLevel;

        [Header("Money Cost")] public float Cost;
        [Header("Money Benefit(accumulate one revenue point each time a tourist visits)")] public float MoneyBenefit;
        [Header("Happiness Benefit(accumulate one revenue point each time a tourist visits)")] public float HappinessBenefit;

        //[Header("maintenance cost")] public float Maintenance; (can be combine to the same concept with MoneyBenefit)
        [Header("The maximum number of tourists that can be received simultaneously")] public int ReceptionCount;
        [Header("The duration of tourists' stay in this building (in seconds)")] public float StayTime;

        [Header("Will tourists disappear (enter the building) during the interaction?")] public bool NeedToEnterTheBuilding;

        [Header("display images in UI")] public Sprite Image;
        [Header("Pre-built components (used for handling the models and gameplay logic within the game)")] public GameObject Prefab;
        [Header("Building size")] public Vector2Int Size;
    }
}