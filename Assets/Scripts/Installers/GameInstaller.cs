using System;
using Followers.Camera;
using Inputs.InputTypes;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField] private Settings _settings;

        public override void InstallBindings()
        {
            if (Application.isMobilePlatform)
            {
                Container.BindInstance(_settings.Joystick).AsSingle();
                Container.Bind<IInput>().To<CanvasJoystickInput>().FromNew().AsSingle().WithArguments(_settings.Joystick);
            }
            else
            {
                var playerInput = new PlayerInput();
                Container.Bind<IInput>().To<NewInputSystem>().FromNew().AsSingle().WithArguments(playerInput);
            }
           
            Container.BindInstance(_settings.CameraFollower).AsTransient();
            Container.BindInstance(_settings.Camera).AsTransient();
        }

        [Serializable]
        public class Settings
        {
            public Joystick Joystick;
            public CameraFollower CameraFollower;
            public Camera Camera;
        }
    }
}
