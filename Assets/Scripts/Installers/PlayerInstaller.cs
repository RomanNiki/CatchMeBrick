using Followers.Camera;
using Inputs.InputTypes;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private Joystick _joystick;
        [SerializeField] private CameraFollower _cameraFollower;
        [SerializeField] private Camera _camera;
        
        public override void InstallBindings()
        {
            if (Application.isMobilePlatform)
            {
                Container.BindInstance(_joystick).AsSingle();
                Container.Bind<IInput>().To<CanvasJoystickInput>().FromNew().AsSingle().WithArguments(_joystick);
            }
            else
            {
                var playerInput = new PlayerInput();
                Container.Bind<IInput>().To<NewInputSystem>().FromNew().AsSingle().WithArguments(playerInput);
            }
           
            Container.BindInstance(_cameraFollower).AsTransient();
            Container.BindInstance(_camera).AsTransient();
        }
    }
}
