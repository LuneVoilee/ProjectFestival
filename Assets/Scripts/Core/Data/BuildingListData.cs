#region

using Core.Reactive;

#endregion

namespace Core.Data
{
    public class BuildingListData : BasePanelData
    {
        public ReactiveList<BuildingCardData> BuildingCards = new();
    }
}