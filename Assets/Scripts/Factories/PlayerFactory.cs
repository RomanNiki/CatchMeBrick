using Damageable.Player;
using Unity.Netcode;
using UnityEngine;


namespace Factories
{
    public class PlayerFactory : NetworkBehaviour
    {
        [SerializeField] private PlayerHealth _playerPrefab;
        [SerializeField] private Transform _spawnPoint;

        public override void OnNetworkSpawn()
        {
            var playerId = NetworkManager.Singleton.LocalClientId;
            Debug.Log(playerId);
            SpawnPlayerServerRpc(playerId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnPlayerServerRpc(ulong playerId)
        {
            var player = Instantiate(_playerPrefab, _spawnPoint.position, Quaternion.identity);
            player.gameObject.name = "Player" + playerId;
            player.NetworkObject.SpawnAsPlayerObject(playerId);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        }
    }
}