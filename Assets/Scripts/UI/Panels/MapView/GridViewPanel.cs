#region

using System.Collections.Generic;
using Core.Data;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace UI.Panels
{
    public class GridViewPanel : BasePanel<GridViewData>
    {
        [SerializeField] private RectTransform m_LineContainer;
        [SerializeField] private GameObject m_HLinePrefab;
        [SerializeField] private GameObject m_VLinePrefab;

        private readonly List<GameObject> m_Lines = new();

        protected override void OnBind()
        {
            Data.IsVisible.Bind(OnVisibilityChanged);
            CreateAllLines();
        }

        protected override void OnUnbind()
        {
            Data.IsVisible.Unbind(OnVisibilityChanged);
            ClearAllLines();
        }

        private void OnVisibilityChanged(bool old, bool now)
        {
            gameObject.SetActive(now);
        }

        private void CreateAllLines()
        {
            var size = Data.GridSize;

            var minX = int.MaxValue;
            var maxX = int.MinValue;
            var minY = int.MaxValue;
            var maxY = int.MinValue;

            foreach (var pos in Data.GridPositions)
            {
                minX = Mathf.Min(minX, pos.x);
                maxX = Mathf.Max(maxX, pos.x);
                minY = Mathf.Min(minY, pos.y);
                maxY = Mathf.Max(maxY, pos.y);
            }

            var gridMaxX = maxX + size;
            var gridMaxY = maxY + size;

            var totalWidth = gridMaxX - minX;
            var totalHeight = gridMaxY - minY;

            for (var y = minY; y <= gridMaxY; y += size)
            {
                CreateLine(m_HLinePrefab, minX, y, totalWidth, true);
            }

            for (var x = minX; x <= gridMaxX; x += size)
            {
                CreateLine(m_VLinePrefab, x, minY, totalHeight, false);
            }
        }

        private void CreateLine
        (
            GameObject prefab, int x, int y,
            int length, bool isHorizontal
        )
        {
            var line = Instantiate(prefab, m_LineContainer);
            var rt = line.GetComponent<RectTransform>();

            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = isHorizontal
                ? new Vector2(length, 1)
                : new Vector2(1, length);

            line.GetComponent<Image>().color = Data.LineColor;
            m_Lines.Add(line);
        }

        private void ClearAllLines()
        {
            foreach (var line in m_Lines)
            {
                Destroy(line);
            }

            m_Lines.Clear();
        }
    }
}