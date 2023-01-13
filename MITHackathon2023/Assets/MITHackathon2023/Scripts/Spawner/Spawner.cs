﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace MITHack.Robot.Spawner
{
    public class Spawner :  MonoBehaviour
    {
        #region defines

        [System.Serializable]
        private enum SpawnerSpawnType
        {
            [InspectorName("Spawn in Order")]
            SpawnType_InOrder,
            [InspectorName("Spawn Randomly")]
            SpawnType_Random
        }
        
        private enum SpawnerState
        {
            [InspectorName("Initial Delay")]
            State_InitialDelay,
            [InspectorName("Spawner Delay")]
            State_SpawnerDelay
        }
        
        #endregion
        
        [Header("Variables")] 
        [SerializeField, Min(0.0f)] 
        private float minSpawnDelay = 0.5f;
        [Space]
        [SerializeField, Min(0.0f)]
        private float startSpawnTimeDiff = 6.0f;
        [SerializeField]
        private float endSpawnTimeDiff = 0.5f;
        [Space]
        [SerializeField, Min(1)]
        private int totalNumberSpawnedBetweenTimeChange = 3;
        [FormerlySerializedAs("totalTimeSubtracted")] [SerializeField, Min(0.1f)]
        private float totalTimeDiffSubtracted = 0.5f;
        
        [Header("Spawner Variables")]
        [SerializeField]
        private GameObject prefab;
        [Space] 
        [SerializeField]
        private SpawnerSpawnType spawnType;
        [SerializeField]
        private List<Transform> spawnPoints;

        private SpawnerState _spawnerState = SpawnerState.State_InitialDelay;
        private int _totalSpawned = 0;
        private float _totalSpawnTimeDifference = 0.0f;
        private float _currentTimeDifference = 0.0f;

        private int _lastSpawnPoint = -1;

        public int TotalSpawned => _totalSpawned;
        
        private void Start()
        {
            _totalSpawnTimeDifference = _currentTimeDifference = minSpawnDelay;
        }
        
        private void Update()
        {
            UpdateSpawnerTime(Time.deltaTime);
        }

        private void UpdateSpawnerTime(float deltaTime)
        {
            if (_currentTimeDifference > 0.0f)
            {
                _currentTimeDifference -= deltaTime;
                if (_currentTimeDifference <= 0.0f)
                {
                    ExecuteSpawn();
                }
            }
        }

        /// <summary>
        /// Spawns the prefab and updates everything else in the spawner.
        /// </summary>
        private void ExecuteSpawn()
        {
            // Instantiates the Game Object.
            if (!Spawn())
            {
                return;
            }
            _totalSpawned++;

            if (_spawnerState == SpawnerState.State_InitialDelay)
            {
                _currentTimeDifference = _totalSpawnTimeDifference = startSpawnTimeDiff;
                _spawnerState = SpawnerState.State_SpawnerDelay;
                return;
            }

            if (_totalSpawned % totalNumberSpawnedBetweenTimeChange == 0
                && _totalSpawned != 0)
            {
                _totalSpawnTimeDifference = Mathf.Max(
                    _totalSpawnTimeDifference - totalTimeDiffSubtracted,
                    endSpawnTimeDiff);
            }
            _currentTimeDifference = _totalSpawnTimeDifference;
        }

        private bool Spawn()
        { 
            if (!prefab)
            {
                return false;
            }
            var selectSpawnPoint = SelectSpawnPoint();
            if (!selectSpawnPoint)
            {
                return false;
            }
            var instantiated = GameObject.Instantiate(prefab);
            var instantiatedTransform = instantiated.transform;
            instantiatedTransform.position = selectSpawnPoint.position;
            instantiatedTransform.rotation = selectSpawnPoint.rotation;
            return true;
        }

        private Transform SelectSpawnPoint()
        {
            var transformCount = spawnPoints.Count;
            if (transformCount <= 0)
            {
                return null;
            }
            
            switch (spawnType)
            {
                case SpawnerSpawnType.SpawnType_Random:
                {
                    int random;
                    do
                    {
                        random = Random.Range(0, transformCount);
                    } while (random == _lastSpawnPoint);
                    _lastSpawnPoint = random;
                    return spawnPoints[random];
                }
                case SpawnerSpawnType.SpawnType_InOrder:
                {
                    var currentSpawnPoint = ++_lastSpawnPoint;
                    return spawnPoints[currentSpawnPoint % transformCount];
                }
            }
            return null;
        }
    }
}