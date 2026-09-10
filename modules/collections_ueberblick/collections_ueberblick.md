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

Ein Werkzeugkasten mit nur einem Hammer ist schnell erklärt, aber für Schrauben ungeeignet. Ähnlich geht es vielen Programmen, die für alles `List<T>` verwenden: Es funktioniert, aber sobald eine Liste als Warteschlange missbraucht wird, jede Suche linear durchläuft oder Duplikate mühsam von Hand vermeidet, wird der Code langsam und umständlich. .NET bringt im Namespace `System.Collections.Generic` eine ganze Familie generischer Collections mit, die jeweils für ein bestimmtes Zugriffsmuster gebaut sind. Aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/collections/collections/) kennen wir `List<T>` und `Dictionary<K, V>` – dieses Modul ordnet sie in die Familie ein und gibt eine Entscheidungshilfe, welche Collection wann die richtige ist.

## `List<T>` – das wachsende Array

Eine `List<T>` ([Prog 1](https://www.erodner.de/prog-lecture/modules/list/list/)) ist intern ein Array, das bei Bedarf gegen ein doppelt so großes ausgetauscht wird. Die Eigenschaft `Capacity` zeigt, wie viel Platz das interne Array gerade hat, `Count` wie viele Elemente tatsächlich darin liegen:

```csharp
List<int> zahlen = new List<int>();
Console.WriteLine($"{zahlen.Count} / {zahlen.Capacity}"); // 0 / 0

for (int i = 1; i <= 5; i++)
{
    zahlen.Add(i);
}
Console.WriteLine($"{zahlen.Count} / {zahlen.Capacity}"); // 5 / 8
```

Das Verdoppeln passiert selten, deshalb ist `Add` am Ende im Mittel konstant schnell. Der Zugriff über den Index `zahlen[3]` ist O(1) wie beim Array. Teuer sind dagegen `Insert(0, x)` und `Remove` in der Mitte: Alle nachfolgenden Elemente müssen verschoben werden, also O(n). Und `Contains` durchsucht die Liste linear. Wer weiß, wie viele Elemente kommen, kann die Kapazität im Konstruktor vorgeben (`new List<int>(10000)`) und spart die Umkopiervorgänge.

## Schlüssel und Mengen: `Dictionary`, `HashSet`

`Dictionary<K, V>` ([Prog 1](https://www.erodner.de/prog-lecture/modules/dictionary/dictionary/)) speichert Wertepaare und findet einen Wert über seinen Schlüssel in O(1) – über den Hashcode, wie im Modul [Hashcodes und Equals](/modules/hashcodes_equals/hashcodes_equals.md) beschrieben. Ein `HashSet<T>` ist ein Dictionary ohne Werte: eine **Menge**, in der jedes Element höchstens einmal vorkommt.

```csharp
HashSet<string> besuchteSeiten = new HashSet<string>();
Console.WriteLine(besuchteSeiten.Add("index.html"));   // True
Console.WriteLine(besuchteSeiten.Add("impressum.html")); // True
Console.WriteLine(besuchteSeiten.Add("index.html"));   // False – schon drin
Console.WriteLine(besuchteSeiten.Contains("index.html")); // True
Console.WriteLine(besuchteSeiten.Count);               // 2
```

`Add` gibt zurück, ob das Element neu war – man braucht keinen vorherigen `Contains`-Aufruf. Ein `HashSet` kennt außerdem Mengenoperationen wie `UnionWith`, `IntersectWith` und `ExceptWith`. Beide Hash-Collections haben **keine definierte Reihenfolge**: Wer über sie iteriert, bekommt die Elemente in einer Ordnung, auf die man sich nicht verlassen darf.

## Sortierte Varianten: `SortedList`, `SortedDictionary`, `SortedSet`

Braucht man die Elemente dauerhaft sortiert, gibt es zu jeder Hash-Collection ein sortiertes Gegenstück. Sie verlangen, dass der Schlüssel [`IComparable<T>`](/modules/icomparable_sortieren/icomparable_sortieren.md) implementiert, und zahlen für die Ordnung mit O(log n) statt O(1) pro Zugriff:

```csharp
SortedDictionary<string, int> punkte = new SortedDictionary<string, int>();
punkte["Zoe"] = 12;
punkte["Anna"] = 9;
punkte["Max"] = 15;

foreach (KeyValuePair<string, int> eintrag in punkte)
{
    Console.WriteLine($"{eintrag.Key}: {eintrag.Value}");
}
// Anna: 9
// Max: 15
// Zoe: 12
```

`SortedDictionary<K, V>` ist intern ein balancierter Baum – Einfügen und Löschen sind O(log n). `SortedList<K, V>` verwaltet dagegen zwei sortierte Arrays: Der Zugriff über den Schlüssel geschieht per binärer Suche in O(log n), Einfügen in der Mitte kostet aber O(n) durch das Verschieben. Dafür braucht sie weniger Speicher und erlaubt Zugriff über einen Positionsindex. Faustregel: viele Einfügungen in beliebiger Reihenfolge → `SortedDictionary`; einmal befüllen, dann nur lesen → `SortedList`. `SortedSet<T>` ist das sortierte `HashSet` mit `Min`, `Max` und Bereichsabfragen wie `GetViewBetween`.

## Reihenfolge des Zugriffs: `Queue`, `Stack`, `LinkedList`

Manchmal ist nicht wichtig, ein beliebiges Element schnell zu finden, sondern das **nächste** in einer bestimmten Reihenfolge zu bekommen. `Queue<T>` arbeitet nach dem Prinzip „wer zuerst kommt, kommt zuerst dran“ (FIFO), `Stack<T>` nach „das zuletzt Abgelegte kommt zuerst wieder“ (LIFO):

```csharp
Queue<string> druckauftraege = new Queue<string>();
druckauftraege.Enqueue("Bericht.pdf");
druckauftraege.Enqueue("Foto.png");
Console.WriteLine(druckauftraege.Dequeue()); // Bericht.pdf

Stack<string> rueckgaengig = new Stack<string>();
rueckgaengig.Push("Kreis eingefügt");
rueckgaengig.Push("Rechteck verschoben");
Console.WriteLine(rueckgaengig.Pop());  // Rechteck verschoben
Console.WriteLine(rueckgaengig.Peek()); // Kreis eingefügt
```

`Peek` schaut auf das nächste Element, ohne es zu entfernen. Beide Operationen sind O(1). Eine `LinkedList<T>` schließlich ist eine doppelt verkettete Liste: Einfügen und Entfernen an einer bekannten Stelle (einem `LinkedListNode<T>`) ist O(1), dafür gibt es keinen Indexzugriff – um das fünfte Element zu erreichen, muss man von vorn hangeln. In der Praxis ist sie selten die beste Wahl; `List<T>` ist wegen des zusammenhängenden Speichers meist trotz des Verschiebens schneller.

## Wann was? Eine Entscheidungstabelle

| Collection | Zugriff auf Element | Einfügen | Ordnung | Typischer Einsatz |
| :--- | :--- | :--- | :--- | :--- |
| `List<T>` | O(1) über Index | O(1) am Ende, O(n) in der Mitte | Einfügereihenfolge | Standard für „ein paar Dinge sammeln“ |
| `Dictionary<K, V>` | O(1) über Schlüssel | O(1) | keine | Nachschlagen: Id → Objekt, Name → Wert |
| `HashSet<T>` | O(1) `Contains` | O(1) | keine | Duplikate verhindern, „schon gesehen?“ |
| `SortedDictionary<K, V>` | O(log n) | O(log n) | nach Schlüssel | sortierte Ausgabe bei vielen Änderungen |
| `SortedList<K, V>` | O(log n), auch per Index | O(n) | nach Schlüssel | einmal füllen, oft sortiert lesen |
| `SortedSet<T>` | O(log n) | O(log n) | sortiert | Menge mit `Min`/`Max`/Bereichen |
| `Queue<T>` | O(1) nur vorne | O(1) hinten | FIFO | Aufträge abarbeiten |
| `Stack<T>` | O(1) nur oben | O(1) oben | LIFO | Rückgängig, Klammern prüfen |
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

Daraus folgt eine Faustregel für Parameter: **So allgemein wie möglich annehmen.** Eine Methode, die nur durchläuft, sollte `IEnumerable<T>` verlangen – dann kann man ihr ein Array, eine Liste, ein `HashSet` oder das Ergebnis einer LINQ-Abfrage übergeben. Braucht sie Indexzugriff, aber ändert nichts, ist `IReadOnlyList<T>` passend:

```csharp
static double Durchschnitt(IEnumerable<double> werte)
{
    double summe = 0;
    int anzahl = 0;
    foreach (double wert in werte)
    {
        summe += wert;
        anzahl++;
    }
    return anzahl == 0 ? 0 : summe / anzahl;
}

Console.WriteLine(Durchschnitt([1.0, 2.0, 3.0]));                  // 2
Console.WriteLine(Durchschnitt(new HashSet<double> { 4.0, 6.0 })); // 5
```

Hätten wir `List<double>` als Parametertyp gewählt, müsste der zweite Aufruf das `HashSet` erst umkopieren. Und der Rückgabetyp? Hier darf es konkreter sein, damit der Aufrufer weiß, was er bekommt – aber wer eine interne Liste nach außen gibt, sollte darüber nachdenken, ob `IReadOnlyList<T>` nicht besser ist, damit niemand von außen `Clear()` aufruft.

Übung: Ein Programm verwaltet Studierende (`Matrikelnummer`, `Name`) und muss (a) zu einer Matrikelnummer schnell den Namen liefern, (b) prüfen, ob eine Matrikelnummer schon vergeben ist, (c) alle Namen alphabetisch ausgeben und (d) Anmeldungen zu einer Sprechstunde in Eingangsreihenfolge abarbeiten. Wähle für jede Anforderung eine Collection aus der Tabelle und begründe – reicht vielleicht eine Collection für (a) und (b) zusammen?
{: .notice--info}

## Weitere Quellen

- [Collections und Datenstrukturen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/collections/)
- [Auswählen einer Collection-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/collections/selecting-a-collection-class)
- [System.Collections.Generic – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic)
