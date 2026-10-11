using RimWorld;
using UnityEngine;
using Verse;

namespace ArmorIntegrity
{
    public static class ArmorIntegrityUtility
    {
        private const float Epsilon = 0.0001f;

        public static float GetBatteredSeverity(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null)
                return 0f;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ArmorIntegrityDefOf.AI_BatteredArmor);
            return hediff == null ? 0f : Mathf.Clamp01(hediff.Severity);
        }

        public static void AddBattered(Pawn pawn, float percentagePoints)
        {
            if (pawn == null || pawn.health == null || percentagePoints <= 0f)
                return;

            float severityDelta = percentagePoints / 100f;
            if (severityDelta <= 0f)
                return;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ArmorIntegrityDefOf.AI_BatteredArmor);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(ArmorIntegrityDefOf.AI_BatteredArmor, pawn);
                hediff.Severity = Mathf.Clamp01(severityDelta);
                pawn.health.AddHediff(hediff);
            }
            else
            {
                hediff.Severity = Mathf.Clamp01(hediff.Severity + severityDelta);
            }
        }

        public static float ResolveArmorPenetration(float amount, float armorPenetration)
        {
            if (!float.IsNaN(armorPenetration) && !float.IsInfinity(armorPenetration) && armorPenetration > Epsilon)
                return armorPenetration;

            float factor = ArmorIntegrityMod.Settings != null
                ? ArmorIntegrityMod.Settings.fallbackApPerDamage
                : 0.015f;

            if (factor <= 0f)
                return 0f;

            return amount * factor;
        }

        public static StatDef ResolveArmorStat(DamageDef damageDef)
        {
            if (damageDef != null && damageDef.armorCategory != null && damageDef.armorCategory.armorRatingStat != null)
                return damageDef.armorCategory.armorRatingStat;

            return StatDefOf.ArmorRating_Sharp;
        }

        public static float CalculateWearPercentage(float amount, float armorPenetration, float currentArmorRating)
        {
            if (amount <= 0f || armorPenetration <= 0f || currentArmorRating <= Epsilon)
                return 0f;

            float factor = ArmorIntegrityMod.Settings != null
                ? ArmorIntegrityMod.Settings.degradationFactor
                : 0.10f;

            return Mathf.Max(0f, amount * armorPenetration / currentArmorRating * Mathf.Max(0f, factor));
        }

        public static void DamageApparel(Apparel apparel, float wearPercentage)
        {
            if (apparel == null || apparel.Destroyed || !apparel.def.useHitPoints || apparel.MaxHitPoints <= 0 || wearPercentage <= 0f)
                return;

            float exactDamage = apparel.MaxHitPoints * wearPercentage / 100f;
            int hpDamage = GenMath.RoundRandom(Mathf.Max(0f, exactDamage));
            if (hpDamage <= 0)
                return;

            apparel.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, hpDamage));
        }

        public static bool IsMetalArmor(Apparel apparel)
        {
            if (apparel == null)
                return false;

            return (apparel.def.apparel != null && apparel.def.apparel.useDeflectMetalEffect)
                || (apparel.Stuff != null && apparel.Stuff.IsMetal);
        }

        public static void DebugLayer(Pawn pawn, string layer, float damageIn, float apIn, float armor, float wear, float damageOut, float apOut)
        {
            if (ArmorIntegrityMod.Settings == null || !ArmorIntegrityMod.Settings.debugCombatLogging)
                return;

            string pawnLabel = pawn != null ? pawn.LabelShortCap : "<null>";
            Log.Message(string.Format(
                "[Armor Integrity] {0} | {1} | D {2:0.###} AP {3:P1} AR {4:P1} | wear {5:0.###}% | rest D {6:0.###} AP {7:P1}",
                pawnLabel, layer, damageIn, apIn, armor, wear, damageOut, apOut));
        }
    }
}
