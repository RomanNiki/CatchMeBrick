using UnityEngine;

namespace Lobby
{
    public class PlayerData : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private Sprite _playerSprite;

        public string Name => _name;
        public Sprite PlayerSprite => _playerSprite;
    }
}