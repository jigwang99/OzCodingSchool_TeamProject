using DG.Tweening;
using PixelRestaurant.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelRestaurant.Gacha
{
    public class GachaRevealCard : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("Card")]
        [SerializeField] private Image cardImage;
        [SerializeField] private Image hoverOutline;

        [Header("Back / Front")]
        [SerializeField] private GameObject back;
        [SerializeField] private GameObject front;

        [Header("Result")]
        [SerializeField] private GachaResultItemDisplay resultDisplay;

        [Header("Eric VFX")]
        [SerializeField] private GameObject commonVFX;
        [SerializeField] private GameObject rareVFX;
        [SerializeField] private GameObject uniqueVFX;
        [SerializeField] private GameObject epicVFX;

        [Header("Move Target")]
        [SerializeField] private RectTransform inventoryTarget;

        private GachaItem _item;

        private bool _isRevealed;
        private bool _isMoving;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        private Vector2 _originalPosition;
        private Vector3 _originalScale;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (hoverOutline != null)
            {
                Color color = hoverOutline.color;
                color.a = 0f;
                hoverOutline.color = color;
            }

            if (front != null)
                front.SetActive(false);

            if (back != null)
                back.SetActive(true);
        }

        public void Setup(
            GachaItem item,
            RectTransform target)
        {
            _item = item;
            inventoryTarget = target;

            _isRevealed = false;
            _isMoving = false;

            _originalPosition = _rectTransform.anchoredPosition;
            _originalScale = _rectTransform.localScale;

            _canvasGroup.alpha = 1f;

            if (back != null)
                back.SetActive(true);

            if (front != null)
                front.SetActive(false);

            if (hoverOutline != null)
            {
                Color color = hoverOutline.color;
                color.a = 0f;
                hoverOutline.color = color;
            }

            if (resultDisplay != null)
            {
                bool isNew = GachaInventory.Instance != null &&
                             GachaInventory.Instance.IsNewItem(item.ItemId);

                resultDisplay.SetItemInfo(item, isNew);
            }

            DisableAllVFX();
        }

        /// <summary>
        /// 카드 클릭
        /// </summary>
        public void OnCardClicked()
        {
            if (_isMoving)
                return;

            if (!_isRevealed)
            {
                RevealCard();
            }
            else
            {
                MoveToInventory();
            }
        }

        /// <summary>
        /// 첫 번째 클릭 - 카드 공개
        /// </summary>
        private void RevealCard()
        {
            if (_item == null)
                return;

            _isRevealed = true;

            // 클릭한 순간 약간 커졌다가 원래 크기로
            transform.DOScale(
                _originalScale * 1.08f,
                0.12f
            )
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(
                    _originalScale,
                    0.18f
                )
                .SetEase(Ease.OutBack);
            });

            // 뒷면을 서서히 어둡게
            if (cardImage != null)
            {
                cardImage
                    .DOColor(new Color(0.05f, 0.05f, 0.05f, 0.3f), 0.55f)
                    .SetEase(Ease.InOutQuad);
            }

            // 앞면 표시
            if (front != null)
            {
                front.SetActive(true);

                CanvasGroup frontGroup =
                    front.GetComponent<CanvasGroup>();

                if (frontGroup == null)
                    frontGroup = front.AddComponent<CanvasGroup>();

                frontGroup.alpha = 0f;

                frontGroup
                    .DOFade(1f, 0.65f)
                    .SetEase(Ease.InOutQuad);
            }

            if (back != null)
            {
                CanvasGroup backGroup =
                    back.GetComponent<CanvasGroup>();

                if (backGroup == null)
                    backGroup = back.AddComponent<CanvasGroup>();

                backGroup
                    .DOFade(0f, 0.65f)
                    .SetEase(Ease.InOutQuad);
            }

            // 레어도별 VFX
            PlayRarityVFX(_item.Rarity);
        }

        /// <summary>
        /// 레어도에 따른 Eric VFX
        /// </summary>
        private void PlayRarityVFX(GachaRarity rarity)
        {
            DisableAllVFX();

            GameObject vfx = null;

            switch (rarity)
            {
                case GachaRarity.Common:
                    vfx = commonVFX;
                    break;

                case GachaRarity.Rare:
                    vfx = rareVFX;
                    break;

                case GachaRarity.Unique:
                    vfx = uniqueVFX;
                    break;

                case GachaRarity.Epic:
                    vfx = epicVFX;
                    break;
            }

            if (vfx == null)
                return;

            vfx.SetActive(true);

            ParticleSystem[] particles =
                vfx.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem particle in particles)
            {
                particle.Stop(true);
                particle.Play(true);
            }
        }

        /// <summary>
        /// 두 번째 클릭 - 인벤토리로 이동
        /// </summary>
        private void MoveToInventory()
        {
            if (_isMoving)
                return;

            _isMoving = true;

            // 버튼 클릭 효과음
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonSound();
            }

            if (inventoryTarget == null)
            {
                Debug.LogWarning(
                    "[GachaRevealCard] inventoryTarget이 없습니다."
                );

                Disappear();
                return;
            }

            // 현재 카드 위치에서 인벤토리 버튼 위치로 이동
            Vector3 targetWorldPosition =
                inventoryTarget.position;

            Vector3 targetLocalPosition =
                _rectTransform.parent.InverseTransformPoint(
                    targetWorldPosition
                );

            DG.Tweening.Sequence sequence = DOTween.Sequence();

            // 카드 축소
            sequence.Append(
                transform.DOScale(
                    _originalScale * 0.35f,
                    0.35f
                )
                .SetEase(Ease.InBack)
            );

            // 인벤토리 방향으로 이동
            sequence.Join(
                _rectTransform.DOAnchorPos(
                    targetLocalPosition,
                    0.5f
                )
                .SetEase(Ease.InBack)
            );

            // 살짝 회전
            sequence.Join(
                transform.DORotate(
                    new Vector3(0, 0, 15f),
                    0.5f
                )
            );

            // 마지막에 사라짐
            sequence.Join(
                _canvasGroup.DOFade(
                    0f,
                    0.4f
                )
            );

            sequence.OnComplete(() =>
            {
                Disappear();
            });
        }

        private void Disappear()
        {
            DisableAllVFX();

            gameObject.SetActive(false);

            // 인벤토리 갱신
            GachaInventoryUI inventoryUI =
                FindObjectOfType<GachaInventoryUI>();

            if (inventoryUI != null)
            {
                inventoryUI.RefreshInventoryDisplay();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_isMoving || hoverOutline == null)
                return;

            hoverOutline
                .DOFade(1f, 0.2f)
                .SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_isMoving || hoverOutline == null)
                return;

            hoverOutline
                .DOFade(0f, 0.2f)
                .SetEase(Ease.OutQuad);
        }

        private void DisableAllVFX()
        {
            if (commonVFX != null)
                commonVFX.SetActive(false);

            if (rareVFX != null)
                rareVFX.SetActive(false);

            if (uniqueVFX != null)
                uniqueVFX.SetActive(false);

            if (epicVFX != null)
                epicVFX.SetActive(false);
        }
    }
}