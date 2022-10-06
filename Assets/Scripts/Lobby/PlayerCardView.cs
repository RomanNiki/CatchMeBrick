using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    public class PlayerCardView : NetworkBehaviour
    {
        private const string ReadyTextTemplate = "Is Ready";
        private const string NotReadyTextTemplate = "Is not Ready";
        [SerializeField] private GameObject _playerDataPanel;
        [SerializeField] private GameObject _waitingPlayerPanel;
        
        [SerializeField] private Image _uiPlayerAvatar;
        [SerializeField] private TMP_Text _uiReadyText;
        [SerializeField] private Outline _readyOutline;
        [SerializeField] private Color _readyColor = Color.green;
        [SerializeField] private Color _notReadyColor = Color.red;

        public void DisableDisplay()
        {
            _waitingPlayerPanel.SetActive(true);
            _playerDataPanel.SetActive(false);
        }
        
        public void UpdateDisplay(LobbyPlayerState playerState)
        {
            SetReady(playerState.IsReady);
        }

        private void SetReady(bool toggle = false)
        {
            var targetColor = toggle ? _readyColor : _notReadyColor;
            var targetText = toggle ? ReadyTextTemplate : NotReadyTextTemplate;
            _readyOutline.effectColor = targetColor;
            _uiReadyText.color = targetColor;
            _uiReadyText.text = targetText;
            _waitingPlayerPanel.SetActive(false);
            _playerDataPanel.SetActive(true);
        }
    }
}