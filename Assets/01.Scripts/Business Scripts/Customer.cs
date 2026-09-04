using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public enum CustomerState
{
    Enter,
    MoveToSeat,
    Order,
    Waiting,
    Eating,
    Exit
}

public class Customer : MonoBehaviour
{
    CustomerState state;

    float moveSpeed = 1.0f;
    public Seats mySeat;

    public GameObject myFood;
    public float eatTime;
    float timer;
    public Slider eatTimeBar;
    public GameObject flipObj;
    public Vector3 exitPoint;

    public Animator animator;
    public GameObject waypointObj;
    public GameObject[] waypoints;
    int myWaypointNum;
    int nowPoint;

    void OnEnable()
    {
        state = CustomerState.Enter;
        nowPoint = 0;
        eatTime = 0;
        timer = 0;
        mySeat = null;
        eatTimeBar.value = 0;
        animator = GetComponentInChildren<Animator>();
        animator.SetBool("Idle", true);
        eatTimeBar.gameObject.SetActive(false);
    }

    void Update()
    {
        switch (state)
        {
            case CustomerState.Enter:
                EnterState();
                break;

            case CustomerState.MoveToSeat:
                MoveToSeatState();
                break;

            case CustomerState.Order:
                OrderState();
                break;

            case CustomerState.Waiting:
                WaitingState();
                break;

            case CustomerState.Eating:
                EatingState();
                break;

            case CustomerState.Exit:
                ExitState();
                break;
        }
    }
    public void ChangeState(CustomerState newState)
    {
        if (state == newState)
            return;

        state = newState;
    }

    void EnterState()  //
    {
        animator.SetBool("Idle", true);
        if (mySeat != null)
        {
            ChangeState(CustomerState.MoveToSeat);

            int seatNum = int.Parse(mySeat.name.Split('_')[1]);

            if (FacilityManager.instance.RestaurantLevel == 1)
            {
                if (seatNum > 6)
                    myWaypointNum = 3;
                else if (seatNum > 2)
                    myWaypointNum = 2;
                else
                    myWaypointNum = 0;
            }
            else if (FacilityManager.instance.RestaurantLevel == 2)
            {
                if (seatNum > 14)
                    myWaypointNum = 4;
                if (seatNum > 12)
                    myWaypointNum = 3;
                else if (seatNum > 6)
                    myWaypointNum = 2;
                else if (seatNum > 2)
                    myWaypointNum = 1;
                else
                    myWaypointNum = 0;
            }
            else if (FacilityManager.instance.RestaurantLevel == 3)
            {
                if (seatNum > 22)
                    myWaypointNum = 6;
                else if (seatNum > 13)
                    myWaypointNum = 4;
                else if (seatNum > 10)
                    myWaypointNum = 3;
                else
                    myWaypointNum = 0;
            }
        }
    }
    void MoveToSeatState()  //
    {
        if (mySeat == null)
            return;
        animator.SetBool("Idle", false);
        animator.SetBool("Run", true);

        if (nowPoint < myWaypointNum)
        {
            if (MoveTo(waypoints[nowPoint].transform.position))
                nowPoint++;
        }
        else
        {
            if (MoveTo(mySeat.transform.position))
            {
                flipObj.transform.localScale = new Vector3(mySeat.transform.localScale.x > 0 ? -1 : 1, 1, 1);
                ChangeState(CustomerState.Order);
            }
        }
    }

    void OrderState()  //
    {
        animator.SetBool("Idle", true);
        animator.SetBool("Run", false);

        ProductionManager.instance.OrderFood(this);
        ChangeState(CustomerState.Waiting);
    }
    void WaitingState()  //
    {
        if (myFood != null)
        {
            eatTimeBar.gameObject.SetActive(true);
            ChangeState(CustomerState.Eating);
        }
    }
    void EatingState()  //
    {
        timer += Time.deltaTime;
        eatTimeBar.value = timer / eatTime;

        if (timer >= eatTime)
        {
            timer = 0f;

            BObjectPoolManager.instance.ReturnObject(myFood.name.ToString().Split("(Clone)")[0], myFood);
            Food f = myFood.GetComponent<Food>();
            FacilityManager.instance.GetGold(f.price, f.isSpecial);

            myFood = null;
            mySeat.isFull = false;
            eatTimeBar.gameObject.SetActive(false);
            nowPoint = myWaypointNum - 1;
            ChangeState(CustomerState.Exit);
        }
    }
    void ExitState()  // 밖으로 나감
    {
        animator.SetBool("Idle", false);
        animator.SetBool("Run", true);

        // 좌석에서 나갈 때 웨이포인트 역순 이동
        if (nowPoint >= 0)
        {
            if (MoveTo(waypoints[nowPoint].transform.position))
                nowPoint--;
        }
        else
        {
            if (MoveTo(exitPoint))
            {
                transform.position = mySeat.transform.position;

                BObjectPoolManager.instance.ReturnObject(name.ToString().Split("(Clone)")[0], gameObject);
            }
        }
    }

    bool MoveTo(Vector3 target)
    {
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (target.x < transform.position.x)
            flipObj.transform.localScale = new Vector3(1, 1, 1);
        else
            flipObj.transform.localScale = new Vector3(-1, 1, 1);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            transform.position = target;
            return true;
        }

        return false;
    }

    public void SetWaypoint(GameObject waypoint)
    {
        waypointObj = waypoint;

        waypoints = new GameObject[waypointObj.transform.childCount];

        for (int i = 0; i < waypointObj.transform.childCount; i++)
        {
            waypoints[i] = waypointObj.transform.GetChild(i).gameObject;
        }
    }
}
