using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class SceneTransitionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum TargetScene // 씬 이름 변경 시 같이 수정해줘야함
    {
        BusinessScene, CombatScene, TitleScene, GachaScene, UpgradeScene
    }

    [Header("목적지 씬")]
    [SerializeField] private TargetScene targetScene;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;

        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClickButton);
    }

    public void CheckAndToggleVisibility(string currentSceneName)
    {
        bool isCurrentScene = (currentSceneName == targetScene.ToString());
        gameObject.SetActive(!isCurrentScene);
    }

    public void OnClickButton()
    {
        if (SceneFadeManager.Instance != null)
        {
            SceneFadeManager.Instance.ChangeScene(targetScene.ToString());
        }
        else
        {
            SceneManager.LoadScene(targetScene.ToString());
        }
    }



    public void OnPointerDown(PointerEventData eventData) => transform.localScale = originalScale * 0.93f;
    public void OnPointerUp(PointerEventData eventData) => transform.localScale = originalScale;
}