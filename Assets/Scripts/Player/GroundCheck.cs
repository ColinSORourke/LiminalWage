using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class GroundCheck : MonoBehaviour
    {
        private Collider groundCheckCollider;
        private Transform groundCheckTransform;

        [SerializeField] private LayerMask groundLayerMask;

        private Vector3 localPosiiton;
        private bool isGrounded;
        private List<Collider> touchGroundList = new List<Collider>();

        private void Awake()
        {
            groundCheckTransform = gameObject.transform;
            groundCheckCollider = gameObject.GetComponent<Collider>();

            localPosiiton = gameObject.transform.localPosition;
            print("localPosiiton: " + localPosiiton); 
        }

        private void Update()
        {
            //groundCheckTransform.position = localPosiiton;

            if (touchGroundList.Count == 0)
            {
                isGrounded = false;
            }
            else
            {
                isGrounded = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other != null
                && ((1 << other.gameObject.layer) & groundLayerMask) != 0
                && !touchGroundList.Contains(other))
            {
                touchGroundList.Add(other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other != null
                && ((1 << other.gameObject.layer) & groundLayerMask) != 0
                && !touchGroundList.Contains(other))
            {
                touchGroundList.Add(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null
                && ((1 << other.gameObject.layer) & groundLayerMask) != 0
                && touchGroundList.Contains(other))
            {
                touchGroundList.Remove(other);
            }
        }

        public bool GetIsGrounded()
        {
            return isGrounded;
        }
    }
}

