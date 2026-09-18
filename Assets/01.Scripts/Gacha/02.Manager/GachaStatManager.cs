using PixelRestaurant.Data;
using System.Collections.Generic;
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    /// <summary>
    /// 가챠 아이템으로 발생하는 공용 스탯 관리
    ///
    /// Weapon    : 여기서 직접 처리하지 않음
    /// Furniture : GoldBonusFlat 누적
    /// Recipe    : GoldBonusRate 누적
    ///
    /// 특수능력은 기본 스탯 계산이 끝난 뒤
    /// 가장 마지막 단계에서 확장한다.
    /// </summary>
    public class GachaStatManager : MonoBehaviour
    {
        private static GachaStatManager _instance;

        public static GachaStatManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance =
                        FindObjectOfType<GachaStatManager>();

                    if (_instance == null)
                    {
                       
                    }
                }

                return _instance;
            }
        }

        // ============================================
        // 현재 누적 스탯
        // ============================================

        private int _furnitureGoldBonusFlat;
        private float _recipeGoldBonusRate;

        public int FurnitureGoldBonusFlat =>
            _furnitureGoldBonusFlat;

        public float RecipeGoldBonusRate =>
            _recipeGoldBonusRate;

        // ============================================
        // Unity
        // ============================================

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

        // ============================================
        // 가챠 결과 적용
        // ============================================

        /// <summary>
        /// 가챠 결과에 포함된 가구/레시피 스탯을 적용한다.
        /// </summary>
        public void ApplyGachaResults(
            List<GachaItem> results)
        {
            if (results == null || results.Count == 0)
                return;

            foreach (GachaItem item in results)
            {
                if (item == null)
                    continue;

                if (item.Stats == null)
                    continue;

                switch (item.Group)
                {
                    case GachaGroup.Furniture:

                        _furnitureGoldBonusFlat +=
                            Mathf.Max(
                                0,
                                item.Stats.goldBonusFlat
                            );

                        break;

                    case GachaGroup.Recipe:

                        _recipeGoldBonusRate +=
                            Mathf.Max(
                                0f,
                                item.Stats.goldBonusRate
                            );

                        break;
                }
            }

           
        }

        // ============================================
        // 저장 데이터로부터 다시 계산
        // ============================================

        /// <summary>
        /// 현재 GachaInventory를 기준으로
        /// 가구/레시피 스탯을 처음부터 다시 계산한다.
        ///
        /// 저장 데이터를 불러온 뒤 사용한다.
        /// </summary>
        public void RebuildFromInventory()
        {
            _furnitureGoldBonusFlat = 0;
            _recipeGoldBonusRate = 0f;

            if (GachaInventory.Instance == null)
            {
               

                return;
            }

            if (GachaManager.Instance == null)
            {

                return;
            }

            Dictionary<string, int> items =
                GachaInventory.Instance.GetAllItems();

            Dictionary<GachaGroup, GachaPoolData> pools =
                GachaManager.Instance.GetAllPools();

            foreach (var pair in items)
            {
                string itemId = pair.Key;
                int count = pair.Value;

                if (count <= 0)
                    continue;

                GachaItem item =
                    FindItemInPools(
                        itemId,
                        pools
                    );

                if (item == null ||
                    item.Stats == null)
                {
                    continue;
                }

                switch (item.Group)
                {
                    case GachaGroup.Furniture:

                        _furnitureGoldBonusFlat +=
                            item.Stats.goldBonusFlat *
                            count;

                        break;

                    case GachaGroup.Recipe:

                        _recipeGoldBonusRate +=
                            item.Stats.goldBonusRate *
                            count;

                        break;
                }
            }

        }

        // ============================================
        // 특정 아이템 찾기
        // ============================================

        private GachaItem FindItemInPools(
            string itemId,
            Dictionary<GachaGroup, GachaPoolData> pools)
        {
            if (pools == null)
                return null;

            foreach (var pair in pools)
            {
                GachaPoolData pool = pair.Value;

                if (pool == null)
                    continue;

                GachaItem item =
                    pool.Items.Find(
                        x =>
                            x != null &&
                            x.ItemId == itemId
                    );

                if (item != null)
                    return item;
            }

            return null;
        }

        // ============================================
        // 최종 골드 계산
        // ============================================

        /// <summary>
        /// 기본 골드에 가구 +N을 적용한 뒤
        /// 레시피 +N%를 적용한다.
        ///
        /// 최종:
        /// (기본 골드 + 가구 보너스)
        /// × (1 + 레시피 비율)
        /// </summary>
        public int CalculateFinalGold(int baseGold)
        {
            if (baseGold < 0)
                baseGold = 0;

            float gold =
                baseGold +
                _furnitureGoldBonusFlat;

            gold *=
                1f +
                (_recipeGoldBonusRate / 100f);

            return Mathf.RoundToInt(gold);
        }

        /// <summary>
        /// 현재 가구 골드 증가량 초기화
        /// 테스트용
        /// </summary>
        public void ClearStats()
        {
            _furnitureGoldBonusFlat = 0;
            _recipeGoldBonusRate = 0f;

           
        }
    }
}