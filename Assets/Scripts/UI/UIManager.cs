#region

using System;
using System.Collections.Generic;
using System.Linq;
using Tool;
using UnityEngine;

#endregion

namespace UI
{
    public enum UICanvasType
    {
        Overlay,
        Camera,
        World
    }

    public class UIManager : SingletonMono<UIManager>
    {
        public RectTransform OverlayCanvasParent;
        public RectTransform CameraCanvasParent;
        public RectTransform WorldCanvasParent;

        private List<GameObject> m_UIPrefabs;
        private readonly Dictionary<string, GameObject> m_UIPrefabDict = new();
        private readonly Dictionary<Type, List<BasePanel>> m_OpenedPanels = new();

        protected override void Awake()
        {
            base.Awake();
            InitializePrefabs();
        }

        private void InitializePrefabs()
        {
            m_UIPrefabs = Resources.LoadAll<GameObject>("UIPanel").ToList();
            foreach (var prefab in m_UIPrefabs.Where(prefab => prefab != null))
            {
                m_UIPrefabDict.TryAdd(prefab.name, prefab);
            }
        }

        public T CreatePanelInSilence<T>
            (UICanvasType canvasType = UICanvasType.Overlay, bool active = true) where T : BasePanel
        {
            var panelType = typeof(T);
            var panelName = panelType.Name.Replace("Panel", "");

            if (!m_UIPrefabDict.TryGetValue(panelName, out var prefab))
            {
                Debug.LogWarning($" No Prefab named {panelName} found in the Resources/UIPanel folder ");
                return null;
            }

            var parent = GetCanvas(canvasType);
            if (!parent)
            {
                Debug.LogWarning($"No Canvas type: {canvasType}");
                return null;
            }

            //NOTICE: ShowPanel will create the new one
            var panelGO = Instantiate(prefab, parent);
            panelGO.name = panelName;


            if (!panelGO.TryGetComponent(out T panelComponent))
            {
                Destroy(panelGO);
                Debug.LogWarning("No UI component added");
                return null;
            }

            if (!m_OpenedPanels.TryGetValue(panelType, out var panelList))
            {
                panelList = new List<BasePanel>();
                m_OpenedPanels.Add(panelType, panelList);
            }

            panelList.Add(panelComponent);
            panelComponent.Canvas = parent;

            if (active)
            {
                ShowPanelInSilence(panelComponent);
            }
            else
            {
                HidePanelInSilence(panelComponent);
            }

            return panelComponent;
        }

        public T CreatePanel<T>
            (UICanvasType canvasType = UICanvasType.Overlay, bool active = true) where T : BasePanel
        {
            var panelType = typeof(T);
            var panelName = panelType.Name.Replace("Panel", "");

            if (!m_UIPrefabDict.TryGetValue(panelName, out var prefab))
            {
                Debug.LogWarning($"No Prefab named {panelName} found in the Resources/UIPanel folder");
                return null;
            }

            var parent = GetCanvas(canvasType);
            if (!parent)
            {
                Debug.LogWarning($"no Canvas type: {canvasType}");
                return null;
            }

            //NOTICE: ShowPanel will create the new one
            var panelGO = Instantiate(prefab, parent);
            panelGO.name = panelName;

            if (!panelGO.TryGetComponent(out T panelComponent))
            {
                Destroy(panelGO);
                Debug.LogWarning("No UI component added");
                return null;
            }

            if (!m_OpenedPanels.TryGetValue(panelType, out var panelList))
            {
                panelList = new List<BasePanel>();
                m_OpenedPanels.Add(panelType, panelList);
            }

            panelList.Add(panelComponent);
            panelComponent.Canvas = parent;


            if (active)
            {
                ShowPanel(panelComponent);
            }
            else
            {
                HidePanel(panelComponent);
            }

            return panelComponent;
        }

        public void ShowPanel(BasePanel panelInstance)
        {
            if (panelInstance == null)
            {
                return;
            }

            panelInstance.gameObject.SetActive(true);

            panelInstance.OnShow();
        }

        //No OnShow
        private void ShowPanelInSilence(BasePanel panelInstance)
        {
            if (panelInstance == null)
            {
                return;
            }

            panelInstance.gameObject.SetActive(true);
        }

        public void HidePanel(BasePanel panelInstance)
        {
            if (panelInstance == null)
            {
                return;
            }

            panelInstance.OnHide();

            panelInstance.gameObject.SetActive(false);
        }

        //No OnHide
        private void HidePanelInSilence(BasePanel panelInstance)
        {
            if (panelInstance == null)
            {
                return;
            }

            panelInstance.gameObject.SetActive(false);
        }

        public void HideAllPanel<T>() where T : BasePanel
        {
            var panelType = typeof(T);
            if (m_OpenedPanels.TryGetValue(panelType, out var panelList))
            {
                foreach (var panel in panelList)
                {
                    panel.OnHide();

                    panel.gameObject.SetActive(false);
                }
            }
        }

        public void RemovePanel(BasePanel panelInstance)
        {
            if (panelInstance == null)
            {
                return;
            }

            var panelType = panelInstance.GetType();
            if (m_OpenedPanels.TryGetValue(panelType, out var panelList))
            {
                panelList.Remove(panelInstance);
            }
        }

        private RectTransform GetCanvas(UICanvasType canvasType)
        {
            return canvasType switch
            {
                UICanvasType.Overlay when OverlayCanvasParent != null => OverlayCanvasParent,
                UICanvasType.Camera when CameraCanvasParent != null => CameraCanvasParent,
                UICanvasType.World when WorldCanvasParent != null => WorldCanvasParent,
                _ => null
            };
        }
    }
}