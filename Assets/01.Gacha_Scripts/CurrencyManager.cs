using System;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    public int currentMoney = 1000;

    public event Action<int> OnMoneyChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool CanSpend(int amount)
    {
        return amount <= currentMoney;
    }

    public bool Spend(int amount)
    {
        if (!CanSpend(amount)) return false;
        currentMoney -= amount;
        OnMoneyChanged?.Invoke(currentMoney);
        return true;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);
    }
}