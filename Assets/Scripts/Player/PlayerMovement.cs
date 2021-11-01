using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private PlayerInput playerInput;
        private CharacterController characterController;
        private Transform playerTransform;
        private GroundCheck groundCheck;
        private CameraManager cameraManager;

        [SerializeField] private Vector3 baseGravity;
        [SerializeField] private float glideGravityMult;
        [SerializeField] private float fallGravityMult;
        [SerializeField] private float lowJumpGravityMult;

        [SerializeField] private float walkAccel;
        [SerializeField] private float sprintAccel;
        [SerializeField] private float maxWalkSpeed;
        [SerializeField] private float maxSprintSpeed;
        [Range(0, 1)]
        [SerializeField] private float decel;
        [SerializeField] private float stopSpeed;
        [SerializeField] private float jumpHeight;

        private Vector3 currentGravity;
        private Vector2 inputVector;
        private Vector3 verticalVelocity;
        private Vector3 horizontalVelocity;
        private float jumpVelocity;

        private bool isGliding;
        private bool isGroundPlayerCooldown;

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

            currentGravity = baseGravity;

            // Convert jump height into jump velocity
            jumpVelocity = Mathf.Sqrt(jumpHeight * 2f * baseGravity.magnitude);
        }

        private void Update()
        {
            inputVector = playerInput.GetInputVector();

            RotatePlayer();

            GroundPlayer();

            GravityControl();

            JumpOrGlide();

            Move();

            ApplyVerticalVelocity();
        }

        private void Move()
        {
            Vector3 newMovement = playerTransform.right * inputVector.x
                + playerTransform.forward * inputVector.y;

            if (playerInput.GetButtonSprint())
            {
                NewHorizontalMove(newMovement, sprintAccel, maxSprintSpeed);
            }
            else
            {
                NewHorizontalMove(newMovement, walkAccel, maxWalkSpeed);
            }

            characterController.Move(horizontalVelocity * Time.deltaTime);

            horizontalVelocity *= decel;
        }

        private void NewHorizontalMove(Vector3 vector, float accel, float maxSpeed)
        {
            horizontalVelocity += vector * accel;

            if(horizontalVelocity.magnitude > maxSpeed)
            {
                horizontalVelocity.Normalize();
                horizontalVelocity *= maxSpeed;
            }
            else if (horizontalVelocity.magnitude <= stopSpeed)
            {
                horizontalVelocity = Vector3.zero;
            }
        }

        private void JumpOrGlide()
        {
            if(playerInput.GetButtonDownJump())
            {
                if (groundCheck.GetIsGrounded())
                {
                    Jump();
                }
                else
                {
                    isGliding = !isGliding;
                }
            }
        }

        private void Jump()
        {
            verticalVelocity = playerTransform.up * jumpVelocity;
            StopCoroutine(GroundPlayerCooldown());
            StartCoroutine(GroundPlayerCooldown());
        }

        private void GravityControl()
        {
            float upDotProduct = Vector3.Dot(playerTransform.up,
                verticalVelocity.normalized);

            // Player is going down
            if (upDotProduct < 0)
            {
                currentGravity = baseGravity * fallGravityMult;
            }
            // Player is going up && not holding jump
            else if (upDotProduct > 0 && !playerInput.GetButtonJump())
            {
                currentGravity = baseGravity * lowJumpGravityMult;
            }
            else
            {
                currentGravity = baseGravity;
            }

            if(isGliding)
            {
                currentGravity = baseGravity * glideGravityMult;
            }
        }

        private void GroundPlayer()
        {
            if (groundCheck.GetIsGrounded() && !isGroundPlayerCooldown)
            {
                isGliding = false;
                verticalVelocity = -playerTransform.up * 0;
            }
        }

        private void ApplyVerticalVelocity()
        {
            verticalVelocity += currentGravity * Time.deltaTime;

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