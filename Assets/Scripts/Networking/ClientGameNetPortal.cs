using System;
using System.Collections;
using System.Text;
using Events;
using Loading;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Networking
{
    [RequireComponent(typeof(GameNetPortal))]
    public class ClientGameNetPortal : MonoBehaviour
    {
        public DisconnectReason DisconnectReason { get; private set; } = new DisconnectReason();
        private const int ReconnectAttempts = 2;
        private Coroutine _tryToReconnectCoroutine;
        private GameNetPortal _gameNetPortal;

        private void Start()
        {
            _gameNetPortal = GetComponent<GameNetPortal>();
            _gameNetPortal.NetManager.OnClientDisconnectCallback += OnDisconnectOrTimeout;
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

            _gameNetPortal.NetManager.OnClientDisconnectCallback -= OnDisconnectOrTimeout;

            if (NetworkManager.Singleton == null)
            {
                return;
            }

            if (NetworkManager.Singleton.CustomMessagingManager == null)
            {
                return;
            }

            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(
                nameof(ReceiveServerToClientConnectResult_CustomMessage));
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(
                nameof(ReceiveServerToClientSetDisconnectReason_CustomMessage));
        }

        public void OnNetworkReady()
        {
            if (!_gameNetPortal.NetManager.IsClient)
            {
                enabled = false;
            }
        }

        public void OnUserDisconnectRequest()
        {
            if (_gameNetPortal.NetManager.IsClient)
            {
                DisconnectReason.SetDisconnectReason(ConnectStatus.UserRequestedDisconnect);

                if (_tryToReconnectCoroutine != null)
                {
                    StopCoroutine(_tryToReconnectCoroutine);
                    _tryToReconnectCoroutine = null;
                    _gameNetPortal.SignalBus.Fire(new ReconnectMessage(ReconnectAttempts, ReconnectAttempts));
                }

                if (!_gameNetPortal.NetManager.IsServer)
                {
                   
                    _gameNetPortal.NetManager.Shutdown();
                }
            }
        }

        public void OnConnectFinished(ConnectStatus status)
        {
            Debug.Log("RecvConnectFinished Got status: " + status);

            if (status != ConnectStatus.Success)
            {
                DisconnectReason.SetDisconnectReason(status);
            }
            else
            {
                if (_tryToReconnectCoroutine != null)
                {
                    StopCoroutine(_tryToReconnectCoroutine);
                    _tryToReconnectCoroutine = null;
                    _gameNetPortal.SignalBus.Fire(new ReconnectMessage(ReconnectAttempts, ReconnectAttempts));
                }

                _gameNetPortal.SignalBus.Fire(status);
            }
        }

        private void OnDisconnectReasonReceived(ConnectStatus status)
        {
            DisconnectReason.SetDisconnectReason(status);
        }

        private void OnDisconnectOrTimeout(ulong clientID)
        {
            Debug.Log("disconnect from session");
            if (!NetworkManager.Singleton.IsHost ||
                NetworkManager.Singleton.IsHost && NetworkManager.Singleton.LocalClientId == clientID)
            {
                switch (DisconnectReason.Reason)
                {
                    case ConnectStatus.UserRequestedDisconnect:
                    case ConnectStatus.HostEndedSession:
                    case ConnectStatus.ServerFull:
                    case ConnectStatus.IncompatibleBuildType:
                        
                        _gameNetPortal.SignalBus.Fire(new QuitGameSessionMessage()
                            {UserRequested = false}); // go through the normal leave flow
                        break;
                    case ConnectStatus.LoggedInAgain:
                        if (_tryToReconnectCoroutine == null)
                        {
                            _gameNetPortal.SignalBus.Fire(new QuitGameSessionMessage()
                            {
                                UserRequested = false
                            });
                        }

                        break;
                    case ConnectStatus.GenericDisconnect:
                    case ConnectStatus.Undefined:
                        DisconnectReason.SetDisconnectReason(ConnectStatus.Reconnecting);
                        _tryToReconnectCoroutine ??= StartCoroutine(TryToReconnect());
                        break;
                    default:
                        throw new NotImplementedException(DisconnectReason.Reason.ToString());
                }

                _gameNetPortal.SignalBus.Fire(DisconnectReason.Reason);
                DisconnectReason.Clear();
            }
        }

        private IEnumerator TryToReconnect()
        {
            Debug.Log("Lost connection to host, trying to reconnect...");
            var nbTries = 0;
            while (nbTries < ReconnectAttempts)
            {
                NetworkManager.Singleton.Shutdown();

                yield return
                    new WaitWhile(() => NetworkManager.Singleton.ShutdownInProgress);
                Debug.Log($"Reconnecting attempt {nbTries + 1}/{ReconnectAttempts}...");
                _gameNetPortal.SignalBus.Fire(new ReconnectMessage(nbTries, ReconnectAttempts));
                ConnectClient();
                yield return new WaitForSeconds(
                    1.1f * NetworkManager.Singleton.NetworkConfig.ClientConnectionBufferTimeout +
                    ((UnityTransport) NetworkManager.Singleton.NetworkConfig.NetworkTransport).DisconnectTimeoutMS /
                    1000.0f);
                nbTries++;
            }

            Debug.Log("All tries failed, returning to main menu");
            _gameNetPortal.SignalBus.Fire(new QuitGameSessionMessage() {UserRequested = false});
            if (!DisconnectReason.HasTransitionReason)
            {
                DisconnectReason.SetDisconnectReason(ConnectStatus.GenericDisconnect);
            }

            _tryToReconnectCoroutine = null;
            _gameNetPortal.SignalBus.Fire(new ReconnectMessage(ReconnectAttempts, ReconnectAttempts));
            _gameNetPortal.SignalBus.Fire(DisconnectReason.Reason);
        }

        public void StartClient(string ipaddress, int port)
        {
            var utp = (UnityTransport) NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(ipaddress, (ushort) port);

            ConnectClient();
        }


        private void ConnectClient()
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                ClientGUID = Guid.NewGuid().ToString(),
                ClientScene = SceneManager.GetActiveScene().buildIndex,
                PlayerName = _gameNetPortal.PlayerName,
                IsDebug = Debug.isDebugBuild
            });

            byte[] payloadBytes = Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            
            _gameNetPortal.NetManager.StartClient();
            SceneLoaderWrapper.Instance.AddOnSceneEventCallback();
            
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(
                nameof(ReceiveServerToClientConnectResult_CustomMessage),
                ReceiveServerToClientConnectResult_CustomMessage);
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(
                nameof(ReceiveServerToClientSetDisconnectReason_CustomMessage),
                ReceiveServerToClientSetDisconnectReason_CustomMessage);
        }

        public void ReceiveServerToClientConnectResult_CustomMessage(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ConnectStatus status);
            OnConnectFinished(status);
        }

        public void ReceiveServerToClientSetDisconnectReason_CustomMessage(ulong clientID, FastBufferReader reader)
        {
            reader.ReadValueSafe(out ConnectStatus status);
            OnDisconnectReasonReceived(status);
        }
    }
}