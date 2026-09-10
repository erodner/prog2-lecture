---
title: "Iterator"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Seit Programmierung 1 schreiben wir `foreach (var x in liste)` und denken nicht darüber nach, was dabei passiert. Dabei ist `foreach` keine Magie: Es funktioniert mit Arrays, mit `List<T>`, mit `Dictionary<K, V>`, mit LINQ-Abfragen und mit Klassen, die du selbst schreibst – obwohl diese Datenstrukturen intern völlig unterschiedlich aufgebaut sind. Ein Array liegt am Stück im Speicher, ein Dictionary besteht aus Buckets, eine LINQ-Abfrage hat noch nicht einmal Daten. Dass `foreach` trotzdem mit allen umgehen kann, verdankt es dem **Iterator**-Muster (Verhaltensmuster, auch *Cursor* genannt): Die Datenstruktur gibt ein eigenes Objekt heraus, das weiß, wie man sie durchläuft, und der Client fragt dieses Objekt Element für Element ab, ohne den inneren Aufbau zu kennen.

## Problem

Eine Datenstruktur soll durchlaufen werden können, ohne dass der Client ihren Aufbau kennt – und möglichst auf verschiedene Arten: vorwärts, rückwärts, nur ein Ausschnitt, nur jedes zweite Element. Würde die Datenstruktur ihre interne Liste einfach herausgeben, wäre die Kapselung dahin, und jede Durchlaufstrategie müsste der Client selbst programmieren. Außerdem soll es möglich sein, dieselbe Struktur mit zwei unabhängigen Durchläufen gleichzeitig zu bearbeiten, etwa in einer verschachtelten Schleife.

## Lösung: `IEnumerable<T>` und `IEnumerator<T>`

In .NET ist das Muster fest in die Sprache eingebaut, mit zwei Rollen und zwei Interfaces:

- **Aggregat** (*Aggregate*): die Datenstruktur. Sie implementiert `IEnumerable<T>` und hat damit genau eine Methode, `GetEnumerator()`, die ein neues Iterator-Objekt liefert.
- **Iterator** (*Iterator*): das Objekt, das den Durchlauf kennt. Es implementiert `IEnumerator<T>` mit der Property `Current` (das aktuelle Element), der Methode `MoveNext()` (einen Schritt weiter; liefert `false`, wenn nichts mehr kommt) und `Reset()` (zurück an den Anfang; wird kaum genutzt).

Ein `foreach` ist nur Zucker für genau diese Aufrufe. Der Compiler übersetzt

```csharp
foreach (string buch in regal)
    Console.WriteLine(buch);
```

in etwa in diese Schleife:

```csharp
IEnumerator<string> e = regal.GetEnumerator();
try
{
    while (e.MoveNext())
        Console.WriteLine(e.Current);
}
finally
{
    e.Dispose();
}
```

Der Iterator ist ein *eigenes* Objekt mit eigenem Zustand (der aktuellen Position). Deshalb können zwei Schleifen gleichzeitig über dasselbe Regal laufen: Jede hat ihren eigenen Enumerator. Das `finally` mit `Dispose()` sorgt dafür, dass ein Iterator, der Ressourcen hält (eine offene Datei etwa), diese auch bei einem `break` freigibt.

## Eine eigene Collection per Hand

Um das Muster ohne Abkürzung zu sehen, bauen wir ein `Regal`, das Buchtitel in einem Array speichert, und schreiben den Enumerator selbst. Das Regal ist das Aggregat:

```csharp
using System.Collections;   // für das nicht-generische IEnumerable

public class Regal : IEnumerable<string>
{
    private readonly string[] buecher;

    public Regal(params string[] buecher) => this.buecher = buecher;

    public IEnumerator<string> GetEnumerator() => new RegalEnumerator(buecher);

    // nicht-generische Altlast, die IEnumerable<T> verlangt:
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Die zweite `GetEnumerator`-Methode ist eine explizite Interface-Implementierung (siehe Modul [Interfaces – erweitert](/modules/interfaces_erweitert/interfaces_erweitert.md)), weil `IEnumerable<T>` vom alten, nicht-generischen `IEnumerable` erbt. Der Enumerator selbst merkt sich das Array und eine Position, die *vor* dem ersten Element beginnt:

```csharp
public class RegalEnumerator : IEnumerator<string>
{
    private readonly string[] buecher;
    private int position = -1;

    public RegalEnumerator(string[] buecher) => this.buecher = buecher;

    public string Current => buecher[position];
    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        position++;
        return position < buecher.Length;
    }

    public void Reset() => position = -1;
    public void Dispose() { }
}

Regal regal = new("Dune", "Neuromancer", "Solaris");
foreach (string buch in regal)
    Console.WriteLine(buch);
// Dune
// Neuromancer
// Solaris
```

Der Start bei `-1` ist wichtig: `foreach` ruft zuerst `MoveNext()` und dann `Current` auf – der erste `MoveNext()` bringt die Position auf 0. Das funktioniert, ist aber viel Code für eine simple Aufgabe: eine zweite Klasse, zwei explizite Interface-Methoden, ein `Dispose`, das nichts tut. Dabei tut die Klasse nichts anderes als „liefere die Elemente der Reihe nach“.

## Die Abkürzung: `yield return`

C# nimmt einem das Schreiben des Enumerators ab. Eine Methode, die `IEnumerator<T>` oder `IEnumerable<T>` zurückgibt und im Rumpf `yield return` verwendet, ist eine **Iterator-Methode**: Der Compiler erzeugt daraus die Enumerator-Klasse mit `MoveNext`, `Current` und dem gesamten Zustandsautomaten selbst. Das ganze `Regal` schrumpft auf

```csharp
public class Regal : IEnumerable<string>
{
    private readonly string[] buecher;

    public Regal(params string[] buecher) => this.buecher = buecher;

    public IEnumerator<string> GetEnumerator()
    {
        foreach (string buch in buecher)
            yield return buch;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Die Klasse `RegalEnumerator` ist verschwunden. `yield return` liefert einen Wert an die aufrufende Schleife und *merkt sich, wo es war* – beim nächsten `MoveNext()` läuft die Methode hinter dem `yield return` weiter, als wäre nichts gewesen. `yield break` beendet den Durchlauf vorzeitig, so wie ein `return` in einer normalen Methode:

```csharp
public IEnumerator<string> GetEnumerator()
{
    foreach (string buch in buecher)
    {
        if (buch.StartsWith("Verboten"))
            yield break;          // ab hier kommt nichts mehr
        yield return buch;
    }
}
```

Iterator-Methoden dürfen kein normales `return wert;` enthalten und keine `out`- oder `ref`-Parameter haben. Beides wäre mit dem Zustandsautomaten, den der Compiler erzeugt, nicht vereinbar.
{: .notice--primary}

## Spezifische Iteratoren

Das Muster verspricht *verschiedene Durchlaufstrategien* für dieselbe Struktur. Mit `yield` sind das einfach weitere Methoden mit Rückgabetyp `IEnumerable<T>` und beliebigem Namen – `foreach` akzeptiert jedes `IEnumerable<T>`, nicht nur das Objekt selbst:

```csharp
public IEnumerable<string> Bereich(int von, int bis)
{
    for (int i = von; i < bis && i < buecher.Length; i++)
        yield return buecher[i];
}

public IEnumerable<string> NurGerade()
{
    for (int i = 0; i < buecher.Length; i += 2)
        yield return buecher[i];
}

public IEnumerable<string> Rueckwaerts
{
    get
    {
        for (int i = buecher.Length - 1; i >= 0; i--)
            yield return buecher[i];
    }
}
```

Eine Property wie `Rueckwaerts` darf ebenfalls ein Iterator sein. Der Client wählt die Strategie beim Aufruf und bleibt bei allen dreien in derselben `foreach`-Schleife:

```csharp
Regal regal = new("A", "B", "C", "D", "E", "F", "G", "H");
foreach (string b in regal.Bereich(2, 7)) Console.Write(b + " ");   // C D E F G
foreach (string b in regal.NurGerade()) Console.Write(b + " ");     // A C E G
foreach (string b in regal.Rueckwaerts) Console.Write(b + " ");     // H G F E D C B A
```

## Iteratoren sind faul

Der wichtigste Aspekt von `yield` ist, dass der Rumpf der Methode **nicht beim Aufruf** läuft, sondern erst, wenn jemand `MoveNext()` ruft – und dann immer nur bis zum nächsten `yield return`. Das zeigt eine Ausgabe im Iterator:

```csharp
public IEnumerable<string> NurGerade()
{
    for (int i = 0; i < buecher.Length; i += 2)
    {
        Console.WriteLine($"  liefere Position {i}");
        yield return buecher[i];
    }
}

IEnumerable<string> gerade = regal.NurGerade();
Console.WriteLine("Iterator erzeugt");
foreach (string b in gerade)
{
    Console.WriteLine(b);
    if (b == "C") break;
}
// Iterator erzeugt
//   liefere Position 0
// A
//   liefere Position 2
// C
```

Zwischen dem Aufruf von `NurGerade()` und der Schleife passiert nichts, und nach dem `break` wird Position 4 nie berechnet. Genau dieses Verhalten kennen wir schon als [verzögerte Ausführung](/modules/linq_deferred_execution/linq_deferred_execution.md) von LINQ – und das ist kein Zufall: `Where`, `Select` und die anderen Operatoren aus der [Methodensyntax](/modules/linq_methodensyntax/linq_methodensyntax.md) sind Iterator-Methoden mit `yield return`. Eine LINQ-Abfrage ist eine Kette von Iteratoren, in der jedes Glied beim nächsten `MoveNext()` ruft. Und weil nie alles auf einmal erzeugt wird, darf ein Iterator sogar unendlich sein: `while (true) yield return i++;` ist ein gültiger Rumpf, solange der Client mit `Take(10)` rechtzeitig aufhört.

## Vor- und Nachteile

Der Code zum Durchlaufen ist von der Datenstruktur getrennt: Der Client braucht kein Wissen über Arrays, Listen oder Bäume, dieselbe `foreach`-Schleife funktioniert für alles, und pro Struktur können beliebig viele Strategien angeboten werden. Da jeder Durchlauf ein eigenes Objekt ist, stören sich mehrere Iterationen nicht gegenseitig.

Die Kehrseite: Ein Iterator ist ein Zeiger *in* eine Struktur, und wenn sich die Struktur während des Durchlaufs ändert, zeigt er ins Leere. `List<T>` erkennt das und wirft eine `InvalidOperationException`, wenn man innerhalb von `foreach` `Add` oder `Remove` aufruft – das ist die zweite Falle aus dem Modul zur verzögerten Ausführung. Und weil Iteratoren faul sind, wird derselbe Rumpf bei jedem `foreach` erneut ausgeführt; wer ein Ergebnis mehrfach braucht, holt es mit `ToList()` einmal ab.

Übung: Schreibe für die `ZusammengesetzteGrafik` aus dem [Composite-Modul](/modules/composite/composite.md) eine Iterator-Methode `IEnumerable<IGrafik> AlleBlaetter()`, die rekursiv alle Ellipsen des Baums liefert. Tipp: Für Teile, die selbst Komposita sind, brauchst du eine innere `foreach`-Schleife mit `yield return`. Wie viele Enumerator-Objekte existieren bei einem drei Ebenen tiefen Baum gleichzeitig?
{: .notice--info}

## Weitere Quellen

- [Iteratoren – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/iterators)
- [`yield`-Anweisung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/statements/yield)
- [`IEnumerable<T>`-Schnittstelle – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.ienumerable-1)
