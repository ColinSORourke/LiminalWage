using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(PlayerMovement))]

    public class PlayerManager : MonoBehaviour
    {
        private Transform playerTransform;

        private GroundCheck groundCheck;

        private PlayerInput playerInput;

        private CharacterController characterController;
        
        private PlayerMovement playerMovement;

        private CameraManager cameraManager;

        public void Construct()
        {
            playerTransform = gameObject.GetComponent<Transform>();

            groundCheck = gameObject.GetComponentInChildren<GroundCheck>();

            playerInput = gameObject.GetComponent<PlayerInput>();

            characterController = gameObject.GetComponent<CharacterController>();
            
            cameraManager = gameObject.GetComponentInChildren<CameraManager>();

            cameraManager.Construct(playerInput);

            playerMovement = gameObject.GetComponent<PlayerMovement>();
            playerMovement.Construct(playerTransform, groundCheck
                , playerInput, characterController, cameraManager);
        }
    }
}