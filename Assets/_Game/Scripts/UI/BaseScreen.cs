using System;
using BusJam.Data;
using DG.Tweening;
using UnityEngine;

namespace BusJam.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseScreen : MonoBehaviour
    {
        [SerializeField]
        private UIConfig uiConfig;

        private CanvasGroup _canvasGroup;

        protected CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }

        public virtual void Show()
        {
            DOTween.Kill(CanvasGroup);
            CanvasGroup.alpha = 1f;
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            DOTween.Kill(CanvasGroup);
            gameObject.SetActive(false);
        }

        public void FadeIn(Action onComplete = null)
        {
            DOTween.Kill(CanvasGroup);
            CanvasGroup.alpha = 0f;
            gameObject.SetActive(true);
            CanvasGroup.DOFade(1f, uiConfig.ScreenFadeDuration)
                .SetEase(uiConfig.ScreenFadeInEase)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
        }

        public void FadeOut(Action onComplete = null)
        {
            DOTween.Kill(CanvasGroup);
            CanvasGroup.DOFade(0f, uiConfig.ScreenFadeDuration)
                .SetEase(uiConfig.ScreenFadeOutEase)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }
    }
}