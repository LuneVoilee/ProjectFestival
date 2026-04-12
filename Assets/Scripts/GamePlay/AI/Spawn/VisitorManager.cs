#region

using System.Collections;
using System.Collections.Generic;
using Core;
using Core.Reactive;
using Tool;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

#endregion

namespace GamePlay.AI
{
    public class VisitorManager : SingletonMono<VisitorManager>
    {
        public List<GameObject> VisitorPrefabs;
        public int InitialVisitorCount = 20;
        public Transform SpawnPoint;
        public float SpawnRadius = 5f;

        public List<VisitorController> AllVisitors { get; private set; }

        private float m_GenRate;
        private float m_RandomOffset;

        protected override void Awake()
        {
            base.Awake();
            AllVisitors = new List<VisitorController>();
        }

        private void Start()
        {
            if (VisitorPrefabs == null || VisitorPrefabs.Count == 0 || SpawnPoint == null)
            {
                Debug.LogWarning("VisitorPrefab or SpawnPoint is not assigned.");
            }

            //InitTestVisitors(100);

            StartCoroutine(SpawnVisitors());
        }

        private void OnEnable()
        {
            GPEvents.OnLevelChangeAction += HandleLevelChange;
        }

        private void OnDisable()
        {
            GPEvents.OnLevelChangeAction -= HandleLevelChange;
        }

        private void HandleLevelChange(ReactiveValue<int> _, float rate, float offset)
        {
            m_GenRate = rate;
            m_RandomOffset = offset;
        }


        private void InitTestVisitors(int count)
        {
            for (int i = 0; i < count; i++)
            {
                // Spawn at a random position
                Vector3 randomOffset = Random.insideUnitSphere * SpawnRadius;
                randomOffset.y = 0;

                Vector3 spawnPosition = SpawnPoint.position + randomOffset;

                if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, SpawnRadius,
                        NavMesh.AllAreas))
                {
                    CreateRandomVisitor(hit.position);
                }
                else
                {
                    Debug.LogWarning("Failed to SamplePosition");
                    CreateRandomVisitor(spawnPosition);
                }
            }
        }


        private IEnumerator SpawnVisitors()
        {
            // Preparation time at the start
            yield return new WaitForSeconds(10f);

            var wait = new WaitForSeconds(5f);
            while (true)
            {
                var random = Random.Range(-m_RandomOffset, m_RandomOffset);

                for (var i = 0; i < m_GenRate + random; i++)
                {
                    var randomOffset = Random.insideUnitSphere * SpawnRadius;
                    randomOffset.y = 0;

                    var spawnPosition = SpawnPoint.position + randomOffset;

                    CreateRandomVisitor(NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit,
                        SpawnRadius,
                        NavMesh.AllAreas)
                        ? hit.position
                        : spawnPosition);
                }

                yield return wait;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        private void CreateRandomVisitor(Vector3 pos)
        {
            var visitorPrefab = VisitorPrefabs[Random.Range(0, VisitorPrefabs.Count)];
            var visitorGO = Instantiate(visitorPrefab, pos, visitorPrefab.transform.rotation);

            visitorGO.transform.SetParent(transform);

            if (visitorGO.TryGetComponent<VisitorController>(out var controller))
            {
                AllVisitors.Add(controller);
            }
        }

        public void KillVisitor(GameObject visitor)
        {
            if (visitor.TryGetComponent<VisitorController>(out var controller))
            {
                AllVisitors.Remove(controller);
            }

            Destroy(visitor);
        }
    }
}