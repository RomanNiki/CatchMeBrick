using System;
using System.Globalization;
using Networking.Sessions;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Networking
{
    [RequireComponent(typeof(ClientGameNetPortal))]
    [RequireComponent(typeof(ServerGameNetPortal))]
    public class GameNetPortal : MonoBehaviour
    {
        [Inject] private SignalBus _signalBus;
        private ClientGameNetPortal _clientPortal;
        private ServerGameNetPortal _serverPortal;
        public string PlayerName { get; private set; }
        public NetworkManager NetManager { get; private set; }

        public SignalBus SignalBus => _signalBus;

        private void Awake()
        {
            NetManager  = FindObjectOfType<NetworkManager>();
            PlayerName = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            _clientPortal = GetComponent<ClientGameNetPortal>();
            _serverPortal = GetComponent<ServerGameNetPortal>();
            NetManager.OnClientConnectedCallback += ClientNetworkReadyWrapper;
        }

        private void OnDestroy()
        {
            if (NetManager != null)
            {
                NetManager.OnClientConnectedCallback -= ClientNetworkReadyWrapper;
            }
        }

        private void ClientNetworkReadyWrapper(ulong clientId)
        {
            if (clientId == NetManager.LocalClientId)
            {
                OnNetworkReady();
                if (NetManager.IsServer)
                {
                    NetManager.SceneManager.OnSceneEvent += OnSceneEvent;
                }
            }
        }

        private void OnNetworkReady()
        {
            if (NetManager.IsHost)
            {
                _clientPortal.OnConnectFinished(ConnectStatus.Success);
            }

            _clientPortal.OnNetworkReady();
            _serverPortal.OnNetworkReady();
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;

            _serverPortal.OnClientSceneChanged(sceneEvent.ClientId,
                SceneManager.GetSceneByName(sceneEvent.SceneName).buildIndex);
        }

        private bool StartHost()
        {
            return NetManager.StartHost();
        }

        public bool StartHost(string ipAddress, int port)
        {
            var utp = (UnityTransport) NetManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(ipAddress, (ushort) port);

            return StartHost();
        }

        public string GetPlayerId()
        {
            return ClientPrefs.GetGuid();
        }

        public void RequestDisconnect()
        {
            if (NetManager.IsServer)
            {
                NetManager.SceneManager.OnSceneEvent -= OnSceneEvent;
                SessionManager<SessionPlayerData>.Instance.OnServerEnded();
            }

            _clientPortal.OnUserDisconnectRequest();
            _serverPortal.OnUserDisconnectRequest();
        }
    }
}