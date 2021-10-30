using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class GroundCheck : MonoBehaviour
    {
        private bool isGrounded;
        [SerializeField] private LayerMask groundLayerMask;

        private void OnTriggerStay(Collider other)
        {
            isGrounded =
                other != null
                && (((1<< other.gameObject.layer) & groundLayerMask) != 0);
        }

        private void OnTriggerExit(Collider other)
        {
            isGrounded = false;
        }

        public bool GetIsGrounded()
        {
            return isGrounded;
        }
    }
}

