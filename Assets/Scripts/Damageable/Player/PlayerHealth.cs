using System;
using Damageables.DyingPolicies;
using Unity.Netcode;
using UnityEngine;

namespace Damageable.Player
{
    public class PlayerHealth : NetworkBehaviour, IDamageable
    {
        [SerializeField] private float _maxHealth;
        private IDamageable _damageable;
        private IDyingPolicy _dyingPolicy;
        public NetworkVariable<float> Value { get; } = new NetworkVariable<float>();
        public Action Die;

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                GetComponent<PlayerMover>().enabled = true;
            }
        }

        private void OnValidate()
        {
            if (_maxHealth <= 0)
            {
                _maxHealth = 1;
            }

            _dyingPolicy = new StandartDyingPolicy();
        }
        
        [ServerRpc]
        public void TakeDamageServerRpc(float damage)
        {
            Value.Value -= damage;
            if (Value.Value <= 0)
                Value.Value = 0;

            if (_dyingPolicy.Died(Value.Value) == false)
                return;
        
            Die?.Invoke();
        }
    }
}