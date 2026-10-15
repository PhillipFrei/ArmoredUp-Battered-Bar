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
            Log.Message("[Armor Integrity] Loaded Armor Integrity 1.0.");
        }
    }

    [HarmonyPatch(typeof(ArmorUtility), nameof(ArmorUtility.GetPostArmorDamage))]
    public static class ArmorUtility_GetPostArmorDamage_Patch
    {
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

            if (pawn == null || amount <= 0f || float.IsNaN(amount))
            {
                __result = Mathf.Max(0f, amount);
                return false;
            }

            // Preserve vanilla/A-Up behavior for damage that is not armor-relevant
            // and for effectively infinite penetration.
            if (damageDef == null ||
                damageDef.armorCategory == null ||
                float.IsInfinity(armorPenetration) ||
                armorPenetration > float.MaxValue * 0.001f)
            {
                __result = amount;
                return false;
            }

            StatDef armorStat = ArmorIntegrityUtility.ResolveArmorStat(damageDef);
            float damage = amount;
            float ap = ArmorIntegrityUtility.ResolveArmorPenetration(amount, armorPenetration);

            if (pawn.apparel != null && part != null)
            {
                // Snapshot the list because a layer can be destroyed during this hit.
                List<Apparel> worn = new List<Apparel>(pawn.apparel.WornApparel);

                // RimWorld and Armored Up both process WornApparel from the end to
                // the beginning: outer layers before inner layers.
                for (int i = worn.Count - 1; i >= 0 && damage > ArmorIntegrityUtility.Epsilon; i--)
                {
                    Apparel apparel = worn[i];
                    if (apparel == null ||
                        apparel.Destroyed ||
                        apparel.def.apparel == null ||
                        !apparel.def.apparel.CoversBodyPart(part))
                        continue;

                    float currentArmor = Mathf.Max(0f, apparel.GetStatValue(armorStat));
                    if (currentArmor <= ArmorIntegrityUtility.Epsilon)
                        continue;

                    float damageIn = damage;
                    float apIn = ap;
                    float nominalBlock = ArmorIntegrityUtility.CalculateNominalBlock(currentArmor, ap);
                    float requestedWear = ArmorIntegrityUtility.CalculateWearPercentage(damage, ap, currentArmor);
                    float availableWear = ArmorIntegrityUtility.GetApparelConditionFactor(apparel) * 100f;
                    float capacityFraction = ArmorIntegrityUtility.CalculateCapacityFraction(availableWear, requestedWear);
                    float effectiveBlock = Mathf.Clamp01(nominalBlock * capacityFraction);

                    float wearToApply = Mathf.Min(requestedWear, availableWear);
                    float exactHpDamage;
                    int appliedHpDamage;
                    bool destroyed;
                    float appliedWear = ArmorIntegrityUtility.ApplyApparelWear(
                        apparel,
                        wearToApply,
                        out exactHpDamage,
                        out appliedHpDamage,
                        out destroyed);

                    float passFraction = 1f - effectiveBlock;
                    damage = Mathf.Max(0f, damage * passFraction);
                    ap = Mathf.Max(0f, ap * passFraction);

                    bool metal = ArmorIntegrityUtility.IsMetalArmor(apparel);
                    if (metal && damage < damageIn - ArmorIntegrityUtility.Epsilon)
                        diminishedByMetalArmor = true;

                    if (damage <= ArmorIntegrityUtility.Epsilon)
                    {
                        damage = 0f;
                        ap = 0f;
                        deflectedByMetalArmor = metal;
                    }

                    ArmorIntegrityUtility.DebugLayer(
                        pawn,
                        apparel.LabelCap,
                        damageIn,
                        apIn,
                        currentArmor,
                        nominalBlock,
                        capacityFraction,
                        requestedWear,
                        appliedWear,
                        damage,
                        ap,
                        destroyed);
                }
            }

            // Natural/gene/implant armor is the final layer after all covering apparel.
            if (damage > ArmorIntegrityUtility.Epsilon)
            {
                float currentArmor = Mathf.Max(0f, pawn.GetStatValue(armorStat));
                if (currentArmor > ArmorIntegrityUtility.Epsilon)
                {
                    float damageIn = damage;
                    float apIn = ap;
                    float nominalBlock = ArmorIntegrityUtility.CalculateNominalBlock(currentArmor, ap);
                    float requestedWear = ArmorIntegrityUtility.CalculateWearPercentage(damage, ap, currentArmor);
                    float availableWear = ArmorIntegrityUtility.GetIntrinsicConditionFactor(pawn) * 100f;
                    float capacityFraction = ArmorIntegrityUtility.CalculateCapacityFraction(availableWear, requestedWear);
                    float effectiveBlock = Mathf.Clamp01(nominalBlock * capacityFraction);
                    float appliedWear = Mathf.Min(requestedWear, availableWear);

                    ArmorIntegrityUtility.AddIntrinsicBattered(pawn, appliedWear);

                    float passFraction = 1f - effectiveBlock;
                    damage = Mathf.Max(0f, damage * passFraction);
                    ap = Mathf.Max(0f, ap * passFraction);

                    bool metal = pawn.RaceProps != null && pawn.RaceProps.IsMechanoid;
                    if (metal && damage < damageIn - ArmorIntegrityUtility.Epsilon)
                        diminishedByMetalArmor = true;

                    if (damage <= ArmorIntegrityUtility.Epsilon)
                    {
                        damage = 0f;
                        ap = 0f;
                        deflectedByMetalArmor = metal;
                    }

                    ArmorIntegrityUtility.DebugLayer(
                        pawn,
                        "intrinsic armor",
                        damageIn,
                        apIn,
                        currentArmor,
                        nominalBlock,
                        capacityFraction,
                        requestedWear,
                        appliedWear,
                        damage,
                        ap,
                        false);
                }
            }

            ArmorIntegrityUtility.EnsureDisplayBattered(pawn);
            __result = damage <= ArmorIntegrityUtility.Epsilon ? 0f : damage;
            return false;
        }
    }
}
