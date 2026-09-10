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

Der Assert-Teil ist das Herz eines Tests: Hier wird behauptet, wie das Ergebnis aussehen muss. Man könnte das mit einem `if` und einer Exception nachbauen – aber dann müsste man bei jedem Fehlschlag selbst formulieren, was erwartet wurde und was stattdessen kam. NUnit nimmt dir das mit dem **Constraint-Modell** ab: Du beschreibst die Erwartung als lesbaren Ausdruck wie `Is.EqualTo(3)` oder `Has.Count.EqualTo(1)`, und NUnit erzeugt daraus sowohl die Prüfung als auch eine Fehlermeldung, die man ohne Debugger versteht.

## Das Muster: `Assert.That(actual, constraint)`

Jede Assertion in NUnit 4 hat dieselbe Form: `Assert.That` bekommt als erstes den tatsächlichen Wert und als zweites eine Bedingung, das *Constraint*. Die Constraints entstehen aus wenigen Einstiegspunkten, die sich fast wie ein englischer Satz lesen:

```csharp
Assert.That(b.Zaehler, Is.EqualTo(3));          // Gleichheit über Equals
Assert.That(verwaltung.Suchen("d"), Is.Not.Null);
Assert.That(verwaltung.Entfernen(kreis), Is.True);
Assert.That(verwaltung.AlleFiguren[0], Is.SameAs(kreis));   // dieselbe Referenz
Assert.That(geladen[1], Is.TypeOf<Kreis>());
```

`Is.EqualTo` nutzt `Equals` – deshalb funktioniert `Is.EqualTo(new Bruch(5, 6))`, obwohl es ein anderes Objekt ist: `Bruch` implementiert `IEquatable<Bruch>`, wie wir es bei [Hashcodes und Equals](/modules/hashcodes_equals/hashcodes_equals.md) besprochen haben. `Is.SameAs` prüft dagegen Identität: Ist es *genau dieses* Objekt? `Is.Not` kehrt jedes Constraint um.

## Gleitkommazahlen: `Within`

`0.1 + 0.2` ist in `double` nicht exakt `0.3`, und `Math.PI * 1 * 1` ist nicht auf jedes Bit gleich `Math.PI`, sobald noch eine Summe im Spiel ist. Ein exakter Vergleich schlägt dann zufällig fehl. Für Gleitkommazahlen gibt man deshalb immer eine Toleranz an:

```csharp
Assert.That(b.AlsDezimalzahl(), Is.EqualTo(0.75).Within(1e-9));
Assert.That(verwaltung.GesamtFlaeche(), Is.EqualTo(6 + Math.PI).Within(1e-9));
```

`Within(1e-9)` bedeutet: Der Unterschied darf bis zu einem Milliardstel betragen. Alternativ gibt es `Within(1).Percent` für relative Toleranzen.

## Sammlungen: `Has`, `Is.Empty`, `Does.Contain`

Für Listen und andere `IEnumerable`s gibt es eigene Constraints, die auf die Elemente schauen:

```csharp
Assert.That(verwaltung.AlleFiguren, Has.Count.EqualTo(1));
Assert.That(speicher.Laden(), Is.Empty);
Assert.That(namen, Does.Contain("k1"));
Assert.That(figuren, Has.All.Matches<Figur>(f => f.Flaeche > 0));
```

`Has.All.Matches<T>` bekommt ein Lambda, das für jedes Element gelten muss – hier: alle Flächen sind positiv. Analog gibt es `Has.Some`, `Has.None`, `Has.Exactly(2).Items` sowie `Is.Ordered` und `Is.EquivalentTo(...)` (gleiche Elemente, beliebige Reihenfolge). `Does.Contain` funktioniert auch mit Strings: `Assert.That(text, Does.Contain("Fläche"))`, ebenso `Does.StartWith` und `Does.EndWith`.

## Exceptions: `Throws`

Dass eine Methode in einem Fehlerfall eine Exception wirft, ist Teil ihres Vertrags und gehört getestet. Statt `try`/`catch` übergibt man `Assert.That` ein Lambda und beschreibt die erwartete Exception:

```csharp
Assert.That(() => new Bruch(1, 0), Throws.ArgumentException);

Assert.That(() => verwaltung.Hinzufuegen(new Rechteck("k1", 0, 0, 2, 3)),
            Throws.ArgumentException);

Assert.That(() => new Bruch(1, 0),
            Throws.TypeOf<ArgumentException>().With.Message.Contains("Nenner"));
```

Das Lambda ist nötig, weil `Assert.That` den Code selbst ausführen muss, um die Exception zu fangen – würde man `new Bruch(1, 0)` direkt hinschreiben, flöge sie schon beim Auswerten des Arguments. `Throws.TypeOf<T>()` verlangt genau den Typ, `Throws.InstanceOf<T>()` akzeptiert auch Unterklassen. Mit `.With.Message.Contains(...)` lässt sich zusätzlich die Fehlermeldung prüfen – das lohnt sich vor allem, wenn eine Methode dieselbe Exception aus mehreren Gründen wirft. Das Gegenstück `Throws.Nothing` stellt sicher, dass ein Aufruf *keine* Exception auslöst.

## Mehrere Fälle: `[TestCase]`

Wenn ein Test mehrmals mit anderen Werten laufen soll, muss man ihn nicht kopieren. `[TestCase]` liefert Parameter an die Testmethode, und NUnit erzeugt für jede Zeile einen eigenen Testfall mit eigenem Namen:

```csharp
[TestCase(1, 2, 0.5)]
[TestCase(3, 4, 0.75)]
[TestCase(-1, 4, -0.25)]
public void AlsDezimalzahl_LiefertErwartetenWert(int zaehler, int nenner, double erwartet)
{
    Bruch b = new Bruch(zaehler, nenner);

    Assert.That(b.AlsDezimalzahl(), Is.EqualTo(erwartet).Within(1e-9));
}
```

In der Testausgabe erscheinen dann drei Tests: `AlsDezimalzahl_LiefertErwartetenWert(1,2,0.5)` und so weiter. Schlägt nur der negative Fall fehl, sieht man das sofort. `[TestCase]` ersetzt `[Test]`; für kompliziertere Daten (Objekte, Listen) gibt es `[TestCaseSource]`.

## Mehrere Prüfungen: `Assert.Multiple`

Normalerweise beendet die erste fehlgeschlagene Assertion den Test – die weiteren werden gar nicht mehr ausgeführt. Wenn man mehrere Eigenschaften desselben Ergebnisses prüft, will man aber alle Abweichungen auf einmal sehen:

```csharp
Assert.Multiple(() =>
{
    Assert.That(b.Zaehler, Is.EqualTo(3));
    Assert.That(b.Nenner, Is.EqualTo(4));
});
```

Innerhalb von `Assert.Multiple` werden alle Assertions ausgeführt und die Fehlschläge am Ende gesammelt gemeldet. Das ist etwas anderes als „mehrere Sachen in einem Test“: Beide Zeilen prüfen ein Verhalten (das Kürzen), nur an zwei Properties.

## Warum das alles? Lesbare Fehlermeldungen

Der eigentliche Grund für das Constraint-Modell zeigt sich erst, wenn ein Test rot wird. Angenommen, `Plus` hätte einen Fehler und addierte Zähler und Nenner getrennt. `dotnet test` meldet dann:

```
Failed Plus_EinHalbPlusEinDrittel_ErgibtFuenfSechstel [14 ms]
  Error Message:
     Assert.That(summe, Is.EqualTo(new Bruch(5, 6)))
    Expected: <5/6>
    But was:  <2/5>
```

Erwartung, tatsächlicher Wert und sogar der Assertion-Ausdruck stehen in der Meldung – ohne dass du eine eigene Fehlermeldung formulieren musstest. Bei Sammlungen zeigt NUnit zusätzlich, an welchem Index der erste Unterschied liegt. Ein selbstgebautes `if (!summe.Equals(erwartet)) throw new Exception("falsch")` könnte das nicht.

Damit die Meldung etwas aussagt, muss `ToString()` etwas aussagen. `Bruch` überschreibt es (`5/6`); ohne die Überschreibung stünde dort `<Bruch.Bruch>` – zweimal, für Erwartung und Ergebnis. Wer Klassen testbar machen will, gibt ihnen eine sinnvolle `ToString`-Darstellung.
{: .notice--primary}

## Legacy: `ClassicAssert`

In älterem Code und in vielen Tutorials findest du noch `Assert.AreEqual(3, b.Zaehler)`, `Assert.IsTrue(...)` oder `Assert.IsNull(...)`. Diese klassischen Asserts wurden in NUnit 4 aus `Assert` entfernt und in die Klasse `ClassicAssert` im Namespace `NUnit.Framework.Legacy` verschoben. Sie funktionieren noch, sind aber weniger ausdrucksstark – und die Reihenfolge von Erwartung und Ergebnis ist eine beliebte Fehlerquelle. Neuen Code schreibst du mit `Assert.That`; wenn du ein altes Projekt auf NUnit 4 hebst, reicht ein `using NUnit.Framework.Legacy;` und `Assert.AreEqual` wird zu `ClassicAssert.AreEqual`.

Übung: Schreibe für `Bruch.Mal` einen `[TestCase]`-Test mit mindestens vier Fällen, darunter einer, bei dem das Ergebnis eine ganze Zahl ist, und einer mit negativem Ergebnis. Prüfe außerdem mit `Throws.TypeOf<ArgumentException>().With.Message.Contains(...)`, dass die Meldung bei Nenner 0 das Wort „Nenner“ enthält.
{: .notice--info}

## Weitere Quellen

- [Constraint-Modell – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/writing-tests/assertions/assertion-models/constraint.html)
- [Übersicht aller Constraints – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/writing-tests/constraints/Constraints.html)
- [TestCase-Attribut – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/writing-tests/attributes/testcase.html)
- [Legacy-Asserts in NUnit 4 – NUnit-Dokumentation](https://docs.nunit.org/articles/nunit/release-notes/Nunit4.0-MigrationGuide.html)
