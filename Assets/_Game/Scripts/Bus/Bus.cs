using System;
using System.Collections.Generic;
using BusJam.Data;
using BusJam.Passengers;
using DG.Tweening;
using UnityEngine;

namespace BusJam.Bus
{
    [Serializable]
    public class ColorTarget
    {
        public Renderer renderer;
        public int materialIndex;

        public void Apply(Color color)
        {
            Material[] materials = renderer.materials;
            materials[materialIndex].color = color;
            renderer.materials = materials;
        }
    }

    public class Bus: MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private List<BusSeat> seats;
        [SerializeField]
        private Transform entryPoint;
        [SerializeField]
        private Transform bodyTransform;
        [SerializeField]
        private List<ColorTarget> colorTargets;
        [SerializeField]
        private Transform leftDoor;
        [SerializeField]
        private Transform rightDoor;
        [SerializeField]
        private BusConfig config;

        private Tween _shakeTween;
        private ColorType _colorType;
        private bool _isDoorOpen;
        private bool _isArrived;
        private float _leftDoorStartZ;
        private float _rightDoorStartZ;
        private Vector3 _bodyStartLocalPosition;

        public ColorType ColorType => _colorType;
        public Transform EntryPoint => entryPoint;
        public int SeatCount => seats.Count;
        public bool IsArrived => _isArrived;

        private void Awake()
        {
            _bodyStartLocalPosition = bodyTransform.localPosition;
            _leftDoorStartZ = leftDoor.localPosition.z;
            _rightDoorStartZ = rightDoor.localPosition.z;
        }

        public void Arrive()
        {
            _isArrived = true;
        }

        public void Init(ColorType colorType, Color visualColor)
        {
            _colorType = colorType;
            _isDoorOpen = false;
            _isArrived = false;
            for (int i = 0; i < colorTargets.Count; i++)
            {
                colorTargets[i].Apply(visualColor);
            }
            for (int i = 0; i < seats.Count; i++)
            {
                seats[i].Clear();
            }
            StartIdleBounce();
        }

        public void ReleasePassengers(Action<Passenger> releaseAction)
        {
            for (int i = 0; i < seats.Count; i++)
            {
                if (seats[i].HasPassenger)
                {
                    releaseAction(seats[i].Passenger);
                }
            }
        }

        public void ResetState()
        {
            if (_shakeTween != null)
            {
                _shakeTween.Kill();
                _shakeTween = null;
            }
            DOTween.Kill(gameObject);
            DOTween.Kill(transform);
            leftDoor.DOKill();
            rightDoor.DOKill();
            bodyTransform.DOKill();
            _isDoorOpen = false;
            _isArrived = false;
            bodyTransform.localPosition = _bodyStartLocalPosition;
            bodyTransform.localRotation = Quaternion.identity;

            var leftPos = leftDoor.localPosition;
            leftPos.z = _leftDoorStartZ;
            leftDoor.localPosition = leftPos;

            var rightPos = rightDoor.localPosition;
            rightPos.z = _rightDoorStartZ;
            rightDoor.localPosition = rightPos;

            for (int i = 0; i < seats.Count; i++)
            {
                seats[i].Clear();
            }
        }

        public bool IsFull()
        {
            for (int i = 0; i < seats.Count; i++)
            {
                if (!seats[i].IsArrived)
                {
                    return false;
                }
            }
            return true;
        }

        public BusSeat GetEmptySeat()
        {
            for (int i = 0; i < seats.Count; i++)
            {
                if (seats[i].IsEmpty)
                {
                    return seats[i];
                }
            }
            return null;
        }

        public void MoveTo(Vector3 targetPosition, Action onComplete = null)
        {
            bodyTransform.localRotation = Quaternion.identity;
            transform.DOMove(targetPosition, config.MoveDuration)
                .SetEase(config.MoveCurve)
                .OnComplete(() =>
                {
                    if (onComplete != null)
                    {
                        onComplete.Invoke();
                    }
                }).SetLink(gameObject);

            DOVirtual.Float(0f, 1f, config.MoveDuration, t =>
            {
                float angle = config.TiltCurve.Evaluate(t) * config.TiltAngle;
                bodyTransform.localRotation = Quaternion.Euler(angle, 0f, 0f);
            }).SetEase(Ease.Linear).SetLink(gameObject);
        }

        public void OpenDoors()
        {
            if (_isDoorOpen)
            {
                return;
            }
            leftDoor.DOKill();
            rightDoor.DOKill();
            _isDoorOpen = true;
            var dist = config.DoorSlideDistance;
            var dur = config.DoorSlideDuration;
            leftDoor.DOLocalMoveZ(_leftDoorStartZ + dist, dur).SetEase(config.DoorOpenEase).SetLink(gameObject);
            rightDoor.DOLocalMoveZ(_rightDoorStartZ - dist, dur).SetEase(config.DoorOpenEase).SetLink(gameObject);
        }

        public void CloseDoors()
        {
            if (!_isDoorOpen)
            {
                return;
            }

            if (HasPendingPassengers())
            {
                return;
            }

            _isDoorOpen = false;
            leftDoor.DOKill();
            rightDoor.DOKill();
            var dur = config.DoorSlideDuration;
            leftDoor.DOLocalMoveZ(_leftDoorStartZ, dur).SetEase(config.DoorCloseEase).SetLink(gameObject);
            rightDoor.DOLocalMoveZ(_rightDoorStartZ, dur).SetEase(config.DoorCloseEase).SetLink(gameObject);
        }

        private void StartIdleBounce()
        {
            _shakeTween = bodyTransform
                .DOLocalMoveY(config.BounceHeight, config.BounceDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }

        private bool HasPendingPassengers()
        {
            for (int i = 0; i < seats.Count; i++)
            {
                var seat = seats[i];
                if (!seat.IsEmpty && !seat.IsArrived)
                {
                    return true;
                }
            }
            return false;
        }
    }
}