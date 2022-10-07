using Player;
using Unity.Netcode;
using UnityEngine;
using Zenject;


namespace Factories
{
    public class PlayerFactory : NetworkBehaviour
    {
        [SerializeField] private PlayerView _playerPrefab;
        [SerializeField] private Transform _spawnPoint;
        [Inject] private DiContainer _container;
        public override void OnNetworkSpawn()
        {
            var playerId = NetworkManager.Singleton.LocalClientId;
            
            Debug.Log(playerId);
            SpawnPlayerServerRpc(playerId);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SpawnPlayerServerRpc(ulong playerId)
        {
            var player = Instantiate(_playerPrefab, _spawnPoint.position + Vector3.right * playerId * 10, Quaternion.identity);
            player.gameObject.name = "Player" + playerId;
            player.NetworkObject.SpawnAsPlayerObject(playerId);
            InjectClientRpc(playerId);
        }
        
        [ClientRpc]
        private void InjectClientRpc(ulong playerId)
        {
            if (playerId == NetworkManager.Singleton.LocalClientId)
            {
                var player = NetworkManager.SpawnManager.GetPlayerNetworkObject(playerId).gameObject;
                _container.InjectGameObject(player);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        }
    }
}