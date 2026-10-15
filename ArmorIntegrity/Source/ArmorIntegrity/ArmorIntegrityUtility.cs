using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArmorIntegrity
{
    public static class ArmorIntegrityUtility
    {
        public const float Epsilon = 0.0001f;

        public static float GetIntrinsicBatteredSeverity(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null)
                return 0f;

            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ArmorIntegrityDefOf.AI_IntrinsicArmorWear);
            return hediff == null ? 0f : Mathf.Clamp01(hediff.Severity);
        }

        public static float GetIntrinsicConditionFactor(Pawn pawn)
        {
            return Mathf.Clamp01(1f - GetIntrinsicBatteredSeverity(pawn));
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
        }

        public static float GetApparelConditionFactor(Apparel apparel)
        {
            if (apparel == null || apparel.Destroyed)
                return 0f;

            ArmorWearTracker tracker = ArmorWearTracker.Instance;

            if (apparel.def.useHitPoints && apparel.MaxHitPoints > 0)
            {
                float pending = tracker != null ? tracker.GetFractionalHpDamage(apparel) : 0f;
                float effectiveHp = Mathf.Max(0f, apparel.HitPoints - pending);
                return Mathf.Clamp01(effectiveHp / apparel.MaxHitPoints);
            }

            float virtualWear = tracker != null ? tracker.GetVirtualWear(apparel) : 0f;
            return Mathf.Clamp01(1f - virtualWear);
        }

        public static float GetApparelBatteredSeverity(Apparel apparel)
        {
            return Mathf.Clamp01(1f - GetApparelConditionFactor(apparel));
        }

        public static bool IsArmorApparel(Apparel apparel)
        {
            if (apparel == null || apparel.Destroyed || apparel.def.apparel == null)
                return false;

            return apparel.GetStatValue(StatDefOf.ArmorRating_Sharp) > Epsilon;
        }

        public static bool TryGetDisplayedBatteredSeverity(Pawn pawn, out float severity)
        {
            severity = 0f;
            if (pawn == null || pawn.Dead)
                return false;

            ArmorWearTracker tracker = ArmorWearTracker.Instance;
            if (tracker == null)
                return false;

            BodyPartRecord lastPart;
            if (!tracker.TryGetLastHitPart(pawn, out lastPart))
                return false;

            // The bar follows the same hit path as the armor calculation.
            // Only the current outermost surviving armor on the last hit body part
            // is eligible. If that layer is intact, no bar is shown even when an
            // inner layer is damaged.
            if (pawn.apparel != null)
            {
                List<Apparel> worn = pawn.apparel.WornApparel;
                for (int i = worn.Count - 1; i >= 0; i--)
                {
                    Apparel apparel = worn[i];
                    if (apparel == null ||
                        apparel.Destroyed ||
                        apparel.def.apparel == null ||
                        !apparel.def.apparel.CoversBodyPart(lastPart) ||
                        !IsArmorApparel(apparel))
                        continue;

                    float battered = GetApparelBatteredSeverity(apparel);
                    if (battered <= Epsilon)
                        return false;

                    severity = battered;
                    return true;
                }
            }

            // Only when there is no surviving wearable armor on that hit path do
            // we expose the intrinsic armor state.
            severity = GetIntrinsicBatteredSeverity(pawn);
            return severity > Epsilon;
        }

        public static void EnsureDisplayBattered(Pawn pawn)
        {
            if (pawn == null || pawn.health == null)
                return;

            Hediff display = pawn.health.hediffSet.GetFirstHediffOfDef(ArmorIntegrityDefOf.AI_BatteredArmor);

            float severity;
            if (!TryGetDisplayedBatteredSeverity(pawn, out severity) || severity <= Epsilon)
            {
                if (display != null)
                    pawn.health.RemoveHediff(display);
                return;
            }

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

        public static float ResolveArmorPenetration(float amount, float armorPenetration)
        {
            if (!float.IsNaN(armorPenetration) &&
                !float.IsInfinity(armorPenetration) &&
                armorPenetration > Epsilon)
                return armorPenetration;

            float factor = ArmorIntegrityMod.Settings != null
                ? ArmorIntegrityMod.Settings.fallbackApPerDamage
                : 0.015f;

            return factor > 0f ? Mathf.Max(0f, amount) * factor : 0f;
        }

        public static StatDef ResolveArmorStat(DamageDef damageDef)
        {
            // Deliberate Armor Integrity rule: every armor-relevant hit uses Sharp.
            // Blunt/Heat are not parallel degradation channels.
            return StatDefOf.ArmorRating_Sharp;
        }

        public static float CalculateWearPercentage(float amount, float armorPenetration, float currentArmorRating)
        {
            if (amount <= 0f || armorPenetration <= 0f || currentArmorRating <= Epsilon)
                return 0f;

            float factor = ArmorIntegrityMod.Settings != null
                ? ArmorIntegrityMod.Settings.degradationFactor
                : 0.10f;

            return Mathf.Max(
                0f,
                amount * armorPenetration / currentArmorRating * Mathf.Max(0f, factor));
        }

        public static float CalculateNominalBlock(float currentArmorRating, float armorPenetration)
        {
            if (currentArmorRating <= Epsilon)
                return 0f;

            float effectiveAp = Mathf.Max(armorPenetration, 0.001f);
            return Mathf.Clamp01(currentArmorRating / effectiveAp);
        }

        public static float CalculateCapacityFraction(float availableWearPercentage, float requestedWearPercentage)
        {
            if (requestedWearPercentage <= Epsilon)
                return 1f;

            return Mathf.Clamp01(
                Mathf.Max(0f, availableWearPercentage) / requestedWearPercentage);
        }

        public static float GetPassthroughDamageFactor()
        {
            float factor = ArmorIntegrityMod.Settings != null
                ? ArmorIntegrityMod.Settings.passthroughDamageFactor
                : 0.75f;

            return Mathf.Clamp(factor, 0.25f, 1f);
        }

        public static float ApplyApparelWear(
            Apparel apparel,
            float wearPercentage,
            out float exactHpDamage,
            out int appliedHpDamage,
            out bool destroyed)
        {
            exactHpDamage = 0f;
            appliedHpDamage = 0;
            destroyed = false;

            if (apparel == null || apparel.Destroyed || wearPercentage <= 0f)
                return 0f;

            float availableWear = GetApparelConditionFactor(apparel) * 100f;
            float appliedWear = Mathf.Min(Mathf.Max(0f, wearPercentage), availableWear);
            if (appliedWear <= Epsilon)
                return 0f;

            ArmorWearTracker tracker = ArmorWearTracker.Instance;

            if (apparel.def.useHitPoints && apparel.MaxHitPoints > 0)
            {
                exactHpDamage = apparel.MaxHitPoints * appliedWear / 100f;

                // If this hit consumes all remaining effective condition, destroy the
                // item directly. This is also what enables same-hit overflow.
                if (appliedWear >= availableWear - Epsilon)
                {
                    appliedHpDamage = apparel.HitPoints;
                    if (tracker != null)
                        tracker.Clear(apparel);

                    DestroyApparel(apparel);
                    destroyed = true;
                    return appliedWear;
                }

                float pending = tracker != null ? tracker.GetFractionalHpDamage(apparel) : 0f;
                float accumulated = Mathf.Max(0f, pending + exactHpDamage);
                int wholeDamage = Mathf.FloorToInt(accumulated + 0.00001f);
                float remainder = accumulated - wholeDamage;

                if (wholeDamage > 0)
                {
                    appliedHpDamage = Mathf.Min(wholeDamage, Mathf.Max(0, apparel.HitPoints - 1));
                    if (appliedHpDamage > 0)
                        apparel.HitPoints -= appliedHpDamage;
                }

                if (tracker != null)
                {
                    tracker.SetFractionalHpDamage(apparel, remainder);
                    tracker.RecordCurrentHp(apparel);
                }

                return appliedWear;
            }

            // Rare modded apparel can be armor without normal Thing hit points.
            // Give it a virtual condition so it still participates consistently.
            if (tracker != null)
            {
                tracker.AddVirtualWear(apparel, appliedWear / 100f);
                if (tracker.GetVirtualWear(apparel) >= 1f - Epsilon)
                {
                    tracker.Clear(apparel);
                    DestroyApparel(apparel);
                    destroyed = true;
                }

                return appliedWear;
            }

            return 0f;
        }

        private static void DestroyApparel(Apparel apparel)
        {
            if (apparel == null || apparel.Destroyed)
                return;

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

        public static bool IsMetalArmor(Apparel apparel)
        {
            if (apparel == null)
                return false;

            return (apparel.def.apparel != null && apparel.def.apparel.useDeflectMetalEffect)
                || (apparel.Stuff != null && apparel.Stuff.IsMetal);
        }

        public static void DebugLayer(
            Pawn pawn,
            string layer,
            float damageIn,
            float apIn,
            float currentArmor,
            float nominalBlock,
            float capacityFraction,
            float requestedWear,
            float appliedWear,
            float damageOut,
            float apOut,
            bool destroyed)
        {
            if (ArmorIntegrityMod.Settings == null || !ArmorIntegrityMod.Settings.debugCombatLogging)
                return;

            Log.Message(string.Format(
                "[Armor Integrity] {0} | {1} | D {2:0.###}->{3:0.###} | AP {4:P1}->{5:P1} | Sharp AR {6:P1} | block {7:P1} × capacity {8:P1} | wear {9:0.###}%/{10:0.###}% | destroyed={11}",
                pawn != null ? pawn.LabelShortCap : "<null>",
                layer,
                damageIn,
                damageOut,
                apIn,
                apOut,
                currentArmor,
                nominalBlock,
                capacityFraction,
                requestedWear,
                appliedWear,
                destroyed));
        }
    }
}
