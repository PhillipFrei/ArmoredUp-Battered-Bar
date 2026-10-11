using RimWorld;
using UnityEngine;
using Verse;

namespace ArmorIntegrity
{
    public sealed class ArmorIntegritySettings : ModSettings
    {
        public float degradationFactor = 0.10f;
        public float fallbackApPerDamage = 0.015f;
        public float batteredRecoveryPerDay = 0.50f;
        public bool showBatteredBar = true;
        public bool debugCombatLogging = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref degradationFactor, "degradationFactor", 0.10f);
            Scribe_Values.Look(ref fallbackApPerDamage, "fallbackApPerDamage", 0.015f);
            Scribe_Values.Look(ref batteredRecoveryPerDay, "batteredRecoveryPerDay", 0.50f);
            Scribe_Values.Look(ref showBatteredBar, "showBatteredBar", true);
            Scribe_Values.Look(ref debugCombatLogging, "debugCombatLogging", false);
            base.ExposeData();
        }

        public void ResetToDefaults()
        {
            degradationFactor = 0.10f;
            fallbackApPerDamage = 0.015f;
            batteredRecoveryPerDay = 0.50f;
            showBatteredBar = true;
            debugCombatLogging = false;
        }
    }

    public sealed class ArmorIntegrityMod : Mod
    {
        public static ArmorIntegritySettings Settings = new ArmorIntegritySettings();

        public ArmorIntegrityMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ArmorIntegritySettings>();
        }

        public override string SettingsCategory()
        {
            return "Armor Integrity";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("Armor degradation factor: " + Settings.degradationFactor.ToString("0.000"));
            Settings.degradationFactor = listing.Slider(Settings.degradationFactor, 0f, 0.50f);
            listing.Label("Battered/Apparel wear per hit = Damage × AP ÷ current AR × this factor. Default: 0.100");
            listing.Gap();

            listing.Label("Fallback AP per damage: " + Settings.fallbackApPerDamage.ToString("0.000"));
            Settings.fallbackApPerDamage = listing.Slider(Settings.fallbackApPerDamage, 0f, 0.05f);
            listing.Label("Used only when incoming AP is zero or unusable. Internal AP = Damage × this value. Default: 0.015");
            listing.Gap();

            listing.Label("Battered recovery per day: " + (Settings.batteredRecoveryPerDay * 100f).ToString("0") + "%");
            Settings.batteredRecoveryPerDay = listing.Slider(Settings.batteredRecoveryPerDay, 0f, 2f);
            listing.Label("Recovery speed for intrinsic armor from genes, implants, races and other pawn armor stats. Default: 50% per day.");
            listing.Gap();

            listing.CheckboxLabeled("Show Battered bar above pawns", ref Settings.showBatteredBar);
            listing.CheckboxLabeled("Debug combat logging", ref Settings.debugCombatLogging, "Writes every Armor Integrity layer calculation to the RimWorld log. Use only for testing.");
            listing.GapLine();

            if (listing.ButtonText("Reset to defaults"))
                Settings.ResetToDefaults();

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
