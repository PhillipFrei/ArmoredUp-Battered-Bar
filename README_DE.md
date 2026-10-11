# Armored Up - Battered Bar v1

Kleines Anzeige-Addon für RimWorld 1.6 und **Armored Up (continue)** (`tyjm.ArmoredUp`).

## Verhalten

- Kein Text, keine Prozentzahl: nur ein schmaler Balken über dem Pawn.
- Der Balken erscheint nur, wenn der Pawn `BatteredArmor` mit Severity > 0 besitzt.
- Er gilt für **alle gespawnten Pawns**: Kolonisten, Gegner, Tiere, Mechanoiden und modded Races, sofern Armored Up ihnen `BatteredArmor` gibt.
- Die Balkenfüllung zeigt die **noch verbleibende Wirksamkeit der von Battered betroffenen Rüstung**.
- Normale Apparel-Haltbarkeit wird nicht dargestellt. Apparel und `BatteredArmor` bleiben getrennte Systeme.

## Armored-Up-Berechnung

Die aus der verwendeten ArmoredUp.dll ermittelte Formel lautet:

```
remainingEffect = MinDamagedArmorEffectiveness
                + (1 - MinDamagedArmorEffectiveness)
                * (1 - BatteredArmor.Severity)
```

`MinDamagedArmorEffectiveness` wird zur Laufzeit direkt aus Armored Up gelesen. Falls das nicht möglich ist, verwendet das Addon 10 % als Fallback.

## GitHub bauen lassen

Die Datei `.github/workflows/build.yml` baut die DLL automatisch.

Nach erfolgreichem Workflow unter **Actions** den Build öffnen und unten das Artifact
`ArmoredUp_BatteredBar_RimWorld_1.6` herunterladen.

Das heruntergeladene Verzeichnis `ArmoredUp_BatteredBar` kommt in RimWorld `Mods` und wird nach **Armored Up (continue)** geladen.
