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
            Container.BindInstance(_joystick).AsSingle();
            Container.Bind<PlayerInput>().To<PlayerInput>().AsSingle();
            Container.Bind<IInput>().To<CanvasJoystickInput>().AsSingle();
            Container.BindInstance(_cameraFollower).AsSingle();
            Container.BindInstance(_camera).AsSingle();
            var f = 5 * 3;
        }
    }
}
