---
title: "LINQ-Methodensyntax"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Im Modul [LINQ-Abfragen](/modules/linq_query_syntax/linq_query_syntax.md) haben wir Abfragen mit `from`, `where`, `orderby` und `select` geschrieben – eine Syntax, die an SQL erinnert und in C# ein wenig wie ein Fremdkörper wirkt. Jetzt, mit [Lambda-Ausdrücken](/modules/lambda_ausdruecke/lambda_ausdruecke.md) im Gepäck, können wir hinter die Kulissen schauen: Die Query-Syntax ist nur Zucker. Der Compiler übersetzt jede Abfrage in eine Kette gewöhnlicher Methodenaufrufe, denen Lambdas übergeben werden. Diese **Methodensyntax** kann man auch direkt schreiben – und in der Praxis wird sie sogar häufiger benutzt, weil sie mehr Operationen kennt und sich nahtlos mit dem restlichen Code mischt.

## Dieselbe Abfrage, zwei Schreibweisen

Nehmen wir die Abfrage aus der letzten Vorlesung: alle Städte, die mit „B“ beginnen, alphabetisch sortiert und in Großbuchstaben. In Query-Syntax:

```csharp
string[] staedte = ["Berlin", "Hamburg", "Bremen", "München", "Bonn", "Köln"];

IEnumerable<string> ergebnis =
    from s in staedte
    where s.StartsWith("B")
    orderby s
    select s.ToUpper();
```

Dieselbe Abfrage in Methodensyntax – und genau das macht der Compiler aus dem obigen Code:

```csharp
IEnumerable<string> ergebnis = staedte
    .Where(s => s.StartsWith("B"))
    .OrderBy(s => s)
    .Select(s => s.ToUpper());

Console.WriteLine(string.Join(", ", ergebnis));   // BERLIN, BONN, BREMEN
```

Jedes Schlüsselwort wird zu einem Methodenaufruf, jede Bedingung zu einem Lambda: `where s.StartsWith("B")` wird zu `Where(s => s.StartsWith("B"))`, `orderby s` zu `OrderBy(s => s)`, `select s.ToUpper()` zu `Select(s => s.ToUpper())`. Die Bereichsvariable `s` aus `from s in staedte` taucht als Lambda-Parameter wieder auf. Man liest so eine Kette von oben nach unten wie eine Verarbeitungsstraße: Erst filtern, dann sortieren, dann umformen.

## Erweiterungsmethoden auf `IEnumerable<T>`

Aber woher hat ein `string[]` plötzlich eine Methode `Where`? In der Klasse `Array` steht sie nicht. `Where`, `Select` und alle anderen LINQ-Methoden sind **Erweiterungsmethoden** (*extension methods*): statische Methoden in der Klasse `System.Linq.Enumerable`, deren erster Parameter mit `this` markiert ist. Dieses `this` erlaubt dem Compiler, den Aufruf so zu schreiben, als gehöre die Methode zum Typ des ersten Arguments. Eine eigene Erweiterungsmethode sieht so aus:

```csharp
static class AufzaehlungErweiterungen
{
    public static int AnzahlLeere(this IEnumerable<string> quelle)
    {
        int anzahl = 0;
        foreach (string s in quelle)
            if (string.IsNullOrWhiteSpace(s))
                anzahl++;
        return anzahl;
    }
}

string[] eingaben = ["Berlin", "", "  ", "Bonn"];
Console.WriteLine(eingaben.AnzahlLeere());                    // 2
Console.WriteLine(AufzaehlungErweiterungen.AnzahlLeere(eingaben)); // 2 – dasselbe
```

Beide Aufrufe sind gleichwertig; die Punktschreibweise ist nur bequemer. Weil der erste Parameter `IEnumerable<string>` ist, funktioniert die Methode auf Arrays, Listen, `HashSet<string>` und auf den Ergebnissen anderer LINQ-Abfragen – auf allem, was wir im [Collections-Überblick](/modules/collections_ueberblick/collections_ueberblick.md) als aufzählbar kennengelernt haben. Genau so sind die LINQ-Methoden gebaut, nur generisch für jedes `T`.

## Die wichtigsten LINQ-Methoden

Die Query-Syntax kennt nur eine Handvoll Schlüsselwörter. Die Methodensyntax bietet deutlich mehr. Angenommen, `e` ist ein `IEnumerable<T>`:

| Methode | Bedeutung | Ergebnis |
| :--- | :--- | :--- |
| `e.Where(x => …)` | nur Elemente, für die die Bedingung gilt | `IEnumerable<T>` |
| `e.Select(x => …)` | jedes Element umformen | `IEnumerable<TResult>` |
| `e.OrderBy(x => …)` / `OrderByDescending` | sortieren nach einem Schlüssel | `IOrderedEnumerable<T>` |
| `….ThenBy(x => …)` / `ThenByDescending` | Nachsortieren bei gleichem Schlüssel | `IOrderedEnumerable<T>` |
| `e.First(x => …)` / `FirstOrDefault` | erstes passendes Element (Exception bzw. `default` bei Fehlen) | `T` bzw. `T?` |
| `e.Any(x => …)` / `e.All(x => …)` | gilt die Bedingung für mindestens ein / alle Elemente? | `bool` |
| `e.Count(x => …)` | Anzahl (optional mit Bedingung) | `int` |
| `e.Sum`, `Min`, `Max`, `Average` | Aggregate, optional mit Selektor `x => …` | Zahl |
| `e.Take(n)` / `e.Skip(n)` | die ersten `n` Elemente nehmen / überspringen | `IEnumerable<T>` |
| `e.Distinct()` | Duplikate entfernen (nutzt `Equals`/`GetHashCode`) | `IEnumerable<T>` |
| `e.GroupBy(x => …)` | nach Schlüssel gruppieren | `IEnumerable<IGrouping<TKey, T>>` |
| `e.ToList()`, `ToArray()`, `ToDictionary(x => …)` | Ergebnis in eine Collection überführen | `List<T>`, `T[]`, `Dictionary` |

Die Methoden lassen sich beliebig verketten, weil die meisten wieder ein `IEnumerable<T>` zurückgeben. Ein Beispiel mit dem Geometrieeditor aus [Vorlesung 03](/lectures/03/03.md):

```csharp
List<Figur> figuren =
[
    new Kreis("K1", 0, 0, 2), new Rechteck("R1", 1, 1, 4, 3),
    new Kreis("K2", 5, 5, 1), new Rechteck("R2", 2, 2, 2, 2)
];

var grosseNamen = figuren
    .Where(f => f.Flaeche > 5)
    .OrderByDescending(f => f.Flaeche)
    .ThenBy(f => f.Name)
    .Select(f => $"{f.Name} ({f.Flaeche:F1})")
    .Take(2)
    .ToList();

Console.WriteLine(string.Join(", ", grosseNamen));   // K1 (12.6), R1 (12.0)
Console.WriteLine(figuren.Any(f => f is Kreis));     // True
Console.WriteLine(figuren.Sum(f => f.Flaeche));      // 31.7079…
```

Beim Lesen hilft es, jede Zeile als Frage zu formulieren: „Welche Figuren haben mehr als 5 Fläche? Sortiere sie absteigend nach Fläche, bei Gleichstand nach Name. Mache aus jeder einen Text. Nimm die ersten zwei. Packe sie in eine Liste.“ `Take` und `ThenBy` haben in der Query-Syntax übrigens keine Entsprechung – wer sie braucht, mischt ohnehin beide Schreibweisen oder bleibt gleich bei der Methodensyntax.

## Query- oder Methodensyntax?

Beide Schreibweisen sind gleichwertig: Der Compiler übersetzt die Query-Syntax vor dem eigentlichen Kompilieren in Methodenaufrufe, das erzeugte Programm ist identisch. Die Query-Syntax liest sich bei komplexen Abfragen mit mehreren `from` (Joins, verschachtelte Collections) oft angenehmer; die Methodensyntax ist kompakter, kennt alle Operationen und passt besser zu kurzen Ausdrücken mitten im Code wie `if (figuren.Any(f => f.Flaeche > 100))`. Im .NET-Ökosystem ist die Methodensyntax der Normalfall – die meisten Beispiele in Dokumentationen und Foren verwenden sie.

Auch in Methodensyntax gilt die **verzögerte Ausführung** aus dem Modul [Deferred Execution](/modules/linq_deferred_execution/linq_deferred_execution.md): `Where`, `Select` und `OrderBy` bauen nur eine Abfrage zusammen, ausgeführt wird sie erst beim Durchlaufen – durch `foreach`, `ToList`, `Count`, `First` und ähnliche Methoden. Wer die Liste `figuren` nach dem Bau der Abfrage verändert, sieht die Änderung im Ergebnis. Und wer eine teure Abfrage zweimal durchläuft, rechnet zweimal.
{: .notice--warning}

Übung: Schreibe für die Liste `figuren` in Methodensyntax: (a) die Namen aller Rechtecke, alphabetisch; (b) den durchschnittlichen Umfang aller Figuren mit `X > 0`; (c) ein `Dictionary<string, double>` von Name auf Fläche; (d) die drei Figuren mit dem kleinsten Umfang. Welche der vier Abfragen ließen sich auch in Query-Syntax schreiben?
{: .notice--info}

## Weitere Quellen

- [Standardabfrageoperatoren – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/standard-query-operators/)
- [Abfragesyntax und Methodensyntax in LINQ – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/get-started/write-linq-queries)
- [Erweiterungsmethoden – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [Enumerable-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.linq.enumerable)
