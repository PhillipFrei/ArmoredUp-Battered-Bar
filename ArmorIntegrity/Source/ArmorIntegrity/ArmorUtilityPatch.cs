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
            Log.Message("[Armor Integrity] Loaded prototype armor system v2.1.");
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

            // One hit, one layer. No damage/AP forwarding in this prototype.
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

                    float exactHpDamage;
                    int appliedHpDamage;
                    ArmorIntegrityUtility.DamageApparel(apparel, wear, out exactHpDamage, out appliedHpDamage);
                    ArmorIntegrityUtility.DebugLayer(
                        pawn,
                        apparel.Destroyed ? null : apparel,
                        layerLabel,
                        amount,
                        resolvedAp,
                        currentArmor,
                        wear,
                        exactHpDamage,
                        appliedHpDamage);

                    diminishedByMetalArmor = metal;
                    deflectedByMetalArmor = metal;
                    __result = 0f;
                    return false;
                }
            }

            // Intrinsic armor (genes, implants, race stats, etc.) is the final layer.
            float currentIntrinsicArmor = Mathf.Max(0f, pawn.GetStatValue(armorStat));
            if (currentIntrinsicArmor > Epsilon)
            {
                float wear = ArmorIntegrityUtility.CalculateWearPercentage(amount, resolvedAp, currentIntrinsicArmor);
                ArmorIntegrityUtility.AddIntrinsicBattered(pawn, wear);
                ArmorIntegrityUtility.DebugLayer(
                    pawn,
                    null,
                    "intrinsic armor",
                    amount,
                    resolvedAp,
                    currentIntrinsicArmor,
                    wear,
                    0f,
                    0);

                bool metal = pawn.RaceProps != null && pawn.RaceProps.IsMechanoid;
                diminishedByMetalArmor = metal;
                deflectedByMetalArmor = metal;
                __result = 0f;
                return false;
            }

            // All armor is exhausted: this later hit reaches the pawn.
            ArmorIntegrityUtility.TraceNoArmor(pawn, amount, resolvedAp);
            __result = amount;
            return false;
        }
    }
}
