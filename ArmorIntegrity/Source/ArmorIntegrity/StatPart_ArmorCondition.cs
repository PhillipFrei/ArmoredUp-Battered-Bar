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
                ArmorWearTracker tracker = ArmorWearTracker.Current;

                if (apparel.def.useHitPoints && apparel.MaxHitPoints > 0)
                {
                    float pending = tracker != null ? tracker.GetFractionalHpDamage(apparel) : 0f;
                    float effectiveHp = Mathf.Max(0f, apparel.HitPoints - pending);
                    return Mathf.Clamp01(effectiveHp / apparel.MaxHitPoints);
                }

                if (tracker != null)
                    return 1f - tracker.GetVirtualWear(apparel);

                return 1f;
            }

            Pawn pawn = thing as Pawn;
            if (pawn != null)
                return 1f - ArmorIntegrityUtility.GetIntrinsicBatteredSeverity(pawn);

            return 1f;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            if (!req.HasThing)
                return;

            val *= GetConditionFactor(req.Thing);
        }

        public override string ExplanationPart(StatRequest req)
        {
            if (!req.HasThing)
                return null;

            float factor = GetConditionFactor(req.Thing);
            if (factor >= 0.9999f)
                return null;

            Apparel apparel = req.Thing as Apparel;
            if (apparel != null)
                return "Armor integrity: x" + (factor * 100f).ToString("0.0") + "%";

            return "Battered armor: x" + (factor * 100f).ToString("0.0") + "%";
        }
    }
}
