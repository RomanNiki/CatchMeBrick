using System;
using Cysharp.Threading.Tasks;
using Unity.Netcode;

namespace Loading.LoadingOperations
{
    public class SynchronizeLoadingOperation : ILoadingOperation
    {
        public string Description { get; } = "Synchronize...";
        private bool _synchronized;

        public SynchronizeLoadingOperation()
        {
            NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
        }
        
        public async UniTask Load(Action<float> onProgress)
        {
            onProgress?.Invoke(0.5f);
            while (_synchronized == false)
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
                _synchronized = true;
            }
        }
    }
}