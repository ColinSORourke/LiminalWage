﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

namespace Collectables
{
    using Player;

    public class Coin : MonoBehaviour
    {
        private PlayerInteract playerInteract;
        private Transform coinTransform;

        [SerializeField] private int scoreValue;
        [SerializeField] private int moraleValue;
        [SerializeField] private bool rotationAnimationOn;
        [SerializeField] private float rotationSpeed;

        public UnityEvent OnCollect = new UnityEvent();

        private Tween rotationTween;

        public void Construct(PlayerInteract playerInteract)
        {
            this.playerInteract = playerInteract;
            coinTransform = gameObject.transform;

            OnCollect.AddListener(OnCollectCallback);

            if (rotationAnimationOn) StartFloatingAnimation();
        }

        private void OnCollectCallback()
        {
            playerInteract.GainPoints(scoreValue);
            playerInteract.GainMorale(moraleValue);

            rotationTween.Kill();

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            rotationTween.Kill();
        }

        private void StartFloatingAnimation()
        {
            Vector3 highPosition = new Vector3(0, 360, 0);

            rotationTween = coinTransform.DORotate(highPosition, rotationSpeed, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear).SetRelative().SetLoops(-1);
        }

        public int GetValue()
        {
            return scoreValue;
        }
    }
}


