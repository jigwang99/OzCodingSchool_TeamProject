using UnityEngine;

public class BInventoryUI : MonoBehaviour
{    
    Vector3 originalPos;
    bool isOpen = false;

    void Start()
    {
        originalPos = transform.position;
        transform.position = new Vector3(9999f, 9999f, 0f);
    }

    public void Toggle()
    {
        isOpen = !isOpen;

        if (isOpen)
            transform.position = originalPos;
        else
            transform.position = new Vector3(9999f, 9999f, 0f);
    }
}
