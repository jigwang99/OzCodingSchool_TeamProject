using System.Collections.Generic;
using UnityEngine;
using PixelRestaurant.Data;
using System;
namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 플레이어가 보유한 아이템 목록 관리
    /// 중복 개수 추적, NEW 표시 관리
    /// SaveManager에서 저장될 데이터
    /// </summary>
    public class GachaInventory : MonoBehaviour
    {
        public event Action OnInventoryChanged;

        private static GachaInventory _instance;

        [SerializeField]
        private Dictionary<string, int> _items = new Dictionary<string, int>();
        // itemId → count
        // 예: "weapon_001" → 3

        [SerializeField]
        private HashSet<string> _newItems = new HashSet<string>();
        // NEW 표시할 itemId들

        public static GachaInventory Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GachaInventory>();
                    if (_instance == null)
                    {
                        Debug.LogError("[가챠 인벤토리] GachaInventory를 찾을 수 없습니다!");
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // 초기화
            if (_items == null)
                _items = new Dictionary<string, int>();

            if (_newItems == null)
                _newItems = new HashSet<string>();

            Debug.Log("[가챠 인벤토리] 초기화 완료");
        }

        /// <summary>
        /// 아이템 추가 (뽑기 후 호출)
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="count">개수 (기본값 1)</param>
        public void AddItem(string itemId, int count = 1)
        {
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogError("[가챠 인벤토리] itemId가 비어있습니다!");
                return;
            }

            if (!_items.ContainsKey(itemId))
                _items[itemId] = 0;

            int previousCount = _items[itemId];
            _items[itemId] += count;

            // 첫 획득 시 NEW에 추가
            if (previousCount == 0)
                _newItems.Add(itemId);

            OnInventoryChanged?.Invoke();

            Debug.Log(
                $"[가챠 인벤토리] {itemId} 획득 → 총 {_items[itemId]}개"
            );
        }

        /// <summary>
        /// 보유 개수 조회
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <returns>보유 개수 (없으면 0)</returns>
        public int GetItemCount(string itemId)
        {
            return _items.ContainsKey(itemId) ? _items[itemId] : 0;
        }

        /// <summary>
        /// NEW 표시 여부 확인
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <returns>NEW 표시 여부</returns>
        public bool IsNewItem(string itemId)
        {
            return _newItems.Contains(itemId);
        }

        /// <summary>
        /// NEW 표시 제거 (인벤토리 열 때)
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        public void MarkAsViewed(string itemId)
        {
            if (_newItems.Remove(itemId))
            {
                OnInventoryChanged?.Invoke();

                Debug.Log(
                    $"[가챠 인벤토리] {itemId} NEW 표시 제거"
                );
            }
        }

        /// <summary>
        /// 특정 그룹의 아이템 ID 리스트 조회
        /// </summary>
        /// <param name="group">그룹</param>
        /// <param name="poolData">풀 데이터 (그룹 내 아이템 정보용)</param>
        /// <returns>해당 그룹의 itemId 리스트</returns>
        public List<string> GetItemsInGroup(
      GachaGroup group,
      GachaPoolData poolData)
        {
            var result = new List<string>();

            if (poolData == null)
            {
                Debug.LogError(
                    "[가챠 인벤토리] poolData가 null입니다."
                );

                return result;
            }

            foreach (var itemId in _items.Keys)
            {
                GachaItem item =
                    poolData.Items.Find(
                        i => i.ItemId == itemId
                    );

                // 현재 Pool에 존재하면 해당 그룹 아이템으로 취급
                if (item != null)
                {
                    result.Add(itemId);
                }
            }

            return result;
        }

        /// <summary>
        /// 특정 레어리티의 아���템 조회
        /// </summary>
        /// <param name="rarity">레어리티</param>
        /// <param name="poolData">풀 데이터</param>
        /// <returns>(itemId, count) 튜플 리스트</returns>
        public List<(string itemId, int count)> GetItemsByRarity(GachaRarity rarity, GachaPoolData poolData)
        {
            var result = new List<(string, int)>();

            foreach (var itemId in _items.Keys)
            {
                var item = poolData.Items.Find(i => i.ItemId == itemId);
                if (item != null && item.Rarity == rarity)
                {
                    result.Add((itemId, _items[itemId]));
                }
            }

            return result;
        }

        /// <summary>
        /// 모든 아이템 조회
        /// </summary>
        /// <returns>itemId와 개수의 딕셔너리</returns>
        public Dictionary<string, int> GetAllItems()
        {
            return new Dictionary<string, int>(_items);
        }

        /// <summary>
        /// 특정 아이템이 보유되어 있는지 확인
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <returns>보유 여부</returns>
        public bool HasItem(string itemId)
        {
            return _items.ContainsKey(itemId) && _items[itemId] > 0;
        }

        /// <summary>
        /// 인벤토리 전체 출력 (디버그용)
        /// </summary>
        public void DebugPrintAllItems()
        {
            Debug.Log("=== [가챠 인벤토리] ===");
            foreach (var kvp in _items)
            {
                string newTag = _newItems.Contains(kvp.Key) ? "[NEW]" : "";
                Debug.Log($"{kvp.Key} x{kvp.Value} {newTag}");
            }
            Debug.Log($"총 종류: {_items.Count}");
        }

        /// <summary>
        /// 인벤토리 리셋 (테스트용)
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            _newItems.Clear();

            OnInventoryChanged?.Invoke();

            Debug.Log("[가챠 인벤토리] 초기화됨");
        }
        public void LoadFromPlayerData()
        {
            if (GameManager.instance == null)
            {
                Debug.LogError(
                    "[가챠 인벤토리] GameManager를 찾을 수 없습니다."
                );
                return;
            }

            PlayerData playerData =
                GameManager.instance.PlayerData;

            if (playerData == null)
            {
                Debug.LogError(
                    "[가챠 인벤토리] PlayerData가 없습니다."
                );
                return;
            }

            if (playerData.gachaInventory == null)
            {
                Debug.Log(
                    "[가챠 인벤토리] 저장된 가챠 데이터가 없습니다."
                );
                return;
            }

            _items.Clear();
            _newItems.Clear();

            foreach (
                GachaOwnedItemData data
                in playerData.gachaInventory.items)
            {
                if (data == null ||
                    string.IsNullOrEmpty(data.itemId) ||
                    data.count <= 0)
                {
                    continue;
                }

                _items[data.itemId] = data.count;

                if (data.isNew)
                {
                    _newItems.Add(data.itemId);
                }
            }

            Debug.Log(
                $"[가챠 인벤토리] 저장 데이터 복원 완료: " +
                $"{_items.Count}종"
            );
        }
    }
}
