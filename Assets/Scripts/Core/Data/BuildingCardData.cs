#region

using Core.Reactive;
using UnityEngine;

#endregion

namespace Core.Data
{
    public class BuildingCardData : BasePanelData
    {
        public int ID { get; }
        public Sprite Image { get; }
        public string Name { get; }
        public string CostText { get; }

        public ReactiveValue<bool> IsSelected { get; } = new();

        public BuildingCardData
        (
            int id, Sprite image, string name,
            string costText
        )
        {
            ID = id;
            Image = image;
            Name = name;
            CostText = costText;
        }
    }
}