using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MakeFood : MonoBehaviour
{
    public Slider makeTimeBar;
    public TextMeshProUGUI makeTimeText;
    public GameObject[] effects;

    Animator animator;
    bool isCooking;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        for (int i = 0; i < effects.Length; i++)
        {
            effects[i].SetActive(false);
        }
    }

    public IEnumerator StartCook(Food food, Customer customer, Vector3 foodPosition)  //ProductionManager.cs
    {
        float timer = 0;

        animator.SetBool("Cook", true);
        animator.SetBool("Idle", false);
        isCooking = true;
        StartCoroutine(RandomEffect());

        float myFoodMakeSpeed = food.cookTime / (1f + FacilityManager.instance.MakeSpeed);

        while (timer < myFoodMakeSpeed)
        {
            timer += Time.deltaTime;
            makeTimeBar.value = timer / myFoodMakeSpeed;
            makeTimeText.text = $"{timer:F1} / {myFoodMakeSpeed:F1}";

            yield return null;
        }

        GameObject foodObject = BObjectPoolManager.instance.GetObject($"{food.name}");
        Vector3 startPos = transform.position + new Vector3(0.5f, 0f, 0f);
        foodObject.transform.position = startPos;
        StartCoroutine(MoveFood(foodObject, foodPosition, customer));
        customer.myFood = foodObject;
        makeTimeBar.value = 0;
        makeTimeText.text = $"0.0 / 0.0";

        animator.SetBool("Idle", true);
        animator.SetBool("Cook", false);
        isCooking = false;
    }

    IEnumerator RandomEffect()
    {
        while (isCooking)
        {
            // 랜덤 대기시간
            yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 0.5f));

            // 이펙트 랜덤 선택
            GameObject effect = effects[Random.Range(0, effects.Length)];

            // 지정 범위 안 랜덤 위치

            GameObject obj = Instantiate(effect, transform);
            obj.SetActive(true);
            obj.transform.localPosition = new Vector3(transform.position.x + Random.Range(-1f, 1f), transform.position.y + Random.Range(-1f, 1f), transform.position.z);
            obj.transform.localRotation = effect.transform.localRotation;
            obj.transform.localScale = effect.transform.localScale;

            // 0.3초 뒤 삭제
            Destroy(obj, 1f);
        }
    }

    IEnumerator MoveFood(GameObject foodObject, Vector3 targetPos, Customer customer)
    {
        float moveTime = 0.5f;
        float timer = 0f;

        Vector3 startPos = foodObject.transform.position;

        while (timer < moveTime)
        {
            timer += Time.deltaTime;

            float t = timer / moveTime;

            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);

            // 중간에서 위로 붕 뜨기
            pos.y += Mathf.Sin(t * Mathf.PI) * 1f;

            foodObject.transform.position = pos;

            yield return null;
        }

        foodObject.transform.position = targetPos;
        customer.myFood = foodObject;
    }
}
