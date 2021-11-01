using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Player
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(PlayerLook))]

    public class CameraManager : MonoBehaviour
    {
        private Camera playerCamera;
        private PlayerInput playerInput;
        private PlayerLook playerLook;

        public float mouseSensitivity;

        [SerializeField] private float defaultFOV;
        [SerializeField] private float sprintFOV;
        [SerializeField] private Ease tweenFOVEase;
        [SerializeField] private float tweenUpFOVDuration;
        [SerializeField] private float tweenDownFOVDuration;

        public bool cameraEnabled = false;

        private Tween tweenFOV;

        public void Construct(PlayerInput playerInput)
        {
            Cursor.lockState = CursorLockMode.Locked;

            playerCamera = gameObject.GetComponent<Camera>();

            playerCamera.fieldOfView = defaultFOV;

            playerCamera.enabled = false;

            this.playerInput = playerInput;

            playerLook = gameObject.GetComponent<PlayerLook>();
            playerLook.Construct(this, playerInput, playerCamera);
        }

        private void Update()
        {
            if (playerInput.GetButtonDownEscape())
            {
                ToggleCursorLock();
            }

            if (playerInput.GetButtonDownSprint())
            {
                StartCoroutine(TweenFOV(sprintFOV, tweenUpFOVDuration));
            }
            else if (playerInput.GetButtonUpSprint())
            {
                StartCoroutine(TweenFOV(defaultFOV, tweenDownFOVDuration));
            }
        }

        private void ToggleCursorLock()
        {
            if(Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
        }

        public IEnumerator TweenFOV(float endValue, float duration)
        {
            if(tweenFOV != null && tweenFOV.IsActive())
            {
                tweenFOV.Kill();
            }
            tweenFOV = playerCamera.DOFieldOfView(endValue, duration)
                .SetEase(tweenFOVEase);

            yield return tweenFOV.WaitForCompletion();
        }
    }
}

