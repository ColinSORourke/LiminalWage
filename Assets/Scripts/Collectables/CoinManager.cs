using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Collectables
{
    using Player;

    public class CoinManager : MonoBehaviour
    {
        private PlayerInteract playerInteract;

        private List<Coin> coinList = new List<Coin>();

        public void Construct(PlayerInteract playerInteract)
        {
            this.playerInteract = playerInteract;
        }

        private void Update()
        {
            FindCoinsToAdd();
        }

        private void FindCoinsToAdd()
        {
            Coin[] foundCustomerArray = FindObjectsOfType<Coin>();
            foreach (Coin found in foundCustomerArray)
            {
                if (!coinList.Contains(found))
                {
                    AddCoin(found);
                }
            }
        }

        private void AddCoin(Coin coin)
        {
            coinList.Add(coin);

            coin.Construct(playerInteract);
        }
    }
}

