using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace ArmorIntegrity
{
    public sealed class ArmorWearTracker : GameComponent
    {
        private const string IntrinsicMarker = "__intrinsic__";

        private Dictionary<string, float> fractionalHpDamage = new Dictionary<string, float>();
        private Dictionary<string, float> virtualWear = new Dictionary<string, float>();
        private Dictionary<string, string> displayedLayerByPawn = new Dictionary<string, string>();

        public ArmorWearTracker(Game game)
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref fractionalHpDamage, "AI_fractionalHpDamage", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref virtualWear, "AI_virtualWear", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref displayedLayerByPawn, "AI_displayedLayerByPawn", LookMode.Value, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (fractionalHpDamage == null)
                    fractionalHpDamage = new Dictionary<string, float>();
                if (virtualWear == null)
                    virtualWear = new Dictionary<string, float>();
                if (displayedLayerByPawn == null)
                    displayedLayerByPawn = new Dictionary<string, string>();
            }
        }

        public static ArmorWearTracker Instance
        {
            get
            {
                return Verse.Current.Game == null ? null : Verse.Current.Game.GetComponent<ArmorWearTracker>();
            }
        }

        private static string ApparelKey(Apparel apparel)
        {
            return apparel == null ? null : apparel.ThingID;
        }

        private static string PawnKey(Pawn pawn)
        {
            return pawn == null ? null : pawn.ThingID;
        }

        public float GetFractionalHpDamage(Apparel apparel)
        {
            string key = ApparelKey(apparel);
            if (key == null)
                return 0f;

            float value;
            return fractionalHpDamage.TryGetValue(key, out value) ? Mathf.Max(0f, value) : 0f;
        }

        public void SetFractionalHpDamage(Apparel apparel, float value)
        {
            string key = ApparelKey(apparel);
            if (key == null)
                return;

            if (value <= 0.0001f)
                fractionalHpDamage.Remove(key);
            else
                fractionalHpDamage[key] = Mathf.Max(0f, value);
        }

        public float GetVirtualWear(Apparel apparel)
        {
            string key = ApparelKey(apparel);
            if (key == null)
                return 0f;

            float value;
            return virtualWear.TryGetValue(key, out value) ? Mathf.Clamp01(value) : 0f;
        }

        public void AddVirtualWear(Apparel apparel, float severityDelta)
        {
            string key = ApparelKey(apparel);
            if (key == null || severityDelta <= 0f)
                return;

            virtualWear[key] = Mathf.Clamp01(GetVirtualWear(apparel) + severityDelta);
        }

        public void SetDisplayedApparel(Pawn pawn, Apparel apparel)
        {
            string pawnKey = PawnKey(pawn);
            string apparelKey = ApparelKey(apparel);
            if (pawnKey == null || apparelKey == null)
                return;

            displayedLayerByPawn[pawnKey] = apparelKey;
        }

        public void SetDisplayedIntrinsic(Pawn pawn)
        {
            string pawnKey = PawnKey(pawn);
            if (pawnKey == null)
                return;

            displayedLayerByPawn[pawnKey] = IntrinsicMarker;
        }

        public bool TryGetDisplayedLayer(Pawn pawn, out Apparel apparel, out bool intrinsic)
        {
            apparel = null;
            intrinsic = false;

            string pawnKey = PawnKey(pawn);
            if (pawnKey == null)
                return false;

            string stored;
            if (!displayedLayerByPawn.TryGetValue(pawnKey, out stored) || string.IsNullOrEmpty(stored))
                return false;

            if (stored == IntrinsicMarker)
            {
                intrinsic = true;
                return true;
            }

            if (pawn.apparel == null)
                return false;

            List<Apparel> worn = pawn.apparel.WornApparel;
            for (int i = 0; i < worn.Count; i++)
            {
                Apparel candidate = worn[i];
                if (candidate != null && !candidate.Destroyed && candidate.ThingID == stored)
                {
                    apparel = candidate;
                    return true;
                }
            }

            displayedLayerByPawn.Remove(pawnKey);
            return false;
        }

        public void Clear(Apparel apparel)
        {
            string key = ApparelKey(apparel);
            if (key == null)
                return;

            fractionalHpDamage.Remove(key);
            virtualWear.Remove(key);

            List<string> pawnsToClear = null;
            foreach (KeyValuePair<string, string> pair in displayedLayerByPawn)
            {
                if (pair.Value != key)
                    continue;

                if (pawnsToClear == null)
                    pawnsToClear = new List<string>();

                pawnsToClear.Add(pair.Key);
            }

            if (pawnsToClear != null)
            {
                for (int i = 0; i < pawnsToClear.Count; i++)
                    displayedLayerByPawn.Remove(pawnsToClear[i]);
            }
        }
    }
}
