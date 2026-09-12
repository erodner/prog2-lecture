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

Aber woher hat ein `string[]` plötzlich eine Methode `Where`? In der Klasse `Array` steht sie nicht. `Where`, `Select` und alle anderen LINQ-Methoden sind **Erweiterungsmethoden** (*extension methods*): statische Methoden in der Klasse `System.Linq.Enumerable`, deren erster Parameter mit `this` markiert ist. Dieses `this` erlaubt dem Compiler, den Aufruf so zu schreiben, als gehöre die Methode zum Typ des ersten Arguments. Eine eigene Erweiterungsmethode für das Adventure sieht so aus:

```csharp
static class SpielobjektErweiterungen
{
    public static int AnzahlBegehbar(this IEnumerable<Spielobjekt> quelle)
    {
        int anzahl = 0;
        foreach (Spielobjekt o in quelle)
            if (o.IstPassierbar)
                anzahl++;
        return anzahl;
    }
}

Console.WriteLine(feld.AlleObjekte.AnzahlBegehbar());                    // 7
Console.WriteLine(SpielobjektErweiterungen.AnzahlBegehbar(feld.AlleObjekte)); // 7 – dasselbe
```

Beide Aufrufe sind gleichwertig; die Punktschreibweise ist nur bequemer. Weil der erste Parameter `IEnumerable<Spielobjekt>` ist, funktioniert die Methode auf `feld.AlleObjekte`, auf einer `List<Spielobjekt>`, auf einem Array und auf den Ergebnissen anderer LINQ-Abfragen – auf allem, was wir im [Collections-Überblick](/modules/collections_ueberblick/collections_ueberblick.md) als aufzählbar kennengelernt haben. Genau so sind die LINQ-Methoden gebaut, nur generisch für jedes `T`.

## Die wichtigsten LINQ-Methoden

Die Query-Syntax kennt nur eine Handvoll Schlüsselwörter. Die Methodensyntax bietet deutlich mehr. Angenommen, `e` ist ein `IEnumerable<T>`:

| Methode | Bedeutung | Ergebnis |
| :--- | :--- | :--- |
| `e.Where(x => …)` | nur Elemente, für die die Bedingung gilt | `IEnumerable<T>` |
| `e.Select(x => …)` | jedes Element umformen | `IEnumerable<TResult>` |
| `e.OfType<TArt>()` | nur die Elemente eines bestimmten Typs, passend typisiert | `IEnumerable<TArt>` |
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

## Abfragen über das Spielfeld

Die Methoden lassen sich beliebig verketten, weil die meisten wieder ein `IEnumerable<T>` zurückgeben. Das Adventure bietet dafür zwei Quellen an: `feld.Gegner` ist eine `IReadOnlyList<Gegner>`, und `feld.AlleObjekte` liefert *jedes* Objekt auf der Karte – erst die statischen, dann die Gegner, zuletzt den Spieler. Damit lassen sich Fragen über das Level formulieren, für die es sonst eine Schleife mit Zähler bräuchte:

```csharp
Position held = feld.Spieler.Position;

var bedrohung = feld.AlleObjekte
    .OfType<Gegner>()
    .Where(g => g.Position.Entfernung(held) <= 5)
    .OrderBy(g => g.Position.Entfernung(held))
    .Select(g => $"{g.Name} in {g.Position.Entfernung(held)} Schritten")
    .Take(3)
    .ToList();

Console.WriteLine(string.Join(", ", bedrohung));
// Verfolger in 3 Schritten, Wache in 5 Schritten

Console.WriteLine(feld.AlleObjekte.Count(o => o is Wand));        // 42
Console.WriteLine(feld.AlleObjekte.Any(o => o is Schluessel));    // True
Console.WriteLine(string.Join("", feld.Gegner.Select(g => g.Symbol)));  // WVV
```

Beim Lesen hilft es, jede Zeile als Frage zu formulieren: „Welche Objekte sind Gegner? Welche davon stehen höchstens fünf Schritte entfernt? Sortiere sie nach Entfernung. Mache aus jedem einen Text. Nimm die ersten drei. Packe sie in eine Liste.“ `OfType<Gegner>()` erledigt dabei zwei Dinge auf einmal: Es filtert *und* liefert ein `IEnumerable<Gegner>` statt `IEnumerable<Spielobjekt>` – erst dadurch darf das nächste Lambda auf gegnerspezifische Mitglieder zugreifen. Ein `Where(o => o is Gegner)` allein würde den statischen Typ nicht ändern.

Auch im Spielcode selbst steckt die Methodensyntax längst. `Spielfeld.IstFrei` fragt mit `All`, ob wirklich kein Gegner auf dem Zielfeld steht, und `Inventar<T>` beantwortet die Frage nach einem Schlüssel mit `Any`:

```csharp
// aus Spielfeld.IstFrei
return gegner.All(g => g.Position != p);

// aus Inventar<T>
public bool Enthaelt<TArt>() where TArt : T => inhalt.Any(d => d is TArt);
```

Beide Aufrufe brechen ab, sobald die Antwort feststeht: `All` beim ersten Gegenbeispiel, `Any` beim ersten Treffer. Eine handgeschriebene Schleife mit `return` täte dasselbe – nur dass man bei `Any` und `All` keinen Zähler und kein `break` mehr falsch machen kann.

## Query- oder Methodensyntax?

Beide Schreibweisen sind gleichwertig: Der Compiler übersetzt die Query-Syntax vor dem eigentlichen Kompilieren in Methodenaufrufe, das erzeugte Programm ist identisch. Die Query-Syntax liest sich bei komplexen Abfragen mit mehreren `from` (Joins, verschachtelte Collections) oft angenehmer; die Methodensyntax ist kompakter, kennt alle Operationen und passt besser zu kurzen Ausdrücken mitten im Code wie `if (feld.Gegner.Any(g => g.Position.Entfernung(held) <= 1))`. `Take`, `ThenBy`, `Distinct` und `OfType` haben in der Query-Syntax gar keine Entsprechung – wer sie braucht, mischt ohnehin beide Schreibweisen oder bleibt gleich bei der Methodensyntax. Im .NET-Ökosystem ist sie der Normalfall.

Auch in Methodensyntax gilt die **verzögerte Ausführung** aus dem Modul [Deferred Execution](/modules/linq_deferred_execution/linq_deferred_execution.md): `Where`, `Select` und `OrderBy` bauen nur eine Abfrage zusammen, ausgeführt wird sie erst beim Durchlaufen – durch `foreach`, `ToList`, `Count`, `First` und ähnliche Methoden. Wer die Abfrage vor der Runde baut und nach der Runde durchläuft, sieht die neuen Gegnerpositionen. Und wer eine teure Abfrage zweimal durchläuft, rechnet zweimal.
{: .notice--warning}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`).

Übung: Schreibe in Methodensyntax: (a) die Namen aller Truhen auf dem Feld, alphabetisch; (b) die durchschnittliche Entfernung aller Gegner zum Helden; (c) ein `Dictionary<Position, char>` von Position auf Symbol für alle nicht passierbaren Objekte; (d) die drei Objekte, die dem Helden am nächsten liegen, ihn selbst ausgenommen. Welche der vier Abfragen ließen sich auch in Query-Syntax schreiben?
{: .notice--info}

## Weitere Quellen

- [Standardabfrageoperatoren – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/standard-query-operators/)
- [Abfragesyntax und Methodensyntax in LINQ – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/get-started/write-linq-queries)
- [Erweiterungsmethoden – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [Enumerable-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.linq.enumerable)
