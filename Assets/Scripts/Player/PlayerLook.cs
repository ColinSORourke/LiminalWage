using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerLook : MonoBehaviour
    {
        private Camera playerCamera;
        private CameraManager cameraManager;
        private PlayerInput playerInput;
        private Transform cameraTransform;

        private float xRotation;

        public void Construct(CameraManager cameraManager
            , PlayerInput playerInput
            , Camera playerCamera)
        {
            if (cameraManager == null)
            {
                throw new ArgumentNullException(nameof(cameraManager));
            }
            if (playerInput == null)
            {
                throw new ArgumentNullException(nameof(playerInput));
            }
            if (playerCamera == null)
            {
                throw new ArgumentNullException(nameof(playerCamera));
            }

            this.cameraManager = cameraManager;
            this.playerInput = playerInput;
            this.playerCamera = playerCamera;

            cameraTransform = playerCamera.gameObject.GetComponent<Transform>();

            playerCamera.enabled = true;
        }

        private void Update()
        {
            if(playerCamera != null && playerCamera.enabled)
            {
                RotateCamVertical();
            }
        }

        private void RotateCamVertical()
        {
            xRotation -= playerInput.GetMouseInputY()
                * cameraManager.mouseSensitivity;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            cameraTransform.localRotation
                = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}