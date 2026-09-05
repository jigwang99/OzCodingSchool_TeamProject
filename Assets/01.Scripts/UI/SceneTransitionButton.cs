using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class SceneTransitionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum TargetScene // 씬 이름 변경 시 같이 수정해줘야함
    {
        BusinessScene, CombatScene, TitleScene, GachaScene, UpgradeTestScene
    }

    [Header("목적지 씬")]
    [SerializeField] private TargetScene targetScene;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        GetComponent<Button>().onClick.AddListener(OnClickButton);

        if (SceneManager.GetActiveScene().name == targetScene.ToString())
        {
            gameObject.SetActive(false);
        }
    }

    private void OnClickButton()
    {
        SceneFadeManager.Instance?.ChangeScene(targetScene.ToString());
    }

    public void OnPointerDown(PointerEventData eventData) => transform.localScale = originalScale * 0.93f;
    public void OnPointerUp(PointerEventData eventData) => transform.localScale = originalScale;
}