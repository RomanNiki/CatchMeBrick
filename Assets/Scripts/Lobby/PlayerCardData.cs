using UnityEngine;

namespace Lobby
{
    public class PlayerCardData : ScriptableObject
    {
        [SerializeField] private string _name;
        [SerializeField] private Sprite _playerSprite;

        public string Name => _name;
        public Sprite PlayerSprite => _playerSprite;
    }
}