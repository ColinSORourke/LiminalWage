using System;
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
        private GameManager gameManager;

        private Transform playerTransform;

        private GroundCheck groundCheck;

        private PlayerInput playerInput;

        private CharacterController characterController;
        
        private PlayerMovement playerMovement;

        private CameraManager cameraManager;

        private Deliver deliver;

        public void Construct(GameManager gameManager)
        {
            if (gameManager == null)
            {
                throw new ArgumentNullException(nameof(gameManager));
            }

            this.gameManager = gameManager;

            playerTransform = gameObject.GetComponent<Transform>();

            groundCheck = gameObject.GetComponentInChildren<GroundCheck>();
            groundCheck.Construct(playerTransform);

            playerInput = gameObject.GetComponent<PlayerInput>();

            characterController = gameObject.GetComponent<CharacterController>();
            
            cameraManager = gameObject.GetComponentInChildren<CameraManager>();

            cameraManager.Construct(playerInput);

            playerMovement = gameObject.GetComponent<PlayerMovement>();
            playerMovement.Construct(playerTransform, groundCheck
                , playerInput, characterController, cameraManager);

            deliver = gameObject.GetComponentInChildren<Deliver>();
            deliver.Construct(gameManager);
        }

        public Deliver GetDeliver()
        {
            return deliver;
        }

        public Transform GetPlayerTransform()
        {
            return playerTransform;
        }
    }
}