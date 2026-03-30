using DG.Tweening;
using UnityEngine;

namespace BusJam.Data
{
    [CreateAssetMenu(menuName = "BusJam/Bus Config")]
    public class BusConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField]
        private float moveDuration = 0.6f;
        [SerializeField]
        private AnimationCurve moveCurve;

        [Header("Tilt")]
        [SerializeField]
        private float tiltAngle = 5f;
        [SerializeField]
        private AnimationCurve tiltCurve;
        
        [Header("Idle Bounce")]
        [SerializeField]
        private float bounceHeight = 0.02f;
        [SerializeField]
        private float bounceDuration = 0.8f;
        public float MoveDuration => moveDuration;
        public AnimationCurve MoveCurve => moveCurve;
        public float TiltAngle => tiltAngle;
        public AnimationCurve TiltCurve => tiltCurve;
        
        [Header("Door")]
        [SerializeField]
        private float doorSlideDistance = 0.3f;
        [SerializeField]
        private float doorSlideDuration = 0.3f;
        [SerializeField]
        private Ease doorOpenEase = Ease.OutQuad;
        [SerializeField]
        
        private Ease doorCloseEase = Ease.InQuad;
        public float BounceHeight => bounceHeight;
        public float BounceDuration => bounceDuration;
        public float DoorSlideDistance => doorSlideDistance;
        public float DoorSlideDuration => doorSlideDuration;
        public Ease DoorOpenEase => doorOpenEase;
        public Ease DoorCloseEase => doorCloseEase;
    }
}