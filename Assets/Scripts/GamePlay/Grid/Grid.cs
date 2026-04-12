using UnityEngine;

namespace GamePlay.Grid
{
    public enum EGridType
    {
        None,
        Building
    }

    public class Grid
    {
        public bool IsAllowBuild => GridType == EGridType.None;
        private EGridType GridType;
        public Vector2Int Position;
        public int VisitorCount = 0;

        public Grid(EGridType gridType, Vector2Int position)
        {
            GridType = gridType;
            Position = position;
        }

        public void SetGridType(EGridType gridType)
        {
            GridType = gridType;
        }
    }
}