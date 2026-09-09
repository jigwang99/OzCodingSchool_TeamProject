using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// 씬 수명의 풀. 간단한 UI 도형으로 타격 스파크를 만들고 월드 공간에 숫자를 표시한다.
public class CombatFeedbackPool : MonoBehaviour
{
    private const int MaxPopups = 32;
    private static CombatFeedbackPool instance;
    private readonly List<Popup> active = new List<Popup>();
    private readonly Stack<Popup> idle = new Stack<Popup>();

    public static void Show(Vector3 position, DamageInfo damage, Color color, TMP_FontAsset font)
    {
        if (instance == null)
            instance = new GameObject("CombatFeedbackPool").AddComponent<CombatFeedbackPool>();
        instance.Spawn(position, damage, color, font);
    }

    private void Spawn(Vector3 position, DamageInfo damage, Color color, TMP_FontAsset font)
    {
        Popup popup;
        if (idle.Count > 0) popup = idle.Pop();
        else if (active.Count < MaxPopups) popup = new Popup(transform);
        else
        {
            popup = active[0];
            active.RemoveAt(0);
        }
        popup.Play(position, damage, color, font);
        active.Add(popup);
    }

    private void LateUpdate()
    {
        for (int i = active.Count - 1; i >= 0; i--)
        {
            if (active[i].Tick(Time.deltaTime)) continue;
            active[i].Root.SetActive(false);
            idle.Push(active[i]);
            active.RemoveAt(i);
        }
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    private sealed class Popup
    {
        private const float Lifetime = 0.8f;
        private const float SparkLifetime = 0.16f;
        public readonly GameObject Root;
        private readonly RectTransform rootTransform;
        private readonly TextMeshProUGUI label;
        private readonly Image[] sparks = new Image[6];
        private readonly Vector2[] directions = new Vector2[6];
        private Color textColor;
        private float elapsed;

        public Popup(Transform parent)
        {
            Root = new GameObject("HitPopup", typeof(RectTransform), typeof(Canvas));
            rootTransform = (RectTransform)Root.transform;
            rootTransform.SetParent(parent, false);
            rootTransform.localScale = Vector3.one * 0.01f;
            rootTransform.sizeDelta = new Vector2(200f, 160f);
            Canvas canvas = Root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 200;

            var labelObject = new GameObject("Damage", typeof(RectTransform), typeof(TextMeshProUGUI));
            label = labelObject.GetComponent<TextMeshProUGUI>();
            label.rectTransform.SetParent(rootTransform, false);
            label.rectTransform.sizeDelta = new Vector2(240f, 70f);
            label.fontSize = 34f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;

            for (int i = 0; i < sparks.Length; i++)
            {
                var spark = new GameObject("Spark", typeof(RectTransform), typeof(Image));
                sparks[i] = spark.GetComponent<Image>();
                sparks[i].raycastTarget = false;
                RectTransform rect = sparks[i].rectTransform;
                rect.SetParent(rootTransform, false);
                rect.pivot = new Vector2(0.5f, 0f);
                float angle = i * 60f + 15f;
                float radians = angle * Mathf.Deg2Rad;
                directions[i] = new Vector2(-Mathf.Sin(radians), Mathf.Cos(radians));
                rect.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        public void Play(Vector3 position, DamageInfo damage, Color color, TMP_FontAsset font)
        {
            elapsed = 0f;
            rootTransform.position = position;
            Camera camera = Camera.main;
            rootTransform.rotation = camera != null ? camera.transform.rotation : Quaternion.identity;
            textColor = damage.IsCritical ? new Color(1f, 0.75f, 0.15f) : color;
            label.font = font != null ? font : TMP_Settings.defaultFontAsset;
            label.text = damage.Damage.ToString("0.##");
            label.color = textColor;
            Root.SetActive(true);
            Tick(0f);
        }

        public bool Tick(float deltaTime)
        {
            elapsed += deltaTime;
            float t = Mathf.Clamp01(elapsed / Lifetime);
            label.rectTransform.anchoredPosition = new Vector2(0f, 60f + 65f * t);
            label.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.25f, 1f, Mathf.Clamp01(t * 5f));
            Color fading = textColor;
            fading.a *= 1f - Mathf.InverseLerp(0.3f, 1f, t);
            label.color = fading;
            float sparkT = Mathf.Clamp01(elapsed / SparkLifetime);
            for (int i = 0; i < sparks.Length; i++)
            {
                sparks[i].rectTransform.anchoredPosition = directions[i] * Mathf.Lerp(5f, 32f, sparkT);
                sparks[i].rectTransform.sizeDelta = new Vector2(5f * (1f - sparkT), 25f * (1f - sparkT));
                sparks[i].color = new Color(1f, 0.85f, 0.4f, 1f - sparkT);
            }
            return elapsed < Lifetime;
        }
    }
}
