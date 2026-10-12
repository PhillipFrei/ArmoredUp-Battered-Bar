using UnityEngine;
using Verse;

namespace ArmorIntegrity
{
    public sealed class HediffCompProperties_ArmorRecovery : HediffCompProperties
    {
        public HediffCompProperties_ArmorRecovery()
        {
            compClass = typeof(HediffComp_ArmorRecovery);
        }
    }

    public sealed class HediffComp_ArmorRecovery : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            float perDay = ArmorIntegrityMod.Settings != null
                ? ArmorIntegrityMod.Settings.batteredRecoveryPerDay
                : 0.50f;

            severityAdjustment -= perDay / 60000f;
        }
    }

    public sealed class HediffCompProperties_ArmorDisplaySync : HediffCompProperties
    {
        public HediffCompProperties_ArmorDisplaySync()
        {
            compClass = typeof(HediffComp_ArmorDisplaySync);
        }
    }

    public sealed class HediffComp_ArmorDisplaySync : HediffComp
    {
        public override bool CompShouldRemove
        {
            get
            {
                float severity;
                return !ArmorIntegrityUtility.TryGetDisplayedBatteredSeverity(parent.pawn, out severity)
                    || severity <= 0.0001f;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            float severity;
            if (ArmorIntegrityUtility.TryGetDisplayedBatteredSeverity(parent.pawn, out severity))
                parent.Severity = Mathf.Clamp(severity, 0.0001f, 1f);
        }
    }
}
