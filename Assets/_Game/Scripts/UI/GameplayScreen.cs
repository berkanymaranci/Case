using TMPro;
using UnityEngine;

namespace BusJam.UI
{
    public class GameplayScreen : BaseScreen
    {
        [SerializeField]
        private TextMeshProUGUI timerText;

        [SerializeField]
        private TextMeshProUGUI levelCountText;

        [SerializeField]
        private TimerController timerController;

        private int _lastDisplayedSeconds = -1;

        public TimerController TimerController => timerController;

        public void SetLevelText(int levelId)
        {
            levelCountText.text = levelId.ToString();
        }

        public void SetTimerText(int seconds)
        {
            if (_lastDisplayedSeconds == seconds)
            {
                return;
            }
            _lastDisplayedSeconds = seconds;
            timerText.text = seconds.ToString();
        }

        private void Update()
        {
            if (timerController == null || !timerController.IsRunning)
            {
                return;
            }
            var currentSeconds = Mathf.CeilToInt(timerController.RemainingTime);
            SetTimerText(currentSeconds);
        }
    }
}
