using UnityEngine;

public class Food : MonoBehaviour
{
    [HideInInspector]
    public Vector3 originalScale;   //MakeFood에서 스케일 조정할때 중첩으로 조정안되게 함
    private void Awake()
    {
        originalScale = transform.localScale;
    }
    public string foodName;

    public int fishCount;
    public int fishRare;

    public float cookTime;
    public float eatTime;

    public int price;
    public bool isSpecial;

}
