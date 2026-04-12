#region

using System.Collections.Generic;
using System.Linq;
using Core;
using Core.Data;
using Core.Reactive;
using GamePlay.AI;
using Tool;
using UnityEngine;

#endregion

namespace GamePlay.Grid
{
    public enum MapViewType
    {
        Grid,
        Occupied,
        Traffic
    }

    public class GridManager : SingletonMono<GridManager>
    {
        public GameObject Terrains;
        public int GridSize = 50;

        [Header("Grid View")] public Color GridColor = Color.white;
        [Header("Building View")] public Color OccupiedColor = Color.red;

        [Header("Crowd View")] public int SomewhatCrowdedThreshold = 5;
        public int CrowdedThreshold = 10;
        public Color NotCrowdedColor = Color.green;
        public Color SomewhatCrowdedColor = Color.yellow;
        public Color CrowdedColor = Color.red;
        public Color PathColor = Color.cyan;

        private Dictionary<Vector2Int, Grid> m_Grids;
        private readonly List<Terrain> m_TerrainList = new();

        private ReactiveValue<int> m_CurrentView;
        private MapViewType m_CurrentViewType = MapViewType.Grid;
        private bool m_FirstFrame = true;

        // Data Layer
        private GridViewData m_GridViewData;
        private OccupiedViewData m_OccupiedViewData;
        private TrafficViewData m_TrafficViewData;

        private float m_TrafficUpdateTimer;
        private const float TrafficUpdateInterval = 1f;

        protected override void Awake()
        {
            base.Awake();

            m_Grids = new Dictionary<Vector2Int, Grid>();
            m_CurrentView = new ReactiveValue<int>(-1);

            if (Terrains == null)
            {
                Debug.LogWarning("Terrains are not assigned; unable to generate the grid");
                return;
            }

            foreach (Transform terrainTrans in Terrains.transform)
            {
                if (!terrainTrans.gameObject.activeSelf) continue;
                if (terrainTrans.TryGetComponent<Terrain>(out var terrain))
                {
                    m_TerrainList.Add(terrain);
                    GenerateGridsForTerrain(terrain);
                }
            }

            InitViewData();
        }

        private void OnEnable()
        {
            UIEvents.OnCardClickAction += HandleBuildingUIClick;
            UIEvents.OnCompleteCardClickAction += TurnToDefaultMode;
            UIEvents.OnDefaultModeAction += HandleDefaultBtn;
            UIEvents.OnBuildingModeAction += HandleBuildingBtn;
            UIEvents.OnTrafficModeAction += HandleTrafficBtn;

            GPEvents.GetViewMode = () => m_CurrentView;
            GPEvents.GetGridViewData = GetGridViewData;
            GPEvents.GetOccupiedViewData = GetOccupiedViewData;
            GPEvents.GetTrafficViewData = GetTrafficViewData;
        }

        private void OnDisable()
        {
            UIEvents.OnCardClickAction -= HandleBuildingUIClick;
            UIEvents.OnCompleteCardClickAction -= TurnToDefaultMode;
            UIEvents.OnDefaultModeAction -= HandleDefaultBtn;
            UIEvents.OnBuildingModeAction -= HandleBuildingBtn;
            UIEvents.OnTrafficModeAction -= HandleTrafficBtn;

            GPEvents.GetViewMode = null;
            GPEvents.GetGridViewData = null;
            GPEvents.GetOccupiedViewData = null;
            GPEvents.GetTrafficViewData = null;
        }

        private void InitViewData()
        {
            var positions = m_Grids.Keys.ToList();

            m_GridViewData = new GridViewData(GridSize, GridColor, positions);

            m_OccupiedViewData = new OccupiedViewData(GridSize, OccupiedColor);

            m_TrafficViewData = new TrafficViewData(
                GridSize,
                positions,
                NotCrowdedColor,
                SomewhatCrowdedColor,
                CrowdedColor,
                PathColor
            );
        }

        private void Update()
        {
            if (m_FirstFrame)
            {
                m_FirstFrame = false;
                SetMapView(m_CurrentViewType);
            }

            if (m_TrafficViewData.IsVisible.Value)
            {
                m_TrafficUpdateTimer -= Time.deltaTime;
                if (m_TrafficUpdateTimer <= 0f)
                {
                    UpdateTrafficData();
                    m_TrafficUpdateTimer = TrafficUpdateInterval;
                }
            }
        }

        private void HandleDefaultBtn() => SetMapView(MapViewType.Grid);
        private void HandleBuildingBtn() => SetMapView(MapViewType.Occupied);
        private void HandleTrafficBtn() => SetMapView(MapViewType.Traffic);
        private void HandleBuildingUIClick(int _, Vector3 __) => SetMapView(MapViewType.Occupied);
        private void TurnToDefaultMode(int _) => SetMapView(MapViewType.Grid);

        #region View Mode Control

        public void SetMapView(MapViewType mode)
        {
            m_CurrentViewType = mode;
            m_CurrentView.Value = (int)mode;

            m_GridViewData.IsVisible.Value = true;

            m_OccupiedViewData.IsVisible.Value = mode == MapViewType.Occupied;

            m_TrafficViewData.IsVisible.Value = mode == MapViewType.Traffic;

            // Update immediately when switching to Traffic view
            if (mode == MapViewType.Traffic)
            {
                UpdateTrafficData();
            }
        }

        #endregion

        #region Occupy Area

        public void OccupyArea(Vector2Int startGridPos, Vector2Int buildingSize)
        {
            for (var x = 0; x < buildingSize.x; x++)
            {
                for (var z = 0; z < buildingSize.y; z++)
                {
                    var pos = new Vector2Int(
                        startGridPos.x + x * GridSize,
                        startGridPos.y + z * GridSize);

                    var grid = GetGrid(pos);
                    grid?.SetGridType(EGridType.Building);

                    // Update the data; OccupiedViewPanel will automatically create the grid cells
                    m_OccupiedViewData.OccupiedPositions.Add(pos);
                }
            }
        }

        #endregion

        #region Traffic Data

        private void UpdateTrafficData()
        {
            if (VisitorManager.Instance == null) return;

            m_TrafficViewData.ChangedCells.Clear();
            m_TrafficViewData.VisitorPaths.Clear();

            var visitorCounts = new Dictionary<Vector2Int, int>();
            var visitors = VisitorManager.Instance.AllVisitors;

            foreach (var visitor in visitors)
            {
                if (visitor == null) continue;

                var gridPos = WorldToGridPosition(visitor.transform.position);
                visitorCounts.TryGetValue(gridPos, out var count);
                visitorCounts[gridPos] = count + 1;

                var navigator = visitor.GetComponent<Navigator>();
                if (navigator != null)
                {
                    var corners = navigator.GetPathCorners();
                    if (corners != null && corners.Length >= 2)
                    {
                        m_TrafficViewData.VisitorPaths.Add(corners);
                    }
                }
            }

            foreach (var kvp in visitorCounts)
            {
                var level = kvp.Value >= CrowdedThreshold ? 2 :
                    kvp.Value >= SomewhatCrowdedThreshold ? 1 : 0;

                m_TrafficViewData.ChangedCells.Add(new TrafficCellData
                {
                    Position = kvp.Key,
                    CrowdLevel = level
                });

                if (level == 2)
                {
                    foreach (var visitor in visitors)
                    {
                        if (WorldToGridPosition(visitor.transform.position) == kvp.Key)
                        {
                             visitor.FeelUnhappy();
                        }
                    }
                }
            }
        }

        #endregion

        #region Data Getters (for UIController binding)

        public GridViewData GetGridViewData() => m_GridViewData;
        public OccupiedViewData GetOccupiedViewData() => m_OccupiedViewData;
        public TrafficViewData GetTrafficViewData() => m_TrafficViewData;

        #endregion

        public Grid GetGrid(Vector2Int pos)
        {
            m_Grids.TryGetValue(pos, out var grid);
            return grid;
        }

        private void GenerateGridsForTerrain(Terrain terrain)
        {
            var bounds = terrain.terrainData.bounds;
            var terrainPos = terrain.transform.position;

            var minX = bounds.min.x + terrainPos.x;
            var minZ = bounds.min.z + terrainPos.z;
            var maxX = bounds.max.x + terrainPos.x;
            var maxZ = bounds.max.z + terrainPos.z;

            var startX = Mathf.FloorToInt(minX / GridSize) * GridSize;
            var startZ = Mathf.FloorToInt(minZ / GridSize) * GridSize;
            var endX = Mathf.FloorToInt(maxX / GridSize) * GridSize;
            var endZ = Mathf.FloorToInt(maxZ / GridSize) * GridSize;

            for (var x = startX; x < endX; x += GridSize)
            {
                for (var z = startZ; z < endZ; z += GridSize)
                {
                    var pos = new Vector2Int(x, z);
                    if (m_Grids.ContainsKey(pos)) continue;

                    var grid = new Grid(EGridType.None, pos);
                    m_Grids.Add(pos, grid);
                }
            }
        }

        public float SampleHeightAt(float x, float z)
        {
            foreach (var terrain in m_TerrainList)
            {
                var pos = terrain.transform.position;
                var size = terrain.terrainData.size;

                if (x >= pos.x && x <= pos.x + size.x &&
                    z >= pos.z && z <= pos.z + size.z)
                {
                    return terrain.SampleHeight(new Vector3(x, 0, z));
                }
            }

            return 0;
        }

        public Vector2Int WorldToGridPosition(Vector3 worldPosition)
        {
            var x = Mathf.FloorToInt(worldPosition.x / GridSize) * GridSize;
            var z = Mathf.FloorToInt(worldPosition.z / GridSize) * GridSize;
            return new Vector2Int(x, z);
        }

        public Vector3 GridToWorldPosition(Vector2Int gridPosition)
        {
            var x = gridPosition.x + GridSize * 0.5f;
            var z = gridPosition.y + GridSize * 0.5f;
            return new Vector3(x, 0f, z);
        }

        public bool IsAreaPlaceable(Vector2Int startGridPos, Vector2Int buildingSize)
        {
            var baseHeight = -1f;

            for (var x = 0; x < buildingSize.x; x++)
            {
                for (var z = 0; z < buildingSize.y; z++)
                {
                    var currentPos = new Vector2Int(
                        startGridPos.x + x * GridSize,
                        startGridPos.y + z * GridSize);
                    var grid = GetGrid(currentPos);

                    if (grid == null || !grid.IsAllowBuild ||
                        !AreaManager.Instance.IsPositionLegal(GridToWorldPosition(currentPos)))
                    {
                        return false;
                    }

                    var currentHeight = SampleHeightAt(currentPos.x, currentPos.y);

                    if (baseHeight < 0)
                        baseHeight = currentHeight;
                    else if (Mathf.Abs(baseHeight - currentHeight) > 0.1f)
                        return false;
                }
            }

            return true;
        }
    }
}