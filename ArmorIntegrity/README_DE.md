# Armor Integrity 1.1 – RimWorld 1.6

Armor Integrity ist ein schlankes Rüstungssystem mit Armored-Up-/Vanilla-Layerreihenfolge, eigener Verschleißformel, Restschaden durch mehrere Schichten und genau einem kontextbezogenen Rüstungsbalken.

## 1. Layer-Reihenfolge

Für das tatsächlich getroffene Körperteil wird `WornApparel` von hinten nach vorne geprüft – dieselbe Grundreihenfolge wie in RimWorld und Armored Up.

Nur Apparel, das `CoversBodyPart(part)` erfüllt und aktive Sharp-AR besitzt, nimmt an der Berechnung teil.

Nach allen passenden Apparel-Schichten kommt intrinsische Rüstung aus Pawn-Stats, z. B. Gene, Implantate oder natürliche Rüstung.

## 2. Armor-Kanal

Armor Integrity verwendet absichtlich **Sharp-AR für jeden rüstungsrelevanten Treffer**.

Blunt und Heat werden nicht als parallele Zustandskanäle abgesenkt.

## 3. Verschleiß

Pro Schicht:

`Verschleiß in Prozentpunkten = Damage × AP / aktuelle Sharp-AR × DegradationFactor`

Standard:

`DegradationFactor = 0.10`

Die aktuelle AR wird vor diesem Treffer gelesen.

## 4. Block und Weitergabe

Nominaler Blockanteil:

`NominalBlock = min(1, currentAR / AP)`

Nach dem Block wird AP wie bisher weitergegeben:

`RestAP = AP × (1 - EffectiveBlock)`

Der Restschaden bekommt zusätzlich einen einstellbaren Dämpfungsfaktor:

`RestDamage = Damage × (1 - EffectiveBlock) × PassthroughDamageFactor`

Standard:

`PassthroughDamageFactor = 0.75`

Der Regler reicht von 0.25 bis 1.00. Er wird nach jeder tatsächlich verarbeiteten Rüstungsschicht erneut angewendet. AP bekommt keinen zusätzlichen Koeffizienten.

## 5. Haltbarkeitsgrenze bei großen Treffern

Eine Schicht darf nicht mehr blocken, als ihre verbleibende Integrität tragen kann.

Wenn der Treffer mehr Verschleiß verlangen würde als noch Zustand vorhanden ist:

`CapacityFraction = verbleibender Zustand / verlangter Verschleiß`

`EffectiveBlock = NominalBlock × CapacityFraction`

Die Schicht wird vollständig verbraucht und der nicht mehr tragbare Teil desselben Treffers geht weiter.

Damit kann ein schwaches Kleidungsstück keine riesige Explosion vollständig verschlucken.

## 6. Apparel-HP

Apparel-Zustand ist direkt an seine HP gekoppelt:

- 100% Zustand -> 100% Sharp-AR
- 70% Zustand -> 70% Sharp-AR
- 0% Zustand -> Apparel wird zerstört

Nachkommastellen des berechneten HP-Verlusts werden gespeichert.

Beispiel:

`0.27 + 0.27 + 0.27 + 0.27 = 1 echter HP Verlust + 0.08 Rest`

Auch der gespeicherte Rest zählt bereits für die effektive Sharp-AR und den Balken.

Wenn ein Item repariert wird, wird ein alter Fractional-Rest automatisch verworfen.

## 7. Intrinsische Rüstung

Intrinsische Rüstung hat keine Apparel-HP und verwendet deshalb `AI_IntrinsicArmorWear`:

- 30% Battered -> 30% weniger Sharp-AR
- 100% Battered -> 0 Sharp-AR
- Gen/Implantat/natürliche Rüstung wird nicht zerstört
- Standardregeneration: 50 Prozentpunkte pro RimWorld-Tag

Wenn intrinsische Rüstung durch einen großen Treffer vollständig erschöpft wird, geht der verbleibende Teil desselben Treffers an den Körper weiter.

## 8. Der eine Rüstungsbalken

Armor Integrity speichert nur das Körperteil des letzten rüstungsrelevanten Treffers.

Für dieses Körperteil wird jedes Mal dieselbe äußere-zu-innere Reihenfolge wie im Kampf verwendet:

1. aktuelle äußerste noch vorhandene Armor finden
2. ist sie unbeschädigt -> kein Balken
3. ist sie beschädigt -> ihr exakter Zustand wird angezeigt
4. ist sie zerstört -> die nächste überlebende Schicht wird automatisch zur äußeren Schicht
5. gibt es auf diesem Trefferpfad kein Apparel mehr -> intrinsisches Battered anzeigen
6. tote Pawns zeigen keinen Balken

Beschädigte innere Kleidung wird also nicht mehr angezeigt, solange darüber eine intakte Rüstungsschicht liegt.

## 9. AP-Fallback

Wenn die eingehende AP null oder unbrauchbar ist:

`AP = Damage × FallbackAPPerDamage`

Standard:

`0.015`

## 10. Nicht enthalten

Bewusst nicht übernommen wurden:

- Blunt Trauma
- zufälliger Leak-through
- Armored-Up Layer-Bonus
- Primitive-vs-Guns-Malus
- Hard-Armor-Blunt-Bonus
- globale Damage-/AP-Multiplikatoren
- 10%-Mindestwirkung zerstörter Rüstung
- Schutz, der Armor künstlich bei 1 HP hält

## Installation

1. Alte `ArmorIntegrity`-Version vollständig aus dem Mods-Ordner löschen.
2. Den neuen `ArmorIntegrity`-Ordner einsetzen.
3. Armored Up und alte separate Battered-Bar-Addons deaktivieren.
4. Harmony und Armor Integrity aktivieren.
5. RimWorld vollständig neu starten, weil die DLL nicht zur Laufzeit neu geladen wird.

Bei aktiviertem `Debug-Kampflogging` schreibt jede verarbeitete Schicht Damage, AP, Sharp-AR, Blockanteil, Kapazität, Verschleiß und Restwerte ins Player.log.
