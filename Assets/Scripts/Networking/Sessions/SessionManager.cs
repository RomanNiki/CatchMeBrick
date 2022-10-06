using System.Collections.Generic;
using UnityEngine;

namespace Networking.Sessions
{
    public class SessionManager<T> where T : struct, ISessionPlayerData
    {
        private SessionManager()
        {
            _clientData = new Dictionary<string, T>();
            _clientIDToPlayerId = new Dictionary<ulong, string>();
        }

        public static SessionManager<T> Instance => _instance ??= new SessionManager<T>();
        private static SessionManager<T> _instance;
        private readonly Dictionary<string, T> _clientData;
        private readonly Dictionary<ulong, string> _clientIDToPlayerId;

        private bool _hasSessionStarted;

        public void DisconnectClient(ulong clientId)
        {
            if (_hasSessionStarted)
            {
                if (_clientIDToPlayerId.TryGetValue(clientId, out var playerId))
                {
                    if (GetPlayerData(playerId)?.ClientID == clientId)
                    {
                        var clientData = _clientData[playerId];
                        clientData.IsConnected = false;
                        _clientData[playerId] = clientData;
                    }
                }
            }
            else
            {
                if (_clientIDToPlayerId.TryGetValue(clientId, out var playerId))
                {
                    _clientIDToPlayerId.Remove(clientId);
                    if (GetPlayerData(playerId)?.ClientID == clientId)
                    {
                        _clientData.Remove(playerId);
                    }
                }
            }
        }

        public bool IsDuplicateConnection(string playerId)
        {
            return _clientData.ContainsKey(playerId) && _clientData[playerId].IsConnected;
        }

        public void SetupConnectingPlayerSessionData(ulong clientId, string playerId, T sessionPlayerData)
        {
            var isReconnecting = false;

            if (IsDuplicateConnection(playerId))
            {
                Debug.LogError(
                    $"Player ID {playerId} already exists. This is a duplicate connection. Rejecting this session data.");
                return;
            }

            if (_clientData.ContainsKey(playerId))
            {
                if (!_clientData[playerId].IsConnected)
                {
                    isReconnecting = true;
                }
            }

            if (isReconnecting)
            {
                sessionPlayerData = _clientData[playerId];
                sessionPlayerData.ClientID = clientId;
                sessionPlayerData.IsConnected = true;
            }

            _clientIDToPlayerId[clientId] = playerId;
            _clientData[playerId] = sessionPlayerData;
        }

        public string GetPlayerId(ulong clientId)
        {
            if (_clientIDToPlayerId.TryGetValue(clientId, out string playerId))
            {
                return playerId;
            }

            Debug.Log($"No client player ID found mapped to the given client ID: {clientId}");
            return null;
        }

        public T? GetPlayerData(ulong clientId)
        {
            var playerId = GetPlayerId(clientId);
            if (playerId != null)
            {
                return GetPlayerData(playerId);
            }

            Debug.Log($"No client player ID found mapped to the given client ID: {clientId}");
            return null;
        }

        public T? GetPlayerData(string playerId)
        {
            if (_clientData.TryGetValue(playerId, out T data))
            {
                return data;
            }

            Debug.Log($"No PlayerData of matching player ID found: {playerId}");
            return null;
        }

        public void SetPlayerData(ulong clientId, T sessionPlayerData)
        {
            if (_clientIDToPlayerId.TryGetValue(clientId, out string playerId))
            {
                _clientData[playerId] = sessionPlayerData;
            }
            else
            {
                Debug.LogError($"No client player ID found mapped to the given client ID: {clientId}");
            }
        }

        public void OnSessionStarted()
        {
            _hasSessionStarted = true;
        }

        public void OnSessionEnded()
        {
            ClearDisconnectedPlayersData();
            ReinitializePlayersData();
            _hasSessionStarted = false;
        }

        public void OnServerEnded()
        {
            _clientData.Clear();
            _clientIDToPlayerId.Clear();
            _hasSessionStarted = false;
        }

        private void ReinitializePlayersData()
        {
            foreach (var id in _clientIDToPlayerId.Keys)
            {
                string playerId = _clientIDToPlayerId[id];
                T sessionPlayerData = _clientData[playerId];
                sessionPlayerData.Reinitialize();
                _clientData[playerId] = sessionPlayerData;
            }
        }

        private void ClearDisconnectedPlayersData()
        {
            List<ulong> idsToClear = new List<ulong>();
            foreach (var id in _clientIDToPlayerId.Keys)
            {
                var data = GetPlayerData(id);
                if (data is {IsConnected: false})
                {
                    idsToClear.Add(id);
                }
            }

            foreach (var id in idsToClear)
            {
                string playerId = _clientIDToPlayerId[id];
                if (GetPlayerData(playerId)?.ClientID == id)
                {
                    _clientData.Remove(playerId);
                }

                _clientIDToPlayerId.Remove(id);
            }
        }
    }
}