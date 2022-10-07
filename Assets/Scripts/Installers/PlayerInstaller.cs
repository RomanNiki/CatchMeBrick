using System;
using Damageable.DyingPolicies;
using Player;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class PlayerInstaller : MonoInstaller
    {
       [SerializeField] private Settings _settings;
        
        public override void InstallBindings()
        {
            Container.Bind<PlayerModel>().AsSingle().WithArguments(_settings.Rigidbody, new StandardDyingPolicy(), _settings.MaxHealth);
            Container.BindInterfacesTo<PlayerMover>().AsSingle();
        }
        
        [Serializable]
        public class Settings
        {
            public Rigidbody Rigidbody;
            public float MaxHealth;
        }
    }
}