using System;
using BusJam.Data;
using UnityEngine;

namespace BusJam.UI
{
    public class LoseScreen : BaseScreen
    {
        [SerializeField]
        private GameEndPopup gameEndPopup;

        public event Action OnRetryClicked;
        public event Action OnCloseClicked;

        private void OnEnable()
        {
            gameEndPopup.OnRetryClicked += HandleRetryClicked;
            gameEndPopup.OnCloseClicked += HandleCloseClicked;
        }

        private void OnDisable()
        {
            gameEndPopup.OnRetryClicked -= HandleRetryClicked;
            gameEndPopup.OnCloseClicked -= HandleCloseClicked;
        }

        public override void Hide()
        {
            gameEndPopup.ResetImmediate();
            base.Hide();
        }

        public void ShowLose(GameEndReason reason)
        {
            gameEndPopup.SetReason(reason);
            FadeIn(() =>
            {
                gameEndPopup.Open();
            });
        }

        public void CloseLose(Action onComplete = null)
        {
            gameEndPopup.Close(() =>
            {
                FadeOut(onComplete);
            });
        }

        private void HandleRetryClicked()
        {
            if (OnRetryClicked != null)
            {
                OnRetryClicked.Invoke();
            }
        }

        private void HandleCloseClicked()
        {
            if (OnCloseClicked != null)
            {
                OnCloseClicked.Invoke();
            }
        }
    }
}