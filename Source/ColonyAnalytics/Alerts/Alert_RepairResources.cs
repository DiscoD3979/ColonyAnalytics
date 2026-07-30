using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonyAnalytics.Alerts
{
    public class Alert_RepairResources : Alert
    {
        private List<Thing> damagedThings = new List<Thing>();
        private List<string> missingResourcesList = new List<string>();

        public override AlertPriority Priority => AlertPriority.Medium;

        public override string GetLabel()
        {
            return "AlertRepairResources".Translate(damagedThings.Count);
        }

        public override TaggedString GetExplanation()
        {
            string listStr = string.Join("\n", missingResourcesList);
            return "AlertRepairResourcesDesc".Translate(damagedThings.Count, listStr);
        }

        public override AlertReport GetReport()
        {
            damagedThings.Clear();
            missingResourcesList.Clear();

            if (!ColonyAnalyticsMod.settings.enableRepairAlert)
                return false;

            Map map = Find.CurrentMap;
            if (map == null) return false;

            foreach (Thing t in map.spawnedThings)
            {
                if (t.def.building == null || t.def.building.repairable == false) continue;
                if (t is Building building && building.Faction == Faction.OfPlayer && building.HitPoints < building.MaxHitPoints)
                {
                    if (!HasResourcesForRepair(building))
                    {
                        damagedThings.Add(building);
                    }
                }
            }

            return damagedThings.Count > 0 ? AlertReport.CulpritsAre(damagedThings) : false;
        }

        private bool HasResourcesForRepair(Building building)
        {
            Map map = building.Map;
            if (building.def.costList == null) return true;

            float hpMissing = building.MaxHitPoints - building.HitPoints;
            float hpTotal = building.MaxHitPoints;
            float repairFraction = hpMissing / hpTotal;

            foreach (ThingDefCountClass cost in building.def.costList)
            {
                int needed = Mathf.CeilToInt(cost.count * repairFraction);
                if (needed <= 0) continue;
                int available = map.resourceCounter.GetCount(cost.thingDef);
                if (available < needed)
                {
                    missingResourcesList.Add($"{building.Label} → {cost.thingDef.label}: {available}/{needed}");
                    return false;
                }
            }
            return true;
        }
    }
}
