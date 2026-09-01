using System.Collections;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MakeFood : MonoBehaviour
{
    public Slider makeTimeBar;
    public TextMeshProUGUI makeTimeText;
    public GameObject effect;

    private void Awake()
    {
        effect = transform.GetChild(0).gameObject;
    }

    public IEnumerator StartCook(Food food, Customer customer, Vector3 foodPosition)  //ProductionManager.cs
    {
        float timer = 0;

        effect.SetActive(true);

        float myFoodMakeSpeed = food.cookTime / (1f + FacilityManager.instance.makeSpeed);

        while (timer < myFoodMakeSpeed)
        {
            timer += Time.deltaTime;
            makeTimeBar.value = timer / myFoodMakeSpeed;
            makeTimeText.text = $"{timer:F1} / {myFoodMakeSpeed:F1}";

            if (timer > 2f)
                effect.SetActive(false);

            yield return null;
        }
        GameObject foodObject = ObjectPoolManager.instance.GetObject($"{food.name}");
        foodObject.transform.position = foodPosition;
        customer.myFood = foodObject;
        makeTimeBar.value = 0;
        makeTimeText.text = $"0.0 / 0.0";
    }
}
