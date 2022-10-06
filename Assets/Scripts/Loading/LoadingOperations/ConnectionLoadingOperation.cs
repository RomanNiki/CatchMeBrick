using System;
using Cysharp.Threading.Tasks;

namespace Loading.LoadingOperations
{
    public class ConnectionLoadingOperation : ILoadingOperation
    {
        private readonly LoadingProgressManager _progressManager;

        public ConnectionLoadingOperation(string sceneName, LoadingProgressManager progressManager = null)
        {
            _progressManager = progressManager;
            UpdateDescription(sceneName);
        }

        public string Description { get; private set; } = "Loading scene...";
        private const string Template = "Loading {0}...";
        
        public async UniTask Load(Action<float> onProgress)
        {
            onProgress?.Invoke(0.5f);
            if (_progressManager == null)
            {
                onProgress?.Invoke(1f);
                return;
            }
            while (_progressManager.LocalProgress < 1)
            {
                onProgress?.Invoke(_progressManager.LocalProgress);
                await UniTask.Delay(1);
            }

            onProgress?.Invoke(1f);
        }

        private void UpdateDescription(string newDescription)
        {
            Description = string.Format(Template, newDescription);
        }
    }
}