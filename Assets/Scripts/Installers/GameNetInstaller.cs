using Loading;
using Networking;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class GameNetInstaller : MonoInstaller
    {
        [SerializeField] private GameNetPortal _gameNetPortalPrefab;
        [SerializeField] private SceneLoaderWrapper _sceneLoaderWrapperPrefab;

        public override void InstallBindings()
        {

            var gameNetPortal = Container.InstantiatePrefab(_gameNetPortalPrefab);
            Container.Bind<GameNetPortal>().FromComponentOn(gameNetPortal).AsSingle();
            Container.Bind<ClientGameNetPortal>().FromComponentOn(gameNetPortal).AsSingle();
            Container.Bind<ServerGameNetPortal>().FromComponentOn(gameNetPortal).AsSingle();

            var loaderWrapper = Instantiate(_sceneLoaderWrapperPrefab);
            Container.InjectGameObject(loaderWrapper.gameObject);
            Container.Bind<SceneLoaderWrapper>().FromInstance(loaderWrapper).AsSingle();
        }
    }
}