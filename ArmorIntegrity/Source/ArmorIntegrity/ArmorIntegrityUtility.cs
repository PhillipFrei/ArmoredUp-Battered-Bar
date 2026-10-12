using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArmorIntegrity
{
    public static class ArmorIntegrityUtility
    {
        private const float Epsilon = 0.0001f;
        private static int automaticTraceBudget = 80;

        public static float GetIntrinsicBatteredSeverity(Pawn pawn)
        {
            if (pawn == null || pawn.health == null || pawn.health.hediffSet == null)
                return 0f;

            float result = 0f;

            Hediff current = pawn.health.hediffSet.GetFirstHediffOfDef(ArmorIntegrityDefOf.AI_IntrinsicArmorWear);
            if (current != null)
                result = Mathf.Max(result, Mathf.Clamp01(current.Severity));

            Hediff legacy = pawn.health.hediffSet.GetFirstHediffOfDef(ArmorIntegrityDefOf.BatteredArmor);
            if (legacy != null)
                result = Mathf.Max(result, Mathf.Clamp01(legacy.Severity));

            return result;
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

            float wear = tracker != null ? tracker.GetVirtualWear(apparel) : 0f;
            return Mathf.Clamp01(1f - wear);
        }

        public static float GetApparelBatteredSeverity(Apparel apparel)
        {
            return 1f - GetApparelConditionFactor(apparel);
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

            // One bar only: the outermost damaged armor item wins.
            if (pawn.apparel != null)
            {
                List<Apparel> worn = pawn.apparel.WornApparel;
                for (int i = worn.Count - 1; i >= 0; i--)
                {
                    Apparel apparel = worn[i];
                    if (apparel == null || apparel.Destroyed || apparel.def.apparel == null)
                        continue;

                    if (!IsArmorApparel(apparel))
                        continue;

                    float battered = GetApparelBatteredSeverity(apparel);
                    if (battered <= Epsilon)
                        continue;

                    severity = Mathf.Clamp01(battered);
                    return true;
                }
            }

            severity = GetIntrinsicBatteredSeverity(pawn);
            return severity > Epsilon;
        }

        private static bool IsArmorApparel(Apparel apparel)
        {
            if (apparel == null || apparel.Destroyed)
                return false;

            // This prototype intentionally uses Sharp as the only core armor channel.
            return apparel.GetStatValue(StatDefOf.ArmorRating_Sharp) > Epsilon;
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

        public static bool DamageApparel(Apparel apparel, float wearPercentage, out float exactHpDamage, out int appliedHpDamage)
        {
            exactHpDamage = 0f;
            appliedHpDamage = 0;

            if (apparel == null || apparel.Destroyed || wearPercentage <= 0f)
                return false;

            Pawn wearer = apparel.Wearer;
            ArmorWearTracker tracker = ArmorWearTracker.Instance;

            if (apparel.def.useHitPoints && apparel.MaxHitPoints > 0)
            {
                exactHpDamage = apparel.MaxHitPoints * wearPercentage / 100f;

                float pending = tracker != null ? tracker.GetFractionalHpDamage(apparel) : 0f;
                float accumulated = Mathf.Max(0f, pending + exactHpDamage);
                int wholeDamage = Mathf.FloorToInt(accumulated + 0.00001f);
                float remainder = accumulated - wholeDamage;

                if (tracker != null)
                    tracker.SetFractionalHpDamage(apparel, remainder);

                if (wholeDamage > 0)
                {
                    appliedHpDamage = Mathf.Min(wholeDamage, apparel.HitPoints);

                    if (wholeDamage >= apparel.HitPoints)
                    {
                        if (tracker != null)
                            tracker.Clear(apparel);

                        DestroyApparel(apparel);
                    }
                    else
                    {
                        apparel.HitPoints -= wholeDamage;
                    }
                }

                // Important: even sub-1-HP wear is a real hit for the bar/Hediff.
                if (wearer != null)
                    EnsureDisplayBattered(wearer);

                return true;
            }

            // Physical apparel without normal HP gets its own virtual integrity.
            if (tracker != null)
            {
                tracker.AddVirtualWear(apparel, wearPercentage / 100f);

                if (tracker.GetVirtualWear(apparel) >= 0.9999f)
                {
                    tracker.Clear(apparel);
                    DestroyApparel(apparel);
                }

                if (wearer != null)
                    EnsureDisplayBattered(wearer);

                return true;
            }

            return false;
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
            Apparel apparel,
            string layer,
            float damageIn,
            float apIn,
            float armor,
            float wear,
            float exactHpDamage,
            int appliedHpDamage)
        {
            bool requested = ArmorIntegrityMod.Settings != null && ArmorIntegrityMod.Settings.debugCombatLogging;
            bool automatic = automaticTraceBudget > 0;

            if (!requested && !automatic)
                return;

            if (automatic)
                automaticTraceBudget--;

            string pawnLabel = pawn != null ? pawn.LabelShortCap : "<null>";
            string itemState = apparel == null
                ? "intrinsic"
                : string.Format(
                    "useHP={0} HP={1}/{2} pending={3:0.###}",
                    apparel.def.useHitPoints,
                    apparel.def.useHitPoints ? apparel.HitPoints : 0,
                    apparel.def.useHitPoints ? apparel.MaxHitPoints : 0,
                    ArmorWearTracker.Instance != null ? ArmorWearTracker.Instance.GetFractionalHpDamage(apparel) : 0f);

            Log.Message(string.Format(
                "[Armor Integrity TRACE] {0} | {1} | D={2:0.###} AP={3:P1} SharpAR={4:P1} wear={5:0.###}% exactHP={6:0.###} appliedHP={7} | {8}",
                pawnLabel, layer, damageIn, apIn, armor, wear, exactHpDamage, appliedHpDamage, itemState));
        }

        public static void TraceNoArmor(Pawn pawn, float amount, float ap)
        {
            bool requested = ArmorIntegrityMod.Settings != null && ArmorIntegrityMod.Settings.debugCombatLogging;
            bool automatic = automaticTraceBudget > 0;
            if (!requested && !automatic)
                return;

            if (automatic)
                automaticTraceBudget--;

            Log.Message(string.Format(
                "[Armor Integrity TRACE] {0} | no active armor layer | D={1:0.###} AP={2:P1} -> body",
                pawn != null ? pawn.LabelShortCap : "<null>", amount, ap));
        }
    }
}
