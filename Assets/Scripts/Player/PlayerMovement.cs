using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Player
{
    using Utility;

    public class PlayerMovement : MonoBehaviour
    {
        // External references
        private PlayerInput playerInput;
        private CharacterController characterController;
        private Transform playerTransform;
        private GroundCheck groundCheck;
        private CameraManager cameraManager;

        [Header("Gravity & jump variables")]
        [Tooltip("Gravity when actively jumping upward; all gravityMult modifies this")]
        [SerializeField] private Vector3 baseGravity;
        [Tooltip("Gliding gravity; when player jumps in the air, toggles glide")]
        [SerializeField] private float glideGravityMult;
        [Tooltip("Falling gravity after a full hold jump")]
        [SerializeField] private float fallGravityMult;
        [Tooltip("Falling gravity after releasing jump before reaching full jump")]
        [SerializeField] private float lowJumpGravityMult;
        [Tooltip("Height reached in Unity units after a full jump")]
        [SerializeField] private float jumpHeight;
        [Tooltip("Maximum vertical speed")]
        [SerializeField] private float terminalVelocity;

        [Header("Ground horizontal movement variables")]
        [Tooltip("Acceleration per second when not sprinting")]
        [SerializeField] private float walkAccel;
        [Tooltip("Acceleration per second when sprinting")]
        [SerializeField] private float sprintAccel;
        [Tooltip("Max speed when walking")]
        [SerializeField] private float maxWalkSpeed;
        [Tooltip("Max speed when sprinting")]
        [SerializeField] private float maxSprintSpeed;
        [Tooltip("On ground, rate of deceleration if no horizontal movement input")]
        [Range(0, 1)]
        [SerializeField] private float groundHorizontalSlowdown;
        [Tooltip("Speed threshold where player's velocity is set to 0")]
        [SerializeField] private float stopSpeed;

        [Header("Air horizontal movement variables")]
        [Tooltip("In air, rate of deceleration if no horizontal movement input")]
        [Range(0, 1)]
        [SerializeField] private float airHorizontalSlowdown;
        [Tooltip("Multiplies movement input magnitude while not grounded")]
        [SerializeField] private float airControlMult;
        [Tooltip("Acceleration multiplier for air movement directly forward")]
        [SerializeField] private float forwardAirControl;
        [Tooltip("Acceleration multiplier for air movement sideways")]
        [SerializeField] private float forwardSideAirControl;
        [Tooltip("Acceleration multiplier for air movement slightly forward of sideways")]
        [SerializeField] private float backSideAirControl;
        [Tooltip("Acceleration multiplier for air movement directly backward")]
        [SerializeField] private float backAirControl;


        // Internal references
        private Vector3 currentGravity;
        private Vector2 inputVector;
        private Vector3 verticalVelocity;
        private Vector3 horizontalVelocity;
        private float jumpVelocity;

        // Internal state trackers
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

            float thisAccel = playerInput.GetButtonSprint()
                ? sprintAccel : walkAccel;

            float thisMaxSpeed = playerInput.GetButtonSprint()
                ? maxSprintSpeed : maxWalkSpeed;

            // No horizontal input
            if (newMovement.magnitude == 0)
            {
                if (groundCheck.GetIsGrounded())
                {
                    horizontalVelocity *= groundHorizontalSlowdown;
                }
                else
                {
                    horizontalVelocity *= airHorizontalSlowdown;
                }
            }
            // Horizontal input and grounded
            else if (groundCheck.GetIsGrounded())
            {
                NewHorizontalMove(newMovement, thisAccel, thisMaxSpeed);
            }
            // Horizontal input and not grounded
            else
            {
                NewAirMovement(newMovement * airControlMult
                    , thisAccel, thisMaxSpeed);
            }

            characterController.Move(horizontalVelocity * Time.deltaTime);
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

        private void NewAirMovement(Vector3 vector, float accel, float maxSpeed)
        {
            float forwardDotProduct = Vector3.Dot(playerTransform.forward,
                vector.normalized);

            if (forwardDotProduct >= 0)
            {
                float forwardMult = UtilityFunctions.Rescale(
                    0, 1, forwardSideAirControl, forwardAirControl, forwardDotProduct);
                NewHorizontalMove(vector, accel * forwardMult, maxSpeed);
            }
            else
            {
                float inverseBackwardDot = 1 + forwardDotProduct;
                float oppositeMult = UtilityFunctions.Rescale(
                    0, 1, backAirControl, backSideAirControl, inverseBackwardDot);
                NewHorizontalMove(vector, accel * oppositeMult, maxSpeed);
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
                if (isGliding)
                {
                    currentGravity = baseGravity * glideGravityMult;
                }
                else
                {
                    currentGravity = baseGravity * fallGravityMult;
                }
            }
            // Player is going up
            else if (upDotProduct > 0)
            {
                if(!playerInput.GetButtonJump())
                {
                    currentGravity = baseGravity * lowJumpGravityMult;
                }
            }
            // Player has no vertical velocity
            else
            {
                currentGravity = baseGravity;
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

            if (verticalVelocity.magnitude > terminalVelocity)
            {
                verticalVelocity.Normalize();
                verticalVelocity *= terminalVelocity;
            }

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