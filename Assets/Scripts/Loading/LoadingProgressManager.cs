using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Loading
{
    public class LoadingProgressManager : NetworkBehaviour
    {
        [SerializeField] private NetworkObject _progressTrackerPrefab;

        public Dictionary<ulong, NetworkedLoadingProgressTracker> ProgressTrackers { get; } = new();
        public event Action OnTrackersUpdated;
        public AsyncOperation LocalLoadOperation
        {
            set
            {
                LocalProgress = 0;
                _localLoadOperation = value;
            }
        }

        private AsyncOperation _localLoadOperation;
        private float _localProgress;
    
        public float LocalProgress
        {
            get => IsSpawned && ProgressTrackers.ContainsKey(NetworkManager.LocalClientId)
                ? ProgressTrackers[NetworkManager.LocalClientId].Progress.Value
                : _localProgress;
            private set
            {
                if (IsSpawned && ProgressTrackers.ContainsKey(NetworkManager.LocalClientId))
                {
                    ProgressTrackers[NetworkManager.LocalClientId].Progress.Value = value;
                }
                else
                {
                    _localProgress = value;
                }
            }
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback += AddTracker;
                NetworkManager.OnClientDisconnectCallback += RemoveTracker;
                AddTracker(NetworkManager.LocalClientId);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.OnClientConnectedCallback -= AddTracker;
                NetworkManager.OnClientDisconnectCallback -= RemoveTracker;
            }

            ProgressTrackers.Clear();
            OnTrackersUpdated?.Invoke();
        }

        private void Update()
        {
            if (_localLoadOperation != null)
            {
                LocalProgress = _localLoadOperation.isDone ? 1 : _localLoadOperation.progress;
            }
        }

        [ClientRpc]
        private void UpdateTrackersClientRpc()
        {
            if (!IsHost)
            {
                ProgressTrackers.Clear();
                foreach (var tracker in FindObjectsOfType<NetworkedLoadingProgressTracker>())
                {
                    if (tracker.IsSpawned)
                    {
                        ProgressTrackers[tracker.OwnerClientId] = tracker;
                        if (tracker.OwnerClientId == NetworkManager.LocalClientId)
                        {
                            LocalProgress = Mathf.Max(_localProgress, LocalProgress);
                        }
                    }
                }
            }

            OnTrackersUpdated?.Invoke();
        }

        void AddTracker(ulong clientId)
        {
            if (IsServer)
            {
                var tracker = Instantiate(_progressTrackerPrefab);
                tracker.SpawnWithOwnership(clientId);
                ProgressTrackers[clientId] = tracker.GetComponent<NetworkedLoadingProgressTracker>();
                UpdateTrackersClientRpc();
            }
        }

        void RemoveTracker(ulong clientId)
        {
            if (IsServer)
            {
                if (ProgressTrackers.ContainsKey(clientId))
                {
                    var tracker = ProgressTrackers[clientId];
                    ProgressTrackers.Remove(clientId);
                    tracker.NetworkObject.Despawn();
                    UpdateTrackersClientRpc();
                }
            }
        }

        public void ResetLocalProgress()
        {
            LocalProgress = 0;
        }
    }
}