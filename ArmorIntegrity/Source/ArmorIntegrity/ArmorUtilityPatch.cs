using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace ArmorIntegrity
{
    [StaticConstructorOnStartup]
    public static class ArmorIntegrityStartup
    {
        static ArmorIntegrityStartup()
        {
            Harmony harmony = new Harmony("phillipfrei.armorintegrity");
            harmony.PatchAll();
            Log.Message("[Armor Integrity] Loaded prototype armor system.");
        }
    }

    [HarmonyPatch(typeof(ArmorUtility), nameof(ArmorUtility.GetPostArmorDamage))]
    public static class ArmorUtility_GetPostArmorDamage_Patch
    {
        private const float Epsilon = 0.0001f;

        [HarmonyPrefix]
        public static bool Prefix(
            Pawn pawn,
            float amount,
            float armorPenetration,
            BodyPartRecord part,
            ref DamageDef damageDef,
            ref bool deflectedByMetalArmor,
            ref bool diminishedByMetalArmor,
            ref float __result)
        {
            deflectedByMetalArmor = false;
            diminishedByMetalArmor = false;

            if (pawn == null || amount <= 0f)
            {
                __result = Mathf.Max(0f, amount);
                return false;
            }

            if (float.IsInfinity(armorPenetration) || float.IsNaN(amount))
            {
                __result = amount;
                return false;
            }

            StatDef armorStat = ArmorIntegrityUtility.ResolveArmorStat(damageDef);
            float remainingDamage = amount;
            float remainingAp = ArmorIntegrityUtility.ResolveArmorPenetration(amount, armorPenetration);
            bool anyArmorReducedDamage = false;
            bool anyMetalArmorReducedDamage = false;

            if (pawn.apparel != null && part != null)
            {
                List<Apparel> worn = new List<Apparel>(pawn.apparel.WornApparel);
                for (int i = worn.Count - 1; i >= 0 && remainingDamage > Epsilon; i--)
                {
                    Apparel apparel = worn[i];
                    if (apparel == null || apparel.Destroyed || apparel.def.apparel == null || !apparel.def.apparel.CoversBodyPart(part))
                        continue;

                    float currentArmor = Mathf.Max(0f, apparel.GetStatValue(armorStat));
                    if (currentArmor <= Epsilon)
                        continue;

                    float damageIn = remainingDamage;
                    float apIn = remainingAp;
                    float wear = ArmorIntegrityUtility.CalculateWearPercentage(damageIn, apIn, currentArmor);

                    float penetrationFraction = apIn > currentArmor
                        ? Mathf.Clamp01((apIn - currentArmor) / apIn)
                        : 0f;

                    float damageOut = damageIn * penetrationFraction;
                    float apOut = apIn > currentArmor ? Mathf.Max(0f, apIn - currentArmor) : 0f;

                    if (apparel.def.useHitPoints)
                        ArmorIntegrityUtility.DamageApparel(apparel, wear);
                    else
                        ArmorIntegrityUtility.AddBattered(pawn, wear);

                    bool reduced = damageOut < damageIn - Epsilon;
                    if (reduced)
                    {
                        anyArmorReducedDamage = true;
                        if (ArmorIntegrityUtility.IsMetalArmor(apparel))
                            anyMetalArmorReducedDamage = true;
                    }

                    ArmorIntegrityUtility.DebugLayer(pawn, apparel.LabelCap, damageIn, apIn, currentArmor, wear, damageOut, apOut);

                    remainingDamage = damageOut;
                    remainingAp = apOut;
                }
            }

            if (remainingDamage > Epsilon)
            {
                float currentNaturalArmor = Mathf.Max(0f, pawn.GetStatValue(armorStat));
                if (currentNaturalArmor > Epsilon)
                {
                    float damageIn = remainingDamage;
                    float apIn = remainingAp;
                    float wear = ArmorIntegrityUtility.CalculateWearPercentage(damageIn, apIn, currentNaturalArmor);

                    float penetrationFraction = apIn > currentNaturalArmor
                        ? Mathf.Clamp01((apIn - currentNaturalArmor) / apIn)
                        : 0f;

                    float damageOut = damageIn * penetrationFraction;
                    float apOut = apIn > currentNaturalArmor ? Mathf.Max(0f, apIn - currentNaturalArmor) : 0f;

                    ArmorIntegrityUtility.AddBattered(pawn, wear);

                    if (damageOut < damageIn - Epsilon)
                    {
                        anyArmorReducedDamage = true;
                        if (pawn.RaceProps != null && pawn.RaceProps.IsMechanoid)
                            anyMetalArmorReducedDamage = true;
                    }

                    ArmorIntegrityUtility.DebugLayer(pawn, "intrinsic armor", damageIn, apIn, currentNaturalArmor, wear, damageOut, apOut);

                    remainingDamage = damageOut;
                    remainingAp = apOut;
                }
            }

            diminishedByMetalArmor = anyMetalArmorReducedDamage;
            deflectedByMetalArmor = anyMetalArmorReducedDamage && remainingDamage <= Epsilon;

            if (ArmorIntegrityMod.Settings != null && ArmorIntegrityMod.Settings.debugCombatLogging && anyArmorReducedDamage)
                Log.Message(string.Format("[Armor Integrity] Final damage for {0}: {1:0.###} -> {2:0.###}", pawn.LabelShortCap, amount, remainingDamage));

            __result = remainingDamage <= Epsilon ? 0f : remainingDamage;
            return false;
        }
    }
}
