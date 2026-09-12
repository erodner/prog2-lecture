---
title: "LINQ – Abfragen direkt in C# (Query-Syntax)"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Wie oft haben wir dieselbe Schleife geschrieben: über eine Liste laufen, mit `if` filtern, Treffer in eine neue Liste kopieren, hinterher sortieren? Das ist jedes Mal die gleiche Tipparbeit, und der Leser muss aus fünf Zeilen erst rekonstruieren, *was* eigentlich gesucht wird. Wer schon einmal eine Datenbank abgefragt hat, kennt eine bessere Sprache dafür: `SELECT … FROM … WHERE … ORDER BY`. **LINQ** (Language Integrated Query) holt diese Idee nach C# – nicht als Text, der irgendwo zur Laufzeit interpretiert wird, sondern als Teil der Sprache, den der Compiler prüft. Und die Abfragen laufen nicht nur auf Datenbanken, sondern auf allem, was `IEnumerable<T>` implementiert: Arrays, Listen, Dictionaries, unser `Inventar<T>` und, wie wir im Modul [Collections im Überblick](/modules/collections_ueberblick/collections_ueberblick.md) gesehen haben, damit auf jeder Collection des Spiels.

## Deklarativ statt imperativ

Die Statusanzeige soll alle Gegner melden, die höchstens fünf Schritte vom Helden entfernt sind – der nächste zuerst. Imperativ, also mit der Beschreibung, *wie* das Ergebnis entsteht, sieht das so aus:

```csharp
List<Gegner> bedrohlich = new List<Gegner>();
foreach (Gegner g in feld.Gegner)
{
    if (g.Position.Entfernung(feld.Spieler.Position) <= 5)
    {
        bedrohlich.Add(g);
    }
}
bedrohlich.Sort(new NachEntfernungComparer(feld.Spieler.Position));
```

Drei Zeilen Verwaltung, eine Hilfsliste und eine eigene Comparer-Klasse – und das alles für eine einzige Frage. Dieselbe Aufgabe deklarativ, also mit der Beschreibung, *was* wir haben wollen:

```csharp
Position held = feld.Spieler.Position;

IEnumerable<Gegner> bedrohlich =
    from g in feld.Gegner
    where g.Position.Entfernung(held) <= 5
    orderby g.Position.Entfernung(held)
    select g;
```

Die Abfrage liest sich fast wie ein Satz: „Aus den Gegnern nimm jeden, der höchstens fünf Schritte entfernt ist, sortiere nach der Entfernung und gib den Gegner zurück.“ Keine Hilfsliste, kein `Add`, kein separates `Sort` – und vor allem keine Comparer-Klasse mehr, denn die Sortierung steht direkt in der Abfrage. Wie das Ergebnis intern zustande kommt, ist Sache von LINQ.

## Die Schlüsselwörter

Eine Abfrage beginnt immer mit `from` und endet mit `select` oder `group`. Dazwischen dürfen die anderen Klauseln in fast beliebiger Reihenfolge und Anzahl stehen:

| Schlüsselwort | Bedeutung |
| :--- | :--- |
| `from x in quelle` | Startpunkt: `quelle` ist ein `IEnumerable<T>`, `x` der Name für das aktuelle Element – wie die Laufvariable einer `foreach`-Schleife. |
| `where bedingung` | Filtert: Nur Elemente, für die `bedingung` `true` ist, kommen weiter. |
| `orderby ausdruck [descending]` | Sortiert nach `ausdruck`, standardmäßig aufsteigend. Mehrere Kriterien mit Komma: `orderby g.Symbol, g.Name`. |
| `select ausdruck` | Bestimmt, was im Ergebnis landet: das Element selbst, eine Property davon oder ein neu gebautes Objekt. |
| `group x by schluessel` | Bildet Gruppen mit gleichem `schluessel` – statt `select`. |
| `let name = ausdruck` | Führt eine Zwischenvariable ein, um einen Ausdruck nicht mehrfach zu schreiben. |
| `join … in … on … equals …` | Verknüpft zwei Quellen über einen gemeinsamen Schlüssel, ähnlich einem SQL-Join. |

Die Reihenfolge ist absichtlich anders als in SQL: `from` steht zuerst, damit der Compiler ab der ersten Zeile weiß, welchen Typ `g` hat – und damit in der IDE die Autovervollständigung für `g.Position` funktioniert.

## Die Abfrage am laufenden Spiel

Bauen wir ein kleines Spielfeld auf, an dem sich das Ergebnis nachrechnen lässt. Der Held steht auf `(5, 3)`, drum herum liegen ein paar Objekte und drei Gegner:

```csharp
Spielfeld feld = new Spielfeld(12, 6, new Spieler("Held", new Position(5, 3)));
feld.Hinzufuegen(new Wand(new Position(0, 3)));
feld.Hinzufuegen(new Wand(new Position(11, 3)));
feld.Hinzufuegen(new Truhe(new Position(2, 1), wert: 100));
feld.Hinzufuegen(new Schluessel(new Position(4, 4)));
feld.Hinzufuegen(new Wache(new Position(7, 3)));
feld.Hinzufuegen(new Verfolger(new Position(8, 2)));
feld.Hinzufuegen(new Wache(new Position(1, 5)));
```

Die Entfernung ist die Manhattan-Distanz aus `Position.Entfernung`, also die Anzahl der Schritte ohne Diagonalen: zur Wache auf `(7, 3)` sind es 2, zum Verfolger auf `(8, 2)` sind es 3 + 1 = 4, zur zweiten Wache auf `(1, 5)` sind es 4 + 2 = 6. Die dritte fällt also aus dem Filter heraus:

```csharp
Position held = feld.Spieler.Position;

var bedrohlich =
    from g in feld.Gegner
    where g.Position.Entfernung(held) <= 5
    orderby g.Position.Entfernung(held)
    select g;

foreach (Gegner g in bedrohlich)
{
    Console.WriteLine($"{g.Name} bei {g.Position}: {g.Position.Entfernung(held)} Schritte");
}
// Wache bei (7, 3): 2 Schritte
// Verfolger bei (8, 2): 4 Schritte
```

Das `select g` gibt die Objekte selbst zurück – deshalb ist das Ergebnis ein `IEnumerable<Gegner>`. Hätten wir `select g.Name` geschrieben, wäre es ein `IEnumerable<string>`, und mit `select $"{g.Name} ({g.Position})"` bauen wir uns direkt fertige Textzeilen. Der Compiler leitet den Typ aus dem `select` ab, und meistens schreibt man deshalb einfach `var bedrohlich = …`.

Dass `orderby` zweimal dieselbe Entfernung ausrechnet, stört bei drei Gegnern niemanden. Wer es sauberer will, führt mit `let abstand = g.Position.Entfernung(held)` eine Zwischenvariable ein und schreibt danach `where abstand <= 5` und `orderby abstand`.
{: .notice--primary}

## Gruppieren mit `group by`

Statt zu filtern, können wir alle Objekte des Spielfelds auch nach ihrer Art zusammenfassen – praktisch für eine Statistik oder einen Editor, der anzeigt, was auf der Karte liegt. Dafür ersetzt `group … by` das `select`, und als Gruppenschlüssel darf ein beliebiger Ausdruck stehen. Der Laufzeittyp eines Objekts liefert ihn uns frei Haus, wie wir im Modul [Laufzeittyp und Verstecken](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md) gesehen haben:

```csharp
var nachArt =
    from o in feld.AlleObjekte
    group o by o.GetType().Name;

foreach (IGrouping<string, Spielobjekt> gruppe in nachArt)
{
    Console.WriteLine($"{gruppe.Key}: {gruppe.Count()}");
}
// Wand: 2
// Truhe: 1
// Schluessel: 1
// Wache: 2
// Verfolger: 1
// Spieler: 1
```

Der Ergebnistyp ist `IEnumerable<IGrouping<string, Spielobjekt>>` – eine Aufzählung von Gruppen, wobei jede Gruppe einen `Key` (hier den Klassennamen) hat und selbst wieder ein `IEnumerable<Spielobjekt>` ist. Deshalb kann man sie mit einer inneren Schleife durchlaufen oder wie hier einfach zählen. Die Reihenfolge der Gruppen folgt dem **ersten Auftreten** des Schlüssels in der Quelle, und `AlleObjekte` liefert erst die statischen Objekte, dann die Gegner, zuletzt den Spieler. Verlassen sollte man sich darauf nicht: Innerhalb der statischen Objekte gibt das Dictionary keine garantierte Reihenfolge vor. Wer eine feste Ordnung braucht, gibt den Gruppen mit `into` einen Namen und sortiert sie anschließend:

```csharp
var nachArtSortiert =
    from o in feld.AlleObjekte
    group o by o.GetType().Name into gruppe
    orderby gruppe.Key
    select gruppe;
// Schluessel, Spieler, Truhe, Verfolger, Wache, Wand
```

Nach `into` ist `o` nicht mehr sichtbar – die Abfrage arbeitet ab hier mit Gruppen, nicht mehr mit einzelnen Objekten.

Das Ergebnis einer LINQ-Abfrage ist grundsätzlich ein `IEnumerable<T>`, keine `List<T>`. Man kann es mit `foreach` durchlaufen und an Methoden übergeben, die `IEnumerable<T>` annehmen – aber es gibt keinen Indexzugriff `bedrohlich[0]` und kein `Count`-Property. Wer eine Liste braucht, ruft `ToList()` auf. Warum das so ist, klärt das Modul zur [verzögerten Ausführung](/modules/linq_deferred_execution/linq_deferred_execution.md).
{: .notice--primary}

## Der Compiler prüft mit

Der große Unterschied zu einer SQL-Abfrage, die als String im Code steht, ist die Typprüfung. Ein Tippfehler in `"SELECT nmae FROM gegner"` fällt erst zur Laufzeit auf, wenn die Datenbank die Anfrage ablehnt. In LINQ ist `g` ein `Gegner`-Objekt, und `g.Psoition` ist ein Compilerfehler, bevor das Spiel überhaupt startet:

```csharp
var falsch =
    from g in feld.Gegner
    where g.Position.Entfernung(held) <= "5"   // CS0019: int und string sind nicht vergleichbar
    select g.Psoition;                         // CS1061: 'Gegner' enthält keine Definition für 'Psoition'
```

Wird `Position` eines Tages umbenannt, erfasst die IDE auch alle LINQ-Abfragen, und die Autovervollständigung kennt alle Member von `g`. Genau das meint „Language *Integrated*“.

LINQ hat neben der hier gezeigten **Query-Syntax** noch eine zweite Schreibweise, die **Methodensyntax**: `feld.Gegner.Where(…).OrderBy(…).Select(…)`. Beide sind gleichwertig – der Compiler übersetzt die Query-Syntax intern in genau diese Methodenaufrufe. Deshalb konnten wir in `Inventar<T>` schon `inhalt.Any(d => d is TArt)` schreiben. Die Methodensyntax braucht allerdings Lambda-Ausdrücke, die wir erst in der nächsten Vorlesung kennenlernen ([LINQ-Methodensyntax](/modules/linq_methodensyntax/linq_methodensyntax.md)).
{: .notice--primary}

Übung: Formuliere für das Spielfeld oben drei Abfragen: (a) die Namen aller Gegner, absteigend nach Entfernung zum Helden; (b) alle statischen Objekte, über die der Held laufen darf (`IstPassierbar`), alphabetisch nach `Name`; (c) eine Gruppierung aller Objekte nach `Symbol` mit Ausgabe der Anzahl je Symbol – wie viele Gruppen entstehen, und welchen Typ hat `gruppe.Key`? Sage die Ausgabe jeweils voraus, bevor du das Programm startest.
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

## Weitere Quellen

- [Language Integrated Query (LINQ) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/)
- [Abfrageschlüsselwörter – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/query-keywords)
- [Gruppieren von Abfrageergebnissen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/standard-query-operators/grouping-data)
- [Standardabfrageoperatoren im Überblick – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/standard-query-operators/)
