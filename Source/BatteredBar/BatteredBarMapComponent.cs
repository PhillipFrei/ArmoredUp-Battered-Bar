using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Verse;

namespace ArmoredUpBatteredBar
{
    public sealed class BatteredBarMapComponent : MapComponent
    {
        private const float BarWidth = 38f;
        private const float BarHeight = 5f;
        private const float VerticalOffset = 31f;
        private const float FallbackMinEffectiveness = 0.10f;

        private static HediffDef batteredDef;
        private static bool integrationResolved;
        private static object armoredUpSettings;
        private static FieldInfo minEffectivenessField;
        private static bool integrationWarningLogged;

        public BatteredBarMapComponent(Map map) : base(map)
        {
        }

        public override void MapComponentOnGUI()
        {
            if (Event.current == null || Event.current.type != EventType.Repaint)
                return;

            if (Find.CurrentMap != map || map?.mapPawns == null)
                return;

            List<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            if (pawns == null || pawns.Count == 0)
                return;

            ResolveArmoredUpIntegration();
            float minEffectiveness = ReadMinEffectiveness();
            CellRect viewRect = Find.CameraDriver.CurrentViewRect.ExpandedBy(2);

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn == null || pawn.Dead || !pawn.Spawned || !viewRect.Contains(pawn.Position))
                    continue;

                Hediff battered = FindBatteredHediff(pawn);
                if (battered == null || battered.Severity <= 0f)
                    continue;

                DrawBatteredEffectBar(pawn, battered, minEffectiveness);
            }
        }

        private static Hediff FindBatteredHediff(Pawn pawn)
        {
            if (batteredDef == null)
                batteredDef = DefDatabase<HediffDef>.GetNamedSilentFail("BatteredArmor");

            List<Hediff> hediffs = pawn.health?.hediffSet?.hediffs;
            if (hediffs == null)
                return null;

            if (batteredDef != null)
            {
                for (int i = 0; i < hediffs.Count; i++)
                {
                    Hediff hediff = hediffs[i];
                    if (hediff?.def == batteredDef)
                        return hediff;
                }
            }

            for (int i = 0; i < hediffs.Count; i++)
            {
                Hediff hediff = hediffs[i];
                string defName = hediff?.def?.defName;
                if (!string.IsNullOrEmpty(defName) &&
                    defName.IndexOf("batter", StringComparison.OrdinalIgnoreCase) >= 0)
                    return hediff;
            }

            return null;
        }

        private static void DrawBatteredEffectBar(Pawn pawn, Hediff battered, float minEffectiveness)
        {
            float severity = Mathf.Clamp01(battered.Severity);
            float remainingEffect = minEffectiveness + (1f - minEffectiveness) * (1f - severity);
            float barFraction = Mathf.Clamp01(remainingEffect);

            Vector2 uiPos = pawn.DrawPos.MapToUIPosition();
            Rect barRect = new Rect(
                uiPos.x - BarWidth * 0.5f,
                uiPos.y - VerticalOffset,
                BarWidth,
                BarHeight);

            Rect fillRect = barRect;
            fillRect.x += 1f;
            fillRect.y += 1f;
            fillRect.height = Mathf.Max(1f, fillRect.height - 2f);
            fillRect.width = Mathf.Max(0f, (barRect.width - 2f) * barFraction);

            Color oldColor = GUI.color;

            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(barRect, BaseContent.WhiteTex);

            GUI.color = EffectColor(barFraction);
            if (fillRect.width > 0.5f)
                GUI.DrawTexture(fillRect, BaseContent.WhiteTex);

            GUI.color = oldColor;
        }

        private static Color EffectColor(float remainingEffect)
        {
            if (remainingEffect >= 0.70f)
                return new Color(0.24f, 0.82f, 0.33f, 0.95f);
            if (remainingEffect >= 0.40f)
                return new Color(0.95f, 0.72f, 0.18f, 0.95f);
            return new Color(0.92f, 0.18f, 0.14f, 0.95f);
        }

        private static void ResolveArmoredUpIntegration()
        {
            if (integrationResolved)
                return;

            integrationResolved = true;

            try
            {
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int i = 0; i < assemblies.Length; i++)
                {
                    Type modType = assemblies[i].GetType("ArmoredUp.ArmoredUp", false);
                    Type settingsType = assemblies[i].GetType("ArmoredUp.AUSettings", false);
                    if (modType == null || settingsType == null)
                        continue;

                    FieldInfo settingsField = modType.GetField(
                        "settings",
                        BindingFlags.Public | BindingFlags.Static);

                    minEffectivenessField = settingsType.GetField(
                        "MinDamagedArmorEffectiveness",
                        BindingFlags.Public | BindingFlags.Instance);

                    armoredUpSettings = settingsField?.GetValue(null);
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Warning("[Armored Up - Battered Bar] Could not read Armored Up settings: " + ex.Message);
            }
        }

        private static float ReadMinEffectiveness()
        {
            try
            {
                if (armoredUpSettings != null && minEffectivenessField != null)
                {
                    object value = minEffectivenessField.GetValue(armoredUpSettings);
                    if (value is float f)
                        return Mathf.Clamp01(f);
                }
            }
            catch (Exception ex)
            {
                if (!integrationWarningLogged)
                {
                    integrationWarningLogged = true;
                    Log.Warning("[Armored Up - Battered Bar] Falling back to 10% minimum effectiveness: " + ex.Message);
                }
            }

            return FallbackMinEffectiveness;
        }
    }
}
