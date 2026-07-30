using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ColonyAnalytics.Alerts
{
    public class Alert_FoodPrediction : Alert
    {
        private float cachedDays = -1f;
        private float totalNutrition;
        private int pawnCount;
        private int mealCount;

        private static ThingCategoryDef _mealsCat;

        private static bool IsAllowedByAnyColonistDiet(ThingDef def, Map map)
        {
            bool anyWithRestriction = false;
            foreach (Pawn pawn in map.mapPawns.FreeColonistsAndPrisonersSpawned)
            {
                if (!pawn.RaceProps.Humanlike || pawn.foodRestriction?.CurrentFoodPolicy == null)
                    continue;
                anyWithRestriction = true;
                if (pawn.foodRestriction.CurrentFoodPolicy.Allows(def))
                    return true;
            }
            return !anyWithRestriction;
        }

        private static bool IsReadyToEat(ThingDef def)
        {
            if (def.ingestible == null) return false;

            if (def.ingestible.preferability >= FoodPreferability.MealAwful)
                return true;

            if (def.thingCategories != null)
            {
                if (_mealsCat == null)
                    _mealsCat = DefDatabase<ThingCategoryDef>.GetNamedSilentFail("Meals");

                if (_mealsCat != null)
                {
                    for (int i = 0; i < def.thingCategories.Count; i++)
                    {
                        ThingCategoryDef cat = def.thingCategories[i];
                        while (cat != null)
                        {
                            if (cat == _mealsCat) return true;
                            cat = cat.parent;
                        }
                    }
                }
            }

            return false;
        }

        public override AlertPriority Priority => AlertPriority.Medium;

        private bool ShouldShow()
        {
            if (!ColonyAnalyticsMod.settings.enableFoodAlert) return false;
            cachedDays = CalculateFoodDays();
            if (ColonyAnalyticsMod.settings.useFoodThreshold)
                return cachedDays < ColonyAnalyticsMod.settings.foodThreshold;
            return true;
        }

        public override string GetLabel()
        {
            return "AlertFoodDays".Translate(cachedDays.ToString("F1"));
        }

        public override TaggedString GetExplanation()
        {
            if (ColonyAnalyticsMod.settings.countOnlyMeals)
                return "AlertFoodMealsDesc".Translate(cachedDays.ToString("F1"), mealCount, pawnCount);
            return "AlertFoodDaysDesc".Translate(cachedDays.ToString("F1"), totalNutrition.ToString("F0"), pawnCount);
        }

        public override AlertReport GetReport()
        {
            return ShouldShow();
        }

        private float CalculateFoodDays()
        {
            Map map = Find.CurrentMap;
            if (map == null) return 999f;

            totalNutrition = 0f;
            mealCount = 0;

            foreach (Thing food in map.spawnedThings)
            {
                if (!food.def.IsIngestible || food.def.IsDrug || !food.def.IsNutritionGivingIngestible
                    || food.def.category != ThingCategory.Item
                    || food.IsForbidden(Faction.OfPlayer)
                    || food.GetSlotGroup() == null
                    || (food.Faction != null && food.Faction != Faction.OfPlayer))
                    continue;

                if (ColonyAnalyticsMod.settings.countOnlyMeals && !IsReadyToEat(food.def))
                    continue;

                if (!IsAllowedByAnyColonistDiet(food.def, map))
                    continue;

                totalNutrition += food.GetStatValue(StatDefOf.Nutrition) * food.stackCount;

                if (IsReadyToEat(food.def))
                    mealCount += food.stackCount;
            }

            float dailyNeed = 0f;
            pawnCount = 0;
            StatDef hungerStat = DefDatabase<StatDef>.GetNamedSilentFail("HungerRate");

            foreach (Pawn pawn in map.mapPawns.FreeColonistsAndPrisonersSpawned)
            {
                if (pawn.RaceProps.Humanlike && pawn.needs?.food != null)
                {
                    float rate = 1f;
                    if (hungerStat != null)
                        rate = pawn.GetStatValue(hungerStat);
                    dailyNeed += 1.6f * rate;
                    pawnCount++;
                }
            }

            if (pawnCount <= 0) return 999f;

            if (ColonyAnalyticsMod.settings.countOnlyMeals && mealCount > 0)
            {
                float mealsPerDay = pawnCount * 2f;
                return mealCount / mealsPerDay;
            }

            if (dailyNeed <= 0f) return 999f;
            return totalNutrition / dailyNeed;
        }
    }
}
