using System;
using BusJam.Data;
using DG.Tweening;
using UnityEngine;

namespace BusJam.UI
{
    public class ScreenManager : MonoBehaviour
    {
        [SerializeField]
        private StartScreen startScreen;

        [SerializeField]
        private GameplayScreen gameplayScreen;

        [SerializeField]
        private WinScreen winScreen;

        [SerializeField]
        private LoseScreen loseScreen;

        [SerializeField]
        private CanvasGroup fadeOverlay;

        [SerializeField]
        private UIConfig uiConfig;

        public StartScreen StartScreen => startScreen;
        public GameplayScreen GameplayScreen => gameplayScreen;
        public WinScreen WinScreen => winScreen;
        public LoseScreen LoseScreen => loseScreen;

        private void Awake()
        {
            fadeOverlay.alpha = 0f;
            fadeOverlay.blocksRaycasts = false;
        }

        public void FadeTransition(Action onBlack, Action onComplete = null)
        {
            fadeOverlay.blocksRaycasts = true;
            fadeOverlay.DOFade(1f, uiConfig.OverlayFadeDuration).SetEase(uiConfig.OverlayFadeInEase).OnComplete(() =>
            {
                onBlack?.Invoke();
                fadeOverlay.DOFade(0f, uiConfig.OverlayFadeDuration).SetEase(uiConfig.OverlayFadeOutEase).OnComplete(() =>
                {
                    fadeOverlay.blocksRaycasts = false;
                    onComplete?.Invoke();
                });
            });
        }
    }
}