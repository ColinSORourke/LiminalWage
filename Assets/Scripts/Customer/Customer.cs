using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Customer
{
    using Player;

    public class Customer : MonoBehaviour
    {
        private Deliver deliver;
        private NameText nameText;
        private PayText payText;
        private MeshRenderer customerMesh;

        [SerializeField] private CustomerData data;

        private bool isActive;

        public UnityEvent OnTryReceive = new UnityEvent();

        private IEnumerator CurrentPayDecrease;

        public void Construct(Deliver deliver)
        {
            if (deliver == null)
            {
                throw new ArgumentNullException(nameof(deliver));
            }
            if (data == null)
            {
                Debug.LogError(this + " error: data cannot be null");
            }
            data.ResetData();

            this.deliver = deliver;

            customerMesh = gameObject.GetComponent<MeshRenderer>();

            nameText = gameObject.GetComponentInChildren<NameText>();
            payText = gameObject.GetComponentInChildren<PayText>();

            OnTryReceive.AddListener(CallbackOnTryReceive);

            ResetToActive();
        }

        private IEnumerator PayDecrease(float time, int amount)
        {
            bool isDecreasing = true;
            while (isDecreasing && isActive)
            {
                yield return new WaitForSeconds(time);

                if(data.minimumPay >= data.currentPay - amount)
                {
                    data.currentPay = data.minimumPay;
                    isDecreasing = false;
                }
                else
                {
                    data.currentPay -= amount;
                }
                payText.ChangeText(data.currentPay.ToString());
            }
            yield return null;
        }

        private void ToggleIsActive(bool state)
        {
            isActive = state;

            if(isActive)
            {
                customerMesh.material.color = data.activeColor;
            }
            else
            {
                customerMesh.material.color = data.inactiveColor;
            }
        }

        private void CallbackOnTryReceive()
        {
            if(isActive)
            {
                ToggleIsActive(false);
                deliver.ReceivePay(ReceivePizza());

                StartCoroutine(ResetToActiveCooldown());
            }
        }

        private int ReceivePizza()
        {
            StopCoroutine(CurrentPayDecrease);
            payText.ChangeText("");
            nameText.ChangeText("");

            int toPay = data.currentPay;
            data.currentPay = 0;

            return toPay;
        }

        private void ResetToActive()
        {
            data.ResetData();

            nameText.Construct(data.nameColor, data.name);
            payText.Construct(data.payColor, data.currentPay);

            ToggleIsActive(true);

            CurrentPayDecrease = PayDecrease(data.payDecreaseTime, data.payDecreaseAmount);

            StartCoroutine(CurrentPayDecrease);
        }

        private IEnumerator ResetToActiveCooldown()
        {
            yield return new WaitForSeconds(data.activeCooldown);
            ResetToActive();
        }
    }
}
