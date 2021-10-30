using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class PlayerInput : MonoBehaviour
    {
        private Vector2 inputVector;
        private float mouseInputX;
        private float mouseInputY;
        private bool buttonDownJump;
        private bool buttonDownEscape;

        private void Update()
        {
            Vector2 thisInputVector = new Vector2(Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical"));
            thisInputVector.Normalize();

            inputVector = thisInputVector;

            mouseInputX = Input.GetAxis("Mouse X") * Time.deltaTime;
            mouseInputY = Input.GetAxis("Mouse Y") * Time.deltaTime;

            buttonDownJump = Input.GetButtonDown("Jump");

            buttonDownEscape = Input.GetKeyDown(KeyCode.Escape);
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

        public bool GetButtonDownJump()
        {
            return buttonDownJump;
        }

        public bool GetButtonDownEscape()
        {
            return buttonDownEscape;
        }
    }

}