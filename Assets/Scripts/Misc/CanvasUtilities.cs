using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

namespace Misc
{
    public class CanvasUtilities : MonoBehaviour
    {
        public static CanvasUtilities Instance;

        [SerializeField] private CanvasGroup _loader;
        [SerializeField] private float _fadeTime;
        [SerializeField] private TMP_Text _loaderText, _errorText;

        private TweenerCore<float, float, FloatOptions> _tween;

        private void Awake() {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Toggle(false, instant: true);
        }

        public void Toggle(bool on, string text = null, bool instant = false) {
            _loaderText.text = text;
            _loader.gameObject.SetActive(on);
            _tween?.Kill();
            _tween = _loader.DOFade(on ? 1 : 0, instant ? 0 : _fadeTime);
        }

        public void ShowError(string error) {
            _errorText.text = error;
            _errorText.DOFade(1, _fadeTime).OnComplete(() => { _errorText.DOFade(0, _fadeTime).SetDelay(1); });
        }

    }
}
