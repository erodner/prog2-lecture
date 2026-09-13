---
title: "Collections im Überblick – wann welche?"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ein Werkzeugkasten mit nur einem Hammer ist schnell erklärt, aber für Schrauben ungeeignet. Ähnlich geht es vielen Programmen, die für alles `List<T>` verwenden: Es funktioniert, aber sobald eine Liste als Warteschlange missbraucht wird, jede Suche linear durchläuft oder Duplikate mühsam von Hand vermieden werden, wird der Code langsam und umständlich. Genau diesen Weg ist unser Spielfeld gegangen – von einer einzigen `List<Spielobjekt>` zu einem `Dictionary<Position, StatischesObjekt>` neben einer `List<Gegner>`. .NET bringt im Namespace `System.Collections.Generic` eine ganze Familie generischer Collections mit, die jeweils für ein bestimmtes Zugriffsmuster gebaut sind. Aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/collections/collections/) kennen wir `List<T>` und `Dictionary<K, V>` – dieses Modul ordnet sie in die Familie ein und gibt eine Entscheidungshilfe, welche Collection wann die richtige ist.

## `List<T>` – das wachsende Array

Eine `List<T>` ([Prog 1](https://www.erodner.de/prog-lecture/modules/list/list/)) ist intern ein Array, das bei Bedarf gegen ein doppelt so großes ausgetauscht wird. Die Eigenschaft `Capacity` zeigt, wie viel Platz das interne Array gerade hat, `Count` wie viele Elemente tatsächlich darin liegen:

```csharp
List<Gegner> gegner = new List<Gegner>();
Console.WriteLine($"{gegner.Count} / {gegner.Capacity}"); // 0 / 0

gegner.Add(new Wache(new Position(13, 2)));
gegner.Add(new Verfolger(new Position(11, 5)));
Console.WriteLine($"{gegner.Count} / {gegner.Capacity}"); // 2 / 4
```

Das Verdoppeln passiert selten, deshalb ist `Add` am Ende im Mittel konstant schnell. Der Zugriff über den Index `gegner[0]` ist O(1) wie beim Array – das nutzt `Spielfeld.Wiederherstellen`, um jedem Gegner seine gespeicherte Position zuzuweisen. Teuer sind dagegen `Insert(0, x)` und `Remove` in der Mitte: Alle nachfolgenden Elemente müssen verschoben werden, also O(n). Und `Contains` durchsucht die Liste linear. Für eine Handvoll Gegner, die jede Runde vollständig durchlaufen werden, ist die Liste trotzdem genau richtig.

## Schlüssel und Mengen: `Dictionary`, `HashSet`

`Dictionary<K, V>` ([Prog 1](https://www.erodner.de/prog-lecture/modules/dictionary/dictionary/)) speichert Wertepaare und findet einen Wert über seinen Schlüssel in O(1) – über den Hashcode, wie im Modul [Hashcodes und Equals](/modules/hashcodes_equals/hashcodes_equals.md) beschrieben. Genau deshalb liegen die Wände, Türen, Truhen und Gegenstände im Spielfeld unter ihrer `Position`. Ein `HashSet<T>` ist ein Dictionary ohne Werte: eine **Menge**, in der jedes Element höchstens einmal vorkommt. Das passt perfekt zu der Frage „War der Held hier schon einmal?“:

```csharp
HashSet<Position> besucht = new HashSet<Position>();
Console.WriteLine(besucht.Add(new Position(1, 1)));   // True
Console.WriteLine(besucht.Add(new Position(1, 2)));   // True
Console.WriteLine(besucht.Add(new Position(1, 1)));   // False – schon dagewesen
Console.WriteLine(besucht.Contains(new Position(1, 2))); // True
Console.WriteLine(besucht.Count);                     // 2
```

`Add` gibt zurück, ob das Element neu war – man braucht keinen vorherigen `Contains`-Aufruf. Ein `HashSet` kennt außerdem Mengenoperationen wie `UnionWith`, `IntersectWith` und `ExceptWith`; „alle Felder, die der Held gesehen, aber nie betreten hat“ ist damit eine Zeile. Beide Hash-Collections haben **keine definierte Reihenfolge**: Wer über sie iteriert, bekommt die Elemente in einer Ordnung, auf die man sich nicht verlassen darf. Deshalb baut `AlsText()` die Karte auch nicht aus `statische.Values` auf, sondern läuft von Hand über alle Zeilen und Spalten.

## Sortierte Varianten: `SortedList`, `SortedDictionary`, `SortedSet`

Braucht man die Elemente dauerhaft sortiert, gibt es zu jeder Hash-Collection ein sortiertes Gegenstück. Sie verlangen, dass der Schlüssel [`IComparable<T>`](/modules/icomparable_sortieren/icomparable_sortieren.md) implementiert, und zahlen für die Ordnung mit O(log n) statt O(1) pro Zugriff:

```csharp
SortedDictionary<string, int> bestzeiten = new SortedDictionary<string, int>();
bestzeiten["Schatzkammer"] = 41;
bestzeiten["Katakomben"] = 88;
bestzeiten["Kerker"] = 23;

foreach (KeyValuePair<string, int> eintrag in bestzeiten)
{
    Console.WriteLine($"{eintrag.Key}: {eintrag.Value} Runden");
}
// Katakomben: 88 Runden
// Kerker: 23 Runden
// Schatzkammer: 41 Runden
```

`SortedDictionary<K, V>` ist intern ein balancierter Baum – Einfügen und Löschen sind O(log n). `SortedList<K, V>` verwaltet dagegen zwei sortierte Arrays: Der Zugriff über den Schlüssel geschieht per binärer Suche in O(log n), Einfügen in der Mitte kostet aber O(n) durch das Verschieben. Dafür braucht sie weniger Speicher und erlaubt Zugriff über einen Positionsindex. Faustregel: viele Einfügungen in beliebiger Reihenfolge → `SortedDictionary`; einmal befüllen, dann nur lesen → `SortedList`. `SortedSet<T>` ist das sortierte `HashSet` mit `Min`, `Max` und Bereichsabfragen wie `GetViewBetween` – für eine Bestenliste, die immer sortiert bleiben soll, die naheliegende Wahl.

## Reihenfolge des Zugriffs: `Queue`, `Stack`, `LinkedList`

Manchmal ist nicht wichtig, ein beliebiges Element schnell zu finden, sondern das **nächste** in einer bestimmten Reihenfolge zu bekommen. `Queue<T>` arbeitet nach dem Prinzip „wer zuerst kommt, kommt zuerst dran“ (FIFO), `Stack<T>` nach „das zuletzt Abgelegte kommt zuerst wieder“ (LIFO). Im Spiel sind beide naheliegend: Eine Warteschlange nimmt geplante Züge auf, die Runde für Runde abgearbeitet werden, ein Stapel merkt sich die bereits gelaufenen Schritte für eine Rückgängig-Funktion.

```csharp
Queue<Richtung> plan = new Queue<Richtung>();
plan.Enqueue(Richtung.Unten);
plan.Enqueue(Richtung.Unten);
plan.Enqueue(Richtung.Rechts);

while (plan.Count > 0 && feld.Status == Spielstatus.Laeuft)
{
    Richtung naechste = plan.Dequeue();   // Unten, Unten, Rechts – in dieser Reihenfolge
    feld.SpielerZieht(naechste);
}

Stack<Richtung> gegangen = new Stack<Richtung>();
gegangen.Push(Richtung.Unten);
gegangen.Push(Richtung.Rechts);
Console.WriteLine(gegangen.Pop());  // Rechts – der letzte Schritt zuerst
Console.WriteLine(gegangen.Peek()); // Unten
```

`Peek` schaut auf das nächste Element, ohne es zu entfernen. Beide Operationen sind O(1). Eine `LinkedList<T>` schließlich ist eine doppelt verkettete Liste: Einfügen und Entfernen an einer bekannten Stelle (einem `LinkedListNode<T>`) ist O(1), dafür gibt es keinen Indexzugriff – um das fünfte Element zu erreichen, muss man von vorn hangeln. In der Praxis ist sie selten die beste Wahl; `List<T>` ist wegen des zusammenhängenden Speichers meist trotz des Verschiebens schneller.

## Wann was? Eine Entscheidungstabelle

| Collection | Zugriff auf Element | Einfügen | Ordnung | Einsatz im Adventure |
| :--- | :--- | :--- | :--- | :--- |
| `List<T>` | O(1) über Index | O(1) am Ende, O(n) in der Mitte | Einfügereihenfolge | `List<Gegner>`: wenige Elemente, jede Runde komplett durchlaufen |
| `Dictionary<K, V>` | O(1) über Schlüssel | O(1) | keine | `Dictionary<Position, StatischesObjekt>`: „Was liegt auf diesem Feld?“ |
| `HashSet<T>` | O(1) `Contains` | O(1) | keine | `HashSet<Position>`: bereits besuchte Felder, Nebel des Krieges |
| `SortedDictionary<K, V>` | O(log n) | O(log n) | nach Schlüssel | Bestzeiten je Level, sortiert ausgegeben |
| `SortedList<K, V>` | O(log n), auch per Index | O(n) | nach Schlüssel | Levelverzeichnis: einmal füllen, oft sortiert lesen |
| `SortedSet<T>` | O(log n) | O(log n) | sortiert | Bestenliste mit `Min`/`Max` |
| `Queue<T>` | O(1) nur vorne | O(1) hinten | FIFO | `Queue<Richtung>`: geplante Züge abarbeiten |
| `Stack<T>` | O(1) nur oben | O(1) oben | LIFO | `Stack<Richtung>`: Rückgängig-Funktion |
| `LinkedList<T>` | O(n) | O(1) an bekannter Stelle | Einfügereihenfolge | selten; häufiges Einfügen in der Mitte |

Im Zweifel `List<T>` – und sobald eine Suche nach Schlüssel, eine Duplikatprüfung oder eine feste Zugriffsreihenfolge auftaucht, zur spezialisierten Collection wechseln. Die Tabelle ist die Landkarte; welchen Aufwand die Buchstaben O(1), O(log n) und O(n) bedeuten, erklärt das Modul [Such- und Sortieralgorithmen](/modules/such_und_sortieralgorithmen/such_und_sortieralgorithmen.md).
{: .notice--primary}

## Die Interfaces dahinter

Alle diese Klassen teilen sich eine Hierarchie von [Interfaces](/modules/interfaces_grundlagen/interfaces_grundlagen.md), und die ist für gute Methodensignaturen wichtiger als die konkreten Klassen:

- `IEnumerable<T>` – kann mit `foreach` durchlaufen werden. Das können **alle** Collections, auch Arrays und Strings.
- `ICollection<T>` – hat zusätzlich `Count`, `Add`, `Remove`, `Contains`.
- `IList<T>` – erlaubt zusätzlich Indexzugriff `[i]` und `Insert`. Implementiert von `List<T>` und Arrays.
- `IDictionary<K, V>` – Zugriff über Schlüssel. Implementiert von `Dictionary`, `SortedDictionary`, `SortedList`.
- `IReadOnlyList<T>`, `IReadOnlyDictionary<K, V>` – nur lesender Zugriff.

Das `Spielfeld` nutzt beide Enden dieser Liste. Nach außen gibt es seine Gegner als `IReadOnlyList<Gegner>` heraus – lesen ja, `Clear()` nein. Und alle Objekte zusammen liefert es als `IEnumerable<Spielobjekt>`, das gar keine Collection mehr ist, sondern eine Aufzählung, die beim Durchlaufen entsteht:

```csharp
public IReadOnlyList<Gegner> Gegner => gegner;

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

Daraus folgt eine Faustregel für Parameter: **So allgemein wie möglich annehmen.** Eine Methode, die nur durchläuft, sollte `IEnumerable<T>` verlangen – dann kann man ihr ein Array, eine Liste, ein `HashSet`, `feld.AlleObjekte` oder das Ergebnis einer LINQ-Abfrage übergeben:

```csharp
static int Gesamtwert(IEnumerable<Gegenstand> sachen)
{
    int summe = 0;
    foreach (Gegenstand g in sachen)
    {
        if (g is Schatz schatz) summe += schatz.Wert;
    }
    return summe;
}

Console.WriteLine(Gesamtwert(held.Inventar));                          // Inventar<Gegenstand> ist IEnumerable<Gegenstand>
Console.WriteLine(Gesamtwert(new[] { new Schatz(new Position(0, 0), 100) }));
```

Hätten wir `List<Gegenstand>` als Parametertyp gewählt, müsste das Inventar erst umkopiert werden. Und der Rückgabetyp? Hier darf es konkreter sein, damit der Aufrufer weiß, was er bekommt – aber wer eine interne Liste nach außen gibt, sollte `IReadOnlyList<T>` wählen, genau wie das Spielfeld es tut.

Übung: Erweitere das Spielfeld gedanklich um ein `HashSet<Position> besucht`, in das bei jedem erfolgreichen Zug die neue Spielerposition eingetragen wird. Schreibe eine Methode `AlsTextMitNebel()`, die nur besuchte Felder zeichnet und alles andere als Leerzeichen ausgibt. Warum ist ein `HashSet<Position>` hier besser als eine `List<Position>` – und wie viele Vergleiche spart es pro gezeichnetem Feld? Was müsste sich ändern, wenn `Position` eine Klasse ohne `Equals`/`GetHashCode` wäre?
{: .notice--info}

## Weitere Quellen

- [Collections und Datenstrukturen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/collections/)
- [Auswählen einer Collection-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/collections/selecting-a-collection-class)
- [System.Collections.Generic – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic)
- [Big-O Cheat Sheet](https://www.bigocheatsheet.com/) – dieselbe Entscheidungstabelle noch einmal, nur nach Datenstruktur statt nach .NET-Klasse sortiert
- [Verkettete Listen visualisiert – VisuAlgo](https://visualgo.net/en/list) – zeigt, warum `LinkedList<T>` beim Einfügen gewinnt und beim Indexzugriff verliert
