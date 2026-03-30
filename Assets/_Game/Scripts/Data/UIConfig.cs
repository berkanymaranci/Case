using DG.Tweening;
using UnityEngine;

namespace BusJam.Data
{
    [CreateAssetMenu(menuName = "BusJam/UI Config")]
    public class UIConfig : ScriptableObject
    {
        [Header("Overlay")]
        [SerializeField]
        private float overlayFadeDuration = 0.3f;

        [SerializeField]
        private Ease overlayFadeInEase = Ease.Linear;

        [SerializeField]
        private Ease overlayFadeOutEase = Ease.Linear;

        public float OverlayFadeDuration => overlayFadeDuration;
        public Ease OverlayFadeInEase => overlayFadeInEase;
        public Ease OverlayFadeOutEase => overlayFadeOutEase;

        [Header("Screen")]
        [SerializeField]
        private float screenFadeDuration = 0.3f;

        [SerializeField]
        private Ease screenFadeInEase = Ease.OutQuad;

        [SerializeField]
        private Ease screenFadeOutEase = Ease.InQuad;

        [Header("Popup")]
        [SerializeField]
        private float popupAnimationDuration = 0.3f;

        [SerializeField]
        private Ease popupOpenEase = Ease.OutBack;

        [SerializeField]
        private Ease popupCloseEase = Ease.InBack;

        public float ScreenFadeDuration => screenFadeDuration;
        public Ease ScreenFadeInEase => screenFadeInEase;
        public Ease ScreenFadeOutEase => screenFadeOutEase;
        public float PopupAnimationDuration => popupAnimationDuration;
        public Ease PopupOpenEase => popupOpenEase;
        public Ease PopupCloseEase => popupCloseEase;
    }
}