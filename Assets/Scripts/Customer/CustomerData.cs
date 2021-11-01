using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Customer
{
    [CreateAssetMenu(fileName = "CustomerData", menuName = "My Game/CustomerData")]

    public class CustomerData : ScriptableObject
    {
        private Customer customer;

        public Color activeColor;
        public Color inactiveColor;

        public Color nameColor;
        public Color payColor;

        public float payDecreaseTime;
        public int payDecreaseAmount;

        public int initialPay;
        public int minimumPay;
        public int currentPay;

        public float activeCooldown;

        public void ResetData()
        {
            currentPay = initialPay;
        }
    }
}