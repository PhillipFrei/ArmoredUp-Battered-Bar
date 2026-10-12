# Armor Integrity – Prototyp v2

V2 vereinfacht das System bewusst auf echte Schicht-für-Schicht-Abnutzung.

## Kernformel

Pro Treffer auf die aktuell äußerste aktive Schutzschicht:

`Verschleiß in Prozentpunkten = Damage × AP / aktuelle Sharp-AR × DegradationFactor`

Standard: `DegradationFactor = 0.10`.

## Armor-Typ

V2 prüft jeden rüstungsrelevanten Treffer absichtlich gegen **Sharp/Schnitt-AR**.

- Sharp ist die einzige AR, die für die Kernberechnung benutzt und durch Zustand/Battered reduziert wird.
- Blunt und Heat werden aktuell nicht als parallele Degradationsspuren benutzt.
- Blunt ist für einen späteren Trauma-Mechanismus reserviert.

## Keine Weitergabe mehr

Ein Treffer wird immer vollständig von genau **einer** aktiven Rüstungsschicht behandelt.

- Äußerstes passendes Apparel nimmt Verschleiß.
- Der Treffer endet dort.
- Auch bei AP > AR gibt es keinen Rest-Damage und keine Rest-AP zur nächsten Schicht.
- Wenn das Apparel durch diesen Treffer zerstört wird, trifft erst der **nächste** Treffer die darunterliegende Schicht.
- Erst wenn keine Apparel-Schicht mehr schützt, wird intrinsische Rüstung belastet.
- Erst wenn auch intrinsische Rüstung auf 100% Battered / 0 AR steht, erreicht ein späterer Treffer den Pawn.

AP bleibt wichtig, weil hohe AP die Verschleißformel stark beschleunigt.

## Apparel

Apparel verliert echte Hitpoints direkt:

- 100% HP = 100% Sharp-AR
- 70% HP = 70% Sharp-AR
- 0 HP = zerstört

Der Verschleiß wird direkt auf die HP geschrieben und nicht mehr als Deterioration-DamageEvent erzeugt.

## Gene, Implantate und natürliche Rüstung

Intrinsische Rüstung verwendet einen eigenen regenerierenden Battered-Zustand:

- 30% Battered = 30% Sharp-AR verloren
- 100% Battered = 0 Sharp-AR
- Implantat/Gen wird nicht zerstört
- Standardregeneration: 50 Prozentpunkte pro RimWorld-Tag

## Ein Balken / ein sichtbarer Battered-Hediff

Es wird nur **eine** sichtbare Battered-Anzeige pro Pawn verwendet.

- Wenn ein beschädigtes Apparel die äußerste sichtbare beschädigte Schicht ist, zeigt Balken/Hediff dessen verlorene HP in Prozent.
- Wenn keine beschädigte Apparel-Schicht angezeigt wird, zeigt er den intrinsischen Battered-Zustand.
- Innere beschädigte Zustände werden nicht gleichzeitig als weitere Balken dargestellt.

## AP-Fallback

Wenn ein Treffer keine brauchbare AP liefert:

`AP = Damage × 0.015`

## Noch nicht enthalten

- Blunt Trauma
- zufälliger Leak-through durch intakte Rüstung
- Armored-Up Layer-Bonus
- Armored-Up globale Damage-/AP-Multiplikatoren
- Minimum-Durability-Damage
- 95%-Max-Block-Cap
