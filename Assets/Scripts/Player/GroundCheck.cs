using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class GroundCheck : MonoBehaviour
    {
        private Collider groundCheckCollider;

        [SerializeField] private LayerMask groundLayerMask;

        private bool isGrounded;

        private bool thisGrounded;

        private void Awake()
        {
            groundCheckCollider = gameObject.GetComponent<Collider>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (other != null
                && ((1 << other.gameObject.layer) & groundLayerMask) != 0)
            {
                isGrounded = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null &&
                ((1 << other.gameObject.layer) & groundLayerMask) != 0
                && !thisGrounded)
            {
                isGrounded = false;
            }
        }

        public bool GetIsGrounded()
        {
            return isGrounded;
        }
    }
}

