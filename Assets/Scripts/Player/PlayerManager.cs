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
        private MoraleUI moraleUI;

        private Transform playerTransform;
        private GroundCheck groundCheck;
        private PlayerInput playerInput;
        private CharacterController characterController;
        private PlayerMovement playerMovement;
        private CameraManager cameraManager;
        private PlayerInteract playerInteract;

        [SerializeField] private int startingMorale;

        private int _morale;
        private bool _moraleDecreasing;

        public int morale { private set { } get { return _morale; } }

        public void Construct(GameManager gameManager, MoraleUI moraleUI)
        {
            if (gameManager == null)
            {
                throw new ArgumentNullException(nameof(gameManager));
            }
            if (moraleUI == null)
            {
                throw new ArgumentNullException(nameof(moraleUI));
            }

            this.gameManager = gameManager;
            this.moraleUI = moraleUI;

            playerTransform = gameObject.GetComponent<Transform>();

            groundCheck = gameObject.GetComponentInChildren<GroundCheck>();
            groundCheck.Construct(playerTransform);

            playerInput = gameObject.GetComponent<PlayerInput>();
            playerInput.Construct(gameManager);

            characterController = gameObject.GetComponent<CharacterController>();
            
            cameraManager = gameObject.GetComponentInChildren<CameraManager>();
            cameraManager.Construct(playerInput);

            playerMovement = gameObject.GetComponent<PlayerMovement>();
            playerMovement.Construct(playerTransform, groundCheck
                , playerInput, characterController, cameraManager);

            playerInteract = gameObject.GetComponentInChildren<PlayerInteract>();
            playerInteract.Construct(gameManager, this);

            _morale = startingMorale;

            _moraleDecreasing = true;
            StartCoroutine(PassiveMoraleDecrease());
        }

        private IEnumerator PassiveMoraleDecrease()
        {
            while(_moraleDecreasing && !gameManager.isGameOver)
            {
                yield return new WaitForSeconds(1);
                ChangeMorale(-1);
            }
        }

        public void ChangeMorale(int toAdd)
        {
            if(!gameManager.isGameOver)
            {
                _morale += toAdd;
                moraleUI.UpdateDisplay(_morale);
            }
            
            if(_morale <= 0)
            {
                _morale = 0;
                gameManager.StartGameOver();
            }
        }

        public PlayerInteract GetPlayerInteract()
        {
            return playerInteract;
        }

        public Transform GetPlayerTransform()
        {
            return playerTransform;
        }
    }
}