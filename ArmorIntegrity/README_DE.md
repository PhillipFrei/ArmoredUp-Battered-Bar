# Armor Integrity – Prototyp

Dieser Prototyp ersetzt die normale RimWorld-Rüstungsberechnung für Pawns durch ein deterministisches Layer-System.

## Kernformel

Pro getroffener aktiver Rüstungsschicht:

`Verschleiß in Prozentpunkten = Damage × AP / aktuelle AR × DegradationFactor`

Standard: `DegradationFactor = 0.10`.

- Apparel mit Hitpoints verliert denselben Prozentanteil seiner MaxHP.
- Gene, Implantate, natürliche Rüstung und andere Pawn-Armor-Stats erhalten denselben Wert als Battered.
- 30% Battered = 30% weniger AR.
- Battered regeneriert standardmäßig mit 50 Prozentpunkten pro RimWorld-Tag.

## Durchschlag

Wenn `AP <= aktuelle AR`, stoppt der normale Treffer an dieser Schicht.

Wenn `AP > aktuelle AR`:

- `Rest-AP = AP - AR`
- `Rest-Damage = Damage × (AP - AR) / AP`

Der Rest trifft die nächste passende Apparel-Schicht und danach die intrinsische Pawn-Rüstung. Erst nach der letzten Rüstungsschicht kann der Rest den Körper treffen.

Wenn ein Apparel-Teil durch denselben Treffer zerstört wird, erzeugt das allein keinen zusätzlichen Gratis-Durchschlag. Die Weitergabe basiert auf der AR vor diesem Treffer.

## AP-Fallback

Wenn ein Treffer keine brauchbare AP liefert:

`AP = Damage × 0.015`

## Noch nicht enthalten

- Blunt Trauma / zufälliger Leak-through durch intakte Rüstung
- Armored-Up Layer-Bonus
- Armored-Up globale Damage-/AP-Multiplikatoren
- Minimum-Durability-Damage
- 95%-Max-Block-Cap

Diese Mechaniken werden erst nach Praxistests des Kerns bewertet.
