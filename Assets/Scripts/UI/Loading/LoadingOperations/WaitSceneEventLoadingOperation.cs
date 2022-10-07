using System;
using Cysharp.Threading.Tasks;

namespace Loading.LoadingOperations
{
    public class WaitSceneEventLoadingOperation : ILoadingOperation
    {
        public string Description { get; } = "Load";
        public async UniTask Load(Action<float> onProgress)
        {throw new NotImplementedException();
        }

        public void UpdateDescription(string newDescription)
        {
            throw new NotImplementedException();
        }
    }
}