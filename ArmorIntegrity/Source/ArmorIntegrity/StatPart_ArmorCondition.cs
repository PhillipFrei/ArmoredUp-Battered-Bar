using RimWorld;
using UnityEngine;
using Verse;

namespace ArmorIntegrity
{
    public sealed class StatPart_ArmorCondition : StatPart
    {
        public static float GetConditionFactor(Thing thing)
        {
            if (thing == null)
                return 1f;

            Apparel apparel = thing as Apparel;
            if (apparel != null)
            {
                if (apparel.def.useHitPoints && apparel.MaxHitPoints > 0)
                    return Mathf.Clamp01((float)apparel.HitPoints / apparel.MaxHitPoints);

                Pawn wearer = apparel.Wearer;
                if (wearer != null)
                    return 1f - ArmorIntegrityUtility.GetIntrinsicBatteredSeverity(wearer);

                return 1f;
            }

            Pawn pawn = thing as Pawn;
            if (pawn != null)
                return 1f - ArmorIntegrityUtility.GetIntrinsicBatteredSeverity(pawn);

            return 1f;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            // V2 intentionally treats armor degradation as Sharp-only.
            // Blunt/Heat values are left untouched for now and are reserved for later trauma work.
            if (parentStat != StatDefOf.ArmorRating_Sharp || !req.HasThing)
                return;

            val *= GetConditionFactor(req.Thing);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (parentStat != StatDefOf.ArmorRating_Sharp || !req.HasThing)
                return null;

            float factor = GetConditionFactor(req.Thing);
            if (factor >= 0.9999f)
                return null;

            Apparel apparel = req.Thing as Apparel;
            if (apparel != null && apparel.def.useHitPoints)
                return "Armor integrity: x" + (factor * 100f).ToString("0") + "%";

            return "Battered armor: x" + (factor * 100f).ToString("0") + "%";
        }
    }
}
