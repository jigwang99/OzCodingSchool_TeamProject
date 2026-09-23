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
        }
        private void SaveGameData()
        {
            if (SaveManager.instance != null)
            {
                // 인수를 넣지 않고 Save() 호출
                SaveManager.instance.Save();

            }

        }
        /// <summary>
        /// 아이템 추가 (뽑기 후 호출)
        /// </summary>
        /// <param name="itemId">아이템 ID</param>
        /// <param name="count">개수 (기본값 1)</param>
        public void AddItem(string itemId, int count = 1)
        {
            AddItem(itemId, count, saveImmediately: true);

        }

        // DrawGacha defers disk writes until the entire result has been awarded.
        internal void AddItem(string itemId, int count, bool saveImmediately)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0)
            {
                return;
            }

            if (!_items.ContainsKey(itemId))
            {
                _items[itemId] = 0;
            }

            int previousCount = _items[itemId];
            _items[itemId] += count;

            if (previousCount == 0)
            {
                _newItems.Add(itemId);
            }

            // 전투 무기 목록에 반영
            if (previousCount == 0) SyncWeaponToPlayerData(itemId, 1);
            // 가챠 가구 목록에 반영
            SyncFurnitureToPlayerData(itemId, count);
            // 가챠 인벤토리 전체를 PlayerData에 직접 반영
            SyncInventoryToPlayerData();
            // 가챠 레시피 목록에 반영
            SyncRecipeToPlayerData(itemId, count);
            // UI 등에 변경 사실 알림
            OnInventoryChanged?.Invoke();

            // PlayerData 반영이 끝난 뒤 실제 저장
            if (saveImmediately)
                SaveGameData();
        }


        public int TryMergeAllWeapons(
    GachaPoolData pool,
    PlayerWeaponCatalog catalog)
        {
            if (pool == null || pool.Items == null || catalog == null)
            {
                Debug.LogWarning("[MergeAll] 무기 풀 또는 카탈로그가 없습니다.");
                return 0;
            }

            int merged = 0;
            bool changed;

            do
            {
                changed = false;

                foreach (GachaItem item in pool.Items)
                {
                    if (item == null || item.Group != GachaGroup.Weapon)
                        continue;

                    if (!CanMergeWeapon(item, pool, catalog))
                        continue;

                    if (!TryMergeWeapon(item, pool, catalog))
                        continue;

                    merged++;
                    changed = true;

                    // 합치기로 상위 단계 무기가 새로 생겼으므로 처음부터 재검사
                    break;
                }
            }
            while (changed);

            // 다른 무기가 합쳐졌더라도 Weapon_15의 상태를 출력
            foreach (GachaItem item in pool.Items)
            {
                if (item == null || item.ItemId != "Weapon_15")
                    continue;

                int count = GetItemCount(item.ItemId);

                if (count >= GachaWeaponMerge.RequiredCount)
                {
                    Debug.LogWarning(
                        $"[MergeAll] Weapon_15 합치기 확인: " +
                        $"보유 수량={count}, " +
                        $"NextMergeItemId={item.NextMergeItemId}, " +
                        $"LinkedWeaponId={item.LinkedWeaponId}, " +
                        $"합치기 가능={CanMergeWeapon(item, pool, catalog)}"
                    );
                }
            }
            DiagnoseWeapon15(pool, catalog);
            return merged;
        }
        public bool HasMergeableWeapon(GachaPoolData pool, PlayerWeaponCatalog catalog)
        {
            if (pool == null || pool.Items == null || catalog == null) return false;
            foreach (GachaItem item in pool.Items)
                if (CanMergeWeapon(item, pool, catalog)) return true;
            return false;
        }

        private bool CanMergeWeapon(GachaItem source, GachaPoolData pool, PlayerWeaponCatalog catalog)
        {
            if (source == null || pool == null || catalog == null ||
                GameManager.instance == null || GameManager.instance.PlayerData == null) return false;
            GachaItem result;
            if (!GachaWeaponMerge.TryFindMergeResult(source, this, pool, out result)) return false;
            if (result == null || source.ItemId == result.ItemId ||
                string.IsNullOrWhiteSpace(source.LinkedWeaponId) ||
                string.IsNullOrWhiteSpace(result.LinkedWeaponId) ||
                source.LinkedWeaponId == result.LinkedWeaponId) return false;
            var sourceWeapon = catalog.Find(source.LinkedWeaponId);
            var resultWeapon = catalog.Find(result.LinkedWeaponId);
            if (sourceWeapon == null || resultWeapon == null ||
                !GameManager.instance.PlayerData.OwnsWeapon(source.LinkedWeaponId)) return false;
            int remaining = GetItemCount(source.ItemId) - GachaWeaponMerge.RequiredCount;
            if (remaining < 0 || (remaining == 0 && source.LinkedWeaponId == "swords_0")) return false;
            return GetItemCount(result.ItemId) < int.MaxValue;
        }
       private void DiagnoseWeapon15(
    GachaPoolData pool,
    PlayerWeaponCatalog catalog)
        {
            if (pool == null || pool.Items == null)
                return;

            GachaItem source = pool.Items.Find(
                item => item != null && item.ItemId == "Weapon_15"
            );

            if (source == null)
            {
                Debug.LogWarning("[MergeAll] Weapon_15가 무기 풀에 없습니다.");
                return;
            }

            int count = GetItemCount(source.ItemId);

            if (count < GachaWeaponMerge.RequiredCount)
                return;

            GachaItem result;

            if (!GachaWeaponMerge.TryFindMergeResult(
                    source, this, pool, out result))
            {
                Debug.LogWarning(
                    $"[MergeAll] Weapon_15: 다음 단계 가챠 아이템을 찾지 못했습니다. " +
                    $"NextMergeItemId={source.NextMergeItemId}, " +
                    $"보유 수량={count}"
                );
                return;
            }

            if (catalog == null ||
                catalog.Find(source.LinkedWeaponId) == null ||
                catalog.Find(result.LinkedWeaponId) == null)
            {
                Debug.LogWarning(
                    $"[MergeAll] Weapon_15: 전투 무기 카탈로그 연결을 확인하세요. " +
                    $"현재 무기={source.LinkedWeaponId}, " +
                    $"결과 무기={result.LinkedWeaponId}"
                );
                return;
            }

            if (GameManager.instance == null ||
                GameManager.instance.PlayerData == null)
            {
                Debug.LogWarning(
                    "[MergeAll] Weapon_15: PlayerData를 찾을 수 없습니다."
                );
                return;
            }

            if (!GameManager.instance.PlayerData.OwnsWeapon(
                    source.LinkedWeaponId))
            {
                Debug.LogWarning(
                    $"[MergeAll] Weapon_15: 가챠 인벤토리에는 {count}개 있지만 " +
                    $"전투 무기 목록에는 {source.LinkedWeaponId}가 등록되지 않았습니다."
                );
                return;
            }

            Debug.LogWarning(
                $"[MergeAll] Weapon_15: 기본 조건은 통과했지만 합치기가 실패했습니다. " +
                $"현재 무기={source.LinkedWeaponId}, " +
                $"결과 무기={result.LinkedWeaponId}"
            );
        }
        // All preconditions are checked before any gacha material is consumed.
        public bool TryMergeWeapon(GachaItem source, GachaPoolData pool, PlayerWeaponCatalog catalog)
        {
            if (!CanMergeWeapon(source, pool, catalog)) return false;
            GachaItem result;
            if (!GachaWeaponMerge.TryFindMergeResult(source, this, pool, out result)) return false;
            PlayerData data = GameManager.instance.PlayerData;
            int remaining = GetItemCount(source.ItemId) - GachaWeaponMerge.RequiredCount;
            bool keepSource = remaining > 0;
            int resultCount = GetItemCount(result.ItemId);

            // PlayerData mutation returns false without changing data on failed validation.
            if (!data.TryApplyGachaWeaponMerge(source.LinkedWeaponId, result.LinkedWeaponId, keepSource))
                return false;

            if (remaining == 0) { _items.Remove(source.ItemId); _newItems.Remove(source.ItemId); }
            else _items[source.ItemId] = remaining;
            _items[result.ItemId] = resultCount + 1;
            if (resultCount == 0) _newItems.Add(result.ItemId);
            SyncInventoryToPlayerData();
            // Reapply sprite and attack damage when the equipped weapon was replaced.
            var equipment = FindObjectOfType<PlayerWeaponEquipment>();
            if (equipment != null) equipment.RestoreEquipment();
            OnInventoryChanged?.Invoke();
            SaveGameData();
            return true;
        }

        private void SyncInventoryToPlayerData()
        {
            if (GameManager.instance == null)
            {

                return;
            }

            PlayerData playerData = GameManager.instance.PlayerData;

            if (playerData == null)
            {

                return;
            }

            if (playerData.gachaInventory == null)
            {
                playerData.gachaInventory =
                    new GachaInventoryData();
            }

            if (playerData.gachaInventory.items == null)
            {
                playerData.gachaInventory.items =
                    new List<GachaOwnedItemData>();
            }

            playerData.gachaInventory.items.Clear();

            foreach (var pair in _items)
            {
                playerData.gachaInventory.items.Add(
                    new GachaOwnedItemData
                    {
                        itemId = pair.Key,
                        count = pair.Value,
                        isNew = _newItems.Contains(pair.Key)
                    }
                );
            }
        }
        // [추가할 헬퍼 함수] GachaInventory 클래스 내부에 아래 함수를 통째로 붙여넣어 줘
        private void SyncWeaponToPlayerData(string gachaItemId, int count)
        {
            if (GameManager.instance == null || GameManager.instance.PlayerData == null) return;

            // 프로젝트 내에 있는 모든 GachaPoolData 에셋을 찾아 검사합니다.
            var allPools = Resources.FindObjectsOfTypeAll<GachaPoolData>();
            foreach (var poolData in allPools)
            {
                if (poolData == null || poolData.Items == null) continue;

                var gachaItem = poolData.Items.Find(i => i != null && i.ItemId == gachaItemId);
                if (gachaItem != null)
                {
                    if (gachaItem.Group == GachaGroup.Weapon && !string.IsNullOrEmpty(gachaItem.LinkedWeaponId))
                    {
                        if (!GameManager.instance.PlayerData.OwnsWeapon(gachaItem.LinkedWeaponId))
                            GameManager.instance.PlayerData.AddWeapon(gachaItem.LinkedWeaponId, 1);

                    }
                    return;
                }

            }

        }
        private void SyncFurnitureToPlayerData(string itemId, int count)
        {
            if (GameManager.instance == null ||
                GameManager.instance.PlayerData == null)
                return;

            PlayerData playerData = GameManager.instance.PlayerData;

            int furnitureIndex = -1;

            switch (itemId)
            {
                case "furniture_0":
                    furnitureIndex = 0;
                    break;

                case "furniture_1":
                    furnitureIndex = 1;
                    break;

                case "furniture_2":
                    furnitureIndex = 2;
                    break;

                case "furniture_3":
                    furnitureIndex = 3;
                    break;

                case "furniture_4":
                    furnitureIndex = 4;
                    break;

                case "furniture_5":
                    furnitureIndex = 5;
                    break;
            }

            if (furnitureIndex < 0)
                return;

            playerData.MachineCount[furnitureIndex] += count;
        }
        private void SyncRecipeToPlayerData(
    string gachaItemId,
    int count)
        {
            if (GameManager.instance == null ||
                GameManager.instance.PlayerData == null)
                return;

            if (string.IsNullOrEmpty(gachaItemId) ||
                count <= 0)
                return;

            var allPools =
                Resources.FindObjectsOfTypeAll<GachaPoolData>();

            foreach (var poolData in allPools)
            {
                if (poolData == null ||
                    poolData.Items == null)
                    continue;

                var gachaItem = poolData.Items.Find(
                    item => item != null &&
                            item.ItemId == gachaItemId &&
                            item.Group == GachaGroup.Recipe
                );

                if (gachaItem == null)
                    continue;

                if (string.IsNullOrWhiteSpace(
                    gachaItem.LinkedRecipeId))
                {



                    return;
                }
                PlayerData playerData =
                    GameManager.instance.PlayerData;

                string recipeId = gachaItem.LinkedRecipeId;


                // 레시피 목록이 없으면 초기화
                if (playerData.ownedRecipes == null)
                {
                    playerData.ownedRecipes =
                        new List<OwnedRecipeData>();
                }

                // 이미 보유하고 있는 레시피인지 확인
                bool alreadyOwned = playerData.ownedRecipes.Exists(
                    recipe => recipe != null &&
                              recipe.recipeId == recipeId

                );

                // 처음 획득한 레시피만 추가
                if (!alreadyOwned)



                    playerData.ownedRecipes.Add(
                        new OwnedRecipeData
                        {
                            recipeId = recipeId,
                            count = 1
                        }
                    );
            }


            return;
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



        public void LoadFromPlayerData()
        {
            if (GameManager.instance == null)
            {

                return;
            }

            PlayerData playerData =
                GameManager.instance.PlayerData;

            if (playerData == null)
            {

                return;
            }

            if (playerData.gachaInventory == null)
            {

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


        }
    }
}
