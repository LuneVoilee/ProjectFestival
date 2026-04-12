#region

using System;
using System.Collections.Generic;
using Core;
using GamePlay.Level;
using Tool;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

#endregion

namespace GamePlay.Grid
{
    [Serializable]
    public class AreaCost
    {
        public GameObject Area;
        public float Cost;
    }

    public class AreaManager : SingletonMono<AreaManager>
    {
        public List<AreaCost> AreaCosts;
        [Header("The default terrain size is not large enough to cover the plane and require additional adjustments.")] public Vector3 ExtraSize;

        private readonly List<Terrain> m_UnlockedArea = new();
        private readonly List<UnlockedAreaData> m_UnlockedAreaData = new();

        private NavMeshData m_SharedNavMeshData;
        private NavMeshDataInstance m_NavMeshInstance;

        private void Start()
        {
            m_UnlockedArea.Clear();

            // Initialize the shared NavMeshData
            InitializeSharedNavMesh();

            for (var i = 0; i < AreaCosts.Count; i++)
            {
                var areaCost = AreaCosts[i];

                //Special initial block
                if (i == 8)
                {
                    areaCost.Area.SetActive(true);

                    UpdateBounds(areaCost.Area);
                    RebuildAllNavMesh();

                    continue;
                }

                UIEvents.OnCreateLockAreaUIAction?.Invoke(i, $"Need {areaCost.Cost} To Unlock",
                    areaCost.Area, areaCost.Cost);

                areaCost.Area.SetActive(false);
            }
        }

        private void InitializeSharedNavMesh()
        {
            var firstSurface = AreaCosts[0].Area.GetComponent<NavMeshSurface>();
            if (firstSurface == null)
            {
                Debug.LogError("The first area has no NavMeshSurface component!");
                return;
            }

            m_SharedNavMeshData = new NavMeshData(firstSurface.agentTypeID);
            m_NavMeshInstance = NavMesh.AddNavMeshData(m_SharedNavMeshData);
        }

        private void OnEnable()
        {
            UIEvents.OnUnlockAreaAction += UnlockArea;
            GPEvents.OnCompeletePlacementAction += ReBakeNavMesh;
        }

        private void OnDisable()
        {
            UIEvents.OnUnlockAreaAction -= UnlockArea;
            GPEvents.OnCompeletePlacementAction -= ReBakeNavMesh;
        }

        protected override void OnDestroy()
        {
            // Clear the shared NavMeshData

            if (m_NavMeshInstance.valid)
            {
                m_NavMeshInstance.Remove();
            }

            base.OnDestroy();
        }

        private void UnlockArea(int index)
        {
            if (index < 0 || index >= AreaCosts.Count)
            {
                return;
            }

            var areaCost = AreaCosts[index];

            if (areaCost.Area.activeSelf)
            {
                return;
            }

            if (ValueManager.Instance.TrySpendMoney(areaCost.Cost))
            {
                var area = areaCost.Area;
                area.SetActive(true);

                UpdateBounds(area);
                RebuildAllNavMesh();

                GPEvents.OnAreaUnlockedAction?.Invoke(index);
            }
        }

        public void ReBakeNavMesh(Vector3 pos)
        {
            // Instead of baking a single region, rebuild the entire NavMesh
            RebuildAllNavMesh();
        }


        private void RebuildAllNavMesh()
        {
            if (m_SharedNavMeshData == null)
            {
                Debug.LogWarning("SharedNavMeshData is not initialized.");
                return;
            }

            if (m_UnlockedArea.Count == 0)
            {
                Debug.LogWarning("No unlocked regions");
                return;
            }

            Bounds totalBounds = CalculateTotalBounds();

            var allSources = new List<NavMeshBuildSource>();
            var allMarkups = new List<NavMeshBuildMarkup>();

            foreach (var terrain in m_UnlockedArea)
            {
                var area = terrain.gameObject;
                var surface = area.GetComponent<NavMeshSurface>();
                if (surface == null) continue;

                var modifiers = area.GetComponentsInChildren<NavMeshModifier>(true);
                foreach (var modifier in modifiers)
                {
                    if (!modifier.enabled) continue;

                    allMarkups.Add(new NavMeshBuildMarkup
                    {
                        root = modifier.transform,
                        overrideArea = modifier.overrideArea,
                        area = modifier.area,
                        ignoreFromBuild = modifier.ignoreFromBuild
                    });
                }
            }

            var baseSurface = m_UnlockedArea[0].GetComponent<NavMeshSurface>();

            NavMeshBuilder.CollectSources(
                totalBounds,
                baseSurface.layerMask,
                baseSurface.useGeometry,
                baseSurface.defaultArea,
                allMarkups,
                allSources
            );

            //Debug.Log($" {allSources.Count} 个Sources，总Bounds: {totalBounds}");

            foreach (var terrain in m_UnlockedArea)
            {
                var area = terrain.gameObject;
                var volumes = area.GetComponentsInChildren<NavMeshModifierVolume>(true);
                foreach (var volume in volumes)
                {
                    if (!volume.enabled) continue;

                    var scale = volume.transform.lossyScale;
                    var size = new Vector3(
                        volume.size.x * Mathf.Abs(scale.x),
                        volume.size.y * Mathf.Abs(scale.y),
                        volume.size.z * Mathf.Abs(scale.z)
                    );

                    allSources.Add(new NavMeshBuildSource
                    {
                        shape = NavMeshBuildSourceShape.ModifierBox,
                        transform = Matrix4x4.TRS(
                            volume.transform.TransformPoint(volume.center),
                            volume.transform.rotation,
                            Vector3.one
                        ),
                        size = size,
                        area = volume.area
                    });
                }
            }

            NavMeshBuilder.UpdateNavMeshDataAsync(
                m_SharedNavMeshData,
                baseSurface.GetBuildSettings(),
                allSources,
                totalBounds
            );

            //Debug.Log($"{m_UnlockedArea.Count} regions");
        }

        private Bounds CalculateTotalBounds()
        {
            if (m_UnlockedArea.Count == 0)
            {
                return new Bounds(Vector3.zero, Vector3.zero);
            }

            var firstTerrain = m_UnlockedArea[0];
            var firstBounds = firstTerrain.terrainData.bounds;
            var firstCenter = firstTerrain.transform.position + firstBounds.center;
            var totalBounds = new Bounds(firstCenter, firstBounds.size + ExtraSize);

            for (int i = 1; i < m_UnlockedArea.Count; i++)
            {
                var terrain = m_UnlockedArea[i];
                var terrainBounds = terrain.terrainData.bounds;
                var center = terrain.transform.position + terrainBounds.center;
                var expandedSize = terrainBounds.size + ExtraSize;

                totalBounds.Encapsulate(new Bounds(center, expandedSize));
            }

            return totalBounds;
        }

        public GameObject GetAreaByPosition(Vector3 pos)
        {
            GameObject area = null;

            foreach (var data in m_UnlockedAreaData)
            {
                if (pos.x < data.Max.x && pos.x > data.Min.x)
                {
                    if (pos.z < data.Max.z && pos.z > data.Min.z)
                    {
                        area = data.Area;
                        break;
                    }
                }
            }

            return area;
        }

        private void UpdateBounds(GameObject area)
        {
            var t = area.GetComponent<Terrain>();
            if (t == null)
            {
                Debug.LogWarning($"Area {area.name} has no Terrain");
                return;
            }

            m_UnlockedArea.Add(t);
            CalculateDataOf(t);
        }

        internal struct UnlockedAreaData
        {
            public GameObject Area;
            public Vector3 Min;
            public Vector3 Max;

            public UnlockedAreaData(GameObject area, Vector3 min, Vector3 max)
            {
                Area = area;
                Min = min;
                Max = max;
            }
        }


        private void CalculateDataOf(Terrain t)
        {
            var bounds = t.terrainData.bounds;
            var center = t.transform.position + bounds.center;

            var max = center + bounds.extents;
            var min = center - bounds.extents;

            m_UnlockedAreaData.Add(new UnlockedAreaData(t.gameObject, min, max));
        }

        public bool IsPositionLegal(Vector3 pos)
        {
            foreach (var data in m_UnlockedAreaData)
            {
                //Debug.Log("range" + max + min);
                if (pos.x < data.Max.x && pos.x > data.Min.x)
                {
                    if (pos.z < data.Max.z && pos.z > data.Min.z)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}