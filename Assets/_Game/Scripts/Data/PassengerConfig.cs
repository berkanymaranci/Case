using UnityEngine;

namespace BusJam.Data
{
    [CreateAssetMenu(menuName = "BusJam/Passenger Config")]
    public class PassengerConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField]
        private float moveSpeed = 5f;
        [SerializeField]
        private float moveToBusDuration = 0.3f;

        [Header("Outline")]
        [SerializeField]
        private float defaultOutlineWidth = 0.15f;
        [SerializeField]
        private float highlightOutlineWidth = 0.4f;

        public float MoveSpeed => moveSpeed;
        public float MoveToBusDuration => moveToBusDuration;
        public float DefaultOutlineWidth => defaultOutlineWidth;
        public float HighlightOutlineWidth => highlightOutlineWidth;
    }
}
