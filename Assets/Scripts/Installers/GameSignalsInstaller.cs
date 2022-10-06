using Events;
using Loading;
using Networking;
using Zenject;

namespace Installers
{
    public class GameSignalsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<QuitGameSessionMessage>().RunAsync();
            Container.DeclareSignal<ConnectionEventMessage>().RunAsync();
            Container.DeclareSignal<ConnectStatus>().RunAsync();
            Container.DeclareSignal<ReconnectMessage>().RunAsync();
            Container.BindSignal<QuitGameSessionMessage>().ToMethod<SceneLoaderWrapper>(a => a.LeaveSession)
                .FromResolve();
        }
    }
}