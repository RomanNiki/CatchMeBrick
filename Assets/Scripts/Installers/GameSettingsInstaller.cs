using System;
using Player;
using UnityEngine;
using Zenject;

namespace Installers
{
    [CreateAssetMenu(menuName = "Catch me brick/Game Settings")]
    public class GameSettingsInstaller : ScriptableObjectInstaller<GameSettingsInstaller>
    {
        public PlayerSettings Player;

        public override void InstallBindings()
        {
            Container.BindInstance(Player.PlayerMover).IfNotBound();
        }
        
        [Serializable]
        public class PlayerSettings
        {
            public PlayerMover.Settings PlayerMover;
        }
    }
}