using UnityEngine;

public class BInventoryUI : MonoBehaviour
{
    public void Toggle()
    {
        // Initial visibility is set in the scene, including initially inactive panels.
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
