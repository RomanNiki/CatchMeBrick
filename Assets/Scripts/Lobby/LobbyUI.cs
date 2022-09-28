using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    public class LobbyUI : NetworkBehaviour
    {
        [SerializeField] private PlayerDataView[] _playerViews;
        [SerializeField] private Button _startButton;

        private NetworkList<LobbyPlayerState> _playerStates;

        private void Awake()
        {
            _playerStates = new NetworkList<LobbyPlayerState>();
        }
        
        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                _playerStates.OnListChanged += HandleLobbyPlayersStateChanged;
            }

            if (!IsServer) return;
            _startButton.gameObject.SetActive(true);

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _playerStates.OnListChanged -= HandleLobbyPlayersStateChanged;

            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
            }
        }

        private void HandleLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> changeEvent)
        {
            throw new System.NotImplementedException();
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            throw new System.NotImplementedException();
        }

        private void HandleClientConnected(ulong clientId)
        {
            throw new System.NotImplementedException();
        }
    }
}