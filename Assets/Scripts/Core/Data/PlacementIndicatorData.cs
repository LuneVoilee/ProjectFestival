#region

using Core.Reactive;
using UnityEngine;

#endregion

namespace Core.Data
{
    public class PlacementIndicatorData : BasePanelData
    {
        public ReactiveValue<bool> IsVisible { get; } = new(false);
        public ReactiveValue<Vector2> Position { get; } = new(Vector2.zero);
        public ReactiveValue<Vector2> Size { get; } = new(Vector2.one);
        public ReactiveValue<bool> IsPlaceable { get; } = new(false);

        public Color ValidColor { get; }
        public Color InvalidColor { get; }
        public int GridSize { get; }

        public PlacementIndicatorData(Color validColor, Color invalidColor, int gridSize)
        {
            ValidColor = validColor;
            InvalidColor = invalidColor;
            GridSize = gridSize;
        }
    }
}
