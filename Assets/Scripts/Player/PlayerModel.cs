using System;
using Damageable.DyingPolicies;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class PlayerModel
    {
        private readonly Rigidbody _rigidBody;
        private readonly IDyingPolicy _dyingPolicy;
        public UnityAction Die;

        public PlayerModel(
            Rigidbody rigidBody, IDyingPolicy dyingPolicy, float maxHealth)
        {
            _dyingPolicy = dyingPolicy;
            _rigidBody = rigidBody;
            Health.Value = maxHealth;
        }

        public bool IsDead
        {
            get; set;
        }
        
        public NetworkVariable<float> Health { get; } = new NetworkVariable<float>();

        public Vector3 LookDir => -_rigidBody.transform.right;
        public Vector3 Forward => -_rigidBody.transform.forward;

        public Quaternion Rotation
        {
            get => _rigidBody.rotation;
            set => _rigidBody.rotation = value;
        }

        public Vector3 Position
        {
            get => _rigidBody.position;
            set => _rigidBody.position = value;
        }

        public Vector3 Velocity => _rigidBody.velocity;

        public void TakeDamage(float healthLoss)
        {
            Health.Value = Mathf.Max(0.0f, Health.Value - healthLoss);
            if (_dyingPolicy.Died(Health.Value))
            {
                Die?.Invoke();
            }
        }

        public void AddForce(Vector3 force)
        {
            _rigidBody.AddForce(force);
        }

        public void Move(Vector3 direction)
        {
            _rigidBody.MovePosition(Position + direction);
        }
        
        [Serializable]
        public class Settings
        {
            public float MaxHealth;
        }
    }
}