using System;
using UnityEngine;
using UnityEngine.UI;

namespace BusJam.UI
{
    public class WinScreen : BaseScreen
    {
        [SerializeField]
        private Button continueButton;

        public event Action OnContinueClicked;

        private void Awake()
        {
            continueButton.onClick.AddListener(HandleContinueClicked);
        }

        private void HandleContinueClicked()
        {
            if (OnContinueClicked != null)
            {
                OnContinueClicked.Invoke();
            }
        }

        private void OnDestroy()
        {
            continueButton.onClick.RemoveListener(HandleContinueClicked);
        }
    }
}