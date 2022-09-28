using AssetLoaders;
using Zenject;

namespace Installers
{
    public class LoadingScreenInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<LoadingScreenProvider>().FromNew().AsSingle();
        }
    }
}
