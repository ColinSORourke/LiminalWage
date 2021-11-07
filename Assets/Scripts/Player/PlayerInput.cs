using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerInput : MonoBehaviour
    {
        private GameManager gameManager;

        private Vector2 inputVector;
        private float mouseInputX;
        private float mouseInputY;

        private bool buttonDownEscape;

        private bool buttonDownJump;
        private bool buttonJump;
        private bool buttonUpJump;

        private bool buttonDownSprint;
        private bool buttonSprint;
        private bool buttonUpSprint;

        public void Construct(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        private void Update()
        {
            if(!gameManager.isGameOver)
            {
                Vector2 thisInputVector = new Vector2(Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"));
                thisInputVector.Normalize();

                inputVector = thisInputVector;

                mouseInputX = Input.GetAxis("Mouse X") * Time.deltaTime;
                mouseInputY = Input.GetAxis("Mouse Y") * Time.deltaTime;

                buttonDownEscape = Input.GetKeyDown(KeyCode.Escape);

                buttonDownJump = Input.GetButtonDown("Jump");
                buttonJump = Input.GetButton("Jump");
                buttonUpJump = Input.GetButtonUp("Jump");

                buttonDownSprint = Input.GetButtonDown("Sprint");
                buttonSprint = Input.GetButton("Sprint");
                buttonUpSprint = Input.GetButtonUp("Sprint");
            }
        }

        public Vector2 GetInputVector()
        {
            return inputVector;
        }

        public float GetMouseInputX()
        {
            return mouseInputX;
        }

        public float GetMouseInputY()
        {
            return mouseInputY;
        }

        public bool GetButtonDownEscape()
        {
            return buttonDownEscape;
        }

        public bool GetButtonDownJump()
        {
            return buttonDownJump;
        }

        public bool GetButtonJump()
        {
            return buttonJump;
        }

        public bool GetButtonUpJump()
        {
            return buttonUpJump;
        }

        public bool GetButtonDownSprint()
        {
            return buttonDownSprint;
        }

        public bool GetButtonSprint()
        {
            return buttonSprint;
        }

        public bool GetButtonUpSprint()
        {
            return buttonUpSprint;
        }
    }
}