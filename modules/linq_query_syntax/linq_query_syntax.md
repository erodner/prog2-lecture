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

Wie oft haben wir in Programmierung 1 dieselbe Schleife geschrieben: über eine Liste laufen, mit `if` filtern, Treffer in eine neue Liste kopieren, hinterher sortieren? Das ist jedes Mal die gleiche Tipparbeit, und der Leser muss aus fünf Zeilen erst rekonstruieren, *was* eigentlich gesucht wird. Wer schon einmal eine Datenbank abgefragt hat, kennt eine bessere Sprache dafür: `SELECT … FROM … WHERE … ORDER BY`. **LINQ** (Language Integrated Query) holt diese Idee nach C# – nicht als Text, der irgendwo zur Laufzeit interpretiert wird, sondern als Teil der Sprache, den der Compiler prüft. Und die Abfragen laufen nicht nur auf Datenbanken, sondern auf allem, was `IEnumerable<T>` implementiert: Arrays, Listen, Dictionaries, Strings und, wie wir im Modul [Collections im Überblick](/modules/collections_ueberblick/collections_ueberblick.md) gesehen haben, damit auf jeder Collection.

## Deklarativ statt imperativ

Nehmen wir eine Liste von Städtenamen. Gesucht sind alle Städte, die mit „B“ beginnen, alphabetisch sortiert und in Großbuchstaben. Imperativ – also mit der Beschreibung, *wie* das Ergebnis entsteht – sieht das so aus:

```csharp
string[] staedte = ["Berlin", "Wien", "Paris", "Bremen", "Bruessel", "Linz"];

List<string> treffer = new List<string>();
foreach (string stadt in staedte)
{
    if (stadt.StartsWith("B"))
    {
        treffer.Add(stadt.ToUpper());
    }
}
treffer.Sort();
```

Dieselbe Aufgabe deklarativ – wir beschreiben nur, *was* wir haben wollen:

```csharp
IEnumerable<string> ergebnis =
    from s in staedte
    where s.StartsWith("B")
    orderby s
    select s.ToUpper();

foreach (string stadt in ergebnis)
{
    Console.WriteLine(stadt);
}
// BERLIN
// BREMEN
// BRUESSEL
```

Die Abfrage liest sich fast wie ein Satz: „Aus `staedte` nimm jedes `s`, das mit B beginnt, sortiere nach `s` und gib `s` in Großbuchstaben zurück.“ Keine Hilfsliste, kein `Add`, kein separates `Sort`. Wie das Ergebnis intern zustande kommt, ist Sache von LINQ.

## Die Schlüsselwörter

Eine Abfrage beginnt immer mit `from` und endet mit `select` oder `group`. Dazwischen dürfen die anderen Klauseln in fast beliebiger Reihenfolge und Anzahl stehen:

| Schlüsselwort | Bedeutung |
| :--- | :--- |
| `from x in quelle` | Startpunkt: `quelle` ist ein `IEnumerable<T>`, `x` der Name für das aktuelle Element – wie die Laufvariable einer `foreach`-Schleife. |
| `where bedingung` | Filtert: Nur Elemente, für die `bedingung` `true` ist, kommen weiter. |
| `orderby ausdruck [descending]` | Sortiert nach `ausdruck`, standardmäßig aufsteigend. Mehrere Kriterien mit Komma: `orderby s.Semester, s.Name`. |
| `select ausdruck` | Bestimmt, was im Ergebnis landet: das Element selbst, eine Property davon oder ein neu gebautes Objekt. |
| `group x by schluessel` | Bildet Gruppen mit gleichem `schluessel` – statt `select`. |
| `let name = ausdruck` | Führt eine Zwischenvariable ein, um einen Ausdruck nicht mehrfach zu schreiben. |
| `join … in … on … equals …` | Verknüpft zwei Quellen über einen gemeinsamen Schlüssel, ähnlich einem SQL-Join. |

Die Reihenfolge ist absichtlich anders als in SQL: `from` steht zuerst, damit der Compiler ab der ersten Zeile weiß, welchen Typ `s` hat – und damit in der IDE die Autovervollständigung für `s.StartsWith` funktioniert.

## Beispiel: Mensaplan

Ein etwas realistischeres Beispiel: Wir wollen aus dem Mensaplan die leichten Gerichte herausfiltern. Jedes Gericht hat einen Namen und einen Kaloriengehalt:

```csharp
public class Gericht
{
    public required string Name { get; init; }
    public int Kilokalorien { get; init; }
}
```

Die Daten legen wir mit **Objektinitialisierern** an – die geschweiften Klammern nach `new Gericht` setzen Properties direkt, ohne dass die Klasse einen Konstruktor mit sechs Parametern braucht:

```csharp
Gericht[] gerichte =
[
    new Gericht { Name = "Brokkoli gratiniert", Kilokalorien = 216 },
    new Gericht { Name = "Nudelsalat", Kilokalorien = 506 },
    new Gericht { Name = "Mozzarella mit Tomaten", Kilokalorien = 300 },
    new Gericht { Name = "Schnitzel mit Pommes", Kilokalorien = 518 + 320 },
    new Gericht { Name = "Gefuellte Paprika", Kilokalorien = 182 },
    new Gericht { Name = "Spaghetti mit Tomatensauce", Kilokalorien = 333 }
];
```

Alle Gerichte mit höchstens 300 kcal, aufsteigend nach Kalorien, ist jetzt eine Abfrage aus drei Klauseln:

```csharp
IEnumerable<Gericht> leichteGerichte =
    from g in gerichte
    where g.Kilokalorien <= 300
    orderby g.Kilokalorien
    select g;

foreach (Gericht gericht in leichteGerichte)
{
    Console.WriteLine($"{gericht.Kilokalorien,4} kcal  {gericht.Name}");
}
//  182 kcal  Gefuellte Paprika
//  216 kcal  Brokkoli gratiniert
//  300 kcal  Mozzarella mit Tomaten
```

Das `select g` gibt die Objekte selbst zurück – deshalb ist das Ergebnis ein `IEnumerable<Gericht>`. Hätten wir `select g.Name` geschrieben, wäre es ein `IEnumerable<string>`. Der Compiler leitet den Typ aus dem `select` ab, und meistens schreibt man deshalb einfach `var leichteGerichte = …`.

## Gruppieren mit `group by`

Statt zu filtern, können wir den Mensaplan auch in zwei Töpfe sortieren: leicht und deftig. Dafür ersetzt `group … by` das `select`, und als Gruppenschlüssel darf ein beliebiger Ausdruck stehen – hier ein `bool`:

```csharp
var nachKategorie =
    from g in gerichte
    group g by g.Kilokalorien > 300;

foreach (IGrouping<bool, Gericht> gruppe in nachKategorie)
{
    Console.WriteLine(gruppe.Key ? "Deftig:" : "Leicht:");
    foreach (Gericht gericht in gruppe)
    {
        Console.WriteLine($"  {gericht.Name}");
    }
}
// Leicht:
//   Brokkoli gratiniert
//   Mozzarella mit Tomaten
//   Gefuellte Paprika
// Deftig:
//   Nudelsalat
//   Schnitzel mit Pommes
//   Spaghetti mit Tomatensauce
```

Der Ergebnistyp ist `IEnumerable<IGrouping<bool, Gericht>>` – eine Aufzählung von Gruppen, wobei jede Gruppe einen `Key` (den Wert des Ausdrucks, `true` oder `false`) hat und selbst wieder ein `IEnumerable<Gericht>` ist. Deshalb die zwei verschachtelten Schleifen. Die Reihenfolge der Gruppen folgt dem ersten Auftreten des Schlüssels in der Quelle: Brokkoli mit `false` kommt zuerst.

Das Ergebnis einer LINQ-Abfrage ist grundsätzlich ein `IEnumerable<T>`, keine `List<T>`. Man kann es mit `foreach` durchlaufen und an Methoden übergeben, die `IEnumerable<T>` annehmen – aber es gibt keinen Indexzugriff `ergebnis[0]` und kein `Count`-Property. Wer eine Liste braucht, ruft `ToList()` auf. Warum das so ist, klärt das Modul zur [verzögerten Ausführung](/modules/linq_deferred_execution/linq_deferred_execution.md).
{: .notice--primary}

## Der Compiler prüft mit

Der große Unterschied zu einer SQL-Abfrage, die als String im Code steht, ist die Typprüfung. Ein Tippfehler in `"SELECT nmae FROM gerichte"` fällt erst zur Laufzeit auf, wenn die Datenbank die Anfrage ablehnt. In LINQ ist `g` ein `Gericht`-Objekt, und `g.Nmae` ist ein Compilerfehler, bevor das Programm überhaupt startet:

```csharp
var falsch =
    from g in gerichte
    where g.Kilokalorien <= "300"   // CS0019: int kann nicht mit string verglichen werden
    select g.Nmae;                  // CS1061: 'Gericht' enthält keine Definition für 'Nmae'
```

Umbenennen einer Property mit der IDE erfasst auch alle LINQ-Abfragen, und die Autovervollständigung kennt alle Member von `g`. Genau das meint „Language *Integrated*“.

LINQ hat neben der hier gezeigten **Query-Syntax** noch eine zweite Schreibweise, die **Methodensyntax**: `gerichte.Where(…).OrderBy(…).Select(…)`. Beide sind gleichwertig – der Compiler übersetzt die Query-Syntax intern in genau diese Methodenaufrufe. Die Methodensyntax braucht allerdings Lambda-Ausdrücke, die wir erst in der nächsten Vorlesung kennenlernen ([LINQ-Methodensyntax](/modules/linq_methodensyntax/linq_methodensyntax.md)).
{: .notice--primary}

Übung: Formuliere für den Mensaplan drei Abfragen: (a) die Namen aller Gerichte mit mehr als 300 kcal, absteigend nach Kalorien; (b) alle Gerichte, deren Name „Tomaten“ enthält, alphabetisch; (c) eine Gruppierung nach Hunderterklasse (`g.Kilokalorien / 100`) – wie viele Gruppen entstehen, und welchen Typ hat `gruppe.Key`? Sage die Ausgabe jeweils voraus, bevor du das Programm startest.
{: .notice--info}

## Weitere Quellen

- [Language Integrated Query (LINQ) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/)
- [Abfrageschlüsselwörter – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/query-keywords)
- [Gruppieren von Abfrageergebnissen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/standard-query-operators/grouping-data)
- [Objekt- und Auflistungsinitialisierer – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers)
