using System;
using UnityEngine;

namespace Zenject.SpaceFighter
{
    public class PlayerHealthWatcher
    {
        readonly SignalBus _signalBus;
        readonly AudioPlayer _audioPlayer;
        readonly Settings _settings;
        readonly Explosion.Factory _explosionFactory;
        readonly Player _player;

        public PlayerHealthWatcher(
            Player player,
            Explosion.Factory explosionFactory,
            Settings settings,
            AudioPlayer audioPlayer,
            SignalBus signalBus)
        {
            _signalBus = signalBus;
            _audioPlayer = audioPlayer;
            _settings = settings;
            _explosionFactory = explosionFactory;
            _player = player;
            _player.Die += Die;
        }

        void Die()
        {
            _player.IsDead = true;

            var explosion = _explosionFactory.Create();
            explosion.transform.position = _player.Position;

            _player.Renderer.enabled = false;

            _signalBus.Fire<PlayerDiedSignal>();

            _audioPlayer.Play(_settings.DeathSound, _settings.DeathSoundVolume);
            _player.Die -= Die;
        }

        [Serializable]
        public class Settings
        {
            public AudioClip DeathSound;
            public float DeathSoundVolume = 1.0f;
        }
    }
}
