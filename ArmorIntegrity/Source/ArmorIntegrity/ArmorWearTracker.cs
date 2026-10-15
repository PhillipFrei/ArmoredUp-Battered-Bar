using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ArmorIntegrity
{
    // RimWorld apparel HP are integers. This component stores only the fractional
    // remainder produced by Armor Integrity's percentage-based wear calculation.
    public sealed class ArmorWearTracker : GameComponent
    {
        private Dictionary<string, float> fractionalHpDamage = new Dictionary<string, float>();
        private Dictionary<string, float> virtualWear = new Dictionary<string, float>();
        private Dictionary<string, int> lastKnownHp = new Dictionary<string, int>();

        public ArmorWearTracker(Game game)
        {
        }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref fractionalHpDamage, "AI_fractionalHpDamage", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref virtualWear, "AI_virtualWear", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref lastKnownHp, "AI_lastKnownHp", LookMode.Value, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (fractionalHpDamage == null)
                    fractionalHpDamage = new Dictionary<string, float>();
                if (virtualWear == null)
                    virtualWear = new Dictionary<string, float>();
                if (lastKnownHp == null)
                    lastKnownHp = new Dictionary<string, int>();
            }
        }

        public static ArmorWearTracker Instance
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

            int knownHp;
            if (lastKnownHp.TryGetValue(key, out knownHp) && apparel.HitPoints > knownHp)
            {
                // The item was repaired by RimWorld or another mod. A fractional
                // remainder from before the repair must not survive that repair.
                fractionalHpDamage.Remove(key);
            }

            lastKnownHp[key] = apparel.HitPoints;

            float value;
            return fractionalHpDamage.TryGetValue(key, out value) ? Mathf.Max(0f, value) : 0f;
        }

        public void SetFractionalHpDamage(Apparel apparel, float value)
        {
            string key = Key(apparel);
            if (key == null)
                return;

            lastKnownHp[key] = apparel.HitPoints;

            if (value <= 0.0001f)
                fractionalHpDamage.Remove(key);
            else
                fractionalHpDamage[key] = Mathf.Max(0f, value);
        }

        public void RecordCurrentHp(Apparel apparel)
        {
            string key = Key(apparel);
            if (key != null)
                lastKnownHp[key] = apparel.HitPoints;
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
            lastKnownHp.Remove(key);
        }
    }
}
