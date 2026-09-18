using UnityEngine;
using System;


//QA 테스트용 스크립트입니다.
public class CheatManager : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static CheatManager instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        //------ 골드 치트 --------
        // D: (1) 골드
        if (Input.GetKeyDown(KeyCode.D))
        {
            AddGold(1);
        }

        // F: (10) 골드
        if (Input.GetKeyDown(KeyCode.F))
        {
            AddGold(10);
        }

        // G: 0.1K (100) 골드
        if (Input.GetKeyDown(KeyCode.G))
        {
            AddGold(100);
        }

        // H: 1K (1,000) 골드
        if (Input.GetKeyDown(KeyCode.H))
        {
            AddGold(1000);
        }

        // J: 10K (10,000) 골드
        if (Input.GetKeyDown(KeyCode.J))
        {
            AddGold(10000);
        }

        // K: 100K (100,000) 골드
        if (Input.GetKeyDown(KeyCode.K))
        {
            AddGold(100000);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            AddGold(1000000000000000000);
        }

        // N: 골드 0으로 초기화 (재화 부족 예외 테스트용)
        if (Input.GetKeyDown(KeyCode.N))
        {
            ResetGold();
        }

        // --- 물고기 치트 ---
        // Y : 모든 물고기 10개씩 추가
        if (Input.GetKeyDown(KeyCode.Y))
        {
            AddAllFish(10);
        }
        // U: 모든 물고기 100개씩 추가
        if (Input.GetKeyDown(KeyCode.U))
        {
            AddAllFish(100);
        }

        // I: 모든 물고기 0으로 초기화
        if (Input.GetKeyDown(KeyCode.I))
        {
            ResetAllFish();
        }
    }

    #region Gold Cheats
    public void AddGold(long amount)
    {
        if (CurrencyManager.instance != null)
        {
            CurrencyManager.instance.AddGold(new BigNumber(amount));
        }
    }

    public void ResetGold()
    {
        if (CurrencyManager.instance != null)
        {
            BigNumber currentGold = CurrencyManager.instance.GetCurrentGold();

            if (!currentGold.IsZeroOrNegative)
            {
                CurrencyManager.instance.SpendGold(currentGold);
            }
        }
    }
    #endregion

    #region Fish Cheats
    public void AddAllFish(int amountPerSpecies = 10)
    {
        if (CurrencyManager.instance == null) return;

        foreach (FishGrade grade in Enum.GetValues(typeof(FishGrade)))
        {
            for (int species = 0; species < 30; species++)
            {
                CurrencyManager.instance.AddFish(grade, species, amountPerSpecies);
            }
        }
    }

    public void ResetAllFish()
    {
        if (CurrencyManager.instance == null) return;

        foreach (FishGrade grade in Enum.GetValues(typeof(FishGrade)))
        {
            int total = CurrencyManager.instance.GetGradeTotal(grade);
            if (total > 0)
            {
                CurrencyManager.instance.SpendFromGrade(grade, total);
            }
        }
    }
    #endregion
#endif
}
