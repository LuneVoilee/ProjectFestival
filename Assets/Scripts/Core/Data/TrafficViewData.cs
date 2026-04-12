#region

using System.Collections.Generic;
using Core.Reactive;
using UnityEngine;

#endregion

namespace Core.Data
{
    public struct TrafficCellData
    {
        public Vector2Int Position;

        // 0=空闲, 1=稍挤, 2=拥挤
        public int CrowdLevel;
    }

    public class TrafficViewData : MapViewDataBase
    {
        public int GridSize { get; }
        public Color NotCrowdedColor { get; }
        public Color SomewhatCrowdedColor { get; }
        public Color CrowdedColor { get; }
        public Color PathColor { get; }

        public List<Vector2Int> AllPositions { get; }

        public ReactiveList<TrafficCellData> ChangedCells { get; } = new();

        public ReactiveList<Vector3[]> VisitorPaths { get; } = new();

        public TrafficViewData
        (
            int gridSize,
            List<Vector2Int> positions,
            Color notCrowded,
            Color somewhatCrowded,
            Color crowded,
            Color pathColor
        )
        {
            GridSize = gridSize;
            AllPositions = positions;
            NotCrowdedColor = notCrowded;
            SomewhatCrowdedColor = somewhatCrowded;
            CrowdedColor = crowded;
            PathColor = pathColor;
        }

        public Color GetColorByLevel(int level) => level switch
        {
            0 => NotCrowdedColor,
            1 => SomewhatCrowdedColor,
            _ => CrowdedColor
        };
    }
}