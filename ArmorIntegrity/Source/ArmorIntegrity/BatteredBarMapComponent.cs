using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace ArmorIntegrity
{
    public sealed class BatteredBarMapComponent : MapComponent
    {
        private const float BarWidth = 38f;
        private const float BarHeight = 5f;
        private const float VerticalOffset = 31f;

        public BatteredBarMapComponent(Map map) : base(map)
        {
        }

        public override void MapComponentOnGUI()
        {
            if (ArmorIntegrityMod.Settings == null || !ArmorIntegrityMod.Settings.showBatteredBar)
                return;
            if (Event.current == null || Event.current.type != EventType.Repaint)
                return;
            if (Find.CurrentMap != map || map == null || map.mapPawns == null)
                return;

            IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
            if (pawns == null || pawns.Count == 0)
                return;

            CellRect viewRect = Find.CameraDriver.CurrentViewRect.ExpandedBy(2);

            for (int i = 0; i < pawns.Count; i++)
            {
                Pawn pawn = pawns[i];
                if (pawn == null || pawn.Dead || !pawn.Spawned || !viewRect.Contains(pawn.Position))
                    continue;
                if (pawn.Position.Fogged(map))
                    continue;

                float severity = ArmorIntegrityUtility.GetBatteredSeverity(pawn);
                if (severity <= 0f)
                    continue;

                DrawBar(pawn, severity);
            }
        }

        private static void DrawBar(Pawn pawn, float severity)
        {
            float remaining = Mathf.Clamp01(1f - severity);
            Vector2 uiPos = pawn.DrawPos.MapToUIPosition();
            Rect barRect = new Rect(uiPos.x - BarWidth * 0.5f, uiPos.y - VerticalOffset, BarWidth, BarHeight);

            Rect fillRect = barRect;
            fillRect.x += 1f;
            fillRect.y += 1f;
            fillRect.height = Mathf.Max(1f, fillRect.height - 2f);
            fillRect.width = Mathf.Max(0f, (barRect.width - 2f) * remaining);

            Color oldColor = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(barRect, BaseContent.WhiteTex);
            GUI.color = EffectColor(remaining);
            if (fillRect.width > 0.5f)
                GUI.DrawTexture(fillRect, BaseContent.WhiteTex);
            GUI.color = oldColor;
        }

        private static Color EffectColor(float remaining)
        {
            if (remaining >= 0.70f)
                return new Color(0.24f, 0.82f, 0.33f, 0.95f);
            if (remaining >= 0.40f)
                return new Color(0.95f, 0.72f, 0.18f, 0.95f);
            return new Color(0.92f, 0.18f, 0.14f, 0.95f);
        }
    }
}
