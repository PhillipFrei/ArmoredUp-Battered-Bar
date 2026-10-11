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
}
