using UnityEngine;

public class SeatManager : MonoBehaviour
{
    public GameObject seatParentObj;
    public Seats[] Seats { get; set; }
    void Awake()
    {
        Seats = seatParentObj.GetComponentsInChildren<Seats>();
    }
    public void ResetSeats()
    {
        Seats = seatParentObj.GetComponentsInChildren<Seats>();

        for (int i = 0; i < Seats.Length; i++)
        {
            Seats[i].isFull = false;
        }
    }
}
