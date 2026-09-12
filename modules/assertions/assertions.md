---
title: "Assertions mit dem Constraint-Modell"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Der Assert-Teil ist das Herz eines Tests: Hier wird behauptet, wie das Ergebnis aussehen muss. Man könnte das mit einem `if` und einer Exception nachbauen – aber dann müsste man bei jedem Fehlschlag selbst formulieren, was erwartet wurde und was stattdessen kam. NUnit nimmt dir das mit dem **Constraint-Modell** ab: Du beschreibst die Erwartung als lesbaren Ausdruck wie `Is.EqualTo(new Position(1, 1))` oder `Does.Contain("Schlüssel")`, und NUnit erzeugt daraus sowohl die Prüfung als auch eine Fehlermeldung, die man ohne Debugger versteht.

## Das Muster: `Assert.That(actual, constraint)`

Jede Assertion in NUnit 4 hat dieselbe Form: `Assert.That` bekommt als erstes den tatsächlichen Wert und als zweites eine Bedingung, das *Constraint*. Die Constraints entstehen aus wenigen Einstiegspunkten, die sich fast wie ein englischer Satz lesen – hier fünf Zeilen aus den Tests des Adventures:

```csharp
Assert.That(f.Spieler.Position, Is.EqualTo(new Position(1, 1)));      // Wertgleichheit
Assert.That(f.Spieler.Inventar.Enthaelt<Schluessel>(), Is.True);      // bool
Assert.That(f.StatischesObjektAn(new Position(1, 0)), Is.Null);       // Schlüssel ist weg
Assert.That(f.Status, Is.EqualTo(Spielstatus.Gewonnen));              // enum
Assert.That(wieder.AlsText(), Is.EqualTo(feld.AlsText()));            // ganze Karte
```

`Is.EqualTo` nutzt `Equals` – deshalb funktioniert der Vergleich mit `new Position(1, 1)`, obwohl das ein frisch erzeugter Wert ist: `Position` ist ein `readonly record struct` und bringt Wertgleichheit automatisch mit, wie wir es bei [Hashcodes und Equals](/modules/hashcodes_equals/hashcodes_equals.md) besprochen haben. Bei Objekten mit Referenzgleichheit prüft `Is.SameAs` dagegen Identität: Ist es *genau dieses* Objekt? `Is.Not` kehrt jedes Constraint um (`Is.Not.Null`, `Is.False` ist die Kurzform für `Is.Not.True`).

Die letzte Zeile ist ein hübscher Spezialfall: `AlsText()` zeichnet das ganze Spielfeld als Zeichenkette. Ein einziges `Is.EqualTo` vergleicht damit jedes Feld der Karte auf einmal – nach dem Laden eines Spielstands muss das wiederhergestellte Feld Zeichen für Zeichen dem gespeicherten entsprechen.

## Texte und Sammlungen: `Does`, `Has`, `Is.Empty`

Für Zeichenketten, Listen und andere `IEnumerable`s gibt es Constraints, die hineinschauen statt auf Gleichheit zu prüfen:

```csharp
Assert.That(f.LetzteMeldung, Does.Contain("Schlüssel"));              // Teilstring
Assert.That(f.AlsText(), Does.Contain("@"));
Assert.That(File.ReadAllText(pfad), Does.Contain("\"Punkte\": 100")); // JSON-Datei
Assert.That(quelle.Laden("mini").Zeilen, Has.Count.EqualTo(3));       // ICollection
Assert.That(f.AlsText().TrimEnd().Split('\n'), Has.Length.EqualTo(3));// Array
Assert.That(quelle.LevelNamen, Is.EqualTo(new[] { "mini" }));         // Elemente einzeln
```

`Does.Contain` ist die richtige Wahl für Meldungen an die Spielerin: Der Test soll festhalten, dass der fehlende Schlüssel erwähnt wird, ohne die Formulierung „Die Tür ist verschlossen. Du brauchst einen Schlüssel.“ Wort für Wort einzubetonieren – sonst wird jede Textkorrektur zum Testfehler. `Has.Count` greift auf die Property `Count` zu, `Has.Length` auf `Length`; wer das verwechselt, bekommt eine klare Meldung vom Analyzer. Bei Sammlungen vergleicht `Is.EqualTo` elementweise in der gegebenen Reihenfolge. Daneben gibt es `Is.Empty`, `Has.All.Matches<T>(...)` mit einem Lambda für alle Elemente, `Has.Some`, `Has.None`, `Has.Exactly(2).Items`, `Is.Ordered` und `Is.EquivalentTo(...)` für „gleiche Elemente, beliebige Reihenfolge“.

## Gleitkommazahlen: `Within`

Die Spielregeln rechnen mit ganzen Zahlen – Lebenspunkte, Punkte und Manhattan-Entfernungen sind `int`, und dort ist `Is.EqualTo` exakt richtig. Sobald `double` im Spiel ist, sieht das anders aus: `0.1 + 0.2` ist nicht exakt `0.3`, und ein exakter Vergleich schlägt dann zufällig fehl. Für Gleitkommazahlen gibt man deshalb immer eine Toleranz an, wie im Beispielprojekt `Bruch`:

```csharp
Assert.That(b.AlsDezimalzahl(), Is.EqualTo(0.75).Within(1e-9));
```

`Within(1e-9)` bedeutet: Der Unterschied darf bis zu einem Milliardstel betragen. Alternativ gibt es `Within(1).Percent` für relative Toleranzen. Merke dir die Regel andersherum – *jeder* `double`-Vergleich ohne `Within` ist verdächtig.
{: .notice--warning}

## Exceptions: `Throws`

Dass eine Methode in einem Fehlerfall eine Exception wirft, ist Teil ihres Vertrags und gehört getestet. Statt `try`/`catch` übergibt man `Assert.That` ein Lambda und beschreibt die erwartete Exception:

```csharp
Assert.That(() => quelle.Laden("gibtEsNicht"), Throws.TypeOf<FileNotFoundException>());

Assert.That(() => new EingebauteLevelQuelle().Laden("Dachboden"),
            Throws.TypeOf<KeyNotFoundException>().With.Message.Contains("Dachboden"));
```

Das Lambda ist nötig, weil `Assert.That` den Code selbst ausführen muss, um die Exception zu fangen – würde man `quelle.Laden("gibtEsNicht")` direkt hinschreiben, flöge sie schon beim Auswerten des Arguments und der Test wäre rot, bevor NUnit etwas prüfen kann. `Throws.TypeOf<T>()` verlangt genau den Typ, `Throws.InstanceOf<T>()` akzeptiert auch Unterklassen. Mit `.With.Message.Contains(...)` lässt sich zusätzlich die Fehlermeldung prüfen – das lohnt sich, wenn der Text beim Benutzer ankommt. Das Gegenstück `Throws.Nothing` stellt sicher, dass ein Aufruf *keine* Exception auslöst.

## Mehrere Fälle: `[TestCase]`

Wenn ein Test mehrmals mit anderen Werten laufen soll, muss man ihn nicht kopieren. `[TestCase]` liefert Parameter an die Testmethode, und NUnit erzeugt für jede Zeile einen eigenen Testfall mit eigenem Namen. Für `Position.Verschoben` gibt es genau vier interessante Fälle – einen pro Himmelsrichtung:

```csharp
[TestCase(Richtung.Oben, 2, 1)]
[TestCase(Richtung.Unten, 2, 3)]
[TestCase(Richtung.Links, 1, 2)]
[TestCase(Richtung.Rechts, 3, 2)]
public void Verschoben_LiefertNachbarfeld(Richtung richtung, int erwartetX, int erwartetY)
{
    Position start = new Position(2, 2);

    Position ziel = start.Verschoben(richtung);

    Assert.That(ziel, Is.EqualTo(new Position(erwartetX, erwartetY)));
}
```

In der Testausgabe erscheinen dann vier Tests: `Verschoben_LiefertNachbarfeld(Oben,2,1)` und so weiter. Vertauscht jemand beim Refactoring `Y - 1` und `Y + 1`, sieht man sofort, welche Richtungen betroffen sind. `[TestCase]` ersetzt `[Test]`; für kompliziertere Daten – etwa ganze Levelkarten – gibt es `[TestCaseSource]` mit einer Methode oder einem Feld als Datenquelle.

## Mehrere Prüfungen: `Assert.Multiple`

Normalerweise beendet die erste fehlgeschlagene Assertion den Test – die weiteren werden gar nicht mehr ausgeführt. Wenn man mehrere Eigenschaften desselben Ergebnisses prüft, will man aber alle Abweichungen auf einmal sehen:

```csharp
Assert.Multiple(() =>
{
    Assert.That(f.Spieler.Position, Is.EqualTo(new Position(1, 1)));
    Assert.That(f.LetzteMeldung, Does.Contain("nicht weiter"));
    Assert.That(f.Runde, Is.EqualTo(1));
});
```

Innerhalb von `Assert.Multiple` werden alle Assertions ausgeführt und die Fehlschläge am Ende gesammelt gemeldet. Das ist etwas anderes als „mehrere Sachen in einem Test“: Alle drei Zeilen prüfen dasselbe Verhalten (der Zug gegen die Wand), nur an drei Stellen des Spielfelds. Ohne `Assert.Multiple` erführest du beim ersten Fehlschlag nicht, ob auch die Meldung falsch ist.

## Warum das alles? Lesbare Fehlermeldungen

Der eigentliche Grund für das Constraint-Modell zeigt sich erst, wenn ein Test rot wird. Angenommen, jemand ändert `Spielfeld.IstFrei` so, dass Wände plötzlich passierbar sind. `dotnet test` meldet dann:

```
Failed Wand_Blockiert [4 ms]
  Error Message:
     Assert.That(f.Spieler.Position, Is.EqualTo(new Position(1, 1)))
    Expected: (1, 1)
    But was:  (2, 1)
```

Erwartung, tatsächlicher Wert und sogar der Assertion-Ausdruck stehen in der Meldung – ohne dass du eine eigene Fehlermeldung formulieren musstest. Man sieht direkt: Der Held ist ein Feld nach rechts gelaufen, obwohl dort eine Wand steht. Bei Sammlungen zeigt NUnit zusätzlich, an welchem Index der erste Unterschied liegt; beim Vergleich zweier `AlsText()`-Karten ist das die Stelle, an der sich die Spielfelder unterscheiden. Ein selbstgebautes `if (!position.Equals(erwartet)) throw new Exception("falsch")` könnte das nicht.

Damit die Meldung etwas aussagt, muss `ToString()` etwas aussagen. `Position` überschreibt es zu `(1, 1)`, und `Spielobjekt.ToString()` liefert `Beschreibung()`, also etwa `Tür bei (2, 0)`. Ohne diese Überschreibungen stünde in der Meldung `<Adventure.Kern.Tuer>` – zweimal, für Erwartung und Ergebnis. Wer Klassen testbar machen will, gibt ihnen eine sinnvolle `ToString`-Darstellung.
{: .notice--primary}

## Legacy: `ClassicAssert`

In älterem Code und in vielen Tutorials findest du noch `Assert.AreEqual(3, b.Zaehler)`, `Assert.IsTrue(...)` oder `Assert.IsNull(...)`. Diese klassischen Asserts wurden in NUnit 4 aus `Assert` entfernt und in die Klasse `ClassicAssert` im Namespace `NUnit.Framework.Legacy` verschoben. Sie funktionieren noch, sind aber weniger ausdrucksstark – und die Reihenfolge von Erwartung und Ergebnis ist eine beliebte Fehlerquelle, weil sie genau umgekehrt zu `Assert.That` ist. Neuen Code schreibst du mit `Assert.That`; wenn du ein altes Projekt auf NUnit 4 hebst, reicht ein `using NUnit.Framework.Legacy;` und aus `Assert.AreEqual` wird `ClassicAssert.AreEqual`.

Übung: Schreibe einen Test für `Position.Entfernung` mit mindestens vier `[TestCase]`-Zeilen, darunter der Fall „gleiche Position“ und ein Fall mit negativer Differenz in beiden Achsen. Ergänze anschließend in `SpielfeldTests` eine Prüfung mit `Assert.Multiple`, die nach `Feld("@T")` und einem Zug nach rechts Punkte, Meldung und `IstGeoeffnet` der Truhe gemeinsam prüft.
{: .notice--info}

## Weitere Quellen

- [Constraint-Modell – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/writing-tests/assertions/assertion-models/constraint.html)
- [Übersicht aller Constraints – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/writing-tests/constraints/Constraints.html)
- [TestCase-Attribut – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/writing-tests/attributes/testcase.html)
- [Legacy-Asserts in NUnit 4 – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/release-notes/Nunit4.0-MigrationGuide.html)
