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

## Beispiel: Auf Diät

Ein etwas realistischeres Beispiel: Wir wollen aus einer Speisekarte diättaugliche Gerichte herausfiltern. Jede Speise hat einen Namen und einen Kaloriengehalt:

```csharp
public class Speise
{
    public required string Name { get; init; }
    public int Kilokalorien { get; init; }
}
```

Die Daten legen wir mit **Objektinitialisierern** an – die geschweiften Klammern nach `new Speise` setzen Properties direkt, ohne dass die Klasse einen Konstruktor mit sechs Parametern braucht:

```csharp
Speise[] speisen =
[
    new Speise { Name = "Brokkoli gratiniert", Kilokalorien = 216 },
    new Speise { Name = "Nudelsalat", Kilokalorien = 506 },
    new Speise { Name = "Mozzarella mit Tomaten", Kilokalorien = 300 },
    new Speise { Name = "Schnitzel mit Pommes", Kilokalorien = 518 + 320 },
    new Speise { Name = "Gefuellte Paprika", Kilokalorien = 182 },
    new Speise { Name = "Spaghetti mit Tomatensauce", Kilokalorien = 333 }
];
```

Alle Speisen mit höchstens 300 kcal, aufsteigend nach Kalorien, ist jetzt eine Abfrage aus drei Klauseln:

```csharp
IEnumerable<Speise> diaet =
    from s in speisen
    where s.Kilokalorien <= 300
    orderby s.Kilokalorien
    select s;

foreach (Speise speise in diaet)
{
    Console.WriteLine($"{speise.Kilokalorien,4} kcal  {speise.Name}");
}
//  182 kcal  Gefuellte Paprika
//  216 kcal  Brokkoli gratiniert
//  300 kcal  Mozzarella mit Tomaten
```

Das `select s` gibt die Objekte selbst zurück – deshalb ist das Ergebnis ein `IEnumerable<Speise>`. Hätten wir `select s.Name` geschrieben, wäre es ein `IEnumerable<string>`. Der Compiler leitet den Typ aus dem `select` ab, und meistens schreibt man deshalb einfach `var diaet = …`.

## Gruppieren mit `group by`

Statt zu filtern, können wir die Speisekarte auch in zwei Töpfe sortieren: diättauglich und nicht. Dafür ersetzt `group … by` das `select`, und als Gruppenschlüssel darf ein beliebiger Ausdruck stehen – hier ein `bool`:

```csharp
var nachKategorie =
    from s in speisen
    group s by s.Kilokalorien > 300;

foreach (IGrouping<bool, Speise> gruppe in nachKategorie)
{
    Console.WriteLine(gruppe.Key ? "Nicht diaettauglich:" : "Diaettauglich:");
    foreach (Speise speise in gruppe)
    {
        Console.WriteLine($"  {speise.Name}");
    }
}
// Diaettauglich:
//   Brokkoli gratiniert
//   Mozzarella mit Tomaten
//   Gefuellte Paprika
// Nicht diaettauglich:
//   Nudelsalat
//   Schnitzel mit Pommes
//   Spaghetti mit Tomatensauce
```

Der Ergebnistyp ist `IEnumerable<IGrouping<bool, Speise>>` – eine Aufzählung von Gruppen, wobei jede Gruppe einen `Key` (den Wert des Ausdrucks, `true` oder `false`) hat und selbst wieder ein `IEnumerable<Speise>` ist. Deshalb die zwei verschachtelten Schleifen. Die Reihenfolge der Gruppen folgt dem ersten Auftreten des Schlüssels in der Quelle: Brokkoli mit `false` kommt zuerst.

Das Ergebnis einer LINQ-Abfrage ist grundsätzlich ein `IEnumerable<T>`, keine `List<T>`. Man kann es mit `foreach` durchlaufen und an Methoden übergeben, die `IEnumerable<T>` annehmen – aber es gibt keinen Indexzugriff `ergebnis[0]` und kein `Count`-Property. Wer eine Liste braucht, ruft `ToList()` auf. Warum das so ist, klärt das Modul zur [verzögerten Ausführung](/modules/linq_deferred_execution/linq_deferred_execution.md).
{: .notice--primary}

## Der Compiler prüft mit

Der große Unterschied zu einer SQL-Abfrage, die als String im Code steht, ist die Typprüfung. Ein Tippfehler in `"SELECT nmae FROM speisen"` fällt erst zur Laufzeit auf, wenn die Datenbank die Anfrage ablehnt. In LINQ ist `s` ein `Speise`-Objekt, und `s.Nmae` ist ein Compilerfehler, bevor das Programm überhaupt startet:

```csharp
var falsch =
    from s in speisen
    where s.Kilokalorien <= "300"   // CS0019: int kann nicht mit string verglichen werden
    select s.Nmae;                  // CS1061: 'Speise' enthält keine Definition für 'Nmae'
```

Umbenennen einer Property mit der IDE erfasst auch alle LINQ-Abfragen, und die Autovervollständigung kennt alle Member von `s`. Genau das meint „Language *Integrated*“.

LINQ hat neben der hier gezeigten **Query-Syntax** noch eine zweite Schreibweise, die **Methodensyntax**: `speisen.Where(…).OrderBy(…).Select(…)`. Beide sind gleichwertig – der Compiler übersetzt die Query-Syntax intern in genau diese Methodenaufrufe. Die Methodensyntax braucht allerdings Lambda-Ausdrücke, die wir erst in der nächsten Vorlesung kennenlernen ([LINQ-Methodensyntax](/modules/linq_methodensyntax/linq_methodensyntax.md)).
{: .notice--primary}

Übung: Formuliere für die Speisekarte drei Abfragen: (a) die Namen aller Speisen mit mehr als 300 kcal, absteigend nach Kalorien; (b) alle Speisen, deren Name „Tomaten“ enthält, alphabetisch; (c) eine Gruppierung nach Hunderterklasse (`s.Kilokalorien / 100`) – wie viele Gruppen entstehen, und welchen Typ hat `gruppe.Key`? Sage die Ausgabe jeweils voraus, bevor du das Programm startest.
{: .notice--info}

## Weitere Quellen

- [Language Integrated Query (LINQ) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/)
- [Abfrageschlüsselwörter – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/query-keywords)
- [Gruppieren von Abfrageergebnissen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/standard-query-operators/grouping-data)
- [Objekt- und Auflistungsinitialisierer – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers)
