using System.Collections;
using TMPro;
using UnityEngine;

public class GetGoldPopup : MonoBehaviour
{
    public TextMeshPro goldText;
    public void Show(int gold, bool isSpecial)
    {
        MeshRenderer mr = goldText.GetComponent<MeshRenderer>();

        mr.sortingLayerName = "Default";
        mr.sortingOrder = 1000;

        goldText.text = $"+{gold}G";
        if (isSpecial)
            goldText.text += $" x2";
        StartCoroutine(Popup());
    }

    IEnumerator Popup()
    {
        float time = 0f;

        Color textColor = goldText.color;

        while (time < 1f)
        {
            time += Time.deltaTime;

            transform.position += Vector3.up * 0.2f * Time.deltaTime;

            textColor.a = 1f - (time / 1f);
            goldText.color = textColor;

            yield return null;
        }

        Destroy(gameObject);
    }
}
