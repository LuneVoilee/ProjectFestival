#region

using System.Collections.Generic;
using Core;
using Core.Data;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace UI
{
    public class MapViewCtrlPanel : BasePanel<MapViewData>
    {
        public Button DefaultBtn;
        public Button BuildingBtn;
        public Button TrafficBtn;

        private Dictionary<int, Button> m_Dict;

        private void Awake()
        {
            m_Dict = new Dictionary<int, Button>
            {
                [0] = DefaultBtn,
                [1] = BuildingBtn,
                [2] = TrafficBtn
            };
        }

        private void OnEnable()
        {
            DefaultBtn.onClick.AddListener(() => { UIEvents.OnDefaultModeAction?.Invoke(); });
            BuildingBtn.onClick.AddListener(() => { UIEvents.OnBuildingModeAction?.Invoke(); });
            TrafficBtn.onClick.AddListener(() => { UIEvents.OnTrafficModeAction?.Invoke(); });
        }

        private void OnDisable()
        {
            DefaultBtn.onClick.RemoveAllListeners();
            BuildingBtn.onClick.RemoveAllListeners();
            TrafficBtn.onClick.RemoveAllListeners();
        }

        protected override void OnBind()
        {
            Data.CurrentMode.Bind(HandleModeChange);
        }

        protected override void OnUnbind()
        {
            Data.CurrentMode.Unbind(HandleModeChange);
        }

        private void HandleModeChange(int oldV, int newV)
        {
            if (oldV != -1)
            {
                SetHighLight(oldV, false);
            }

            SetHighLight(newV, true);
        }

        private void SetHighLight(int btnIndex, bool isHighLight)
        {
            m_Dict[btnIndex].image.color = isHighLight ? Color.yellow : Color.white;
        }
    }
}