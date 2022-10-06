using AssetLoaders;
using Loading;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class LoadingScreenInstaller : MonoInstaller
    {
        [SerializeField] private LoadingProgressManager _loadingProgressPrefab;

        public override void InstallBindings()
        {
            Container.Bind<LoadingScreenProvider>().FromNew().AsSingle();
            var loadingProgressPrefab = Instantiate(_loadingProgressPrefab);
            Container.InjectGameObject(loadingProgressPrefab.gameObject);
            Container.Bind<LoadingProgressManager>()
                .FromInstance(loadingProgressPrefab)
                .AsSingle();
        }
    }
}