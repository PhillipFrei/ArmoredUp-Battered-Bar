using RimWorld;
using Verse;

namespace ArmorIntegrity
{
    [DefOf]
    public static class ArmorIntegrityDefOf
    {
        public static HediffDef AI_BatteredArmor;
        public static HediffDef AI_IntrinsicArmorWear;

        // Legacy Armored Up save compatibility. Old saves can still contain this defName.
        public static HediffDef BatteredArmor;

        static ArmorIntegrityDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ArmorIntegrityDefOf));
        }
    }
}
