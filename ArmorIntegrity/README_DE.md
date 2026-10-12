# Armor Integrity – Prototyp v2.2

## Kernregel

Ein Treffer belastet genau **eine** Rüstungsschicht: die äußerste passende aktive Schicht.

`Verschleiß in Prozentpunkten = Damage × AP / aktuelle Sharp-AR × DegradationFactor`

Standard: `DegradationFactor = 0.10`.

Es gibt keine Weitergabe von Rest-Damage oder Rest-AP. Wird eine Schicht durch einen Treffer zerstört, trifft erst der nächste Treffer die darunterliegende Schicht.

## Sharp als einziger Kernwert

Alle rüstungsrelevanten Treffer werden gegen **Sharp/Schnitt-AR** geprüft. Nur Sharp wird durch den Rüstungszustand reduziert. Blunt/Heat bleiben für spätere Mechaniken unangetastet.

## Präziser Apparel-Verschleiß

Apparel mit normalen HP verliert echte HP. Bruchteile eines HP werden zwischen Treffern gespeichert und gehen nicht durch Rundung verloren.

## Ein Balken – exakt die getroffene Schicht

V2.2 ändert die Anzeige grundlegend:

- Sobald eine Rüstungsschicht einen Treffer abfängt, merkt sich der Mod genau dieses Apparel.
- Der eine Balken zeigt den verbleibenden Zustand genau dieser Schicht.
- Weitere Treffer auf dieselbe Schicht reduzieren denselben Balken.
- Wird die Schicht zerstört, verschwindet ihr Balken.
- Der nächste Treffer auf die darunterliegende Schicht lässt denselben einzelnen Balken für diese neue Schicht erscheinen.
- Trifft der Kampf schließlich intrinsische Rüstung aus Genen/Implantaten, zeigt derselbe Balken den intrinsischen Battered-Zustand.

Der Balken versucht nicht mehr selbst, aus allen getragenen Kleidungsstücken eine Schicht zu erraten.

## Intrinsische Rüstung

Gene, Implantate, Rassenarmor und andere Pawn-Armor verwenden regenerierendes Battered:

- 30% Battered = 30% weniger Sharp-AR
- 100% Battered = 0 Sharp-AR
- Gen/Implantat wird nicht zerstört
- Standardregeneration: 50 Prozentpunkte pro RimWorld-Tag

## Alte Armored-Up-Saves

Der alte `BatteredArmor`-DefName bleibt als Save-Kompatibilität enthalten.

## Diagnose

Die ersten 80 verarbeiteten Rüstungstreffer schreiben automatisch `[Armor Integrity TRACE]` ins Player.log.
