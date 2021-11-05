using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class GroundCheck : MonoBehaviour
    {
        private Collider groundCheckCollider;
        private Transform groundCheckTransform;
        private Transform playerTransform;

        [SerializeField] private LayerMask groundLayerMask;
        [SerializeField] private float slopeCheckRayLength;

        private Vector3 localPosiiton;
        private bool isGrounded;
        private List<Collider> touchGroundList = new List<Collider>();

        public void Construct(Transform playerTransform)
        {
            this.playerTransform = playerTransform;

            groundCheckTransform = gameObject.transform;
            groundCheckCollider = gameObject.GetComponent<Collider>();

            localPosiiton = gameObject.transform.localPosition;
        }

        private void Update()
        {
            for(int i = 0; i < touchGroundList.Count; i++)
            {
                Collider thisCollider = touchGroundList[0];
                if (thisCollider == null)
                {
                    touchGroundList.Remove(thisCollider);
                }
            }

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
            TryAddGroundCollider(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryAddGroundCollider(other);
        }

        private void TryAddGroundCollider(Collider other)
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

        public float GetGroundAngle()
        {
            float result = 0;

            RaycastHit hit;
            bool raycast = Physics.Raycast(groundCheckTransform.position,
                -playerTransform.up, out hit, slopeCheckRayLength, groundLayerMask);

            if(raycast)
            {
                result = Vector3.Angle(hit.normal, playerTransform.up);
            }

            return result;
        }

        public bool GetIsGrounded()
        {
            return isGrounded;
        }
    }
}

