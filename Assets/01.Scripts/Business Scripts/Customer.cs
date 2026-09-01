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
    public GameObject sprites;
    public Vector3 exitPoint;

    void OnEnable()
    {
        state = CustomerState.Enter;
        exitPoint = new Vector3(5, 0.5f, 0);
        eatTime = 0;
        timer = 0;
        mySeat = null;
        eatTimeBar.value = 0;
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
        if (mySeat != null)
            ChangeState(CustomerState.MoveToSeat);
    }
    void MoveToSeatState()  //
    {
        if (mySeat == null)
            return;

        transform.position = Vector3.MoveTowards(transform.position, mySeat.transform.position, moveSpeed * Time.deltaTime);
        float distance = Vector3.Distance(transform.position, mySeat.transform.position);
        //  현재 손님 이동방향이 어느 방향인지
        if (mySeat.transform.position.x < transform.position.x)
            sprites.transform.localScale = new Vector3(1, 1, 1);
        else
            sprites.transform.localScale = new Vector3(-1, 1, 1);

        if (distance < 0.05f)
        {
            transform.position = mySeat.transform.position;
            if (mySeat.transform.localScale.x < 0)
            {
                sprites.transform.localScale = new Vector3(1, 1, 1);
            }
            ChangeState(CustomerState.Order);
        }
    }
    void OrderState()  //
    {
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

            ObjectPoolManager.instance.ReturnObject(myFood.name.ToString().Split("(Clone)")[0], myFood);
            Food f = myFood.GetComponent<Food>();
            FacilityManager.instance.GetGold(f.price, f.isSpecial);

            myFood = null;
            mySeat.isFull = false;
            eatTimeBar.gameObject.SetActive(false);
            ChangeState(CustomerState.Exit);
        }
    }
    void ExitState()  // 밖으로 나감
    {
        transform.position = Vector3.MoveTowards(transform.position, exitPoint, moveSpeed * Time.deltaTime);
        float distance = Vector3.Distance(transform.position, exitPoint);
        //  현재 손님 이동방향이 어느 방향인지
        if (exitPoint.x < transform.position.x)
            sprites.transform.localScale = new Vector3(1, 1, 1);
        else
            sprites.transform.localScale = new Vector3(-1, 1, 1);

        if (distance < 0.05f)
        {
            transform.position = mySeat.transform.position;
            ObjectPoolManager.instance.ReturnObject(name.ToString().Split("(Clone)")[0], gameObject);
        }
    }
}
