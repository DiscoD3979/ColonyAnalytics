using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ColonyAnalytics.Alerts
{
    public class Alert_StarvingAnimals : Alert
    {
        private List<Pawn> starvingAnimals = new List<Pawn>();

        public override AlertPriority Priority => AlertPriority.Medium;

        public override string GetLabel()
        {
            return "AlertStarvingAnimals".Translate(starvingAnimals.Count);
        }

        public override TaggedString GetExplanation()
        {
            return "AlertStarvingAnimalsDesc".Translate(starvingAnimals.Count);
        }

        public override AlertReport GetReport()
        {
            starvingAnimals.Clear();

            if (!ColonyAnalyticsMod.settings.enableStarvingAnimalsAlert)
                return false;

            Map map = Find.CurrentMap;
            if (map == null) return false;

            foreach (Pawn animal in map.mapPawns.AllPawnsSpawned)
            {
                if (animal.RaceProps.Animal && animal.Faction == Faction.OfPlayer)
                {
                    bool hasMalnutrition = animal.health?.hediffSet?.HasHediff(HediffDefOf.Malnutrition, false) ?? false;
                    bool foodZero = animal.needs?.food?.CurLevelPercentage <= 0f;

                    if (hasMalnutrition || foodZero)
                    {
                        starvingAnimals.Add(animal);
                    }
                }
            }

            int count = starvingAnimals.Count;
            if (ColonyAnalyticsMod.settings.useStarvingAnimalThreshold)
                return count >= ColonyAnalyticsMod.settings.starvingAnimalThreshold;
            return count > 0 ? AlertReport.CulpritsAre(starvingAnimals.Cast<Thing>().ToList()) : false;
        }
    }
}
