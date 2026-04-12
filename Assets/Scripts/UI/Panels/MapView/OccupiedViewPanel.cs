using System.Collections.Generic;
using Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Panels
{
    public class OccupiedViewPanel : BasePanel<OccupiedViewData>
    {
        [SerializeField] private RectTransform m_CellContainer;
        [SerializeField] private GameObject m_CellPrefab;

        private readonly Dictionary<Vector2Int, GameObject> m_Cells = new();

        protected override void OnBind()
        {
            Data.IsVisible.Bind(OnVisibilityChanged);
            Data.OccupiedPositions.BindAdd(OnCellAdded);
            Data.OccupiedPositions.BindRemove(OnCellRemoved);
            Data.OccupiedPositions.BindClear(OnAllCellsCleared);

            for (var i = 0; i < Data.OccupiedPositions.Count; i++)
            {
                CreateCell(Data.OccupiedPositions[i]);
            }
        }

        protected override void OnUnbind()
        {
            Data.IsVisible.Unbind(OnVisibilityChanged);
            Data.OccupiedPositions.UnbindAdd(OnCellAdded);
            Data.OccupiedPositions.UnbindRemove(OnCellRemoved);
            Data.OccupiedPositions.UnbindClear(OnAllCellsCleared);

            ClearAllCells();
        }

        private void OnVisibilityChanged(bool old, bool now)
        {
            gameObject.SetActive(now);
        }

        private void OnCellAdded(int index, Vector2Int pos)
        {
            CreateCell(pos);
        }

        private void OnCellRemoved(int index, Vector2Int pos)
        {
            if (m_Cells.TryGetValue(pos, out var cell))
            {
                Destroy(cell);
                m_Cells.Remove(pos);
            }
        }

        private void OnAllCellsCleared()
        {
            ClearAllCells();
        }

        private void CreateCell(Vector2Int pos)
        {
            if (m_Cells.ContainsKey(pos)) return;

            var cell = Instantiate(m_CellPrefab, m_CellContainer);
            var rt = cell.GetComponent<RectTransform>();

            rt.anchoredPosition = new Vector2(pos.x, pos.y);
            rt.sizeDelta = new Vector2(Data.GridSize, Data.GridSize);

            cell.GetComponent<Image>().color = Data.OccupiedColor;
            m_Cells[pos] = cell;
        }

        private void ClearAllCells()
        {
            foreach (var cell in m_Cells.Values)
            {
                Destroy(cell);
            }
            m_Cells.Clear();
        }
    }
}
