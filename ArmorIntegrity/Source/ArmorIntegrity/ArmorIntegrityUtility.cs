using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArmorIntegrity
{
    public static class ArmorIntegrityUtility
    {
        private const float Epsilon = 0.0001f;

        public static float GetIntrinsicBatteredSeverity(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null)
                return 0f;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ArmorIntegrityDefOf.AI_IntrinsicArmorWear);
            return hediff == null ? 0f : Mathf.Clamp01(hediff.Severity);
        }

        public static void AddIntrinsicBattered(Pawn pawn, float percentagePoints)
        {
            if (pawn == null || pawn.health == null || percentagePoints <= 0f)
                return;

            float severityDelta = percentagePoints / 100f;
            if (severityDelta <= 0f)
                return;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ArmorIntegrityDefOf.AI_IntrinsicArmorWear);
            if (hediff == null)
            {
                hediff = HediffMaker.MakeHediff(ArmorIntegrityDefOf.AI_IntrinsicArmorWear, pawn);
                hediff.Severity = Mathf.Clamp01(severityDelta);
                pawn.health.AddHediff(hediff);
            }
            else
            {
                hediff.Severity = Mathf.Clamp01(hediff.Severity + severityDelta);
            }

            EnsureDisplayBattered(pawn);
        }

        public static void EnsureDisplayBattered(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
                return;

            float severity;
            if (!TryGetDisplayedBatteredSeverity(pawn, out severity) || severity <= Epsilon)
                return;

            Hediff display = pawn.health.hediffSet.GetFirstHediffOfDef(ArmorIntegrityDefOf.AI_BatteredArmor);
            if (display == null)
            {
                display = HediffMaker.MakeHediff(ArmorIntegrityDefOf.AI_BatteredArmor, pawn);
                display.Severity = Mathf.Clamp(severity, 0.0001f, 1f);
                pawn.health.AddHediff(display);
            }
            else
            {
                display.Severity = Mathf.Clamp(severity, 0.0001f, 1f);
            }
        }

        public static bool TryGetDisplayedBatteredSeverity(Pawn pawn, out float severity)
        {
            severity = 0f;
            if (pawn == null)
                return false;

            // Show only one apparel layer: the outermost damaged armor item in worn order.
            if (pawn.apparel != null)
            {
                List<Apparel> worn = pawn.apparel.WornApparel;
                for (int i = worn.Count - 1; i >= 0; i--)
                {
                    Apparel apparel = worn[i];
                    if (apparel == null || apparel.Destroyed || !apparel.def.useHitPoints || apparel.MaxHitPoints <= 0)
                        continue;

                    float hpFraction = Mathf.Clamp01((float)apparel.HitPoints / apparel.MaxHitPoints);
                    if (hpFraction >= 0.9999f)
                        continue;

                    if (!IsArmorApparel(apparel))
                        continue;

                    severity = 1f - hpFraction;
                    return severity > Epsilon;
                }
            }

            // Intrinsic armor is the last layer and becomes visible only when no damaged apparel layer is shown.
            severity = GetIntrinsicBatteredSeverity(pawn);
            return severity > Epsilon;
        }

        private static bool IsArmorApparel(Apparel apparel)
        {
            if (apparel == null)
                return false;

            return apparel.GetStatValue(StatDefOf.ArmorRating_Sharp) > Epsilon
                || apparel.GetStatValue(StatDefOf.ArmorRating_Blunt) > Epsilon
                || apparel.GetStatValue(StatDefOf.ArmorRating_Heat) > Epsilon;
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
            // V2: every armor-relevant hit is intentionally checked against Sharp.
            // Blunt is reserved for the later trauma system rather than being a parallel degradation track.
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

        public static bool DamageApparel(Apparel apparel, float wearPercentage)
        {
            if (apparel == null || apparel.Destroyed || !apparel.def.useHitPoints || apparel.MaxHitPoints <= 0 || wearPercentage <= 0f)
                return false;

            float exactDamage = apparel.MaxHitPoints * wearPercentage / 100f;
            int hpDamage = GenMath.RoundRandom(Mathf.Max(0f, exactDamage));
            if (hpDamage <= 0)
                return false;

            Pawn wearer = apparel.Wearer;

            if (hpDamage >= apparel.HitPoints)
            {
                bool oldAllow = Thing.allowDestroyNonDestroyable;
                try
                {
                    Thing.allowDestroyNonDestroyable = true;
                    apparel.Destroy(DestroyMode.KillFinalize);
                }
                finally
                {
                    Thing.allowDestroyNonDestroyable = oldAllow;
                }
            }
            else
            {
                apparel.HitPoints -= hpDamage;
            }

            if (wearer != null)
                EnsureDisplayBattered(wearer);

            return true;
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
