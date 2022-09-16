using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    public class LobbyUI : NetworkBehaviour
    {
        [SerializeField] private PlayerDataView[] _playerViews;
        [SerializeField] private Button _startButton;

        private NetworkList<LobbyPlayerState> _playerStates = new();
    }
}