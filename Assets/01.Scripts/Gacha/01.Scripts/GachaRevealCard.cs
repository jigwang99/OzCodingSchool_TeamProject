using DG.Tweening;
using PixelRestaurant.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelRestaurant.Gacha
{
    public class GachaRevealCard : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        private enum CardState
        {
            Spawning,
            Waiting,
            Flipping,
            Revealed,
            Moving
        }

        [Header("Card References")]
        [SerializeField]
        private RectTransform cardVisual;

        [SerializeField]
        private GameObject back;

        [SerializeField]
        private GameObject front;

        [SerializeField]
        private Image hoverOutline;

        [Header("Result")]
        [SerializeField]
        private GachaResultItemDisplay resultDisplay;

        [Header("Spawn Animation")]
        [SerializeField]
        private float spawnDuration = 0.6f;

        [SerializeField]
        private float spawnStartScale = 0.6f;

        [SerializeField]
        private Ease spawnEase = Ease.OutBack;

        [Header("Flip Animation")]
        [SerializeField]
        private float flipDuration = 0.6f;

        [SerializeField]
        private Ease flipEase = Ease.InOutSine;

        [Header("Back Spawn VFX")]
        [SerializeField]
        private GameObject commonSpawnVFX;

        [SerializeField]
        private GameObject rareSpawnVFX;

        [SerializeField]
        private GameObject uniqueSpawnVFX;

        [SerializeField]
        private GameObject epicSpawnVFX;

        [Header("Result Reveal VFX")]
        [SerializeField]
        private GameObject commonRevealVFX;

        [SerializeField]
        private GameObject rareRevealVFX;

        [SerializeField]
        private GameObject uniqueRevealVFX;

        [SerializeField]
        private GameObject epicRevealVFX;

        [Header("Inventory")]
        [SerializeField]
        private RectTransform inventoryTarget;

        [SerializeField]
        private float inventoryMoveDuration = 0.5f;

        private GachaItem _item;
        private CardState _state;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        private Vector3 _originalScale;

        private Sequence _animation;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // 결과 UI 자동으로 찾기
            if (resultDisplay == null)
            {
                resultDisplay =
                    GetComponentInChildren<GachaResultItemDisplay>(true);
            }

            if (cardVisual != null)
            {
                _originalScale = cardVisual.localScale;
            }

            _state = CardState.Spawning;

            ResetCardVisual();
        }
        // 카드 초기화
        public void Setup(
     GachaItem item,
     RectTransform target)
        {
            StopCurrentAnimation();

            _item = item;
            inventoryTarget = target;

            _state = CardState.Spawning;

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _rectTransform.localRotation = Quaternion.identity;

            ResetCardVisual();
            DisableAllVFX();

            if (resultDisplay != null)
            {
                resultDisplay.ResetCard();
            }

            if (resultDisplay != null && item != null)
            {
                bool isNew =
                    GachaInventory.Instance != null &&
                    GachaInventory.Instance.IsNewItem(item.ItemId);

                resultDisplay.SetItemInfo(item, isNew);
            }
        }

        // Back 카드 등장
        public void PlaySpawnAnimation(
            Vector2 spawnPosition,
            Vector2 targetPosition)
        {
            if (_item == null ||
                cardVisual == null)
            {
                return;
            }

            StopCurrentAnimation();

            _state = CardState.Spawning;

            ResetCardVisual();
            DisableAllVFX();

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _rectTransform.anchoredPosition =
                spawnPosition;

            _rectTransform.localRotation =
                Quaternion.identity;

            cardVisual.localScale =
                _originalScale * spawnStartScale;

            // 등급에 맞는 Back 등장 이펙트
            PlaySpawnVFX(_item.Rarity);

            _animation = DOTween.Sequence();

            // 리스폰 지점에서 목표 위치로 이동
            _animation.Append(
                _rectTransform.DOAnchorPos(
                    targetPosition,
                    spawnDuration
                ).SetEase(spawnEase)
            );

            // 이동하면서 카드 크기 증가
            _animation.Join(
                cardVisual.DOScale(
                    _originalScale,
                    spawnDuration
                ).SetEase(Ease.OutBack)
            );

            _animation.OnComplete(() =>
            {
                _animation = null;

                //DisableSpawnVFX();

                _state = CardState.Waiting;

                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            });
        }

        // 카드 클릭
        public void OnCardClicked()
        {
            switch (_state)
            {
                case CardState.Waiting:
                    PlayFlipAnimation();
                    break;

                case CardState.Revealed:
                    MoveToInventory();
                    break;
            }
        }
        // 모두 뒤집기 버튼에서 호출
        public void RevealCard()
        {
            if (_state == CardState.Waiting)
            {
                PlayFlipAnimation();
            }
        }
        // 카드 Y축 뒤집기
        private void PlayFlipAnimation()
        {
            if (_item == null ||
                cardVisual == null)
            {
                return;
            }

            _state = CardState.Flipping;

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            DisableSpawnVFX();

            _animation = DOTween.Sequence();

            // Back: 0도 -> 90도
            _animation.Append(
                cardVisual.DOLocalRotate(
                    new Vector3(0f, 90f, 0f),
                    flipDuration * 0.5f
                ).SetEase(flipEase)
            );

            // 카드가 옆면을 향하는 순간 교체
            _animation.AppendCallback(() =>
            {
                if (back != null)
                    back.SetActive(false);

                if (front != null)
                {
                    front.transform.localRotation =
                        Quaternion.Euler(
                            0f,
                            180f,
                            0f
                        );

                    front.SetActive(true);
                }
            });

            // Front: 90도 -> 180도
            _animation.Append(
                cardVisual.DOLocalRotate(
                    new Vector3(0f, 180f, 0f),
                    flipDuration * 0.5f
                ).SetEase(flipEase)
            );

            _animation.OnComplete(() =>
            {
                _animation = null;

                _state = CardState.Revealed;

                PlayRevealVFX(_item.Rarity);

                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            });
        }

        // 등급별 Back 등장 이펙트
        private void PlaySpawnVFX(GachaRarity rarity)
        {

            DisableSpawnVFX();

            GameObject vfx = GetRarityVFX(
                rarity,
                commonSpawnVFX,
                rareSpawnVFX,
                uniqueSpawnVFX,
                epicSpawnVFX
            );

          

            PlayVFX(vfx);
        }

        // 등급별 결과 공개 이펙트
        private void PlayRevealVFX(
            GachaRarity rarity)
        {
            DisableRevealVFX();

            GameObject vfx = GetRarityVFX(
                rarity,
                commonRevealVFX,
                rareRevealVFX,
                uniqueRevealVFX,
                epicRevealVFX
            );

            PlayVFX(vfx);
        }

        private GameObject GetRarityVFX(
            GachaRarity rarity,
            GameObject common,
            GameObject rare,
            GameObject unique,
            GameObject epic)
        {
            switch (rarity)
            {
                case GachaRarity.Common:
                    return common;

                case GachaRarity.Rare:
                    return rare;

                case GachaRarity.Unique:
                    return unique;

                case GachaRarity.Epic:
                    return epic;

                default:
                    return null;
            }
        }

        private void PlayVFX(GameObject vfx)
        {
            if (vfx == null)
            {
              
            }

            vfx.SetActive(true);

            ParticleSystem[] particles =
                vfx.GetComponentsInChildren<ParticleSystem>(true);


            foreach (ParticleSystem particle in particles)
            {

                particle.Stop(true);
                particle.Play(true);
            }
        }

        private void DisableSpawnVFX()
        {
            SetVFXActive(commonSpawnVFX, false);
            SetVFXActive(rareSpawnVFX, false);
            SetVFXActive(uniqueSpawnVFX, false);
            SetVFXActive(epicSpawnVFX, false);
        }

        private void DisableRevealVFX()
        {
            SetVFXActive(commonRevealVFX, false);
            SetVFXActive(rareRevealVFX, false);
            SetVFXActive(uniqueRevealVFX, false);
            SetVFXActive(epicRevealVFX, false);
        }

        private void DisableAllVFX()
        {
            DisableSpawnVFX();
            DisableRevealVFX();
        }

        private void SetVFXActive(
            GameObject vfx,
            bool active)
        {
            if (vfx != null)
                vfx.SetActive(active);
        }

        // 카드 상태 초기화
        private void ResetCardVisual()
        {
            if (cardVisual != null)
            {
                cardVisual.localRotation =
                    Quaternion.identity;

                cardVisual.localScale =
                    _originalScale;
            }

            if (back != null)
                back.SetActive(true);

            if (front != null)
            {
                front.transform.localRotation =
                    Quaternion.Euler(
                        0f,
                        180f,
                        0f
                    );

                front.SetActive(false);
            }

            if (hoverOutline != null)
            {
                hoverOutline.DOKill();

                Color color = hoverOutline.color;
                color.a = 0f;

                hoverOutline.color = color;
            }
        }

        // 결과 카드 -> 인벤토리
        private void MoveToInventory()
        {
            if (_state != CardState.Revealed)
                return;

            _state = CardState.Moving;

            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonSound();
            }

            DisableAllVFX();

            if (inventoryTarget == null)
            {
                Disappear();
                return;
            }

            Vector3 targetLocalPosition =
                _rectTransform.parent.InverseTransformPoint(
                    inventoryTarget.position
                );

            _animation = DOTween.Sequence();

            _animation.Append(
                _rectTransform.DOLocalMove(
                    targetLocalPosition,
                    inventoryMoveDuration
                ).SetEase(Ease.InBack)
            );

            _animation.Join(
                cardVisual.DOScale(
                    _originalScale * 0.35f,
                    inventoryMoveDuration
                ).SetEase(Ease.InBack)
            );

            _animation.Join(
                _canvasGroup.DOFade(
                    0f,
                    inventoryMoveDuration
                )
            );

            _animation.OnComplete(() =>
            {
                _animation = null;
                Disappear();
            });
        }

        private void Disappear()
        {
            DisableAllVFX();

            gameObject.SetActive(false);

            GachaInventoryUI inventoryUI =
                FindObjectOfType<GachaInventoryUI>();

            if (inventoryUI != null)
            {
                inventoryUI.RefreshInventoryDisplay();
            }
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            if (_state != CardState.Waiting &&
                _state != CardState.Revealed)
            {
                return;
            }

            if (hoverOutline == null)
                return;

            hoverOutline.DOKill();

            hoverOutline.DOFade(
                1f,
                0.2f
            );
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            if (hoverOutline == null)
                return;

            hoverOutline.DOKill();

            hoverOutline.DOFade(
                0f,
                0.2f
            );
        }

        private void StopCurrentAnimation()
        {
            if (_animation != null)
            {
                _animation.Kill();
                _animation = null;
            }

            if (_rectTransform != null)
                _rectTransform.DOKill();

            if (cardVisual != null)
                cardVisual.DOKill();

            if (_canvasGroup != null)
                _canvasGroup.DOKill();
        }

        private void OnDisable()
        {
            StopCurrentAnimation();
            DisableAllVFX();
        }

        private void OnDestroy()
        {
            StopCurrentAnimation();
        }
    }
}