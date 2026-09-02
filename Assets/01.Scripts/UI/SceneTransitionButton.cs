using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class SceneTransitionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum TargetScene { BusinessScene, CombatScene, TitleScene }

    [Header("¸ñÀûÁö ¾À")]
    [SerializeField] private TargetScene targetScene;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        GetComponent<Button>().onClick.AddListener(OnClickButton);
    }

    private void OnClickButton()
    {
        SceneFadeManager.Instance?.ChangeScene(targetScene.ToString());
    }

    public void OnPointerDown(PointerEventData eventData) => transform.localScale = originalScale * 0.93f;
    public void OnPointerUp(PointerEventData eventData) => transform.localScale = originalScale;
}