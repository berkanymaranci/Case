using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BusJam.UI
{
    public class StartScreen : BaseScreen
    {
        [SerializeField]
        private TextMeshProUGUI levelText;

        [SerializeField]
        private Button playButton;

        [SerializeField]
        private TextMeshProUGUI completedText;
        public event Action OnPlayButtonClicked;

        private void Awake()
        {
            playButton.onClick.AddListener(HandlePlayClicked);
        }

        public void SetLevelText(int levelId)
        {
            levelText.text = "Level " + levelId;
            playButton.gameObject.SetActive(true);
        }

        public void SetCompleted()
        {
            playButton.gameObject.SetActive(false);
            completedText.gameObject.SetActive(true);
        }

        private void HandlePlayClicked()
        {
            if (OnPlayButtonClicked != null)
            {
                OnPlayButtonClicked.Invoke();
            }
        }

        private void OnDestroy()
        {
            playButton.onClick.RemoveListener(HandlePlayClicked);
        }
    }
}
