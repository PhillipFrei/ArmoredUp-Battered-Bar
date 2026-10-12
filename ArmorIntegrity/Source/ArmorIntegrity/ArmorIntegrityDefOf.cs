using RimWorld;
using Verse;

namespace ArmorIntegrity
{
    [DefOf]
    public static class ArmorIntegrityDefOf
    {
        public static HediffDef AI_BatteredArmor;
        public static HediffDef AI_IntrinsicArmorWear;

        static ArmorIntegrityDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ArmorIntegrityDefOf));
        }
    }
}
