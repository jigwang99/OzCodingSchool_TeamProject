using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawn : MonoBehaviour
{
    public static CustomerSpawn Instance;

    public GameObject[] customers;

    public RestaurantPosition[] res;
    int myRestaurant;
    WaitForSeconds checkTime;

    // 현재 자리 기다리는 손님들
    public List<Customer> waitingCustomers = new List<Customer>();

    private void Awake()
    {
        Instance = this;

        checkTime = new WaitForSeconds(.5f);
    }

    void Start()
    {
        StartCoroutine(SpawnCustomers());
        StartCoroutine(EmptySeatCheck());
    }

    IEnumerator SpawnCustomers()
    {
        while (true)
        {
            myRestaurant = FacilityManager.instance.RestaurantLevel;

            GameObject spawnCustomer = BObjectPoolManager.instance.GetObject($"cat{UnityEngine.Random.Range(0,5)}");
            spawnCustomer.GetComponent<Customer>().SetWaypoint(res[myRestaurant - 1].waypoints);

            spawnCustomer.transform.position = res[myRestaurant - 1].spawnPoint.transform.position; 
            waitingCustomers.Add(spawnCustomer.GetComponent<Customer>());
            spawnCustomer.GetComponent<Customer>().exitPoint = res[myRestaurant - 1].exitPoint.transform.position;

            if (myRestaurant == 2)
                spawnCustomer.transform.localScale = new Vector3(.15f, .15f, .15f);
            else if(myRestaurant == 3)
                spawnCustomer.transform.localScale = new Vector3(.10f, .10f, .10f);

            yield return new WaitForSeconds(UnityEngine.Random.Range(2.5f,3f) - (myRestaurant - 1));
        }
    }

    IEnumerator EmptySeatCheck()
    {
        while (true)
        {
            yield return checkTime;
            if (waitingCustomers.Count == 0)
            {
                continue;
            }

            for (int i = 0; i < res[myRestaurant  - 1].seats.Seats.Length; i++)
            {
                if (res[myRestaurant - 1].seats.Seats[i].isFull == false)
                {
                    waitingCustomers[0].mySeat = res[myRestaurant - 1].seats.Seats[i];
                    waitingCustomers.RemoveAt(0);

                    res[myRestaurant - 1].seats.Seats[i].isFull = true;

                    break;
                }
            }
        }
    }
}