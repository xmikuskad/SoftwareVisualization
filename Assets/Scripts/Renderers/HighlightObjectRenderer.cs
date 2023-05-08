using System;
using System.Collections.Generic;
using Helpers;
using PathologicalGames;
using UnityEngine;
using UnityEngine.Serialization;

namespace Renderers
{
    public class HighlightObjectRenderer:MonoBehaviour
    {
        public DataRenderer dataRenderer;

        public List<GameObject> spawnedHighlightObjects = new();
        
        public GameObject highlightObjPrefab;


        private float spaceBetweenObjects = 0f;

        private void Awake()
        {
            SingletonManager.Instance.dataManager.DatesSelectedEvent += OnDatesSelected;
            SingletonManager.Instance.dataManager.DatesRangeSelectedEvent += OnDateRangeSelected;

            // Get references from dataRenderer
            spaceBetweenObjects = dataRenderer.spaceBetweenObjects;
        }

        private void OnDatesSelected(Pair<long, List<DateTime>> pair)
        {
            ResetObjects();
        }
        
        private void OnDateRangeSelected(Pair<long, List<DateTime>> pair)
        {
            // Transform obj = PoolManager.Pools[PoolNames.VERTICE].Spawn(highlightObjPrefab, spawnPos, Quaternion.identity);
        }

        public void ResetObjects()
        {
            PoolManager.Pools[PoolNames.HIGHLIGHT_OBJECTS].DespawnAll();
            spawnedHighlightObjects.Clear();
        }

    }
}