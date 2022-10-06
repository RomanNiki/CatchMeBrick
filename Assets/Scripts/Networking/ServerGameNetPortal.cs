using System.Collections;
using System.Collections.Generic;
using System.Text;
using Events;
using Loading;
using Networking.Sessions;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{
    [RequireComponent(typeof(GameNetPortal))]
    public class ServerGameNetPortal : MonoBehaviour
    {
        [SerializeField] private int _maxPlayers = 4;
        private Dictionary<ulong, int> _clientSceneMap;
        private static int ServerScene => SceneManager.GetActiveScene().buildIndex;
        private bool _gameInProgress;
        private const int MaxConnectionPayload = 1024;
        private GameNetPortal _gameNetPortal;

        private void Start()
        {
            _gameNetPortal = GetComponent<GameNetPortal>();

            _gameNetPortal.NetManager.ConnectionApprovalCallback += ApprovalCheck;
            _gameNetPortal.NetManager.OnServerStarted += ServerStartedHandler;
            
            _clientSceneMap = new Dictionary<ulong, int>();
        }

        private void OnDestroy()
        {
            if (_gameNetPortal == null)
            {
                return;
            }

            if (_gameNetPortal.NetManager == null)
            {
                return;
            }

            _gameNetPortal.NetManager.ConnectionApprovalCallback -= ApprovalCheck;
            _gameNetPortal.NetManager.OnServerStarted -= ServerStartedHandler;
        }

        public void OnNetworkReady()
        {
            if (!_gameNetPortal.NetManager.IsServer)
            {
                enabled = false;
            }
            else
            {
                _gameNetPortal.NetManager.OnClientDisconnectCallback += OnClientDisconnect;

                if (_gameNetPortal.NetManager.IsHost)
                {
                    _clientSceneMap[_gameNetPortal.NetManager.LocalClientId] = ServerScene;
                }
            }
        }

        private void OnClientDisconnect(ulong clientId)
        {
            _clientSceneMap.Remove(clientId);

            if (clientId == _gameNetPortal.NetManager.LocalClientId)
            {
                _gameNetPortal.NetManager.OnClientDisconnectCallback -= OnClientDisconnect;
                SessionManager<SessionPlayerData>.Instance.OnServerEnded();
            }
            else
            {
                var playerId = SessionManager<SessionPlayerData>.Instance.GetPlayerId(clientId);
                if (playerId != null)
                {
                    var sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(playerId);
                    if (sessionData.HasValue)
                    {
                        _gameNetPortal.SignalBus.Fire(new ConnectionEventMessage { ConnectStatus = ConnectStatus.GenericDisconnect, PlayerName = sessionData.Value.PlayerName });
                    }
                    SessionManager<SessionPlayerData>.Instance.DisconnectClient(clientId);
                }
            }
        }

        public void StartGame()
        {
            _gameInProgress = true;
            _gameNetPortal.NetManager.SceneManager.LoadScene(Scenes.Game, LoadSceneMode.Single);
        }

        public void EndRound()
        {
            _gameInProgress = false;
        }

        public void OnClientSceneChanged(ulong clientId, int sceneIndex)
        {
            _clientSceneMap[clientId] = sceneIndex;
        }

        public void OnUserDisconnectRequest()
        {
            if (_gameNetPortal.NetManager.IsHost)
            {
                SendServerToAllClientsSetDisconnectReason(ConnectStatus.HostEndedSession);
                StartCoroutine(WaitToShutdown());
            }

            ClearData();
        }
        
        private void ClearData()
        {
            _clientSceneMap.Clear();
            _gameInProgress = false;
        }
        
        public bool AreAllClientsInServerScene()
        {
            foreach (var kvp in _clientSceneMap)
            {
                if (kvp.Value != ServerScene)
                {
                    return false;
                }
            }

            return true;
        }
        
        private void ServerStartedHandler()
        {
            SceneLoaderWrapper.Instance.AddOnSceneEventCallback();
            NetworkManager.Singleton.SceneManager.LoadScene(Scenes.Lobby, LoadSceneMode.Single);
        }

        private ConnectStatus CanClientConnect(ConnectionPayload connectionPayload)
        {
            if (_gameNetPortal.NetManager.ConnectedClientsIds.Count >= _maxPlayers)
            {
                return ConnectStatus.ServerFull;
            }

            if (_gameInProgress)
            {
                return ConnectStatus.GameInProgress;
            }

            if (connectionPayload.IsDebug != Debug.isDebugBuild)
            {
                return ConnectStatus.IncompatibleBuildType;
            }

            return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.ClientGUID) ? ConnectStatus.LoggedInAgain : ConnectStatus.Success;
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request,
            NetworkManager.ConnectionApprovalResponse response)
        {
            response.CreatePlayerObject = false;
            var clientId = request.ClientNetworkId;
            var connectionData = request.Payload;
            response.PlayerPrefabHash = null;
            if (connectionData.Length > MaxConnectionPayload)
            {
                response.Approved = false;
                return;
            }

            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, _gameNetPortal.GetPlayerId(),
                    new SessionPlayerData(clientId, _gameNetPortal.PlayerName, 0, true));

                response.Approved = true;
                response.CreatePlayerObject = true;
                return;
            }

            var payload = Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

            var gameReturnStatus = CanClientConnect(connectionPayload);

            if (gameReturnStatus == ConnectStatus.Success)
            {
                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.ClientGUID,
                    new SessionPlayerData(clientId, connectionPayload.PlayerName, 0, true));

                SendServerToClientConnectResult(clientId, gameReturnStatus);

                _clientSceneMap[clientId] = connectionPayload.ClientScene;
                _gameNetPortal.SignalBus.Fire(new ConnectionEventMessage {ConnectStatus = gameReturnStatus, PlayerName = connectionPayload.PlayerName});

                response.Approved = true;
                response.CreatePlayerObject = true;
                response.Position = Vector3.zero;
                response.Rotation = Quaternion.identity;
                         
                return;
            }

            response.Approved = true;
            
            IEnumerator WaitToDenyApproval(NetworkManager.ConnectionApprovalResponse approvalResponse)
            {
                approvalResponse.Pending = true; // give some time for server to send connection status message to clients
                approvalResponse.Approved = false;
                SendServerToClientConnectResult(clientId, gameReturnStatus);
                SendServerToClientSetDisconnectReason(clientId, gameReturnStatus);
                yield return null; // wait a frame so UTP can flush it's messages on next update
                approvalResponse.Pending = false; // connection approval process can be finished.
            }

            StartCoroutine(WaitToDenyApproval(response));
        }

        private IEnumerator WaitToShutdown()
        {
            yield return null;
            _gameNetPortal.NetManager.Shutdown();
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();
        }

        private static void SendServerToAllClientsSetDisconnectReason(ConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll(
                nameof(ClientGameNetPortal.ReceiveServerToClientSetDisconnectReason_CustomMessage), writer);
        }

        private static void SendServerToClientSetDisconnectReason(ulong clientID, ConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                nameof(ClientGameNetPortal.ReceiveServerToClientSetDisconnectReason_CustomMessage), clientID, writer);
        }

        private static void SendServerToClientConnectResult(ulong clientID, ConnectStatus status)
        {
            var writer = new FastBufferWriter(sizeof(ConnectStatus), Allocator.Temp);
            writer.WriteValueSafe(status);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                nameof(ClientGameNetPortal.ReceiveServerToClientConnectResult_CustomMessage), clientID, writer);
        }
    }
}