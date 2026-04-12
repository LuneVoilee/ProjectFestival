#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Core.Data
{
    public class GridViewData : MapViewDataBase
    {
        public int GridSize { get; }
        public Color LineColor { get; }

        public List<Vector2Int> GridPositions { get; }

        public GridViewData(int gridSize, Color lineColor, List<Vector2Int> positions)
        {
            GridSize = gridSize;
            LineColor = lineColor;
            GridPositions = positions;
        }
    }
}