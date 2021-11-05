using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Customer
{
    using Player;

    public class CustomerManager : MonoBehaviour
    {
        private PlayerInteract playerInteract;

        private List<Customer> customerList = new List<Customer>();

        [SerializeField] List<CustomerData> customerDataList = new List<CustomerData>();

        public void Construct(PlayerInteract playerInteract)
        {
            this.playerInteract = playerInteract;

            FindCustomersToAdd();
        }

        private void Update()
        {
            FindCustomersToAdd();
        }

        private void FindCustomersToAdd()
        {
            Customer[] foundCustomerArray = FindObjectsOfType<Customer>();
            foreach (Customer found in foundCustomerArray)
            {
                if (!customerList.Contains(found))
                {
                    AddCustomer(found);
                }
            }
        }

        private void AddCustomer(Customer customer)
        {
            customerList.Add(customer);

            customer.Construct(playerInteract);
        }
    }
}

