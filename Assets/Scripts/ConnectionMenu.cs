using System.Collections.Generic;
using AssetLoaders;
using Loading.LoadingOperations;
using Unity.Netcode;
using UnityEngine;
using Zenject;

public class ConnectionMenu : MonoBehaviour
{
    private static LoadingScreenProvider _provider;

    [Inject] 
    public void Constructor(LoadingScreenProvider provider)
    {
        _provider = provider;
    }
    
    private void OnEnable()
    {
        NetworkManager.Singleton.OnServerStarted += ServerStarted;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnServerStarted -= ServerStarted;
    }

    private static async void ServerStarted()
    {
        var loadingOperations = new Queue<ILoadingOperation>();
        loadingOperations.Enqueue(new LobbyLoadingOperation());
        await _provider.LoadAndDestroy(loadingOperations);
    }
    
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
