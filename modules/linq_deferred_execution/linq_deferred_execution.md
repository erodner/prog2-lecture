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

Ein Kochrezept ist kein Essen. Wer ein Rezept aufschreibt, hat noch nichts gekocht – erst wenn jemand es Schritt für Schritt ausführt, entsteht das Gericht, und zwar aus den Zutaten, die *in diesem Moment* im Kühlschrank liegen. Eine LINQ-Abfrage verhält sich genauso: Die Zeile `from s in speisen where … select s` kocht nichts, sie beschreibt nur, was zu tun wäre. Das nennt man **verzögerte Ausführung** (*deferred execution*), und wer es nicht kennt, erlebt Ergebnisse, die sich scheinbar von allein ändern, oder Programme, die eine teure Abfrage unbemerkt zehnmal ausführen. In diesem Modul schauen wir, wann eine Abfrage tatsächlich läuft und welche Fallen sich daraus ergeben.

## Eine Abfrage ist ein Plan, keine Liste

Im Modul zur [Query-Syntax](/modules/linq_query_syntax/linq_query_syntax.md) haben wir gesehen, dass eine Abfrage ein `IEnumerable<T>` liefert. Dieses Objekt enthält keine Daten, sondern die Anweisung, wie Daten zu erzeugen sind. Ausgeführt wird sie erst, wenn jemand über das Ergebnis iteriert – meist per `foreach`. Das lässt sich beobachten, indem wir die Datenquelle **nach** der Definition der Abfrage verändern:

```csharp
List<int> zahlen = [1, 2, 3, 4, 5];

IEnumerable<int> gerade =
    from z in zahlen
    where z % 2 == 0
    select z;

zahlen.Add(6);
zahlen.Add(8);

Console.WriteLine(string.Join(", ", gerade)); // 2, 4, 6, 8
```

Obwohl `gerade` definiert wurde, als die Liste nur bis 5 ging, enthält das Ergebnis die 6 und die 8. Die Abfrage wurde erst in der `WriteLine`-Zeile ausgeführt – `string.Join` iteriert über `gerade`, und in diesem Moment hatte die Liste sieben Elemente. Die Abfrage hält nur eine Referenz auf `zahlen`, nicht eine Kopie.

Dass die Abfrage bei jeder Iteration neu läuft, kann man sichtbar machen, indem man in der `where`-Klausel eine Ausgabe einbaut:

```csharp
static bool IstGerade(int z)
{
    Console.WriteLine($"  pruefe {z}");
    return z % 2 == 0;
}

IEnumerable<int> gerade2 =
    from z in zahlen
    where IstGerade(z)
    select z;

Console.WriteLine("Abfrage definiert – noch nichts passiert.");
foreach (int z in gerade2)
{
    Console.WriteLine(z);
}
// Abfrage definiert – noch nichts passiert.
//   pruefe 1
//   pruefe 2
// 2
//   pruefe 3
//   pruefe 4
// 4
//   ...
```

Zwei Dinge fallen auf: Zwischen der Definition und der Schleife wird nichts geprüft. Und die Abfrage arbeitet **elementweise** – sie filtert nicht erst alle Zahlen und liefert dann das Ergebnis, sondern reicht jede passende Zahl sofort an die Schleife weiter. Bei einem `break` nach dem ersten Treffer wären die restlichen Zahlen nie geprüft worden.

## Ausführung erzwingen: `ToList()` und `ToArray()`

Manchmal will man das Ergebnis *jetzt* haben – etwa weil die Quelle sich gleich ändert, das Ergebnis mehrfach gebraucht wird oder eine Methode eine `List<T>` verlangt. Die Methoden `ToList()` und `ToArray()` führen die Abfrage sofort aus und kopieren das Ergebnis in eine echte Collection:

```csharp
List<int> zahlen2 = [1, 2, 3, 4, 5];

List<int> geradeFest =
    (from z in zahlen2
     where z % 2 == 0
     select z).ToList();

zahlen2.Add(6);

Console.WriteLine(string.Join(", ", geradeFest)); // 2, 4
```

Diesmal fehlt die 6: `ToList()` hat die Abfrage in der Definitionszeile ausgeführt, und die Liste `geradeFest` ist seitdem von `zahlen2` entkoppelt. Dasselbe gilt für alle Methoden, die ein einzelnes Ergebnis brauchen und deshalb die ganze Quelle lesen müssen – `Count()`, `Max()`, `First()`. Man spricht dann von **sofortiger Ausführung** (*immediate execution*).

## Falle 1: mehrfache Enumeration

Weil ein `IEnumerable<T>` bei jedem Durchlauf neu ausgeführt wird, kostet jedes `foreach`, jedes `Count()` und jedes `Any()` einen vollständigen Durchlauf. Bei einer teuren Abfrage – über eine große Liste, mit aufwendiger Berechnung oder gar mit einer Datenbank als Quelle – ist das ein leicht zu übersehendes Leistungsproblem:

```csharp
IEnumerable<Gericht> leichteGerichte =
    from g in gerichte
    where g.Kilokalorien <= 300
    select g;

if (leichteGerichte.Any())                                 // 1. Durchlauf
{
    Console.WriteLine($"{leichteGerichte.Count()} Treffer"); // 2. Durchlauf
    foreach (Gericht g in leichteGerichte)                    // 3. Durchlauf
    {
        Console.WriteLine(g.Name);
    }
}
```

Drei Zeilen, drei komplette Filterläufe. Die IDE warnt bei solchen Mustern mit dem Hinweis „mögliche mehrfache Enumeration“. Die Lösung ist ein einziges `ToList()` nach der Definition – dann wird einmal gefiltert und die Liste danach dreimal gelesen.

Faustregel: Wird das Ergebnis genau einmal durchlaufen, ist `IEnumerable<T>` richtig – sparsam und ohne Kopie. Wird es mehrfach gebraucht oder darf es sich nicht mehr ändern, sofort `ToList()`. Und wer eine Abfrage aus einer Methode zurückgibt, sollte im Namen oder in der Dokumentation klarmachen, ob der Aufrufer einen Plan oder eine Liste bekommt.
{: .notice--primary}

## Falle 2: Quelle während der Iteration ändern

Die zweite Falle betrifft nicht nur LINQ, sondern jedes `foreach` über eine `List<T>` – mit LINQ ist sie aber leichter zu übersehen, weil zwischen Quelle und Schleife eine Abfrage steht. Wer während des Durchlaufs die zugrunde liegende Liste verändert, bekommt eine Exception:

```csharp
List<string> namen = ["Anna", "Bela", "Zoe"];

IEnumerable<string> kurze =
    from n in namen
    where n.Length <= 4
    select n;

foreach (string n in kurze)
{
    namen.Remove(n); // InvalidOperationException: Collection was modified
}
```

Es sieht so aus, als würden wir über `kurze` laufen und `namen` ändern – zwei verschiedene Dinge. Tatsächlich iteriert `kurze` aber direkt über `namen`, und die Liste bemerkt beim nächsten Schritt, dass sie sich seit Beginn des Durchlaufs verändert hat. Die Lösung ist wieder `ToList()`: Dann läuft die Schleife über eine Kopie, und `namen.Remove(n)` ist erlaubt.

```csharp
foreach (string n in kurze.ToList())
{
    namen.Remove(n); // funktioniert: Schleife läuft über die Kopie
}
Console.WriteLine(string.Join(", ", namen)); // (leer – alle drei hatten <= 4 Zeichen)
```

Verzögerte Ausführung ist kein Fehler, sondern der Grund, warum LINQ so leichtgewichtig ist: Man kann Abfragen definieren, weiterreichen, kombinieren und erst am Ende ausführen, ohne Zwischenlisten zu erzeugen. Wer die Regel „ein Plan wird bei jedem Durchlauf ausgeführt“ im Kopf hat, hat beide Fallen im Griff.
{: .notice--warning}

## Ausblick

Die Query-Syntax ist nur die Oberfläche. Intern übersetzt der Compiler `where z % 2 == 0` in einen Aufruf `zahlen.Where(z => z % 2 == 0)` – eine Methode, die als Parameter ein Stück Code bekommt. Solche Code-als-Parameter-Konstrukte heißen **Delegaten**, ihre Kurzschreibweise `z => z % 2 == 0` ist ein **Lambda-Ausdruck**. Beide sind das Thema der nächsten Vorlesung ([Lambda-Ausdrücke](/modules/lambda_ausdruecke/lambda_ausdruecke.md), [LINQ-Methodensyntax](/modules/linq_methodensyntax/linq_methodensyntax.md)) – und mit ihnen öffnet sich der Rest der LINQ-Welt: `Sum`, `Average`, `Take`, `Skip`, `Distinct` und viele mehr.

Übung: Erstelle eine `List<int>` mit den Zahlen 1 bis 10 und eine Abfrage, die alle Zahlen größer als 5 liefert. Gib das Ergebnis aus, entferne dann die 10 aus der Liste, füge die 20 hinzu und gib das Ergebnis erneut aus. Sage die beiden Ausgaben voraus. Wiederhole das Experiment mit `ToArray()` hinter der Abfrage – was ändert sich, und warum?
{: .notice--info}

## Weitere Quellen

- [Einführung in LINQ-Abfragen (verzögerte Ausführung) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/get-started/introduction-to-linq-queries)
- [Klassifizierung von Standardabfrageoperatoren nach Ausführungsart – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/linq/standard-query-operators/#classification-of-standard-query-operators-by-manner-of-execution)
- [Enumerable.ToList – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.linq.enumerable.tolist)
