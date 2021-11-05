using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Customer
{
    using Player;

    public class CustomerManager : MonoBehaviour
    {
        private Deliver deliver;

        private List<Customer> CustomerList = new List<Customer>();

        [SerializeField] List<CustomerData> CustomerDataList = new List<CustomerData>();

        public void Construct(Deliver deliver)
        {
            this.deliver = deliver;

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
                if (!CustomerList.Contains(found))
                {
                    AddCustomer(found);
                }
            }
        }

        private void AddCustomer(Customer customer)
        {
            CustomerList.Add(customer);

            customer.Construct(deliver);
        }
    }
}

