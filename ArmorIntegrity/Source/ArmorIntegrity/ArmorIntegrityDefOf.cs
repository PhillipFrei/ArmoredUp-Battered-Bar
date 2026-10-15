using Verse;

namespace ArmorIntegrity
{
    [DefOf]
    public static class ArmorIntegrityDefOf
    {
        public static HediffDef AI_BatteredArmor;
        public static HediffDef AI_IntrinsicArmorWear;

        // Kept only so saves that once used Armored Up can still resolve the old defName.
        // Armor Integrity does not use this Hediff for its own armor calculation.
        public static HediffDef BatteredArmor;

        static ArmorIntegrityDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ArmorIntegrityDefOf));
        }
    }
}
