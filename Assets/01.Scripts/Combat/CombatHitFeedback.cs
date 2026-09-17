using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(UnitHealth))]
public class CombatHitFeedback : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private TMP_FontAsset damageFont;
    [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.25f);
    [SerializeField, Min(0.01f)] private float flashDuration = 0.12f;
    [SerializeField] private Vector3 hitOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Color damageColor = new Color(1f, 0.95f, 0.75f);

    private UnitHealth health;
    private SpriteRenderer[] sprites;
    private Color[] originalColors;
    private float flashRemaining;
    private bool flashActive;

    private void Awake()
    {
        health = GetComponent<UnitHealth>();
        if (visualRoot == null)
        {
            var cat = GetComponentInChildren<CatUnitView>(true);
            var monster = GetComponentInChildren<MonsterUnitView>(true);
            visualRoot = cat != null ? cat.transform : monster != null ? monster.transform : transform;
        }
        sprites = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        originalColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
            originalColors[i] = sprites[i].color;
    }

    private void OnEnable() => health.OnDamaged += HandleDamaged;

    private void OnDisable()
    {
        health.OnDamaged -= HandleDamaged;
        RestoreColors();
    }

    private void HandleDamaged(DamageInfo info)
    {
        flashRemaining = flashDuration;
        // 연속 피격은 표시 시간을 연장하고, 같은 색을 다시 쓰지는 않는다.
        if (!flashActive)
        {
            ApplyFlash();
            flashActive = true;
        }
        // 유닛의 자식으로 만들지 않아 적이 풀에 돌아가도 숫자와 타격 이펙트는 끝까지 재생된다.
        CombatFeedbackPool.Show(transform.position + hitOffset, info, damageColor, damageFont);
    }

    private void LateUpdate()
    {
        if (!flashActive) return;
        flashRemaining -= Time.deltaTime;
        if (flashRemaining <= 0f) RestoreColors();
    }

    private void ApplyFlash()
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null) continue;
            Color color = flashColor;
            color.a = originalColors[i].a;
            sprites[i].color = color;
        }
    }

    private void RestoreColors()
    {
        flashRemaining = 0f;
        if (!flashActive) return;
        flashActive = false;
        if (sprites == null) return;
        for (int i = 0; i < sprites.Length; i++)
            if (sprites[i] != null) sprites[i].color = originalColors[i];
    }
}
