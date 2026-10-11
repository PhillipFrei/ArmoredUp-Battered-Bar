using RimWorld;
using Verse;

namespace ArmorIntegrity
{
    [DefOf]
    public static class ArmorIntegrityDefOf
    {
        public static HediffDef AI_BatteredArmor;

        static ArmorIntegrityDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ArmorIntegrityDefOf));
        }
    }
}
