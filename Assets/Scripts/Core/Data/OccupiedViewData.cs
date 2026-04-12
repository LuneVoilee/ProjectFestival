#region

using Core.Reactive;
using UnityEngine;

#endregion

namespace Core.Data
{
    public class OccupiedViewData : MapViewDataBase
    {
        public int GridSize { get; }
        public Color OccupiedColor { get; }

        public ReactiveList<Vector2Int> OccupiedPositions { get; } = new();

        public OccupiedViewData(int gridSize, Color color)
        {
            GridSize = gridSize;
            OccupiedColor = color;
        }
    }
}