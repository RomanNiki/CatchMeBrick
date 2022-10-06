using System;
using Cysharp.Threading.Tasks;

namespace Loading.LoadingOperations
{
    public interface ILoadingOperation
    {
        string Description { get;}
        UniTask Load(Action<float> onProgress);
    }
}
