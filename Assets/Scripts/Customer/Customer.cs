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
        private PlayerInteract playerInteract;
        private NameText nameText;
        private PayText payText;
        [SerializeField] private MeshRenderer customerMesh;

        [SerializeField] private CustomerData data;

        private bool isActive;

        public UnityEvent OnTryReceive = new UnityEvent();

        private IEnumerator CurrentPayDecrease;

        public void Construct(PlayerInteract deliver)
        {
            if (deliver == null)
            {
                throw new ArgumentNullException(nameof(deliver));
            }

            this.playerInteract = deliver;

            nameText = gameObject.GetComponentInChildren<NameText>();
            payText = gameObject.GetComponentInChildren<PayText>();
            nameText.Construct(Color.magenta, "");
            payText.Construct(Color.magenta, 0);

            OnTryReceive.AddListener(CallbackOnTryReceive);

            if (data == null)
            {
                ClearText();
                ToggleIsActive(false);
            }
            else
            {
                ResetToActive();
                data.ResetData();
            }
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

            if(data == null)
            {
                customerMesh.material.color = Color.grey;
                return;
            }

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
                playerInteract.GainPoints(ReceivePizza());
                playerInteract.GainMorale(data.moraleValue);

                StartCoroutine(ResetToActiveCooldown());
            }
        }

        private int ReceivePizza()
        {
            StopCoroutine(CurrentPayDecrease);
            ClearText();

            int toPay = data.currentPay;
            data.currentPay = 0;

            return toPay;
        }

        private void ClearText()
        {
            payText.ChangeText("");
            nameText.ChangeText("");
        }

        private void ResetToActive()
        {
            data.ResetData();

            nameText.ChangeText(data.name);
            payText.ChangeText(data.currentPay);
            nameText.ChangeColor(data.nameColor);
            payText.ChangeColor(data.payColor);

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
