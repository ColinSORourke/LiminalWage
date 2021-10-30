using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public bool cameraEnabled = false;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;

            playerCamera = gameObject.GetComponent<Camera>();

            playerCamera.enabled = false;
        }

        public void Construct(PlayerInput playerInput)
        {
            this.playerInput = playerInput;

            playerLook = gameObject.GetComponent<PlayerLook>();
            playerLook.Construct(this, playerInput, playerCamera);
        }

        private void Update()
        {
            if(playerInput.GetButtonDownEscape())
            {
                ToggleCursorLock();
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
    }
}

