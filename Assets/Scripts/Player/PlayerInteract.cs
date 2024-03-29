﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    using Customer;
    using Collectables;

    [RequireComponent(typeof(Collider))]

    public class PlayerInteract : MonoBehaviour
    {
        private PlayerManager playerManager;
        private GameManager gameManager;
        private Collider interactCollider;

        public void Construct(GameManager gameManager, PlayerManager playerManager)
        {
            this.playerManager = playerManager;
            this.gameManager = gameManager;
            interactCollider = gameObject.GetComponent<Collider>();
        }

        private void OnTriggerEnter(Collider other)
        {
            Customer thisCustomer = other.gameObject.GetComponent<Customer>();

            Coin thisCoin = other.gameObject.GetComponent<Coin>();

            if (thisCustomer != null)
            {
                thisCustomer.OnTryReceive.Invoke();
            }

            if (thisCoin != null)
            {
                thisCoin.OnCollect.Invoke();
            }
        }

        public void GainPoints(int pay)
        {
            gameManager.AddScore(pay);
        }

        public void GainMorale(int value)
        {
            playerManager.ChangeMorale(value);
        }
    }
}