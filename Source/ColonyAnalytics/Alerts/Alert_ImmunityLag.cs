using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ColonyAnalytics.Alerts
{
    public class Alert_ImmunityLag : Alert
    {
        private List<Pawn> laggingPawns = new List<Pawn>();
        private List<float> lagPercentages = new List<float>();
        private Pawn worstPawn;

        public override AlertPriority Priority => AlertPriority.Critical;

        public override string GetLabel()
        {
            return "AlertImmunityLag".Translate(laggingPawns.Count);
        }

        public override TaggedString GetExplanation()
        {
            string listStr = "";
            for (int i = 0; i < laggingPawns.Count; i++)
            {
                listStr += $"{laggingPawns[i].LabelShort}: {lagPercentages[i]:F1}%\n";
            }
            return "AlertImmunityLagDesc".Translate(listStr);
        }

        public override AlertReport GetReport()
        {
            laggingPawns.Clear();
            lagPercentages.Clear();
            worstPawn = null;

            if (!ColonyAnalyticsMod.settings.enableImmunityAlert)
                return false;

            Map map = Find.CurrentMap;
            if (map == null) return false;

            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (pawn.health == null) continue;
                if (pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer) continue;

                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_Injury || hediff is Hediff_MissingPart) continue;
                    if (hediff.TendableNow())
                    {
                        HediffComp_Immunizable immunizable = hediff.TryGetComp<HediffComp_Immunizable>();
                        if (immunizable != null)
                        {
                            float immunity = immunizable.Immunity;
                            float severity = hediff.Severity;
                            float lag = immunity - severity;
                            if (lag < 0)
                            {
                                laggingPawns.Add(pawn);
                                lagPercentages.Add(lag * 100f);
                                if (worstPawn == null || lag < GetWorstLag(worstPawn))
                                {
                                    worstPawn = pawn;
                                }
                                break;
                            }
                        }
                    }
                }
            }

            int count = laggingPawns.Count;
            if (count == 0) return false;

            if (ColonyAnalyticsMod.settings.useImmunityThreshold)
                return count >= ColonyAnalyticsMod.settings.immunityThreshold;

            return worstPawn != null ? AlertReport.CulpritIs(worstPawn) : AlertReport.CulpritsAre(laggingPawns.Cast<Thing>().ToList());
        }

        private float GetWorstLag(Pawn pawn)
        {
            float worst = 0f;
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
            {
                HediffComp_Immunizable immunizable = hediff.TryGetComp<HediffComp_Immunizable>();
                if (immunizable != null)
                {
                    float lag = immunizable.Immunity - hediff.Severity;
                    if (lag < worst) worst = lag;
                }
            }
            return worst;
        }
    }
}
