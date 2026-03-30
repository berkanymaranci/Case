using System;
using BusJam.Data;
using DG.Tweening;
using UnityEngine;

namespace BusJam.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupBase : MonoBehaviour
    {
        [SerializeField]
        private UIConfig uiConfig;

        [SerializeField]
        private Transform content;

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

        public virtual void Open()
        {
            KillTweens();
            gameObject.SetActive(true);
            CanvasGroup.alpha = 0f;
            content.localScale = Vector3.zero;
            DOTween.Sequence()
                .Join(CanvasGroup.DOFade(1f, uiConfig.PopupAnimationDuration))
                .Join(content.DOScale(1f, uiConfig.PopupAnimationDuration).SetEase(uiConfig.PopupOpenEase));
        }

        public virtual void Close(Action onComplete = null)
        {
            KillTweens();
            DOTween.Sequence()
                .Join(CanvasGroup.DOFade(0f, uiConfig.PopupAnimationDuration))
                .Join(content.DOScale(0f, uiConfig.PopupAnimationDuration).SetEase(uiConfig.PopupCloseEase))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }

        public void ResetImmediate()
        {
            KillTweens();
            CanvasGroup.alpha = 0f;
            content.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }

        private void KillTweens()
        {
            DOTween.Kill(CanvasGroup);
            DOTween.Kill(content);
        }
    }
}