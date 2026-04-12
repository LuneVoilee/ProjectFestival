#region

using System.Collections.Generic;
using Core;
using Core.Data;
using Core.Reactive;
using UI.Panels;
using UnityEngine;

#endregion

namespace UI
{
    public class UIController : MonoBehaviour
    {
        private readonly Dictionary<int, BuildingCardData> m_BuildingCards = new();

        private readonly Dictionary<int, LockAreaData> m_LockAreaDataDict = new();

        private void Start()
        {
            BindEconomyPanel();

            BindTimeScalerPanel();

            BindViewCtrlPanel();

            BindMapViewPanels();

            BindSettingsPanel();
        }


        private void OnEnable()
        {
            UIEvents.OnCreateLockAreaUIAction += CreateUnlockAreaPanel;
            UIEvents.OnCreateCardUIAction += AddBuildingPanel;
            UIEvents.OnCardClickAction += HandleUIClicked;
            GPEvents.OnCompeletePlacementAction += HandleCompleteClick;

            UIEvents.OnShowBuildingListAction += ShowBuildingListPanel;
            UIEvents.OnHideBuildingListAction += HideBuildingListPanel;

            GPEvents.OnFeelHappyAction += visitor => CreateEmotionIndicator(visitor, true);
            GPEvents.OnFeelUnhappyAction += visitor => CreateEmotionIndicator(visitor, false);
            GPEvents.OnLevelChangeAction += HandleLevelChange;

            InputManager.Instance.OnRightClickAction += HandleCompleteClick;
            InputManager.Instance.OnESCClickAction += ShowOrCreateSettingsPanel;

            GPEvents.OnBeginConversationAction += BeginConversation;
            GPEvents.OnAreaUnlockedAction += HandleAreaUnlocked;
        }

        private void OnDisable()
        {
            UIEvents.OnCreateLockAreaUIAction -= CreateUnlockAreaPanel;
            UIEvents.OnCreateCardUIAction -= AddBuildingPanel;
            UIEvents.OnCardClickAction -= HandleUIClicked;
            GPEvents.OnCompeletePlacementAction -= HandleCompleteClick;

            GPEvents.OnFeelHappyAction -= visitor => CreateEmotionIndicator(visitor, true);
            GPEvents.OnFeelUnhappyAction -= visitor => CreateEmotionIndicator(visitor, false);

            GPEvents.OnLevelChangeAction -= HandleLevelChange;

            if (InputManager.Instance)
            {
                InputManager.Instance.OnRightClickAction -= HandleCompleteClick;
                InputManager.Instance.OnESCClickAction -= ShowOrCreateSettingsPanel;
            }

            GPEvents.OnBeginConversationAction -= BeginConversation;
            GPEvents.OnAreaUnlockedAction -= HandleAreaUnlocked;
        }

        private void Update()
        {
            if (IsShowBuildingListPanel)
            {
                if (m_BuildingListPanel && !m_BuildingListPanel.IsShowing)
                {
                    UIManager.Instance.ShowPanel(m_BuildingListPanel);
                }
            }
            else
            {
                if (m_BuildingListPanel && m_BuildingListPanel.IsShowing)
                {
                    UIManager.Instance.HidePanel(m_BuildingListPanel);
                }
            }
        }

        #region SettingsPanel

        private void ShowOrCreateSettingsPanel()
        {
            if (m_SettingsPanel == null)
            {
                Debug.LogWarning("m_SettingsPanel == null");
                return;
            }

            UIManager.Instance.ShowPanel(m_SettingsPanel);
        }

        #endregion

        #region SecretaryPanel

        private SecretaryPanel m_SecretaryPanel;

        private void BeginConversation(int id, List<string> content)
        {
            if (content.Count == 0)
            {
                return;
            }

            if (m_SecretaryPanel == null)
            {
                m_SecretaryPanel = UIManager.Instance.CreatePanel<SecretaryPanel>();
            }
            else
            {
                UIManager.Instance.ShowPanel(m_SecretaryPanel);
            }

            var data = new SecretaryData(id, content);
            m_SecretaryPanel.Bind(data);
        }

        #endregion

        #region LevelUp

        private LevelUpPanel m_LevelUpPanel;

        private void HandleLevelChange(ReactiveValue<int> level, float _, float __)
        {
            if (level == 1)
            {
                if (!m_LevelUpPanel)
                {
                    //NOTICE: Avoid triggering OnShow/OnHide on m_LevelUpPanel, which would prevent BuildingListPanel from hiding as expected by SecretaryPanel
                    m_LevelUpPanel =
                        UIManager.Instance.CreatePanelInSilence<LevelUpPanel>(active: false);
                    var data = new LevelUpData(level);
                    m_LevelUpPanel.Bind(data);
                }

                return;
            }

            if (m_LevelUpPanel != null)
            {
                UIManager.Instance.ShowPanel(m_LevelUpPanel);
            }
        }

        #endregion

        #region EmotionIndicator

        private void CreateEmotionIndicator(Transform target, bool isHappy)
        {
            var emotionPanel = UIManager.Instance.CreatePanel<EmotionIndicatorPanel>();
            if (emotionPanel != null)
            {
                emotionPanel.Init(target, isHappy);
            }
        }

        #endregion

        #region BuildingPanel

        private BuildingListPanel m_BuildingListPanel;
        private BuildingListData m_BuildingListData;

        private IndicatorPanel m_IndicatorPanel;
        private int m_CurrentPanelID = -1;

        private bool IsShowBuildingListPanel = true;
        private SettingsPanel m_SettingsPanel;

        private GridViewPanel m_GridViewPanel;
        private OccupiedViewPanel m_OccupiedViewPanel;
        private TrafficViewPanel m_TrafficViewPanel;
        private PlacementIndicatorPanel m_PlacementIndicatorPanel;

        private void BindMapViewPanels()
        {
            if (GPEvents.GetGridViewData == null || GPEvents.GetOccupiedViewData == null ||
                GPEvents.GetTrafficViewData == null) return;

            var gridViewData = GPEvents.GetGridViewData();
            var occupiedViewData = GPEvents.GetOccupiedViewData();
            var trafficViewData = GPEvents.GetTrafficViewData();


            m_OccupiedViewPanel =
                UIManager.Instance.CreatePanelInSilence<OccupiedViewPanel>(UICanvasType.World,
                    false);
            m_OccupiedViewPanel?.Bind(occupiedViewData);
            m_OccupiedViewPanel?.transform.SetSiblingIndex(0);

            m_TrafficViewPanel =
                UIManager.Instance.CreatePanelInSilence<TrafficViewPanel>(UICanvasType.World,
                    false);
            m_TrafficViewPanel?.Bind(trafficViewData);
            m_TrafficViewPanel?.transform.SetSiblingIndex(1);

            m_GridViewPanel = UIManager.Instance.CreatePanel<GridViewPanel>(UICanvasType.World);
            m_GridViewPanel?.Bind(gridViewData);
            m_GridViewPanel?.transform.SetSiblingIndex(2);

            BindPlacementIndicatorPanel();
        }

        private void BindPlacementIndicatorPanel()
        {
            if (GPEvents.GetPlacementIndicatorData == null) return;

            var data = GPEvents.GetPlacementIndicatorData();
            if (data == null) return;

            m_PlacementIndicatorPanel =
                UIManager.Instance.CreatePanelInSilence<PlacementIndicatorPanel>(UICanvasType.World, false);
            m_PlacementIndicatorPanel?.Bind(data);
            m_PlacementIndicatorPanel?.transform.SetSiblingIndex(3);
        }

        private void ShowBuildingListPanel()
        {
            IsShowBuildingListPanel = true;
        }

        private void HideBuildingListPanel()
        {
            IsShowBuildingListPanel = false;
        }

        public void AddBuildingPanel
        (
            int id, Sprite image, string nam,
            string cost
        )
        {
            if (m_BuildingListPanel == null)
            {
                m_BuildingListPanel = UIManager.Instance.CreatePanel<BuildingListPanel>();
                m_BuildingListData = new BuildingListData();
                m_BuildingListPanel.Bind(m_BuildingListData);
            }

            var data = new BuildingCardData(id, image, nam, cost);
            m_BuildingCards[id] = data;
            m_BuildingListData.BuildingCards.Add(data);
        }

        private void HandleUIClicked(int id, Vector3 pos)
        {
            if (m_CurrentPanelID == id)
            {
                return;
            }

            // Cancel the previous selected one
            if (m_CurrentPanelID != -1)
            {
                if (m_BuildingCards.TryGetValue(m_CurrentPanelID, out var prevData))
                {
                    prevData.IsSelected.Value = false;
                }
            }

            m_CurrentPanelID = id;

            // Set the current selected one
            if (m_BuildingCards.TryGetValue(id, out var currentData))
            {
                currentData.IsSelected.Value = true;
            }

            if (m_IndicatorPanel == null)
            {
                m_IndicatorPanel = UIManager.Instance.CreatePanel<IndicatorPanel>();
            }

            m_IndicatorPanel.ShowIndicator(pos);
        }

        private void HandleCompleteClick(Vector3 _)
        {
            HandleCompleteClick();
        }

        private void HandleCompleteClick()
        {
            // Cancel the selected one
            if (m_CurrentPanelID != -1 &&
                m_BuildingCards.TryGetValue(m_CurrentPanelID, out var data))
            {
                data.IsSelected.Value = false;
            }

            if (m_IndicatorPanel != null)
            {
                m_IndicatorPanel.HideIndicator();
            }

            UIEvents.OnCompleteCardClickAction?.Invoke(m_CurrentPanelID);
            m_CurrentPanelID = -1;
        }

        #endregion


        #region UnlockAreaPanel

        public void CreateUnlockAreaPanel
        (
            int index, string text, GameObject area,
            float areaCost
        )
        {
            var unlockAreaPanel =
                UIManager.Instance.CreatePanel<LockAreaPanel>(UICanvasType.World);
            if (unlockAreaPanel == null)
            {
                return;
            }

            unlockAreaPanel.transform.SetSiblingIndex(10);

            var data = new LockAreaData(index, areaCost, area.transform);
            m_LockAreaDataDict[index] = data;
            unlockAreaPanel.Bind(data);
        }

        private void HandleAreaUnlocked(int index)
        {
            if (m_LockAreaDataDict.TryGetValue(index, out var data))
            {
                data.IsUnlocked.Value = true;
            }
        }

        #endregion

        #region Bind

        private void BindEconomyPanel()
        {
            if (GPEvents.GetEconomyData != null)
            {
                var (money, happiness) = GPEvents.GetEconomyData();
                if (money != null && happiness != null)
                {
                    var economyPanel = UIManager.Instance.CreatePanel<EconomyPanel>();
                    if (economyPanel != null)
                    {
                        var data = new EconomyPanelData(money, happiness);
                        economyPanel.Bind(data);
                    }
                }
            }
        }

        private void BindTimeScalerPanel()
        {
            if (GPEvents.GetTimeScale != null)
            {
                var timeScale = GPEvents.GetTimeScale();
                if (timeScale != null)
                {
                    var timeScalePanel =
                        UIManager.Instance.CreatePanel<TimeScalePanel>();
                    if (timeScalePanel != null)
                    {
                        var data = new TimeScaleData(timeScale);
                        timeScalePanel.Bind(data);
                    }
                }
            }
        }

        private void BindViewCtrlPanel()
        {
            if (GPEvents.GetViewMode != null)
            {
                var viewMode = GPEvents.GetViewMode();
                if (viewMode != null)
                {
                    var viewCtrlPanel =
                        UIManager.Instance.CreatePanel<MapViewCtrlPanel>();
                    if (viewCtrlPanel != null)
                    {
                        var data = new MapViewData(viewMode);
                        viewCtrlPanel.Bind(data);
                    }
                }
            }
        }

        private void BindSettingsPanel()
        {
            if (GPEvents.GetMoveSpeed != null && GPEvents.GetZoomSpeed != null &&
                GPEvents.GetVolume != null && GPEvents.GetDifficulty != null)
            {
                var moveSpeed = GPEvents.GetMoveSpeed();
                var zoomSpeed = GPEvents.GetZoomSpeed();
                var volume = GPEvents.GetVolume();
                var difficulty = GPEvents.GetDifficulty();
                if (moveSpeed != null && zoomSpeed != null && volume != null && difficulty != null)
                {
                    if (m_SettingsPanel == null)
                    {
                        m_SettingsPanel =
                            UIManager.Instance.CreatePanelInSilence<SettingsPanel>(
                                UICanvasType.Overlay, false);

                        var data = new SettingsData
                        {
                            CharacterMovement = moveSpeed,
                            CharacterZoom = zoomSpeed,
                            Volume = volume,
                            Difficulty = difficulty
                        };

                        m_SettingsPanel.Bind(data);

                        UIEvents.OnAfterBindSettingsAction?.Invoke();
                    }
                }
            }
        }

        #endregion
    }
}