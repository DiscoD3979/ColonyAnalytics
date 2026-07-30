using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ColonyAnalytics
{
    public class ColonyAnalyticsMod : Mod
    {
        public static ColonyAnalyticsSettings settings;
        public static ColonyAnalyticsMod Instance;

        public ColonyAnalyticsMod(ModContentPack content) : base(content)
        {
            Instance = this;
            settings = GetSettings<ColonyAnalyticsSettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "Colony Analytics";
        }
    }

    public class ColonyAnalyticsSettings : ModSettings
    {
        public bool enableFoodAlert = true;
        public bool enableMedicineAlert = true;
        public bool enableStarvingAnimalsAlert = true;
        public bool enableDoctorAlert = true;
        public bool enableRepairAlert = true;
        public bool enableImmunityAlert = true;

        public bool countOnlyMeals = true;
        public bool useFoodThreshold = false;
        public float foodThreshold = 5f;
        public bool useMedicineThreshold = false;
        public float medicineThreshold = 3f;
        public bool useStarvingAnimalThreshold = false;
        public float starvingAnimalThreshold = 1f;
        public bool useImmunityThreshold = false;
        public float immunityThreshold = 1f;

        public bool showWelcomeMessage = true;
        public bool welcomeShownThisSession = false;

        private int selectedTab = 0;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableFoodAlert, "enableFoodAlert", true);
            Scribe_Values.Look(ref enableMedicineAlert, "enableMedicineAlert", true);
            Scribe_Values.Look(ref enableStarvingAnimalsAlert, "enableStarvingAnimalsAlert", true);
            Scribe_Values.Look(ref enableDoctorAlert, "enableDoctorAlert", true);
            Scribe_Values.Look(ref enableRepairAlert, "enableRepairAlert", true);
            Scribe_Values.Look(ref enableImmunityAlert, "enableImmunityAlert", true);
            Scribe_Values.Look(ref countOnlyMeals, "countOnlyMeals", true);
            Scribe_Values.Look(ref useFoodThreshold, "useFoodThreshold", false);
            Scribe_Values.Look(ref foodThreshold, "foodThreshold", 5f);
            Scribe_Values.Look(ref useMedicineThreshold, "useMedicineThreshold", false);
            Scribe_Values.Look(ref medicineThreshold, "medicineThreshold", 3f);
            Scribe_Values.Look(ref useStarvingAnimalThreshold, "useStarvingAnimalThreshold", false);
            Scribe_Values.Look(ref starvingAnimalThreshold, "starvingAnimalThreshold", 1f);
            Scribe_Values.Look(ref useImmunityThreshold, "useImmunityThreshold", false);
            Scribe_Values.Look(ref immunityThreshold, "immunityThreshold", 1f);
            Scribe_Values.Look(ref showWelcomeMessage, "showWelcomeMessage", true);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            List<TabRecord> tabs = new List<TabRecord>
            {
                new TabRecord("TabFood".Translate(), delegate { selectedTab = 0; }, selectedTab == 0),
                new TabRecord("TabMedicine".Translate(), delegate { selectedTab = 1; }, selectedTab == 1),
                new TabRecord("TabAnimals".Translate(), delegate { selectedTab = 2; }, selectedTab == 2),
                new TabRecord("TabDoctor".Translate(), delegate { selectedTab = 3; }, selectedTab == 3),
                new TabRecord("TabRepair".Translate(), delegate { selectedTab = 4; }, selectedTab == 4),
                new TabRecord("TabImmunity".Translate(), delegate { selectedTab = 5; }, selectedTab == 5),
                new TabRecord("TabOther".Translate(), delegate { selectedTab = 6; }, selectedTab == 6),
            };
            Rect tabRect = inRect;
            tabRect.yMin += 20f;
            TabDrawer.DrawTabs(tabRect, tabs);

            Rect contentRect = inRect;
            contentRect.yMin += 64f;
            contentRect.yMax -= 4f;
            contentRect.x += 4f;
            contentRect.width -= 8f;

            Listing_Standard list = new Listing_Standard();
            list.Begin(contentRect);

            switch (selectedTab)
            {
                case 0: DrawFoodTab(list); break;
                case 1: DrawMedicineTab(list); break;
                case 2: DrawAnimalsTab(list); break;
                case 3: DrawDoctorTab(list); break;
                case 4: DrawRepairTab(list); break;
                case 5: DrawImmunityTab(list); break;
                case 6: DrawOtherTab(list); break;
            }

            list.End();
        }

        private void DrawToggle(Listing_Standard list, string label, ref bool value)
        {
            Rect rect = list.GetRect(26f);
            Widgets.Checkbox(rect.x, rect.y, ref value, 24f);
            Widgets.Label(new Rect(rect.x + 28f, rect.y, rect.width - 28f, rect.height), label);
        }

        private void DrawSlider(Listing_Standard list, string label, ref float value, float min, float max)
        {
            Rect rect = list.GetRect(32f);
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 20f), label + ": " + value.ToString("F1"));
            Rect sliderRect = new Rect(rect.x, rect.y + 16f, rect.width, 20f);
            value = Widgets.HorizontalSlider(sliderRect, value, min, max, true);
        }

        private void DrawFoodTab(Listing_Standard list)
        {
            list.Label("TabFoodDesc".Translate());
            list.Gap(8f);
            DrawToggle(list, "SettingEnable".Translate() + ": " + "AlertFoodPrediction".Translate(), ref enableFoodAlert);

            if (enableFoodAlert)
            {
                list.Gap(4f);
                DrawToggle(list, "OnlyCountMeals".Translate(), ref countOnlyMeals);
                list.Gap(4f);
                DrawToggle(list, "UseThreshold".Translate(), ref useFoodThreshold);
                if (useFoodThreshold)
                    DrawSlider(list, "AlertDaysLeft".Translate(), ref foodThreshold, 1f, 30f);
            }
        }

        private void DrawMedicineTab(Listing_Standard list)
        {
            list.Label("TabMedicineDesc".Translate());
            list.Gap(8f);
            DrawToggle(list, "SettingEnable".Translate() + ": " + "AlertMedicinePrediction".Translate(), ref enableMedicineAlert);

            if (enableMedicineAlert)
            {
                list.Gap(4f);
                DrawToggle(list, "UseThreshold".Translate(), ref useMedicineThreshold);
                if (useMedicineThreshold)
                    DrawSlider(list, "TreatmentsMin".Translate(), ref medicineThreshold, 1f, 30f);
            }
        }

        private void DrawAnimalsTab(Listing_Standard list)
        {
            list.Label("TabAnimalsDesc".Translate());
            list.Gap(8f);
            DrawToggle(list, "SettingEnable".Translate() + ": " + "AlertStarvingAnimals".Translate(), ref enableStarvingAnimalsAlert);

            if (enableStarvingAnimalsAlert)
            {
                list.Gap(4f);
                DrawToggle(list, "UseThreshold".Translate(), ref useStarvingAnimalThreshold);
                if (useStarvingAnimalThreshold)
                    DrawSlider(list, "AlertAnimalCount".Translate(), ref starvingAnimalThreshold, 1f, 20f);
            }
        }

        private void DrawDoctorTab(Listing_Standard list)
        {
            list.Label("TabDoctorDesc".Translate());
            list.Gap(8f);
            DrawToggle(list, "SettingEnable".Translate() + ": " + "AlertDoctor".Translate(), ref enableDoctorAlert);
        }

        private void DrawRepairTab(Listing_Standard list)
        {
            list.Label("TabRepairDesc".Translate());
            list.Gap(8f);
            DrawToggle(list, "SettingEnable".Translate() + ": " + "AlertRepairResources".Translate(), ref enableRepairAlert);
        }

        private void DrawImmunityTab(Listing_Standard list)
        {
            list.Label("TabImmunityDesc".Translate());
            list.Gap(8f);
            DrawToggle(list, "SettingEnable".Translate() + ": " + "AlertImmunity".Translate(), ref enableImmunityAlert);

            if (enableImmunityAlert)
            {
                list.Gap(4f);
                DrawToggle(list, "UseThreshold".Translate(), ref useImmunityThreshold);
                if (useImmunityThreshold)
                    DrawSlider(list, "AlertImmunityLagPercent".Translate(), ref immunityThreshold, 0.1f, 20f);
            }
        }

        private void DrawOtherTab(Listing_Standard list)
        {
            list.Label("TabOtherDesc".Translate());
            list.Gap(8f);
            DrawToggle(list, "ShowWelcomeMessage".Translate(), ref showWelcomeMessage);
        }

        public void ShowWelcome()
        {
            if (!showWelcomeMessage || welcomeShownThisSession) return;
            welcomeShownThisSession = true;

            Dialog_MessageBox welcome = new Dialog_MessageBox(
                "WelcomeText".Translate(),
                "WelcomeButtonOk".Translate(),
                null,
                "WelcomeButtonSettings".Translate(),
                delegate { Find.WindowStack.Add(new Dialog_ModSettings(ColonyAnalyticsMod.Instance)); },
                null,
                false
            );
            welcome.closeOnCancel = false;
            Find.WindowStack.Add(welcome);
        }
    }
}
