using System;
using UnityEngine;
using UnityEngine.UI;

// 전투 드롭 UI와 같은 슬롯/프레임 에셋을 사용하는 미접속 보상 수령창.
[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))]
public class IdleFishRewardUI : MonoBehaviour
{
    [Serializable]
    private sealed class RewardSlot
    {
        public FishGrade grade;
        public int species;
        public RectTransform root;
        public Text countLabel;
    }

    [SerializeField] private GameObject modal;
    [SerializeField] private RectTransform panel;
    [SerializeField] private Text timeLabel;
    [SerializeField] private Text totalLabel;
    [SerializeField] private Button claimButton;
    [SerializeField] private RewardSlot[] rewardSlots;
    [SerializeField] private RectTransform[] footer;
    private IdleFishManager manager;
    private Vector2[] slotPositions;
    private Vector2[] footerPositions;
    private float fullHeight;
    private bool initialized;

    private void Awake()
    {
        if (modal == null || panel == null || timeLabel == null || totalLabel == null ||
            claimButton == null || rewardSlots == null || rewardSlots.Length != 15 || footer == null)
        {
#if UNITY_EDITOR
            Debug.LogError("[IdleFishRewardUI] 미접속 보상 UI 참조를 확인하세요.", this);
#endif
            enabled = false;
            return;
        }
        slotPositions = new Vector2[rewardSlots.Length];
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            if (rewardSlots[i]?.root == null || rewardSlots[i].countLabel == null)
            {
                enabled = false;
#if UNITY_EDITOR
                Debug.LogError("[IdleFishRewardUI] 물고기 슬롯 참조를 확인하세요.", this);
#endif
                return;
            }
            slotPositions[i] = rewardSlots[i].root.anchoredPosition;
        }
        fullHeight = panel.rect.height;
        footerPositions = new Vector2[footer.Length];
        for (int i = 0; i < footer.Length; i++)
            if (footer[i] != null) footerPositions[i] = footer[i].anchoredPosition;
        initialized = true;
        modal.SetActive(false);
        claimButton.onClick.AddListener(Claim);
    }

    private void Start()
    {
        if (!initialized) return;
        manager = GameManager.instance.GetComponent<IdleFishManager>();
        if (manager == null)
        {
#if UNITY_EDITOR
            Debug.LogError("[IdleFishRewardUI] IdleFishManager를 찾을 수 없습니다.", this);
#endif
            return;
        }
        manager.OnPendingRewardsChanged += Refresh;
        Refresh();
    }

    private void OnEnable()
    {
        if (manager != null) Refresh();
    }

    private void OnDisable()
    {
        if (modal != null) modal.SetActive(false);
    }

    private void OnDestroy()
    {
        if (manager != null) manager.OnPendingRewardsChanged -= Refresh;
        if (claimButton != null) claimButton.onClick.RemoveListener(Claim);
    }

    private void Claim()
    {
        if (manager == null || !isActiveAndEnabled) return;
        claimButton.interactable = false;
        manager.ClaimPendingRewards();
        Refresh();
    }

    private void Refresh()
    {
        if (!initialized || manager == null || !isActiveAndEnabled) return;
        long total = manager.PendingFishCount;
        modal.SetActive(total > 0);
        if (total <= 0) return;
        TimeSpan duration = TimeSpan.FromSeconds(manager.PendingSeconds);
        timeLabel.text = $"누적 {duration.Hours}시간 {duration.Minutes}분 · 최대 8시간";
        totalLabel.text = $"수령할 물고기 ×{total:N0}";
        int visible = 0;
        for (int i = 0; i < rewardSlots.Length; i++)
        {
            RewardSlot slot = rewardSlots[i];
            int count = manager.GetPendingFish(slot.grade, slot.species);
            slot.root.gameObject.SetActive(count > 0);
            if (count <= 0) continue;
            slot.countLabel.text = $"+{count:N0}";
            slot.root.anchoredPosition = slotPositions[visible++];
        }
        float removedHeight = (Mathf.CeilToInt(rewardSlots.Length / 4f) - Mathf.CeilToInt(visible / 4f)) * 88f;
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fullHeight - removedHeight);
        for (int i = 0; i < footer.Length; i++)
            if (footer[i] != null) footer[i].anchoredPosition = footerPositions[i] + Vector2.up * removedHeight;
        claimButton.interactable = true;
        ResizePanel();
    }

    private void OnRectTransformDimensionsChange() => ResizePanel();

    private void ResizePanel()
    {
        if (!initialized) return;
        Rect bounds = ((RectTransform)transform).rect;
        float scale = Mathf.Min(1f, (bounds.width - 40f) / panel.rect.width,
            (bounds.height - 40f) / panel.rect.height);
        panel.localScale = Vector3.one * Mathf.Max(0.1f, scale);
    }
}
