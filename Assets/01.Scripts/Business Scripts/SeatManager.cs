using UnityEngine;

public class SeatManager : MonoBehaviour
{
    public GameObject seatParentObj;
    public Seats[] Seats { get; set; }
    void Awake()
    {
        Seats = seatParentObj.GetComponentsInChildren<Seats>();
    }
}
