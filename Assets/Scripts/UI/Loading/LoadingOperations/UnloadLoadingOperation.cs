using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;

namespace Loading.LoadingOperations
{
    public class UnloadLoadingOperation : ILoadingOperation
    {
        private bool _loadEvenStarted;
        public string Description { get; } = "Unload...";

        public UnloadLoadingOperation()
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        }
        
        public async UniTask Load(Action<float> onProgress)
        {
        
            onProgress?.Invoke(0.5f);
            while (_loadEvenStarted == false)
            {
                await UniTask.Delay(1);
            }
            onProgress?.Invoke(1f);
            NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            if (sceneEvent.SceneEventType is SceneEventType.Load or SceneEventType.LoadComplete)
            {
                _loadEvenStarted = true;
            }
        }
    }
}