using UnityEngine;
using PixelRestaurant.Data;

namespace PixelRestaurant.Gacha
{
    public static class GachaWeaponMerge
    {
        public const int RequiredCount = 5;

        // 레어도별 최대 등급
        public static int GetMaxGrade(GachaRarity rarity)
        {
            switch (rarity)
            {
                case GachaRarity.Common:
                    return 8;

                case GachaRarity.Rare:
                    return 8;

                case GachaRarity.Unique:
                    return 4;

                case GachaRarity.Epic:
                    return 3;

                default:
                    return 0;
            }
        }

        // 다음 레어도 확인
        private static bool TryGetNextRarity(
            GachaRarity current,
            out GachaRarity next)
        {
            switch (current)
            {
                case GachaRarity.Common:
                    next = GachaRarity.Rare;
                    return true;

                case GachaRarity.Rare:
                    next = GachaRarity.Unique;
                    return true;

                case GachaRarity.Unique:
                    next = GachaRarity.Epic;
                    return true;

                default:
                    next = current;
                    return false;
            }
        }

        // 다음 단계의 레어도와 등급 계산
        public static bool TryGetNextStage(
            GachaRarity rarity,
            int grade,
            out GachaRarity nextRarity,
            out int nextGrade)
        {
            nextRarity = rarity;
            nextGrade = grade;

            int maxGrade = GetMaxGrade(rarity);

            // 유효하지 않은 등급
            if (grade < 1 || grade > maxGrade)
                return false;

            // Epic 3등급은 최종 단계
            if (rarity == GachaRarity.Epic &&
                grade == maxGrade)
            {
                return false;
            }

            // 같은 레어도의 다음 등급
            if (grade < maxGrade)
            {
                nextGrade = grade + 1;
                return true;
            }

            // 마지막 등급이면 다음 레어도 1등급
            if (TryGetNextRarity(
                rarity,
                out nextRarity))
            {
                nextGrade = 1;
                return true;
            }

            return false;
        }

        // 합치기 가능 여부 및 결과 무기 검색
        public static bool TryFindMergeResult(
      GachaItem sourceItem,
      GachaInventory inventory,
      GachaPoolData weaponPool,
      out GachaItem resultItem)
        {
            resultItem = null;

            if (sourceItem == null ||
                inventory == null ||
                weaponPool == null ||
                weaponPool.Items == null)
            {
                return false;
            }

            if (sourceItem.Group != GachaGroup.Weapon)
                return false;

            if (inventory.GetItemCount(sourceItem.ItemId) < RequiredCount)
                return false;

            if (!TryGetNextStage(
                    sourceItem.Rarity,
                    sourceItem.Grade,
                    out GachaRarity nextRarity,
                    out int nextGrade))
            {
                return false;
            }

            string nextItemId = sourceItem.NextMergeItemId;

            if (string.IsNullOrWhiteSpace(nextItemId))
            {
   
                
                return false;
            }

            nextItemId = nextItemId.Trim();

            GachaItem candidate = weaponPool.Items.Find(
                item => item != null &&
                        item.ItemId != null &&
                        item.ItemId.Trim() == nextItemId
            );

            if (candidate == null)
            {
          
                
                return false;
            }

            if (candidate.Group != GachaGroup.Weapon ||
                candidate.Rarity != nextRarity ||
                candidate.Grade != nextGrade)
            {
           
                return false;
            }

            if (string.IsNullOrWhiteSpace(candidate.LinkedWeaponId))
            {
           
                return false;
            }

            resultItem = candidate;
            return true;
        }
    }
}