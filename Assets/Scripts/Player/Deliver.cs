using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    using Customer;

    [RequireComponent(typeof(Collider))]

    public class Deliver : MonoBehaviour
    {
        private GameManager gameManager;
        private Collider deliverCollider;

        public void Construct(GameManager gameManager)
        {
            this.gameManager = gameManager;
            deliverCollider = gameObject.GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            Customer thisCustomer = other.gameObject.GetComponent<Customer>();

            if(thisCustomer != null)
            {
                thisCustomer.OnTryReceive.Invoke();
            }
        }

        public void ReceivePay(int pay)
        {
            gameManager.AddScore(pay);
        }
    }
}