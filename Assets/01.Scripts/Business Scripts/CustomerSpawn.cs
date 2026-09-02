using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawn : MonoBehaviour
{
    public static CustomerSpawn Instance;

    public GameObject[] customers;
    public Transform spawnPoint;

    public SeatManager seatManager;

    WaitForSeconds spawnTime;
    WaitForSeconds checkTime;

    // 현재 자리 기다리는 손님들
    public List<Customer> waitingCustomers = new List<Customer>();

    private void Awake()
    {
        Instance = this;

        spawnTime = new WaitForSeconds(3f);
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
            GameObject spawnCustomer = BObjectPoolManager.instance.GetObject($"cat{Random.Range(0,2)}");
            spawnCustomer.transform.position = spawnPoint.position; 
            waitingCustomers.Add(spawnCustomer.GetComponent<Customer>());

            yield return spawnTime;
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

            for (int i = 0; i < seatManager.Seats.Length; i++)
            {
                if (seatManager.Seats[i].isFull == false)
                {
                    waitingCustomers[0].mySeat = seatManager.Seats[i];
                    waitingCustomers.RemoveAt(0);

                    seatManager.Seats[i].isFull = true;

                    break;
                }
            }
        }
    }
}