using System;
using BusJam.Grid;
using UnityEngine;

namespace BusJam.Input
{
    public class InputDetector : MonoBehaviour
    {
        [SerializeField]
        private Camera gameplayCamera;

        [SerializeField]
        private LayerMask cellLayer;

        private bool _isEnabled;

        public event Action<Cell> OnCellTapped;

        public void SetEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
        }

        private void Update()
        {
            if (!_isEnabled)
            {
                return;
            }
            if (!UnityEngine.Input.GetMouseButtonDown(0))
            {
                return;
            }
            var ray = gameplayCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (!Physics.Raycast(ray, out var hit, 100f, cellLayer))
            {
                return;
            }
            var cell = hit.collider.GetComponent<Cell>();
            if (cell == null)
            {
                return;
            }
            if (OnCellTapped != null)
            {
                OnCellTapped.Invoke(cell);
            }
        }
    }
}