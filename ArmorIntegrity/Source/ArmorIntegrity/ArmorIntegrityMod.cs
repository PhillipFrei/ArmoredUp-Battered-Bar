using UnityEngine;
using Verse;

namespace ArmorIntegrity
{
    public sealed class ArmorIntegritySettings : ModSettings
    {
        public float degradationFactor = 0.10f;
        public float fallbackApPerDamage = 0.015f;
        public float batteredRecoveryPerDay = 0.50f;
        public float passthroughDamageFactor = 0.75f;
        public bool showBatteredBar = true;
        public bool debugCombatLogging = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref degradationFactor, "degradationFactor", 0.10f);
            Scribe_Values.Look(ref fallbackApPerDamage, "fallbackApPerDamage", 0.015f);
            Scribe_Values.Look(ref batteredRecoveryPerDay, "batteredRecoveryPerDay", 0.50f);
            Scribe_Values.Look(ref passthroughDamageFactor, "passthroughDamageFactor", 0.75f);
            Scribe_Values.Look(ref showBatteredBar, "showBatteredBar", true);
            Scribe_Values.Look(ref debugCombatLogging, "debugCombatLogging", false);
            base.ExposeData();
        }

        public void ResetToDefaults()
        {
            degradationFactor = 0.10f;
            fallbackApPerDamage = 0.015f;
            batteredRecoveryPerDay = 0.50f;
            passthroughDamageFactor = 0.75f;
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

            listing.Label("Rüstungsverschleiß-Faktor: " + Settings.degradationFactor.ToString("0.000"));
            Settings.degradationFactor = listing.Slider(Settings.degradationFactor, 0f, 0.50f);
            listing.Label("Verschleiß pro Treffer = Damage × AP ÷ aktuelle Sharp-AR × Faktor. Standard: 0.100");
            listing.Gap();

            listing.Label("Fallback-AP pro Damage: " + Settings.fallbackApPerDamage.ToString("0.000"));
            Settings.fallbackApPerDamage = listing.Slider(Settings.fallbackApPerDamage, 0f, 0.05f);
            listing.Label("Nur wenn ein Treffer keine brauchbare AP liefert. AP = Damage × Wert. Standard: 0.015");
            listing.Gap();

            listing.Label("Weitergeleiteter Schaden: " + (Settings.passthroughDamageFactor * 100f).ToString("0") + "%");
            Settings.passthroughDamageFactor = listing.Slider(Settings.passthroughDamageFactor, 0.25f, 1.00f);
            listing.Label("Zusätzliche Dämpfung des Restschadens nach jeder durchdrungenen Rüstungsschicht. AP wird davon nicht zusätzlich reduziert. Standard: 75%.");
            listing.Gap();

            listing.Label("Battered-Regeneration pro Tag: " + (Settings.batteredRecoveryPerDay * 100f).ToString("0") + "%");
            Settings.batteredRecoveryPerDay = listing.Slider(Settings.batteredRecoveryPerDay, 0f, 2f);
            listing.Label("Regeneration intrinsischer Rüstung aus Genen, Implantaten und natürlicher Rüstung. Standard: 50% pro Tag.");
            listing.Gap();

            listing.CheckboxLabeled("Battered-Balken über Pawns anzeigen", ref Settings.showBatteredBar);
            listing.CheckboxLabeled(
                "Debug-Kampflogging",
                ref Settings.debugCombatLogging,
                "Schreibt die Armor-Integrity-Berechnung jeder verarbeiteten Rüstungsschicht ins RimWorld-Log.");
            listing.GapLine();

            if (listing.ButtonText("Auf Standardwerte zurücksetzen"))
                Settings.ResetToDefaults();

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
