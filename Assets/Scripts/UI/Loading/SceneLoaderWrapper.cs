using System.Collections.Generic;
using AssetLoaders;
using Events;
using Loading.LoadingOperations;
using Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Loading
{
    public class SceneLoaderWrapper : NetworkBehaviour
    {
        private LoadingScreenProvider _loadingScreenProvider;
        private LoadingProgressManager _loadingProgressManager;
        private GameNetPortal _gameNetPortal;
        private Queue<ILoadingOperation> _loadingOperations;
        
        [Inject]
        public void Constructor(LoadingScreenProvider loadingScreenProvider,
            LoadingProgressManager loadingProgressManager,
            GameNetPortal gameNetPortal)
        {
            _loadingProgressManager = loadingProgressManager;
            _loadingScreenProvider = loadingScreenProvider;
            _gameNetPortal = gameNetPortal;
        }

        private bool IsNetworkSceneManagementEnabled => NetworkManager != null && NetworkManager.SceneManager != null &&
                                                        NetworkManager.NetworkConfig.EnableSceneManagement;

        public static SceneLoaderWrapper Instance { get; private set; }

        public void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (NetworkManager.IsListening == false)
            {
                return;
            }
            if (NetworkManager != null && NetworkManager.SceneManager != null)
            {
                NetworkManager.SceneManager.OnSceneEvent -= OnSceneEvent;
      
            }
        }

        public void AddOnSceneEventCallback()
        {
            if (IsNetworkSceneManagementEnabled)
            {
                NetworkManager.SceneManager.OnSceneEvent += OnSceneEvent;
            }
        }
       
        private async void OnSceneEvent(SceneEvent sceneEvent)
        { 
            if (NetworkManager.IsClient == false) return;
            switch (sceneEvent.SceneEventType)
            {
                case SceneEventType.Unload:
                    _loadingOperations = new Queue<ILoadingOperation>();
                        _loadingOperations.Enqueue(new UnloadLoadingOperation());
                        _loadingOperations.Enqueue(new ConnectionLoadingOperation(sceneEvent.SceneName, _loadingProgressManager));
                        await _loadingScreenProvider.LoadAndDestroy(_loadingOperations);
                    break;
                
                case SceneEventType.Synchronize:
                    if (NetworkManager.IsHost)
                    {
                        return;
                    }
                    _loadingOperations = new Queue<ILoadingOperation>();
                    _loadingOperations.Enqueue(new SynchronizeLoadingOperation());
                    _loadingOperations.Enqueue(new ConnectionLoadingOperation(sceneEvent.SceneName, _loadingProgressManager));
                    await _loadingScreenProvider.LoadAndDestroy(_loadingOperations);
                    break;

                case SceneEventType.Load:
                    _loadingProgressManager.LocalLoadOperation = sceneEvent.AsyncOperation;
                    break;

                case SceneEventType.LoadEventCompleted:
                    _loadingProgressManager.ResetLocalProgress();
                    break;
                
            }
        }
        
        public async void LeaveSession(QuitGameSessionMessage msg)
        {
            if (msg.UserRequested)
            {
                if (_gameNetPortal != null)
                {
                    _gameNetPortal.RequestDisconnect();
                }
            }

            var loadingOperations = new Queue<ILoadingOperation>();
            loadingOperations.Enqueue(new LoadMenuLoadingOperation());
            await _loadingScreenProvider.LoadAndDestroy(loadingOperations);
        }
    }
}