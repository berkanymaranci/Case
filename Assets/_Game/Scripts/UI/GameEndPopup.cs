using System;
using BusJam.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusJam.UI
{
    public class GameEndPopup : PopupBase
    {
        [SerializeField]
        private TextMeshProUGUI loseMessageText;

        [SerializeField]
        private Button retryButton;

        [SerializeField]
        private Button closeButton;

        public event Action OnRetryClicked;
        public event Action OnCloseClicked;

        private void Awake()
        {
            retryButton.onClick.AddListener(HandleRetryClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        public void SetReason(GameEndReason reason)
        {
            loseMessageText.text = GetReasonText(reason);
        }

        private string GetReasonText(GameEndReason reason)
        {
            switch (reason)
            {
                case GameEndReason.TimeUp:
                    return "Time's Up";
                case GameEndReason.OutOfTiles:
                    return "Out of Tiles";
                default:
                    return "";
            }
        }

        private void HandleRetryClicked()
        {
            if (OnRetryClicked != null)
            {
                OnRetryClicked.Invoke();
            }
            Close();
        }

        private void HandleCloseClicked()
        {
            if (OnCloseClicked != null)
            {
                OnCloseClicked.Invoke();
            }
            Close();
        }

        private void OnDestroy()
        {
            retryButton.onClick.RemoveListener(HandleRetryClicked);
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }
    }
}