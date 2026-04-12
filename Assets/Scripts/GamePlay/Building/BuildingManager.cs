#region

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Core;
using Core.Data;
using Core.Reactive;
using GamePlay.Camera;
using GamePlay.Grid;
using GamePlay.Level;
using Tool;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace GamePlay.Building
{
    public class BuildingManager : SingletonMono<BuildingManager>
    {
        public List<BaseBuilding> AllBuildings { get; private set; }

        public List<BuildingData> Buildings;
        public Dictionary<int, BuildingData> BuildingDict;

        [SerializeField] private Color m_ValidPlacementColor;
        [SerializeField] private Color m_InvalidPlacementColor;

        public BuildingData CurrentPlacingBuildingData { get; private set; }

        private PlacementIndicatorData m_PlacementIndicatorData;
        private Vector2 m_CurrentIndicatorPosition;

        protected override void Awake()
        {
            base.Awake();
            AllBuildings = new List<BaseBuilding>();
            BuildingDict = new Dictionary<int, BuildingData>();

            foreach (var so in Buildings)
            {
                BuildingDict.TryAdd(so.ID, so);
            }

            InitPlacementIndicatorData();
        }

        private void InitPlacementIndicatorData()
        {
            var gridSize = GridManager.Instance != null ? GridManager.Instance.GridSize : 50;
            m_PlacementIndicatorData = new PlacementIndicatorData(
                m_ValidPlacementColor,
                m_InvalidPlacementColor,
                gridSize
            );
        }

        public PlacementIndicatorData GetPlacementIndicatorData() => m_PlacementIndicatorData;

        private void Start()
        {
            UpdateBuildingListPanel();
        }

        private void UpdateBuildingListPanel()
        {
            foreach (var b in Buildings)
            {
                if (b.UnlockLevel != ValueManager.Instance.CurrentLevel)
                {
                    continue;
                }

                UIEvents.OnCreateCardUIAction?.Invoke(b.ID, b.Image, b.Name,
                    b.Cost.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void OnEnable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnClickAction += HandlePlacement;
            }

            UIEvents.OnCardClickAction += SelectBuildingToPlace;

            GPEvents.OnLevelChangeAction += HandleLevelChange;

            UIEvents.AskForBuildingImages = AskForBuildingImages;

            UIEvents.OnCompleteCardClickAction += CancelPlacement;

            GPEvents.GetPlacementIndicatorData = GetPlacementIndicatorData;
        }

        private void OnDisable()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnClickAction -= HandlePlacement;
            }

            UIEvents.OnCardClickAction -= SelectBuildingToPlace;
            GPEvents.OnLevelChangeAction -= HandleLevelChange;

            UIEvents.AskForBuildingImages = null;

            UIEvents.OnCompleteCardClickAction -= CancelPlacement;

            GPEvents.GetPlacementIndicatorData = null;
        }

        private void HandleLevelChange(ReactiveValue<int> _, float __, float ___)
        {
            UpdateBuildingListPanel();
        }

        private void Update()
        {
            if (CurrentPlacingBuildingData == null)
            {
                return;
            }

            //Place the indicator
            var ray = CameraController.MainCam.ScreenPointToRay(InputManager.Instance
                .MousePosition);
            if (Physics.Raycast(ray, out var hit, 1000f, LayerMask.GetMask("Terrain")))
            {
                //Debug.Log(hit.point);
                UpdatePlacementIndicator(hit);
            }
        }

        private List<Sprite> AskForBuildingImages(int level)
        {
            var buildingImages = new List<Sprite>();
            foreach (var b in Buildings)
            {
                if (b.UnlockLevel != level)
                {
                    continue;
                }

                buildingImages.Add(b.Image);
            }

            return buildingImages;
        }

        public void SelectBuildingToPlace(int id, Vector3 _)
        {
            if (!Instance.TryGetBuildingByID(id, out var building))
            {
                return;
            }

            CurrentPlacingBuildingData = building;

            // Update the data; the Panel will respond automatically
            m_PlacementIndicatorData.Size.Value = new Vector2(building.Size.x, building.Size.y);
            m_PlacementIndicatorData.IsVisible.Value = true;
        }

        public void CancelPlacement(int _ = 0)
        {
            CurrentPlacingBuildingData = null;

            // Update the data; the Panel will respond automatically
            m_PlacementIndicatorData.IsVisible.Value = false;
        }

        private void HandlePlacement()
        {
            StartCoroutine(HandlePlacementDelay());
        }

        private IEnumerator HandlePlacementDelay()
        {
            //InputSystem events fire earlier than UI's IsPointerOverGameObject, so we delay one frame to ensure correct detection.
            yield return null;

            if (CurrentPlacingBuildingData == null ||
                EventSystem.current.IsPointerOverGameObject())
            {
                yield break;
            }

            // Use the position from the data layer
            var worldPos = new Vector3(
                m_CurrentIndicatorPosition.x + 50f * CurrentPlacingBuildingData.Size.x,
                0,
                m_CurrentIndicatorPosition.y + 50f * CurrentPlacingBuildingData.Size.y
            );
            worldPos.y = GridManager.Instance.SampleHeightAt(worldPos.x, worldPos.z) + 0.1f;

            var gridPos = GridManager.Instance.WorldToGridPosition(new Vector3(
                m_CurrentIndicatorPosition.x,
                0,
                m_CurrentIndicatorPosition.y
            ));

            if (GridManager.Instance.IsAreaPlaceable(gridPos, CurrentPlacingBuildingData.Size))
            {
                if (ValueManager.Instance.TrySpendMoney(CurrentPlacingBuildingData.Cost))
                {
                    var buildingGO = Instantiate(CurrentPlacingBuildingData.Prefab, worldPos,
                        CurrentPlacingBuildingData.Prefab.transform.rotation);

                    var area = AreaManager.Instance.GetAreaByPosition(worldPos);
                    if (area)
                    {
                        buildingGO.transform.SetParent(area.transform);
                    }

                    if (buildingGO.TryGetComponent(out BaseBuilding building))
                    {
                        building.Init(CurrentPlacingBuildingData);
                    }

                    GridManager.Instance.OccupyArea(gridPos, CurrentPlacingBuildingData.Size);
                    GPEvents.OnCompeletePlacementAction?.Invoke(worldPos);
                }
            }
        }

        private void UpdatePlacementIndicator(RaycastHit hit)
        {
            var gridPos = GridManager.Instance.WorldToGridPosition(hit.point);
            var isPlaceable =
                GridManager.Instance.IsAreaPlaceable(gridPos, CurrentPlacingBuildingData.Size) &&
                ValueManager.Instance.CanAfford(CurrentPlacingBuildingData.Cost);

            // Save the current position for building placement
            m_CurrentIndicatorPosition = new Vector2(gridPos.x, gridPos.y);

            // Update the data; the Panel will respond automatically
            m_PlacementIndicatorData.Position.Value = m_CurrentIndicatorPosition;
            m_PlacementIndicatorData.IsPlaceable.Value = isPlaceable;
        }

        public bool TryGetBuildingByID(int id, out BuildingData buildingData)
        {
            return BuildingDict.TryGetValue(id, out buildingData);
        }
    }
}