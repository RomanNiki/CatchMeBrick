using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Lobby
{
    public class PlayerDataView : NetworkBehaviour
    {
        [SerializeField] private Image _uiPlayerAvatar;
        [SerializeField] private TMP_Text _uiReadyText;
        [SerializeField] private Color _readyColor = Color.green;
        [SerializeField] private Color _notReadyColor = Color.red;
        [SerializeField] private Outline _readyOutline;
        [SerializeField] private Toggle _readyButton;
        private const string ReadyTextTemplate = "Is Ready";
        private const string NotReadyTextTemplate = "Is not Ready";

        private void OnEnable()
        {
            _readyButton.onValueChanged.AddListener(ReadyButtonClick);
        }

        private void OnDisable()
        {
            _readyButton.onValueChanged.RemoveListener(ReadyButtonClick);
        }

        private void ReadyButtonClick(bool toggle)
        {
            SetReady(toggle);
        }

        public void SetReady(bool toggle)
        {
            var targetColor = toggle ? _readyColor : _notReadyColor;
            var targetText = toggle ? ReadyTextTemplate : NotReadyTextTemplate;
            _readyOutline.effectColor = targetColor;
            _uiReadyText.color = targetColor;
            _uiReadyText.text = targetText;
        }
    }
}