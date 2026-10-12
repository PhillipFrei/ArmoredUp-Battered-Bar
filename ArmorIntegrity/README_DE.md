# Armor Integrity – Prototyp v2.1

## Kernregel

Ein Treffer belastet genau **eine** Rüstungsschicht: die äußerste passende aktive Schicht.

`Verschleiß in Prozentpunkten = Damage × AP / aktuelle Sharp-AR × DegradationFactor`

Standard: `DegradationFactor = 0.10`.

Es gibt keine Weitergabe von Rest-Damage oder Rest-AP. Wird eine Schicht durch einen Treffer zerstört, trifft erst der nächste Treffer die darunterliegende Schicht.

## Sharp als einziger Kernwert

Alle rüstungsrelevanten Treffer werden gegen **Sharp/Schnitt-AR** geprüft. Nur Sharp wird durch den Rüstungszustand reduziert. Blunt/Heat bleiben für spätere Mechaniken, besonders Trauma, unangetastet.

## Präziser Apparel-Verschleiß

Apparel mit normalen HP verliert weiterhin echte HP. V2.1 speichert aber Bruchteile eines HP zwischen Treffern.

Beispiel:

- Treffer 1 berechnet 0,35 HP Verschleiß
- Treffer 2 berechnet 0,35 HP
- Treffer 3 berechnet 0,35 HP
- zusammen 1,05 HP -> 1 sichtbarer HP wird abgezogen, 0,05 bleibt gespeichert

Dadurch geht kein kleiner Treffer mehr durch Rundung verloren. Die effektive Sharp-AR und der Balken berücksichtigen auch den gespeicherten Bruchteil sofort.

Apparel ohne normale HP erhält einen eigenen virtuellen Integritätswert. Als physische äußere Schicht wird es bei vollständiger Erschöpfung zerstört.

## Nur ein Balken

Der Balken zeigt immer nur die äußerste bereits beschädigte Armor-Schicht. Nach deren Zerstörung kann derselbe Balken die nächste beschädigte Schicht anzeigen. Intrinsische Rüstung wird erst angezeigt, wenn keine beschädigte Apparel-Schicht davor angezeigt wird.

## Gene, Implantate, natürliche Armor

Intrinsische Armor benutzt regenerierendes Battered:

- 30% Battered = 30% weniger Sharp-AR
- 100% Battered = 0 Sharp-AR
- Gen/Implantat bleibt erhalten
- Standardregeneration: 50 Prozentpunkte pro RimWorld-Tag

## Alte Armored-Up-Saves

V2.1 enthält wieder einen kompatiblen `BatteredArmor`-DefName, damit alte Saves nach dem Entfernen von Armored Up ihre gespeicherten Hediffs nicht als fehlende Defs laden müssen.

## Diagnose

Die ersten 80 von Armor Integrity verarbeiteten Treffer schreiben automatisch Zeilen mit `[Armor Integrity TRACE]` ins Player.log. Damit sieht man Layer, Damage, AP, Sharp-AR, berechneten Verschleiß und tatsächlichen HP-Abzug.

## Noch nicht enthalten

- Blunt Trauma
- zufälliger Leak-through
- Damage/AP-Weitergabe zwischen Layern
- globale Armored-Up Damage-/AP-Multiplikatoren
- Layer-Bonus
- 95%-Max-Block-Cap
