using Events;
using Networking;
using Networking.Sessions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Lobby
{
    public class LobbyUI : NetworkBehaviour
    {
        [SerializeField] private PlayerCardView[] _playerViews;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _readyButton;
        [SerializeField] private Button _leaveButton;
        private SignalBus _signalBus;
        private readonly NetworkList<LobbyPlayerState> _playerStates = new();
        private ServerGameNetPortal _serverGameNetPortal;

        [Inject]
        public void Constructor(SignalBus signalBus, ServerGameNetPortal serverGameNetPortal)
        {
            _signalBus = signalBus;
            _serverGameNetPortal = serverGameNetPortal;
        }

        private void OnEnable()
        {
            _startButton.onClick.AddListener(OnStartGameClicked);
            _readyButton.onClick.AddListener(OnReadyClicked);
            _leaveButton.onClick.AddListener(OnLeaveClicked);
        }

        private void OnDisable()
        {
            _startButton.onClick.RemoveListener(OnStartGameClicked);
            _readyButton.onClick.RemoveListener(OnReadyClicked);
            _leaveButton.onClick.RemoveListener(OnLeaveClicked);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (IsClient)
            {
                _playerStates.OnListChanged -= HandleLobbyPlayersStateChanged;
            }

            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
            }
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

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }
        
        private void OnLeaveClicked()
        {
            _signalBus.Fire(new QuitGameSessionMessage() { UserRequested = true });
        }

        private void OnReadyClicked()
        {
            ToggleReadyServerRpc();
        }

        private void OnStartGameClicked()
        {
            StartGameServerRpc();
        }

        private bool IsEveryoneReady()
        {
            if (_playerStates.Count < 2)
            {
                return false;
            }

            foreach (var player in _playerStates)
            {
                if (player.IsReady == false)
                {
                    return false;
                }
            }

            return true;
        }

        private void HandleLobbyPlayersStateChanged(NetworkListEvent<LobbyPlayerState> changeEvent)
        {
            for (var i = 0; i < _playerViews.Length; i++)
            {
                if (_playerStates.Count > i)
                {
                    _playerViews[i].UpdateDisplay(_playerStates[i]);
                }
                else
                {
                    _playerViews[i].DisableDisplay();
                }
            }

            if(IsHost)
            {
                _startButton.interactable = IsEveryoneReady();
            }
        }

        private void HandleClientDisconnect(ulong clientId)
        {
            for (var i = 0; i < _playerStates.Count; i++)
            {
                if (_playerStates[i].ClientId == clientId)
                {
                    _playerStates.RemoveAt(i);
                    break;
                }
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            var playerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);

            if (!playerData.HasValue) { return; }

            _playerStates.Add(new LobbyPlayerState(
                clientId,
                playerData.Value.PlayerName,
                false
            ));
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void ToggleReadyServerRpc(ServerRpcParams serverRpcParams = default)
        {
            for (var i = 0; i < _playerStates.Count; i++)
            {
                if (_playerStates[i].ClientId == serverRpcParams.Receive.SenderClientId)
                {
                    _playerStates[i] = new LobbyPlayerState(
                        _playerStates[i].ClientId,
                        _playerStates[i].PlayerName,
                        !_playerStates[i].IsReady
                    );
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartGameServerRpc(ServerRpcParams serverRpcParams = default)
        {
            if (serverRpcParams.Receive.SenderClientId != NetworkManager.Singleton.LocalClientId) { return; }

            if (!IsEveryoneReady()) { return; }

            _serverGameNetPortal.StartGame();
        }
    }
}