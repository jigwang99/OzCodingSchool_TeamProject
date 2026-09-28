using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelRestaurant.Gacha
{
    // Stored with the existing gacha inventory, including the recipe's actual rarity.
    [Serializable]
    public class GachaRecipeIncomeRule
    {
        public string itemId;
        public int rarity;
        public int goldPerDraw;
        public string linkedRecipeId;
    }

    // Runs before customers settle their bills. Only runtime pooled Food instances
    // are changed; business scripts, scenes and prefab assets remain untouched.
    [DefaultExecutionOrder(-1000)]
    public class GachaRecipeIncomeAdapter : MonoBehaviour
    {
        public const int DefaultGoldPerDraw = 50;
        private readonly List<Food> foods = new List<Food>();
        private readonly Dictionary<Food, int> basePrices = new Dictionary<Food, int>();
        private readonly long[] bonuses = new long[4];
        private BObjectPoolManager currentPool;
        private ProductionManager currentProduction;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<GachaRecipeIncomeAdapter>() != null) return;
            var host = new GameObject(nameof(GachaRecipeIncomeAdapter));
            DontDestroyOnLoad(host);
            host.AddComponent<GachaRecipeIncomeAdapter>();
        }

        public static void Configure(GachaInventoryData inventory, GachaPoolData pool, int[] rates)
        {
            if (inventory == null || pool == null || pool.Items == null) return;
            var rules = new List<GachaRecipeIncomeRule>();
            foreach (var item in pool.Items)
            {
                if (item == null || item.Group != PixelRestaurant.Data.GachaGroup.Recipe) continue;
                int rarity = (int)item.Rarity;
                if (rarity < 0 || rarity >= rates.Length) continue;
                rules.Add(new GachaRecipeIncomeRule
                {
                    itemId = item.ItemId,
                    rarity = rarity,
                    goldPerDraw = Math.Max(0, rates[rarity]),
                    linkedRecipeId = item.LinkedRecipeId
                });
            }
            inventory.recipeIncomeRules = rules;
        }

        public static void CalculateBonuses(GachaInventoryData inventory, long[] result)
        {
            Array.Clear(result, 0, result.Length);
            if (inventory == null || inventory.items == null) return;
            foreach (var owned in inventory.items)
            {
                if (owned == null || owned.count <= 0) continue;
                var rule = inventory.recipeIncomeRules?.Find(r => r != null && r.itemId == owned.itemId);
                int rarity;
                int rate;
                if (rule != null)
                {
                    rarity = rule.rarity;
                    rate = Math.Max(0, rule.goldPerDraw);
                }
                else
                {
                    // Existing saves have counts but no rules. These are the four
                    // original Recipe.asset IDs, independent of linkedRecipeId.
                    switch (owned.itemId)
                    {
                        case "Recipe_0": rarity = 0; break;
                        case "Recipe_1": rarity = 1; break;
                        case "Recipe_2": rarity = 2; break;
                        case "Recipe_3": rarity = 3; break;
                        default: continue;
                    }
                    rate = DefaultGoldPerDraw;
                }
                if (rarity < 0 || rarity >= result.Length) continue;
                result[rarity] = Math.Min(int.MaxValue, result[rarity] + (long)owned.count * rate);
            }
        }

        public static int CalculatePrice(int basePrice, long bonus)
        {
            return (int)Math.Min(int.MaxValue, Math.Max(0L, basePrice) + Math.Max(0L, bonus));
        }

        public static void CalculateUnlocks(GachaInventoryData inventory, int[] result)
        {
            Array.Clear(result, 0, result.Length);
            if (inventory?.items == null) return;
            foreach (var owned in inventory.items)
            {
                if (owned == null || owned.count <= 0) continue;
                var rule = inventory.recipeIncomeRules?.Find(r => r != null && r.itemId == owned.itemId);
                string link = rule?.linkedRecipeId;
                // Older saves predate the corrected links. Resolve from the gacha
                // ID, not the old ownedRecipes IDs (recipe_0/recipe_3/recipe_6).
                if (string.IsNullOrEmpty(link))
                {
                    switch (owned.itemId)
                    {
                        case "Recipe_0": link = "recipe_2"; break;
                        case "Recipe_1": link = "recipe_5"; break;
                        case "Recipe_2": link = "recipe_7"; break;
                        case "Recipe_3": link = "recipe_8"; break;
                        default: continue;
                    }
                }
                int rarity;
                switch (link)
                {
                    case "recipe_2": rarity = 0; break;
                    case "recipe_5": rarity = 1; break;
                    case "recipe_7": rarity = 2; break;
                    case "recipe_8": rarity = 3; break;
                    default: continue;
                }
                if (rarity < result.Length) result[rarity] = 1;
            }
        }

        private void ApplyRecipeUnlocks(GachaInventoryData inventory)
        {
            var production = ProductionManager.instance;
            if (production == null) return;
            if (production.Recipes == null || production.Recipes.Length != 4)
                production.Recipes = new int[4];
            CalculateUnlocks(inventory, production.Recipes);

            if (currentProduction == production) return;
            // ProductionManager indexes dishes by rarity + consumed fish count.
            // Normalize runtime references without editing the business scene.
            var ordered = new Food[9];
            int[] starts = { 0, 3, 6, 8 };
            int[] counts = { 3, 3, 2, 1 };
            if (production.foods == null || production.foods.Length != ordered.Length) return;
            foreach (var food in production.foods)
            {
                if (food == null || food.fishRare < 0 || food.fishRare >= starts.Length) return;
                if (food.fishCount < 1 || food.fishCount > counts[food.fishRare]) return;
                int index = starts[food.fishRare] + food.fishCount - 1;
                if (ordered[index] != null) return;
                ordered[index] = food;
            }
            production.foods = ordered;
            currentProduction = production;
        }

        private void Update()
        {
            var data = GameManager.instance != null ? GameManager.instance.PlayerData : null;
            ApplyRecipeUnlocks(data?.gachaInventory);
            var pool = BObjectPoolManager.instance;
            if (currentPool != pool)
            {
                RestorePrices();
                currentPool = pool;
            }
            if (pool == null) return;

            CalculateBonuses(data?.gachaInventory, bonuses);

            // The pool can create extra dishes when exhausted. Reuse the list and
            // include inactive dishes so reused and newly created dishes both work.
            foods.Clear();
            pool.GetComponentsInChildren<Food>(true, foods);
            foreach (var food in foods)
            {
                if (!basePrices.TryGetValue(food, out int basePrice))
                {
                    basePrice = food.price;
                    basePrices.Add(food, basePrice);
                }
                int rarity = food.fishRare;
                long bonus = rarity >= 0 && rarity < bonuses.Length ? bonuses[rarity] : 0;
                food.price = CalculatePrice(basePrice, bonus);
            }
        }

        private void RestorePrices()
        {
            foreach (var pair in basePrices)
                if (pair.Key != null) pair.Key.price = pair.Value;
            basePrices.Clear();
        }

        private void OnDestroy() => RestorePrices();
    }
}
