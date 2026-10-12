using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace ArmorIntegrity
{
    public sealed class ArmorWearTracker : GameComponent
    {
        private Dictionary<string, float> fractionalHpDamage = new Dictionary<string, float>();
        private Dictionary<string, float> virtualWear = new Dictionary<string, float>();

        public ArmorWearTracker(Game game)
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref fractionalHpDamage, "AI_fractionalHpDamage", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref virtualWear, "AI_virtualWear", LookMode.Value, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (fractionalHpDamage == null)
                    fractionalHpDamage = new Dictionary<string, float>();
                if (virtualWear == null)
                    virtualWear = new Dictionary<string, float>();
            }
        }

        public static ArmorWearTracker Current
        {
            get
            {
                return Current.Game == null ? null : Current.Game.GetComponent<ArmorWearTracker>();
            }
        }

        private static string Key(Apparel apparel)
        {
            return apparel == null ? null : apparel.ThingID;
        }

        public float GetFractionalHpDamage(Apparel apparel)
        {
            string key = Key(apparel);
            if (key == null)
                return 0f;

            float value;
            return fractionalHpDamage.TryGetValue(key, out value) ? Mathf.Max(0f, value) : 0f;
        }

        public void SetFractionalHpDamage(Apparel apparel, float value)
        {
            string key = Key(apparel);
            if (key == null)
                return;

            if (value <= 0.0001f)
                fractionalHpDamage.Remove(key);
            else
                fractionalHpDamage[key] = Mathf.Max(0f, value);
        }

        public float GetVirtualWear(Apparel apparel)
        {
            string key = Key(apparel);
            if (key == null)
                return 0f;

            float value;
            return virtualWear.TryGetValue(key, out value) ? Mathf.Clamp01(value) : 0f;
        }

        public void AddVirtualWear(Apparel apparel, float severityDelta)
        {
            string key = Key(apparel);
            if (key == null || severityDelta <= 0f)
                return;

            virtualWear[key] = Mathf.Clamp01(GetVirtualWear(apparel) + severityDelta);
        }

        public void Clear(Apparel apparel)
        {
            string key = Key(apparel);
            if (key == null)
                return;

            fractionalHpDamage.Remove(key);
            virtualWear.Remove(key);
        }
    }
}
