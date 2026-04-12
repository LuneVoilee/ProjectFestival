using System.Collections.Generic;
using Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class TrafficViewPanel : BasePanel<TrafficViewData>
    {
        [SerializeField] private RectTransform m_CellContainer;
        [SerializeField] private RectTransform m_PathContainer;
        [SerializeField] private GameObject m_CellPrefab;
        [SerializeField] private GameObject m_PathLinePrefab; 

        private readonly Dictionary<Vector2Int, Image> m_Cells = new();
        private readonly List<GameObject> m_PathLines = new();

        protected override void OnBind()
        {
            Data.IsVisible.Bind(OnVisibilityChanged);
            Data.ChangedCells.BindAdd(OnCellDataChanged);
            Data.ChangedCells.BindClear(OnCellDataReset);
            Data.VisitorPaths.BindAdd(OnPathAdded);
            Data.VisitorPaths.BindRemove(OnPathRemoved);
            Data.VisitorPaths.BindClear(OnAllPathsCleared);

            CreateAllCells();
        }

        protected override void OnUnbind()
        {
            Data.IsVisible.Unbind(OnVisibilityChanged);
            Data.ChangedCells.UnbindAdd(OnCellDataChanged);
            Data.ChangedCells.UnbindClear(OnCellDataReset);
            Data.VisitorPaths.UnbindAdd(OnPathAdded);
            Data.VisitorPaths.UnbindRemove(OnPathRemoved);
            Data.VisitorPaths.UnbindClear(OnAllPathsCleared);

            ClearAll();
        }

        private void OnVisibilityChanged(bool old, bool now)
        {
            gameObject.SetActive(now);
        }

        #region Cells

        private void CreateAllCells()
        {
            foreach (var pos in Data.AllPositions)
            {
                var cell = Instantiate(m_CellPrefab, m_CellContainer);
                var rt = cell.GetComponent<RectTransform>();

                rt.anchoredPosition = new Vector2(pos.x, pos.y);
                rt.sizeDelta = new Vector2(Data.GridSize, Data.GridSize);

                var image = cell.GetComponent<Image>();
                image.color = Data.NotCrowdedColor;
                m_Cells[pos] = image;
            }
        }

        private void OnCellDataChanged(int index, TrafficCellData cellData)
        {
            if (m_Cells.TryGetValue(cellData.Position, out var image))
            {
                image.color = Data.GetColorByLevel(cellData.CrowdLevel);
            }
        }

        private void OnCellDataReset()
        {
            foreach (var image in m_Cells.Values)
            {
                image.color = Data.NotCrowdedColor;
            }
        }

        #endregion

        #region Paths

        private void OnPathAdded(int index, Vector3[] path)
        {
            var pathLine = Instantiate(m_PathLinePrefab, m_PathContainer);
            var lineRenderer = pathLine.GetComponent<LineRenderer>();

            lineRenderer.positionCount = path.Length;
            lineRenderer.SetPositions(path);
            lineRenderer.startColor = Data.PathColor;
            lineRenderer.endColor = Data.PathColor;

            m_PathLines.Add(pathLine);
        }

        private void OnPathRemoved(int index, Vector3[] path)
        {
            if (index >= 0 && index < m_PathLines.Count)
            {
                Destroy(m_PathLines[index]);
                m_PathLines.RemoveAt(index);
            }
        }

        private void OnAllPathsCleared()
        {
            foreach (var line in m_PathLines)
            {
                Destroy(line);
            }
            m_PathLines.Clear();
        }

        #endregion

        private void ClearAll()
        {
            foreach (var cell in m_Cells.Values)
            {
                if (cell != null)
                    Destroy(cell.gameObject);
            }
            m_Cells.Clear();

            OnAllPathsCleared();
        }
    }
}
