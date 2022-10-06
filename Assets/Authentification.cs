using System.Collections.Generic;
using System.Threading.Tasks;
using AssetLoaders;
using Loading.LoadingOperations;
using UnityEngine;
using Zenject;

public class Authentification : MonoBehaviour
{ 
    [Inject] private LoadingScreenProvider _provider;
    private async void Start()
    {
        await Authentificate();
    }

    private async Task Authentificate()
    {
        var a = new Queue<ILoadingOperation>();
        a.Enqueue(new LoadMenuLoadingOperation());
        await _provider.LoadAndDestroy(a);
    }
}
