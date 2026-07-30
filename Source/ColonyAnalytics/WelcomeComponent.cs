using RimWorld;
using Verse;

namespace ColonyAnalytics
{
    public class ColonyAnalyticsMapComponent : MapComponent
    {
        private bool welcomeQueued;

        public ColonyAnalyticsMapComponent(Map map) : base(map)
        {
        }

        public override void MapGenerated()
        {
            if (ColonyAnalyticsMod.settings.showWelcomeMessage)
                welcomeQueued = true;
        }

        public override void MapComponentTick()
        {
            if (welcomeQueued)
            {
                welcomeQueued = false;
                ColonyAnalyticsMod.settings.ShowWelcome();
            }
        }
    }
}
