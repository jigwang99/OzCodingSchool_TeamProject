using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 씬 수명의 전용 Canvas 하나에서 타격 숫자와 스파크를 재사용한다.
[RequireComponent(typeof(Canvas))]
public class CombatFeedbackPool : MonoBehaviour
{
    private const int MaxPopups = 16;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private TMP_FontAsset prewarmFont;
    [SerializeField, Min(1)] private int prewarmPerFrame = 4;
    private static CombatFeedbackPool instance;
    private readonly List<Popup> active = new List<Popup>(MaxPopups);
    private readonly Stack<Popup> idle = new Stack<Popup>(MaxPopups);
    private Canvas feedbackCanvas;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        if (targetCamera == null) targetCamera = Camera.main;
        if (prewarmFont == null) prewarmFont = TMP_Settings.defaultFontAsset;
        feedbackCanvas = GetComponent<Canvas>();
        feedbackCanvas.renderMode = RenderMode.WorldSpace;
        feedbackCanvas.worldCamera = targetCamera;
        feedbackCanvas.sortingOrder = 200;
        transform.localScale = Vector3.one * 0.01f;
        feedbackCanvas.enabled = false;
    }

    private static CombatFeedbackPool GetOrCreate()
    {
        if (instance == null)
            new GameObject("CombatFeedbackCanvas", typeof(RectTransform), typeof(Canvas), typeof(CombatFeedbackPool));
        return instance;
    }

    // StageManager가 검은 화면에서 호출한다. 재도전에는 기존 풀을 그대로 사용한다.
    public static async UniTask PrepareForStageAsync(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        CombatFeedbackPool pool = GetOrCreate();
        pool.ClearPopups();
        int batch = 0;
        while (pool.idle.Count < MaxPopups)
        {
            token.ThrowIfCancellationRequested();
            pool.idle.Push(new Popup(pool.transform, pool.prewarmFont));
            if (++batch >= Mathf.Max(1, pool.prewarmPerFrame))
            {
                batch = 0;
                await UniTask.NextFrame(token);
            }
        }
    }

    public static void Show(Vector3 position, DamageInfo damage, Color color, TMP_FontAsset font)
    {
        CombatFeedbackPool pool = GetOrCreate();
        if (pool.isActiveAndEnabled) pool.Spawn(position, damage, color, font);
    }

    private void Spawn(Vector3 position, DamageInfo damage, Color color, TMP_FontAsset font)
    {
        Popup popup;
        if (idle.Count > 0) popup = idle.Pop();
        // 전투씬 밖에서 단독으로 사용되는 경우의 초기화 경로.
        else if (active.Count < MaxPopups) popup = new Popup(transform, font != null ? font : prewarmFont);
        else
        {
            popup = active[0];
            active.RemoveAt(0);
        }
        Quaternion rotation = targetCamera != null ? targetCamera.transform.rotation : Quaternion.identity;
        popup.Play(position, rotation, damage, color, font != null ? font : prewarmFont);
        active.Add(popup);
        feedbackCanvas.enabled = true;
    }

    private void LateUpdate()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f) return;
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (active[i].Tick(deltaTime)) continue;
            active[i].Root.SetActive(false);
            idle.Push(active[i]);
            active.RemoveAt(i);
        }
        if (active.Count == 0 && feedbackCanvas.enabled) feedbackCanvas.enabled = false;
    }

    private void ClearPopups()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            active[i].Root.SetActive(false);
            idle.Push(active[i]);
        }
        active.Clear();
        if (feedbackCanvas != null) feedbackCanvas.enabled = false;
    }

    private void OnDisable() => ClearPopups();

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private sealed class Popup
    {
        private const float Lifetime = 0.8f;
        private const float SparkLifetime = 0.16f;
        private const float FadeStart = 0.24f;
        public readonly GameObject Root;
        private readonly RectTransform rootTransform;
        private readonly TextMeshProUGUI label;
        private readonly RectTransform labelTransform;
        private readonly CombatSparkGraphic sparks;
        private readonly char[] damageText = new char[64];
        private Color textColor;
        private float elapsed;
        private bool sparksVisible;
        private bool scaleSettled;

        public Popup(Transform parent, TMP_FontAsset font)
        {
            // Canvas는 공용 부모에만 두고, 각 팝업에는 RectTransform만 둔다.
            Root = new GameObject("HitPopup", typeof(RectTransform));
            rootTransform = (RectTransform)Root.transform;
            rootTransform.SetParent(parent, false);
            rootTransform.sizeDelta = new Vector2(200f, 160f);

            var labelObject = new GameObject("Damage", typeof(RectTransform), typeof(TextMeshProUGUI));
            label = labelObject.GetComponent<TextMeshProUGUI>();
            labelTransform = label.rectTransform;
            labelTransform.SetParent(rootTransform, false);
            labelTransform.sizeDelta = new Vector2(240f, 70f);
            label.fontSize = 34f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            if (font != null) label.font = font;

            var sparkObject = new GameObject("Sparks", typeof(RectTransform), typeof(CombatSparkGraphic));
            sparks = sparkObject.GetComponent<CombatSparkGraphic>();
            sparks.rectTransform.SetParent(rootTransform, false);
            sparks.rectTransform.sizeDelta = new Vector2(100f, 100f);
            sparks.raycastTarget = false;
            sparks.color = new Color(1f, 0.85f, 0.4f);
            // 첫 숫자 표시 때 폰트와 메시 준비가 몰리지 않도록 미리 갱신한다.
            label.SetText("0123456789.,-+");
            label.ForceMeshUpdate(true, true);
            Root.SetActive(false);
        }

        public void Play(Vector3 position, Quaternion rotation, DamageInfo damage, Color color, TMP_FontAsset font)
        {
            elapsed = 0f;
            sparksVisible = true;
            scaleSettled = false;
            rootTransform.SetPositionAndRotation(position, rotation);
            textColor = damage.IsCritical ? new Color(1f, 0.75f, 0.15f) : color;
            if (font != null && label.font != font) label.font = font;
            // 기존 0.## 반올림/문화권 표기를 유지하면서 버퍼를 재사용한다.
            if (damage.Damage.TryFormat(damageText.AsSpan(), out int length, "0.##", CultureInfo.CurrentCulture))
                label.SetCharArray(damageText, 0, length);
            else
                label.text = damage.Damage.ToString("0.##");
            label.color = textColor;
            labelTransform.anchoredPosition = new Vector2(0f, 60f);
            labelTransform.localScale = Vector3.one * 1.25f;
            sparks.SetProgress(0f);
            sparks.enabled = true;
            Root.SetActive(true);
        }

        public bool Tick(float deltaTime)
        {
            elapsed += deltaTime;
            if (elapsed >= Lifetime) return false;
            float t = elapsed / Lifetime;
            labelTransform.anchoredPosition = new Vector2(0f, 60f + 65f * t);
            if (!scaleSettled)
            {
                labelTransform.localScale = Vector3.one * Mathf.Lerp(1.25f, 1f, Mathf.Clamp01(elapsed / SparkLifetime));
                scaleSettled = elapsed >= SparkLifetime;
            }
            if (elapsed > FadeStart)
            {
                Color fading = textColor;
                fading.a *= 1f - Mathf.InverseLerp(FadeStart, Lifetime, elapsed);
                label.color = fading;
            }
            if (sparksVisible)
            {
                if (elapsed < SparkLifetime) sparks.SetProgress(elapsed / SparkLifetime);
                else
                {
                    sparks.enabled = false;
                    sparksVisible = false;
                }
            }
            return true;
        }

    }
}
