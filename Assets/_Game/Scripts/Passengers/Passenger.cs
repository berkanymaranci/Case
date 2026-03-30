using System;
using System.Collections.Generic;
using BusJam.Data;
using DG.Tweening;
using UnityEngine;

namespace BusJam.Passengers
{
    public class Passenger : MonoBehaviour
    {
        private static readonly int s_isRunning = Animator.StringToHash("IsRunning");
        private static readonly int s_isSitting = Animator.StringToHash("IsSitting");
        private static readonly int s_baseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int s_outlineWidth = Shader.PropertyToID("_OutlineWidth");

        [SerializeField]
        private PassengerConfig config;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private Renderer bodyRenderer;

        private ColorType _colorType;
        public ColorType Color => _colorType;

        public void Init(ColorType color, Color visualColor)
        {
            _colorType = color;
            bodyRenderer.material.SetColor(s_baseColor, visualColor);
            bodyRenderer.material.SetFloat(s_outlineWidth, config.DefaultOutlineWidth);
        }

        public void ResetState()
        {
            DOTween.Kill(gameObject);
            animator.SetBool(s_isRunning, false);
            animator.SetBool(s_isSitting, false);
            transform.localScale = Vector3.one;
            transform.rotation = Quaternion.identity;
            SetHighlight(false);
        }

        public void SetHighlight(bool isHighlighted)
        {
            var width = isHighlighted
                ? config.HighlightOutlineWidth
                : config.DefaultOutlineWidth;
            bodyRenderer.material.SetFloat(s_outlineWidth, width);
        }

        public void MoveToCell(Transform target, Action onComplete)
        {
            animator.SetBool(s_isRunning, true);
            var direction = target.position - transform.position;
            transform.rotation = Quaternion.LookRotation(direction);
            transform.DOMove(target.position, config.MoveSpeed)
                .SetSpeedBased(true)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    animator.SetBool(s_isRunning, false);
                    transform.rotation = target.rotation;
                    if (onComplete != null)
                    {
                        onComplete.Invoke();
                    }
                }).SetLink(gameObject);
        }

        public void MoveToBus(Transform target, Action onComplete)
        {
            animator.SetBool(s_isRunning, true);
            Vector3 direction = target.position - transform.position;
            transform.rotation = Quaternion.LookRotation(direction);
            transform.DOMove(target.position, config.MoveToBusDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    animator.SetBool(s_isRunning, false);
                    if (onComplete != null)
                    {
                        onComplete.Invoke();
                    }
                }).SetLink(gameObject);
        }

        public void MoveAlongPath(List<Vector3> positionsList, Action onComplete)
        {
            animator.SetBool(s_isRunning, true);
            var pathArray = positionsList.ToArray();
            transform.DOPath(pathArray, config.MoveSpeed, PathType.Linear)
                .SetSpeedBased(true)
                .SetLookAt(0.01f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    animator.SetBool(s_isRunning, false);
                    transform.rotation = Quaternion.identity;
                    if (onComplete != null)
                    {
                        onComplete();
                    }
                }).SetLink(gameObject);
        }

        public void MoveToSeat(Transform target, Action onComplete)
        {
            animator.SetBool(s_isSitting, true);
            transform.localScale = Vector3.zero;
            transform.position = target.position;
            transform.rotation = target.rotation;
            transform.DOScale(Vector3.one, 0.1f).OnComplete(onComplete.Invoke).SetLink(gameObject);
        }
    }
}