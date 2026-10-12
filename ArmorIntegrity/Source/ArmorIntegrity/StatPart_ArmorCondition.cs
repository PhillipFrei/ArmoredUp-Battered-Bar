using RimWorld;
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
                return ArmorIntegrityUtility.GetApparelConditionFactor(apparel);

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
