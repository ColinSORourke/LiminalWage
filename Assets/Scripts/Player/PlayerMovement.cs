using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private PlayerInput playerInput;
        private CharacterController characterController;
        private Transform playerTransform;
        private GroundCheck groundCheck;
        private CameraManager cameraManager;

        [SerializeField] private Vector3 gravity;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float jumpHeight;        

        private Vector2 inputVector;
        private Vector3 verticalVelocity;
        private float jumpVelocity;

        private bool isGroundPlayerCooldown = false;

        public void Construct(Transform playerTransform
            , GroundCheck groundCheck
            , PlayerInput playerInput
            , CharacterController characterController
            , CameraManager cameraManager)
        {
            if (playerTransform == null)
            {
                throw new ArgumentNullException(nameof(playerTransform));
            }
            if (groundCheck == null)
            {
                throw new ArgumentNullException(nameof(groundCheck));
            }
            if (playerInput == null)
            {
                throw new ArgumentNullException(nameof(playerInput));
            }
            if (characterController == null)
            {
                throw new ArgumentNullException(nameof(characterController));
            }
            if (cameraManager == null)
            {
                throw new ArgumentNullException(nameof(cameraManager));
            }

            this.playerTransform = playerTransform;
            this.groundCheck = groundCheck;
            this.playerInput = playerInput;
            this.characterController = characterController;
            this.cameraManager = cameraManager;

            // Convert jump height into jump velocity
            jumpVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity.magnitude);
        }

        private void Update()
        {
            inputVector = playerInput.GetInputVector();

            RotatePlayer();

            GroundPlayer();

            Jump();

            Move();

            ApplyVerticalVelocity();
        }

        private void Move()
        {
            Vector3 newMovement = playerTransform.right * inputVector.x
                + playerTransform.forward * inputVector.y;

            characterController.Move(newMovement * moveSpeed * Time.deltaTime);
        }

        private void Jump()
        {
            if(playerInput.GetButtonDownJump() && groundCheck.GetIsGrounded())
            {
                verticalVelocity = playerTransform.up * jumpVelocity;
                StopCoroutine(GroundPlayerCooldown());
                StartCoroutine(GroundPlayerCooldown());
            }
        }

        private void GroundPlayer()
        {
            if (groundCheck.GetIsGrounded() && !isGroundPlayerCooldown)
            {
                verticalVelocity = -playerTransform.up * 0;
            }
        }

        private void ApplyVerticalVelocity()
        {
            verticalVelocity += gravity * Time.deltaTime;

            characterController.Move(verticalVelocity * Time.deltaTime);
        }

        private void RotatePlayer()
        {
            playerTransform.Rotate(Vector3.up * playerInput.GetMouseInputX()
                * cameraManager.mouseSensitivity);
        }

        private IEnumerator GroundPlayerCooldown()
        {
            isGroundPlayerCooldown = true;
            yield return new WaitForSeconds(0.05f);
            isGroundPlayerCooldown = false;
        }
    }
}