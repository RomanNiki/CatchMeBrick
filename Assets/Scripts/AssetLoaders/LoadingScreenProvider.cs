using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Loading;
using Loading.LoadingOperations;

namespace AssetLoaders
{
    public class LoadingScreenProvider : LocalAssetLoader
    {
        public async UniTask LoadAndDestroy(Queue<ILoadingOperation> loadingOperations)
        {
            var loadingScreen = await Load();
            await loadingScreen.Load(loadingOperations);
            Unload();
        }
        
        public Task<LoadingScreen> Load()
        {
            return LoadInternal<LoadingScreen>(nameof(LoadingScreen));
        }

        public void Unload()
        {
            UnloadInternal();
        }
    }
}