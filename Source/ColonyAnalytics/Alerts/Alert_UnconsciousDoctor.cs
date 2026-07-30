using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace ColonyAnalytics.Alerts
{
    public class Alert_UnconsciousDoctor : Alert
    {
        private List<Pawn> downedDoctors = new List<Pawn>();

        public override AlertPriority Priority => AlertPriority.Critical;

        public override string GetLabel()
        {
            return "AlertUnconsciousDoctor".Translate();
        }

        public override TaggedString GetExplanation()
        {
            return "AlertUnconsciousDoctorDesc".Translate(downedDoctors.Count);
        }

        public override AlertReport GetReport()
        {
            downedDoctors.Clear();

            if (!ColonyAnalyticsMod.settings.enableDoctorAlert)
                return false;

            Map map = Find.CurrentMap;
            if (map == null) return false;

            List<Pawn> allDoctors = new List<Pawn>();
            foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
            {
                if (pawn.workSettings != null && pawn.workSettings.GetPriority(WorkTypeDefOf.Doctor) > 0)
                {
                    allDoctors.Add(pawn);
                }
            }

            int availableDoctors = 0;
            foreach (Pawn doctor in allDoctors)
            {
                if (doctor.Downed || doctor.health?.capacities?.GetLevel(PawnCapacityDefOf.Manipulation) < 0.3f
                    || doctor.health?.capacities?.GetLevel(PawnCapacityDefOf.Consciousness) < 0.3f)
                {
                    downedDoctors.Add(doctor);
                }
                else
                {
                    availableDoctors++;
                }
            }

            if (availableDoctors == 0 && downedDoctors.Count > 0)
                return AlertReport.CulpritsAre(downedDoctors.Cast<Thing>().ToList());

            return false;
        }
    }
}
