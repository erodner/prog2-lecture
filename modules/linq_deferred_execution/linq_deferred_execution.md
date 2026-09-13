---
title: "LINQ – verzögerte Ausführung"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ein Kochrezept ist kein Essen. Wer ein Rezept aufschreibt, hat noch nichts gekocht – erst wenn jemand es Schritt für Schritt ausführt, entsteht das Gericht, und zwar aus den Zutaten, die *in diesem Moment* im Kühlschrank liegen. Eine LINQ-Abfrage verhält sich genauso: Die Zeile `from g in feld.Gegner where … select g` sucht keinen einzigen Gegner, sie beschreibt nur, wonach zu suchen wäre. Das nennt man **verzögerte Ausführung** (*deferred execution*). Für unser Spiel ist das ein Geschenk: Wir definieren die Abfrage „Gegner in Sichtweite“ ein einziges Mal beim Start und geben sie nach jeder Runde neu aus – sie liefert immer die aktuelle Lage, ohne dass wir sie anfassen. Wer das Prinzip nicht kennt, erlebt allerdings Ergebnisse, die sich scheinbar von allein ändern, oder Programme, die eine teure Abfrage unbemerkt zehnmal ausführen.

## Eine Abfrage ist ein Plan, keine Liste

Im Modul zur [Query-Syntax](/modules/linq_query_syntax/linq_query_syntax.md) haben wir gesehen, dass eine Abfrage ein `IEnumerable<T>` liefert. Dieses Objekt enthält keine Daten, sondern die Anweisung, wie Daten zu erzeugen sind. Ausgeführt wird sie erst, wenn jemand über das Ergebnis iteriert – meist per `foreach`. Nehmen wir dasselbe Spielfeld wie im vorigen Modul: Der Held startet auf `(5, 3)`, eine Wache steht auf `(7, 3)`, ein Verfolger auf `(8, 2)`, eine zweite Wache patrouilliert bei `(1, 5)`.

```csharp
IEnumerable<Gegner> inSichtweite =
    from g in feld.Gegner
    where g.Position.Entfernung(feld.Spieler.Position) <= 5
    orderby g.Position.Entfernung(feld.Spieler.Position)
    select g;

static void Melden(string runde, IEnumerable<Gegner> gegner, Spielfeld feld)
{
    Console.WriteLine(runde);
    foreach (Gegner g in gegner)
    {
        Console.WriteLine($"  {g.Name} bei {g.Position}: " +
                          $"{g.Position.Entfernung(feld.Spieler.Position)} Schritte");
    }
}
```

Beachte, dass in der Abfrage `feld.Spieler.Position` steht und nicht eine vorher berechnete Kopie. Damit fragt der Filter bei jeder Ausführung nach der **aktuellen** Heldenposition. Jetzt spielen wir zwei Runden und geben dazwischen jeweils dieselbe Variable `inSichtweite` aus:

```csharp
Melden("Vor der ersten Runde:", inSichtweite, feld);
feld.SpielerZieht(Richtung.Links);
feld.SpielerZieht(Richtung.Links);
Melden("Nach zwei Runden:", inSichtweite, feld);

// Vor der ersten Runde:
//   Wache bei (7, 3): 2 Schritte
//   Verfolger bei (8, 2): 4 Schritte
// Nach zwei Runden:
//   Wache bei (3, 5): 2 Schritte
//   Verfolger bei (6, 2): 4 Schritte
```

Die zweite Ausgabe zeigt völlig andere Gegner als die erste – und das, obwohl die Abfrage zwischen beiden Aufrufen nicht angefasst wurde. In den zwei Runden ist der Held nach links gelaufen, die erste Wache ist stur weiter nach rechts marschiert und aus dem Radius gefallen, der Verfolger hat die Jagd aufgenommen, und die zweite Wache ist von links herangerückt. `inSichtweite` ist eben keine Liste mit zwei Gegnern, sondern ein **Plan**, der bei jedem `foreach` neu abgearbeitet wird. Genau das wollen wir hier: Die Statusanzeige wird einmal formuliert und ist danach für immer aktuell.

Dass die Abfrage bei jeder Iteration neu läuft, kann man sichtbar machen, indem man die Bedingung in eine Methode mit Ausgabe auslagert:

```csharp
static bool IstNah(Gegner g, Spielfeld feld)
{
    Console.WriteLine($"  pruefe {g.Name} bei {g.Position}");
    return g.Position.Entfernung(feld.Spieler.Position) <= 5;
}

IEnumerable<Gegner> nah =
    from g in feld.Gegner
    where IstNah(g, feld)
    select g;

Console.WriteLine("Abfrage definiert – noch nichts passiert.");
foreach (Gegner g in nah)
{
    Console.WriteLine(g.Name);
}
```

Zwei Dinge fallen auf: Zwischen der Definition und der Schleife wird kein einziger Gegner geprüft. Und die Abfrage arbeitet **elementweise** – sie filtert nicht erst alle Gegner und liefert dann das Ergebnis, sondern reicht jeden passenden Gegner sofort an die Schleife weiter. Bei einem `break` nach dem ersten Treffer wären die restlichen nie geprüft worden. (Mit `orderby` ist das anders: Zum Sortieren muss LINQ zwangsläufig erst alle Elemente einsammeln.)

## Ausführung erzwingen: `ToList()` und `ToArray()`

Manchmal will man das Ergebnis *jetzt* haben – etwa weil sich das Spielfeld gleich ändert, das Ergebnis mehrfach gebraucht wird oder eine Methode eine `List<T>` verlangt. Die Methoden `ToList()` und `ToArray()` führen die Abfrage sofort aus und kopieren das Ergebnis in eine echte Collection:

```csharp
List<Gegner> zeugenDerRunde =
    (from g in feld.Gegner
     where g.Position.Entfernung(feld.Spieler.Position) <= 5
     select g).ToList();

feld.SpielerZieht(Richtung.Links);
feld.SpielerZieht(Richtung.Links);

Console.WriteLine(zeugenDerRunde.Count);        // 2 – die Gegner von vorhin, unverändert
Console.WriteLine(zeugenDerRunde[0].Position);  // (9, 3) – aber ihre Positionen sind aktuell!
```

Die Liste selbst ist eingefroren: Sie enthält genau die zwei Gegner, die zum Zeitpunkt der Auswertung nah waren, und behält sie, auch wenn sie längst davongelaufen sind. Eingefroren ist aber nur die **Auswahl**, nicht der Zustand der Objekte – die Liste speichert Referenzen, und ein Gegner, der sich bewegt, bewegt sich auch in dieser Liste. Die Wache stand bei der Auswertung auf `(7, 3)` und ist in den zwei Runden zwei Schritte nach rechts marschiert; in `zeugenDerRunde[0]` steckt dasselbe Objekt und damit die neue Position `(9, 3)`.

Dasselbe sofortige Ausführen passiert bei allen Methoden, die ein einzelnes Ergebnis brauchen und deshalb die ganze Quelle lesen müssen: `Count()`, `Max()`, `First()`. Man spricht dann von **sofortiger Ausführung** (*immediate execution*).

## Falle 1: mehrfache Enumeration

Weil ein `IEnumerable<T>` bei jedem Durchlauf neu ausgeführt wird, kostet jedes `foreach`, jedes `Count()` und jedes `Any()` einen vollständigen Durchlauf. Bei einer teuren Abfrage – über viele Objekte, mit aufwendiger Berechnung wie `HatSichtlinie` oder gar mit einer Datei als Quelle – ist das ein leicht zu übersehendes Leistungsproblem:

```csharp
IEnumerable<Gegner> gefaehrlich =
    from g in feld.Gegner
    where feld.HatSichtlinie(g.Position, feld.Spieler.Position)
    select g;

if (gefaehrlich.Any())                                  // 1. Durchlauf
{
    Console.WriteLine($"{gefaehrlich.Count()} sehen dich"); // 2. Durchlauf
    foreach (Gegner g in gefaehrlich)                       // 3. Durchlauf
    {
        Console.WriteLine(g.Name);
    }
}
```

Drei Zeilen, drei komplette Sichtlinienberechnungen für jeden Gegner. Die IDE warnt bei solchen Mustern mit dem Hinweis „mögliche mehrfache Enumeration“. Die Lösung ist ein einziges `ToList()` nach der Definition – dann wird einmal gefiltert und die Liste danach dreimal gelesen.

Faustregel: Wird das Ergebnis genau einmal durchlaufen und soll es den aktuellen Stand zeigen, ist `IEnumerable<T>` richtig – sparsam und ohne Kopie. Wird es mehrfach gebraucht oder darf sich die Auswahl nicht mehr ändern, sofort `ToList()`. Und wer eine Abfrage aus einer Methode zurückgibt, sollte im Namen oder in der Dokumentation klarmachen, ob der Aufrufer einen Plan oder eine Liste bekommt.
{: .notice--primary}

## Falle 2: die Quelle während der Iteration ändern

Die zweite Falle betrifft nicht nur LINQ, sondern jedes `foreach` über eine `List<T>` – mit LINQ ist sie aber leichter zu übersehen, weil zwischen Quelle und Schleife eine Abfrage steht. Angenommen, ein Zauberspruch vertreibt alle Gegner, die direkt neben dem Helden stehen:

```csharp
IEnumerable<Gegner> daneben =
    from g in feld.Gegner
    where g.Position.Entfernung(feld.Spieler.Position) <= 1
    select g;

foreach (Gegner g in daneben)
{
    feld.Entfernen(g); // InvalidOperationException: Collection was modified
}
```

Es sieht so aus, als würden wir über `daneben` laufen und das Spielfeld ändern – zwei verschiedene Dinge. Tatsächlich iteriert `daneben` aber direkt über die interne `List<Gegner>` des Spielfelds, und `Entfernen` löscht genau daraus. Die Liste bemerkt beim nächsten Schritt, dass sie sich seit Beginn des Durchlaufs verändert hat, und bricht ab. Die Lösung ist wieder `ToList()`: Dann läuft die Schleife über eine Kopie der Trefferliste, und das Entfernen aus dem Original ist erlaubt.

```csharp
foreach (Gegner g in daneben.ToList())
{
    feld.Entfernen(g); // funktioniert: die Schleife läuft über die Kopie
}
```

Dieselbe Falle lauert übrigens in `GegnerZiehen`: Dort läuft ein `foreach` über alle Gegner, und wenn dabei ein Gegner entfernt würde, gäbe es dieselbe Exception. Deshalb sammelt man in solchen Fällen erst die Kandidaten und räumt danach auf.
{: .notice--warning}

Verzögerte Ausführung ist kein Fehler, sondern der Grund, warum LINQ so leichtgewichtig ist: Man kann Abfragen definieren, weiterreichen, kombinieren und erst am Ende ausführen, ohne Zwischenlisten zu erzeugen. Wer die Regel „ein Plan wird bei jedem Durchlauf ausgeführt“ im Kopf hat, hat beide Fallen im Griff.

## Ausblick

Die Query-Syntax ist nur die Oberfläche. Intern übersetzt der Compiler `where g.Position.Entfernung(held) <= 5` in einen Aufruf `feld.Gegner.Where(g => g.Position.Entfernung(held) <= 5)` – eine Methode, die als Parameter ein Stück Code bekommt. Solche Code-als-Parameter-Konstrukte heißen **Delegaten**, ihre Kurzschreibweise `g => …` ist ein **Lambda-Ausdruck**. Beide sind das Thema der nächsten Vorlesung ([Lambda-Ausdrücke](/modules/lambda_ausdruecke/lambda_ausdruecke.md), [LINQ-Methodensyntax](/modules/linq_methodensyntax/linq_methodensyntax.md)) – und mit ihnen öffnet sich der Rest der LINQ-Welt: `Sum`, `Average`, `Take`, `Skip`, `Distinct` und viele mehr.

Übung: Definiere eine Abfrage, die alle Gegenstände liefert, die noch auf dem Boden liegen (`from o in feld.AlleObjekte where o is Gegenstand select o`). Gib das Ergebnis aus, lass den Helden dann über einen Schlüssel laufen und gib dasselbe Ergebnis erneut aus. Sage beide Ausgaben voraus – und erkläre, warum sich die zweite unterscheidet, obwohl du die Abfrage nicht verändert hast. Wiederhole das Experiment mit `ToList()` hinter der Abfrage. Was ändert sich, und warum bleibt die Anzahl der Lebenspunkte des Helden trotzdem in beiden Varianten aktuell?
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

## Weitere Quellen

- [Einführung in LINQ-Abfragen (verzögerte Ausführung) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/get-started/introduction-to-linq-queries)
- [Klassifizierung von Standardabfrageoperatoren nach Ausführungsart – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/standard-query-operators/#classification-of-standard-query-operators-by-manner-of-execution)
- [Enumerable.ToList – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.linq.enumerable.tolist)
- [yield – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/statements/yield) – das Schlüsselwort, mit dem `AlleObjekte` und jeder LINQ-Operator ihre Elemente erst auf Abruf liefern
- [101 LINQ Samples – GitHub](https://github.com/dotnet/try-samples/tree/main/101-linq-samples) – die klassische Beispielsammlung; besonders die Abschnitte zu `Take`, `First` und `ToList` zeigen den Unterschied zwischen verzögerter und sofortiger Ausführung
- [.NET Fiddle](https://dotnetfiddle.net/) – eine Abfrage mit `Console.WriteLine` im `where` in den Browser tippen und live sehen, wann sie wirklich losläuft
