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

Seit Programmierung 1 schreiben wir `foreach (var x in liste)` und denken nicht darüber nach, was dabei passiert. Dabei ist `foreach` keine Magie: Es funktioniert mit Arrays, mit `List<T>`, mit `Dictionary<K, V>`, mit LINQ-Abfragen – und im Adventure mit `spieler.Inventar` und `feld.AlleObjekte`, obwohl diese Datenstrukturen intern völlig unterschiedlich aufgebaut sind. Hinter dem Inventar steckt eine Liste, hinter den Objekten des Spielfelds ein Dictionary *plus* eine Liste *plus* ein einzelner Spieler, und eine LINQ-Abfrage hat noch nicht einmal Daten. Dass `foreach` trotzdem mit allen umgehen kann, verdankt es dem **Iterator**-Muster (Verhaltensmuster, auch *Cursor* genannt): Die Datenstruktur gibt ein eigenes Objekt heraus, das weiß, wie man sie durchläuft, und der Client fragt dieses Objekt Element für Element ab, ohne den inneren Aufbau zu kennen.

## Problem

Eine Datenstruktur soll durchlaufen werden können, ohne dass der Client ihren Aufbau kennt – und möglichst auf verschiedene Arten: alles der Reihe nach, nur die Gegner, nur die freien Nachbarfelder. Würde `Spielfeld` sein `Dictionary<Position, StatischesObjekt>` und seine `List<Gegner>` einfach öffentlich machen, wäre die Kapselung dahin: Jede Oberfläche müsste wissen, dass es zwei Behälter und einen Sonderfall (den Spieler) gibt, und jede Durchlaufstrategie müsste sie selbst programmieren. Außerdem soll es möglich sein, dieselbe Struktur mit zwei unabhängigen Durchläufen gleichzeitig zu bearbeiten, etwa in einer verschachtelten Schleife über alle Objektpaare.

## Lösung: `IEnumerable<T>` und `IEnumerator<T>`

In .NET ist das Muster fest in die Sprache eingebaut, mit zwei Rollen und zwei Interfaces:

- **Aggregat** (*Aggregate*): die Datenstruktur. Sie implementiert `IEnumerable<T>` und hat damit genau eine Methode, `GetEnumerator()`, die ein neues Iterator-Objekt liefert.
- **Iterator** (*Iterator*): das Objekt, das den Durchlauf kennt. Es implementiert `IEnumerator<T>` mit der Property `Current` (das aktuelle Element), der Methode `MoveNext()` (einen Schritt weiter; liefert `false`, wenn nichts mehr kommt) und `Reset()` (zurück an den Anfang; wird kaum genutzt).

Ein `foreach` ist nur Zucker für genau diese Aufrufe. Der Compiler übersetzt

```csharp
foreach (Spielobjekt objekt in feld.AlleObjekte)
    Console.WriteLine(objekt.Beschreibung());
```

in etwa in diese Schleife:

```csharp
IEnumerator<Spielobjekt> e = feld.AlleObjekte.GetEnumerator();
try
{
    while (e.MoveNext())
        Console.WriteLine(e.Current.Beschreibung());
}
finally
{
    e.Dispose();
}
```

Der Iterator ist ein *eigenes* Objekt mit eigenem Zustand (der aktuellen Position). Deshalb können zwei Schleifen gleichzeitig über dasselbe Spielfeld laufen: Jede hat ihren eigenen Enumerator. Das `finally` mit `Dispose()` sorgt dafür, dass ein Iterator, der Ressourcen hält (eine offene Datei etwa), diese auch bei einem `break` freigibt.

## Eine eigene Collection per Hand

Um das Muster ohne Abkürzung zu sehen, geben wir einer Wache eine feste Patrouillenroute: eine Klasse `Route`, die Stationen in einem Array speichert. Sie ist das Aggregat:

```csharp
using System.Collections;   // für das nicht-generische IEnumerable

public class Route : IEnumerable<Position>
{
    private readonly Position[] stationen;

    public Route(params Position[] stationen) => this.stationen = stationen;

    public IEnumerator<Position> GetEnumerator() => new RouteEnumerator(stationen);

    // nicht-generische Altlast, die IEnumerable<T> verlangt:
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Die zweite `GetEnumerator`-Methode ist eine explizite Interface-Implementierung (siehe Modul [Interfaces – erweitert](/modules/interfaces_erweitert/interfaces_erweitert.md)), weil `IEnumerable<T>` vom alten, nicht-generischen `IEnumerable` erbt. Der Enumerator selbst merkt sich das Array und eine Position, die *vor* der ersten Station beginnt:

```csharp
public class RouteEnumerator : IEnumerator<Position>
{
    private readonly Position[] stationen;
    private int index = -1;

    public RouteEnumerator(Position[] stationen) => this.stationen = stationen;

    public Position Current => stationen[index];
    object IEnumerator.Current => Current;

    public bool MoveNext()
    {
        index++;
        return index < stationen.Length;
    }

    public void Reset() => index = -1;
    public void Dispose() { }
}

Route route = new(new Position(1, 1), new Position(4, 1), new Position(4, 3));
foreach (Position p in route)
    Console.WriteLine(p);
// (1, 1)
// (4, 1)
// (4, 3)
```

Der Start bei `-1` ist wichtig: `foreach` ruft zuerst `MoveNext()` und dann `Current` auf – der erste `MoveNext()` bringt den Index auf 0. Das funktioniert, ist aber viel Code für eine simple Aufgabe: eine zweite Klasse, zwei explizite Interface-Mitglieder, ein `Dispose`, das nichts tut. Dabei tut die Klasse nichts anderes als „liefere die Stationen der Reihe nach“.

## Die Abkürzung: `yield return`

C# nimmt einem das Schreiben des Enumerators ab. Eine Methode, die `IEnumerator<T>` oder `IEnumerable<T>` zurückgibt und im Rumpf `yield return` verwendet, ist eine **Iterator-Methode**: Der Compiler erzeugt daraus die Enumerator-Klasse mit `MoveNext`, `Current` und dem gesamten Zustandsautomaten selbst. Die ganze `Route` schrumpft auf

```csharp
public class Route : IEnumerable<Position>
{
    private readonly Position[] stationen;

    public Route(params Position[] stationen) => this.stationen = stationen;

    public IEnumerator<Position> GetEnumerator()
    {
        foreach (Position station in stationen)
            yield return station;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Die Klasse `RouteEnumerator` ist verschwunden. `yield return` liefert einen Wert an die aufrufende Schleife und *merkt sich, wo es war* – beim nächsten `MoveNext()` läuft die Methode hinter dem `yield return` weiter, als wäre nichts gewesen. `yield break` beendet den Durchlauf vorzeitig, so wie ein `return` in einer normalen Methode. Noch kürzer geht es, wenn man gar keine eigene Reihenfolge braucht: `Inventar<T>` reicht mit `public IEnumerator<T> GetEnumerator() => inhalt.GetEnumerator();` einfach den Enumerator seiner internen Liste durch – auch das ist das Muster, nur mit geliehenem Iterator.

Iterator-Methoden dürfen kein normales `return wert;` enthalten und keine `out`- oder `ref`-Parameter haben. Beides wäre mit dem Zustandsautomaten, den der Compiler erzeugt, nicht vereinbar.
{: .notice--primary}

## `Spielfeld.AlleObjekte`

Genau so entsteht im Adventure die Sicht auf alle Objekte des Feldes. Die Property fasst drei völlig verschiedene Quellen zu *einer* Folge zusammen, und zwar in einer festgelegten Reihenfolge:

```csharp
/// <summary>Alle Objekte auf dem Feld – erst die statischen, dann die Gegner, zuletzt der Spieler.</summary>
public IEnumerable<Spielobjekt> AlleObjekte
{
    get
    {
        foreach (StatischesObjekt s in statische.Values) yield return s;
        foreach (Gegner g in gegner) yield return g;
        yield return Spieler;
    }
}
```

Drei `yield return` in drei verschiedenen Konstrukten, kein Zwischenergebnis, keine `List<Spielobjekt>`, die erst gefüllt und dann zurückgegeben wird. Der Aufrufer sieht davon nichts – für ihn ist `foreach (Spielobjekt objekt in feld.AlleObjekte)` eine einzige Folge, die mit den Wänden beginnt und mit `Held bei (1, 1), 3/3 Lebenspunkte, 0 Punkte, Inventar: leer` endet. Dass der Spieler *zuletzt* kommt, ist Absicht: Wer die Folge zum Zeichnen benutzt und jedes Objekt an seine Position malt, überschreibt damit alles, was unter dem Helden liegt – er liegt im Bild ganz oben. Die Reihenfolge ist Teil der Zusicherung dieses Iterators, auch wenn der Typ `IEnumerable<Spielobjekt>` davon nichts verrät.

## Spezifische Iteratoren

Das Muster verspricht *verschiedene Durchlaufstrategien* für dieselbe Struktur. Mit `yield` sind das einfach weitere Methoden mit Rückgabetyp `IEnumerable<T>` und beliebigem Namen – `foreach` akzeptiert jedes `IEnumerable<T>`, nicht nur das Objekt selbst. Für die Gegner-Logik brauchen wir ständig die vier Nachbarfelder einer Position:

```csharp
public IEnumerable<Position> NachbarFelder(Position p)
{
    foreach (Richtung r in Enum.GetValues<Richtung>())
    {
        Position nachbar = p.Verschoben(r);
        if (IstInnerhalb(nachbar))
            yield return nachbar;
    }
}

public IEnumerable<Position> FreieNachbarFelder(Position p)
{
    foreach (Position nachbar in NachbarFelder(p))
        if (IstFrei(nachbar))
            yield return nachbar;
}
```

`FreieNachbarFelder` ist selbst ein Iterator, der über einen Iterator läuft – genau so sind LINQ-Ketten aufgebaut. Am Rand des Feldes liefert `NachbarFelder` nur zwei oder drei Positionen, ohne dass der Aufrufer eine Randprüfung schreiben muss: Steht der Held bei (1, 1) und links von ihm eine Wand, schreibt `foreach (Position p in feld.FreieNachbarFelder(feld.Spieler.Position)) Console.Write(p + " ");` genau `(1, 0) (1, 2) (2, 1)` – in der Reihenfolge der Aufzählung `Richtung`.

Ein Iterator muss übrigens keine Property oder `GetEnumerator`-Methode sein und die Klasse auch nicht `IEnumerable<T>` implementieren: `Spielfeld` ist keine Collection, bietet aber drei Durchlaufstrategien an.

## Iteratoren sind faul

Der wichtigste Aspekt von `yield` ist, dass der Rumpf der Methode **nicht beim Aufruf** läuft, sondern erst, wenn jemand `MoveNext()` ruft – und dann immer nur bis zum nächsten `yield return`. Das zeigt eine Ausgabe im Iterator:

```csharp
// erste Zeile der Schleife in NachbarFelder, nur zur Demonstration:
Console.WriteLine($"  pruefe {r}");

IEnumerable<Position> nachbarn = feld.NachbarFelder(new Position(1, 1));
Console.WriteLine("Iterator erzeugt");
foreach (Position p in nachbarn)
{
    Console.WriteLine(p);
    break;
}
// Iterator erzeugt
//   pruefe Oben
// (1, 0)
```

Zwischen dem Aufruf von `NachbarFelder(...)` und der Schleife passiert nichts, und nach dem `break` werden `Unten`, `Links` und `Rechts` nie geprüft. Genau dieses Verhalten kennen wir schon als [verzögerte Ausführung](/modules/linq_deferred_execution/linq_deferred_execution.md) von LINQ – und das ist kein Zufall: `Where`, `Select` und die anderen Operatoren aus der [Methodensyntax](/modules/linq_methodensyntax/linq_methodensyntax.md) sind Iterator-Methoden mit `yield return`. Eine LINQ-Abfrage ist eine Kette von Iteratoren, in der jedes Glied beim nächsten `MoveNext()` ruft. Und weil nie alles auf einmal erzeugt wird, darf ein Iterator sogar unendlich sein – eine Wache, die ihre Route ewig abläuft, ist ein gültiger Rumpf, solange der Client rechtzeitig aufhört:

```csharp
public static IEnumerable<Position> Patrouille(Route route)
{
    while (true)
        foreach (Position station in route)
            yield return station;
}

// Patrouille(route).Take(5) liefert: (1, 1) (4, 1) (4, 3) (1, 1) (4, 1)
```

## Vor- und Nachteile

Der Code zum Durchlaufen ist von der Datenstruktur getrennt: Der Client braucht kein Wissen über Dictionaries, Listen oder Sonderfälle, dieselbe `foreach`-Schleife funktioniert für alles, und pro Struktur können beliebig viele Strategien angeboten werden. Da jeder Durchlauf ein eigenes Objekt ist, stören sich mehrere Iterationen nicht gegenseitig.

Die Kehrseite: Ein Iterator ist ein Zeiger *in* eine Struktur, und wenn sich die Struktur während des Durchlaufs ändert, zeigt er ins Leere. `List<T>` und `Dictionary<K, V>` erkennen das und werfen eine `InvalidOperationException` – ein `foreach (Spielobjekt o in feld.AlleObjekte) feld.Entfernen(o);` stürzt deshalb ab, und die Lösung ist ein `ToList()` vor der Schleife. Das ist die zweite Falle aus dem Modul zur verzögerten Ausführung. Und weil Iteratoren faul sind, wird derselbe Rumpf bei jedem `foreach` erneut ausgeführt; wer ein Ergebnis mehrfach braucht, holt es ebenfalls einmal mit `ToList()` ab.

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

Übung: Ergänze `Spielfeld` um eine Iterator-Methode `IEnumerable<Gegner> GegnerInSichtweite(int maxEntfernung)`, die nur Gegner liefert, deren Manhattan-Entfernung (`Position.Entfernung`) zum Spieler höchstens `maxEntfernung` beträgt und für die `HatSichtlinie` gilt. Vergleiche sie danach mit der LINQ-Variante `Gegner.Where(...)` – wo steckt dort das `yield return`? Baue anschließend `NachbarFelder` so um, dass es die vier Richtungen im Uhrzeigersinn liefert, und überlege, welcher aufrufende Code sich dadurch ändert.
{: .notice--info}

## Weitere Quellen

- [Iteratoren – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/iterators)
- [`yield`-Anweisung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/statements/yield)
- [`IEnumerable<T>`-Schnittstelle – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.ienumerable-1)
