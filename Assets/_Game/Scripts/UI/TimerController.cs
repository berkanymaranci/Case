using System;
using UnityEngine;

namespace BusJam.UI
{
    public class TimerController : MonoBehaviour
    {
        private float _remainingTime;
        private bool _isRunning;

        public event Action OnTimeUp;

        public float RemainingTime => _remainingTime;
        public bool IsRunning => _isRunning;

        public void StartTimer()
        {
            _isRunning = true;
        }

        public void SetTimer(int seconds)
        {
            _remainingTime = seconds;
        }

        public void StopTimer()
        {
            _isRunning = false;
        }

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            _remainingTime -= Time.deltaTime;

            if (_remainingTime <= 0f)
            {
                _remainingTime = 0f;
                _isRunning = false;
                if (OnTimeUp != null)
                {
                    OnTimeUp.Invoke();
                }
            }
        }
    }
}
