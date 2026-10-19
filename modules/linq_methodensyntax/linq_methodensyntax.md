---
title: "LINQ – Abfragen mit Lambdas"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Wie oft haben wir dieselbe Schleife geschrieben: über eine Liste laufen, mit `if` filtern, Treffer in eine neue Liste kopieren, hinterher sortieren? Der Leser muss aus fünf Zeilen erst rekonstruieren, *was* eigentlich gesucht wird. **LINQ** (*Language Integrated Query*) macht daraus eine kurze Kette von Methodenaufrufen, denen wir die Bedingungen als [Lambda-Ausdrücke](/modules/lambda_ausdruecke/lambda_ausdruecke.md) übergeben. Das funktioniert mit allem, was `IEnumerable<T>` ist: Arrays, Listen, Dictionaries, unserem `Inventar<T>` und `feld.AlleObjekte`.

## Deklarativ statt imperativ

Die Statusanzeige soll alle Gegner melden, die höchstens fünf Schritte vom Helden entfernt sind, der nächste zuerst. Mit dem Wissen aus [`IComparable<T>` und Sortieren](/modules/icomparable_sortieren/icomparable_sortieren.md) beschreiben wir Schritt für Schritt, *wie* das Ergebnis entsteht (imperativ):

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

Mit LINQ beschreiben wir nur, *was* wir haben wollen (deklarativ):

```csharp
Koordinate held = feld.Spieler.Position;

IEnumerable<Gegner> bedrohlich = feld.Gegner
    .Where(g => g.Position.Entfernung(held) <= 5)
    .OrderBy(g => g.Position.Entfernung(held));
```

Man liest die Kette von oben nach unten wie eine Verarbeitungsstraße: aus den Gegnern die nahen auswählen, dann nach Entfernung sortieren. Keine Hilfsliste, kein `Add`, keine Comparer-Klasse. Jede Methode bekommt ein Lambda: `Where` eine Bedingung, `OrderBy` den Wert, nach dem sortiert wird.

## Die Abfrage am laufenden Spiel

Bauen wir ein kleines Spielfeld auf, an dem sich das Ergebnis nachrechnen lässt. Der Held steht auf `(5, 3)`, drum herum liegen ein paar Objekte und drei Gegner:

```csharp
Spielfeld feld = new Spielfeld(12, 6, new Spieler("Held", new Koordinate(5, 3)));
feld.Hinzufuegen(new Wand(new Koordinate(0, 3)));
feld.Hinzufuegen(new Wand(new Koordinate(11, 3)));
feld.Hinzufuegen(new Truhe(new Koordinate(2, 1), wert: 100));
feld.Hinzufuegen(new Schluessel(new Koordinate(4, 4)));
feld.Hinzufuegen(new Wache(new Koordinate(7, 3)));
feld.Hinzufuegen(new Verfolger(new Koordinate(8, 2)));
feld.Hinzufuegen(new Wache(new Koordinate(1, 5)));
```

Zur Wache auf `(7, 3)` sind es 2 Schritte, zum Verfolger auf `(8, 2)` 3 + 1 = 4, zur zweiten Wache auf `(1, 5)` 4 + 2 = 6. Die dritte fällt also aus dem Filter heraus:

```csharp
foreach (Gegner g in bedrohlich)
{
    Console.WriteLine($"{g.Name} bei {g.Position}: {g.Position.Entfernung(held)} Schritte");
}
// Wache bei (7, 3): 2 Schritte
// Verfolger bei (8, 2): 4 Schritte
```

Mit `Select` formt man jedes Element um: `.Select(g => g.Name)` macht aus den Gegnern ihre Namen, also ein `IEnumerable<string>`. Den Ergebnistyp leitet der Compiler ab, deshalb schreibt man meist einfach `var`.

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

// feld ist das eingebaute Level „Kerker“, 20 × 9 Felder
Console.WriteLine(feld.AlleObjekte.AnzahlBegehbar());                    // 3
Console.WriteLine(SpielobjektErweiterungen.AnzahlBegehbar(feld.AlleObjekte)); // 3 – dasselbe
```

Nur drei? `AlleObjekte` enthält keine Bodenfelder – leerer Boden ist im Adventure schlicht das Fehlen eines Objekts. Passierbar sind im Kerker deshalb genau der Schlüssel, der Trank und der Ausgang; die Tür ist es erst, wenn sie offen ist.

Beide Aufrufe sind gleichwertig; die Punktschreibweise ist nur bequemer. Weil der erste Parameter `IEnumerable<Spielobjekt>` ist, funktioniert die Methode auf `feld.AlleObjekte`, auf einer `List<Spielobjekt>`, auf einem Array und auf den Ergebnissen anderer LINQ-Abfragen – auf allem, was wir im [Collections-Überblick](/modules/collections_ueberblick/collections_ueberblick.md) als aufzählbar kennengelernt haben. Genau so sind die LINQ-Methoden gebaut, nur generisch für jedes `T`.

## Die wichtigsten LINQ-Methoden

Angenommen, `e` ist ein `IEnumerable<T>`:

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
Koordinate held = feld.Spieler.Position;          // (1, 1) im Level „Kerker“

var bedrohung = feld.AlleObjekte
    .OfType<Gegner>()
    .Where(g => g.Position.Entfernung(held) <= 15)
    .OrderBy(g => g.Position.Entfernung(held))
    .Select(g => $"{g.Name} in {g.Position.Entfernung(held)} Schritten")
    .Take(3)
    .ToList();

Console.WriteLine(string.Join(", ", bedrohung));
// Wache in 13 Schritten, Verfolger in 14 Schritten

Console.WriteLine(feld.AlleObjekte.Count(o => o is Wand));        // 60
Console.WriteLine(feld.AlleObjekte.Any(o => o is Schluessel));    // True
Console.WriteLine(string.Join("", feld.Gegner.Select(g => g.Symbol())));  // WV
```

Beim Lesen hilft es, jede Zeile als Frage zu formulieren: „Welche Objekte sind Gegner? Welche davon stehen höchstens fünfzehn Schritte entfernt? Sortiere sie nach Entfernung. Mache aus jedem einen Text. Nimm die ersten drei. Packe sie in eine Liste.“ `OfType<Gegner>()` erledigt dabei zwei Dinge auf einmal: Es filtert *und* liefert ein `IEnumerable<Gegner>` statt `IEnumerable<Spielobjekt>` – erst dadurch darf das nächste Lambda auf gegnerspezifische Mitglieder zugreifen. Ein `Where(o => o is Gegner)` allein würde den statischen Typ nicht ändern.

Auch im Spielcode ließen sich einige Schleifen so verkürzen. `Spielfeld.IstFrei` prüft mit einer Schleife, ob kein Gegner auf dem Zielfeld steht, und `Inventar<T>` sucht mit einer Schleife nach einem Schlüssel. Mit `All` und `Any` wird daraus je eine Zeile:

```csharp
// Spielfeld.IstFrei, kürzer
return gegner.All(g => g.Position != p);

// Inventar<T>, kürzer
public bool Enthaelt<TArt>() where TArt : T => inhalt.Any(d => d is TArt);
```

Beide Aufrufe brechen ab, sobald die Antwort feststeht: `All` beim ersten Gegenbeispiel, `Any` beim ersten Treffer. Eine handgeschriebene Schleife mit `return` täte dasselbe – nur dass man bei `Any` und `All` keinen Zähler und kein `break` mehr falsch machen kann.

## Gruppieren mit `GroupBy`

Statt zu filtern, können wir die Objekte auch nach ihrer Art zusammenfassen, etwa für eine Statistik. `GroupBy` bekommt ein Lambda, das den Gruppenschlüssel liefert, hier den Klassennamen des Laufzeittyps:

```csharp
foreach (var gruppe in feld.AlleObjekte.GroupBy(o => o.GetType().Name))
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

Jede Gruppe hat einen `Key` und ist selbst wieder eine Aufzählung ihrer Elemente, die man durchlaufen oder zählen kann. Wer die Gruppen sortiert haben will, hängt `.OrderBy(gruppe => gruppe.Key)` an.

## Der Compiler prüft mit

Anders als eine Datenbankabfrage, die als Text im Code steht, wird LINQ vom Compiler geprüft. Ein Tippfehler wie `g.Psoition` oder der Vergleich einer Zahl mit `"5"` ist ein Compilerfehler, bevor das Spiel startet, und die IDE vervollständigt `g.` mit allen Mitgliedern von `Gegner`. Genau das meint „Language *Integrated*“.

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag [`v03-blazor`](https://github.com/erodner/prog2-adventure/tree/v03-blazor)).

Übung: Schreibe in Methodensyntax: (a) die Namen aller Truhen auf dem Feld, alphabetisch; (b) die durchschnittliche Entfernung aller Gegner zum Helden; (c) ein `Dictionary<Koordinate, char>` von Position auf Symbol für alle nicht passierbaren Objekte; (d) die drei Objekte, die dem Helden am nächsten liegen, ihn selbst ausgenommen.
{: .notice--info}

## Weitere Quellen

- [Standardabfrageoperatoren – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/standard-query-operators/)
- [Language Integrated Query (LINQ) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/)
- [Erweiterungsmethoden – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/extension-methods)
- [Enumerable-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.linq.enumerable)
- [101 LINQ-Beispiele – dotnet/try-samples](https://github.com/dotnet/try-samples/tree/main/101-linq-samples) – für jeden Operator ein winziges lauffähiges Beispiel; ideal zum Nachschlagen, wenn man den Namen der passenden Methode sucht.
- [.NET Fiddle](https://dotnetfiddle.net/) – eine Methodenkette Glied für Glied verlängern und nach jedem Schritt das Zwischenergebnis ansehen.
