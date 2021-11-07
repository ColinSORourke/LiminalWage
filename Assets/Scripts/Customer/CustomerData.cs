using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Customer
{
    [CreateAssetMenu(fileName = "CustomerData", menuName = "My Game/CustomerData")]

    public class CustomerData : ScriptableObject
    {
        private Customer customer;

        [Header("Colors")]
        [Tooltip("Active customer mesh material color")]
        public Color activeColor;
        [Tooltip("Inactive customer mesh material color")]
        public Color inactiveColor;
        [Tooltip("Name text text color")]
        public Color nameColor;
        [Tooltip("Pay text text color")]
        public Color payColor;

        [Header("Morale variables")]
        [Tooltip("Morale gained by player upon pay")]
        public int moraleValue;

        [Header("Pay variables")]
        [Tooltip("Starting pay, when reset currentPay = initialPay")]
        public int initialPay;
        [Tooltip("Pay cannot be decreased below this amount by payDecreaseAmount")]
        public int minimumPay;
        [Tooltip("Tracks this customer's current pay")]
        public int currentPay;

        [Header("Pay change variables")]
        [Tooltip("Time in seconds between decrements of pay")]
        public float payDecreaseTime;
        [Tooltip("Amount pay decreases per payDecreaseTime")]
        public int payDecreaseAmount;

        [Header("Control variables")]
        [Tooltip("Time in seconds until an inactive customer becomes active")]
        public float activeCooldown;

        public void ResetData()
        {
            currentPay = initialPay;
        }
    }
}