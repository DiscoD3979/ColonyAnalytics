using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonyAnalytics.Alerts
{
    public class Alert_MedicinePrediction : Alert
    {
        private int cachedTreatments;
        private int cachedInjuries;
        private List<string> injuredPawns = new List<string>();

        public override AlertPriority Priority => AlertPriority.Medium;

        private bool ShouldShow()
        {
            if (!ColonyAnalyticsMod.settings.enableMedicineAlert) return false;
            CountMedicineAndInjuries(out cachedTreatments, out cachedInjuries);
            if (ColonyAnalyticsMod.settings.useMedicineThreshold)
                return cachedTreatments < cachedInjuries || cachedTreatments < ColonyAnalyticsMod.settings.medicineThreshold;
            return true;
        }

        public override string GetLabel()
        {
            return "AlertMedicineCount".Translate(cachedTreatments, cachedInjuries);
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("AlertMedicineCountDesc".Translate(cachedTreatments, cachedInjuries));
            sb.Append("\n\n");
            for (int i = 0; i < injuredPawns.Count; i++)
            {
                sb.AppendLine(injuredPawns[i]);
            }
            string status = cachedTreatments < cachedInjuries
                ? "\n(" + "MedicineShortage".Translate() + ")"
                : "\n(" + "MedicineSufficient".Translate() + ")";
            sb.Append(status);
            return sb.ToString();
        }

        public override AlertReport GetReport()
        {
            return ShouldShow();
        }

        private void CountMedicineAndInjuries(out int treatments, out int injuries)
        {
            treatments = 0;
            injuries = 0;
            injuredPawns.Clear();

            Map map = Find.CurrentMap;
            if (map == null) return;

            foreach (Thing med in map.spawnedThings)
            {
                if (med.def.IsMedicine
                    && med.def.category == ThingCategory.Item
                    && !med.IsForbidden(Faction.OfPlayer)
                    && med.GetSlotGroup() != null
                    && (med.Faction == null || med.Faction == Faction.OfPlayer))
                {
                    treatments += med.stackCount;
                }
            }

            foreach (Pawn pawn in map.mapPawns.FreeColonistsAndPrisonersSpawned)
            {
                if (pawn.health == null) continue;
                int pawnInjuries = 0;
                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff is Hediff_Injury && hediff.TendableNow())
                    {
                        pawnInjuries++;
                        injuries++;
                    }
                }
                if (pawnInjuries > 0)
                {
                    string extra = DeathTimeString(pawn);
                    injuredPawns.Add("PawnInjuryCount".Translate(pawn.LabelShort, pawnInjuries) + extra);
                }
            }
        }

        private string DeathTimeString(Pawn pawn)
        {
            Hediff bloodLoss = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.BloodLoss);
            if (bloodLoss == null || bloodLoss.Severity < 0.01f) return "";

            float totalBleedRate = pawn.health.hediffSet.BleedRateTotal;
            if (totalBleedRate < 0.001f) return "";

            float hours = (1f - bloodLoss.Severity) * 24f / totalBleedRate;

            if (hours >= 1f)
                return "PawnDeathInHours".Translate(Mathf.CeilToInt(hours));

            if (hours >= 0.1f)
                return "PawnDeathInHours".Translate(hours.ToString("F1"));

            return "PawnDeathInSeconds".Translate(Mathf.CeilToInt(hours * 3600f));
        }
    }
}
