using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Loading.LoadingOperations
{
    public class LobbyLoadingOperation : ILoadingOperation
    {
        public string Description { get; } = "Lobby loading";

        public async UniTask Load(Action<float> onProgress)
        {
            onProgress?.Invoke(0.5f);
            NetworkManager.Singleton.SceneManager.LoadScene(Scenes.Lobby, LoadSceneMode.Single);
            await UniTask.Delay(1);

            onProgress?.Invoke(1f);
        }
    }
}