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
            Log.Message("[Armor Integrity] Loaded prototype armor system v2.");
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
            float resolvedAp = ArmorIntegrityUtility.ResolveArmorPenetration(amount, armorPenetration);

            // V2 deliberately has no damage/AP forwarding.
            // Exactly one active layer handles a hit; the next layer is only considered by a later hit.
            if (pawn.apparel != null && part != null)
            {
                List<Apparel> worn = new List<Apparel>(pawn.apparel.WornApparel);
                for (int i = worn.Count - 1; i >= 0; i--)
                {
                    Apparel apparel = worn[i];
                    if (apparel == null || apparel.Destroyed || apparel.def.apparel == null || !apparel.def.apparel.CoversBodyPart(part))
                        continue;

                    float currentArmor = Mathf.Max(0f, apparel.GetStatValue(armorStat));
                    if (currentArmor <= Epsilon)
                        continue;

                    string layerLabel = apparel.LabelCap;
                    bool metal = ArmorIntegrityUtility.IsMetalArmor(apparel);
                    float wear = ArmorIntegrityUtility.CalculateWearPercentage(amount, resolvedAp, currentArmor);

                    ArmorIntegrityUtility.DamageApparel(apparel, wear);
                    ArmorIntegrityUtility.DebugLayer(pawn, layerLabel, amount, resolvedAp, currentArmor, wear, 0f, 0f);

                    diminishedByMetalArmor = metal;
                    deflectedByMetalArmor = metal;
                    __result = 0f;
                    return false;
                }
            }

            // No wearable layer was available for this body part: intrinsic armor is the final layer.
            float currentIntrinsicArmor = Mathf.Max(0f, pawn.GetStatValue(armorStat));
            if (currentIntrinsicArmor > Epsilon)
            {
                float wear = ArmorIntegrityUtility.CalculateWearPercentage(amount, resolvedAp, currentIntrinsicArmor);
                ArmorIntegrityUtility.AddIntrinsicBattered(pawn, wear);
                ArmorIntegrityUtility.DebugLayer(pawn, "intrinsic armor", amount, resolvedAp, currentIntrinsicArmor, wear, 0f, 0f);

                bool metal = pawn.RaceProps != null && pawn.RaceProps.IsMechanoid;
                diminishedByMetalArmor = metal;
                deflectedByMetalArmor = metal;
                __result = 0f;
                return false;
            }

            // All armor layers are gone/exhausted. Only now does the original hit reach the pawn.
            if (ArmorIntegrityMod.Settings != null && ArmorIntegrityMod.Settings.debugCombatLogging)
                Log.Message(string.Format("[Armor Integrity] {0} | no active armor layer | full body damage {1:0.###}", pawn.LabelShortCap, amount));

            __result = amount;
            return false;
        }
    }
}
