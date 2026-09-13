---
title: "Such- und Sortieralgorithmen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Wer in einem Telefonbuch den Namen „Meier“ sucht, blättert nicht auf Seite 1 los und liest jeden Eintrag. Man schlägt es in der Mitte auf, sieht „K“, weiß, dass „M“ dahinter liegt, und halbiert den Rest – nach einer Handvoll Schritte ist man am Ziel. Diese Strategie funktioniert nur, weil das Telefonbuch **sortiert** ist. Suchen und Sortieren gehören deshalb zusammen: Das eine macht das andere schnell. In diesem Modul vergleichen wir die naive Suche mit der binären Suche, schauen uns an, warum unser Spielfeld seine erste Fassung nicht behalten konnte, und lernen, den Aufwand eines Algorithmus grob abzuschätzen, ohne ihn auszuführen.

## Lineare Suche – die erste Fassung des Spielfelds

Die einfachste Suche prüft ein Element nach dem anderen, bis der Treffer gefunden ist. Genau so hat das `Spielfeld` in [Vorlesung 01](/lectures/01/01.md) begonnen: Alle Objekte lagen in einer einzigen Liste, und `ObjektAn` lief bei jeder Anfrage von vorne los.

```csharp
private readonly List<Spielobjekt> objekte = new();

public Spielobjekt? ObjektAn(Position position)
{
    foreach (Spielobjekt o in objekte)
    {
        if (o.Position == position) return o;
    }
    return null;
}
```

Das Muster mit `foreach` und vorzeitigem Verlassen der Schleife kennen wir aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/arrays_schleifen/arrays_schleifen/); `List<T>.Contains` und `IndexOf` arbeiten intern genauso. Der Code hat einen großen Vorteil: Er setzt nichts voraus – keine Sortierung, keinen Hashcode. Und er hat einen großen Nachteil: Steht das gesuchte Objekt hinten oder gar nicht in der Liste, wird die gesamte Liste durchlaufen. Bei n Objekten sind das im schlimmsten Fall n Vergleiche.

Für ein Spielfeld ist das teurer, als es klingt. Das Level „Kerker“ ist 20 × 9 Felder groß und enthält 67 Objekte – allein 61 davon sind Wände und die Tür. `AlsText()` fragt für **jedes** der 180 Felder nach, was dort liegt – macht bis zu 180 × 67 ≈ 12.000 Positionsvergleiche für ein einziges gezeichnetes Bild. Dazu kommen `IstFrei` vor jedem Schritt und `HatSichtlinie` für jeden Verfolger, ebenfalls Feld für Feld. Bei einer großen Karte fängt das Spiel an zu ruckeln, ohne dass irgendetwas „falsch“ wäre.

## Nachschlagen statt suchen

Ab [Vorlesung 02](/lectures/02/02.md) sieht das Spielfeld deshalb anders aus. Die statischen Objekte liegen in einem `Dictionary<Position, StatischesObjekt>`, und aus der Schleife wird ein einziger Zugriff:

```csharp
private readonly Dictionary<Position, StatischesObjekt> statische = new();
private readonly List<Gegner> gegner = new();

public StatischesObjekt? StatischesObjektAn(Position p)
{
    return statische.TryGetValue(p, out StatischesObjekt? s) ? s : null;
}

public Spielobjekt? ObjektAn(Position p)
{
    if (p == Spieler.Position) return Spieler;
    Gegner? g = gegner.FirstOrDefault(x => x.Position == p);
    return g ?? (Spielobjekt?)StatischesObjektAn(p);
}
```

`TryGetValue` berechnet den Hashcode der Position, springt in den passenden Bucket und vergleicht dort höchstens eine Handvoll Einträge – unabhängig davon, ob zehn oder zehntausend Wände gespeichert sind. Aus 60 Vergleichen pro Feld wird einer. Interessant ist die zweite Methode: Für die Gegner bleibt es bei einer linearen Suche (`FirstOrDefault` durchläuft die Liste). Das ist kein Versehen, sondern eine bewusste Abwägung – in einem Level stehen zwei oder drei Gegner, und die bewegen sich jede Runde. Ein Dictionary müsste bei jedem Zug umgebaut werden, und veränderliche Schlüssel sind, wie wir im Modul [Hashcodes und Equals](/modules/hashcodes_equals/hashcodes_equals.md) gesehen haben, ohnehin eine schlechte Idee. **Die richtige Datenstruktur hängt am Zugriffsmuster, nicht an der Eleganz.**

## Binäre Suche: Teilen und Herrschen

Es gibt einen dritten Weg zwischen „alles durchgehen“ und „über den Hashcode nachschlagen“ – und der lohnt sich immer dann, wenn die Daten sortiert sind. Nehmen wir die Bestenliste des Spiels: eine aufsteigend sortierte `List<int>` mit den Punktzahlen aller bisherigen Durchgänge. Denken wir an ein Ratespiel: Jemand denkt sich eine Zahl zwischen 1 und 64, und wir dürfen fragen „größer oder kleiner als …?“. Die beste Strategie ist, immer die **Mitte** des verbleibenden Bereichs zu nennen:

| Frage | Verbleibender Bereich |
| :--- | :--- |
| Start | 64 Zahlen |
| 1. Frage („größer als 32?“) | 32 Zahlen |
| 2. Frage | 16 Zahlen |
| 3. Frage | 8 Zahlen |
| 4. Frage | 4 Zahlen |
| 5. Frage | 2 Zahlen |
| 6. Frage | 1 Zahl – gefunden |

Nach sechs Fragen ist die Zahl sicher bestimmt, weil 2⁶ = 64. Allgemein: Bei n sortierten Elementen braucht die binäre Suche höchstens **log₂ n** Vergleiche (aufgerundet). Der Unterschied zur linearen Suche wächst mit der Datenmenge:

| n | linear (max. Vergleiche) | binär (max. Vergleiche) |
| :--- | :--- | :--- |
| 64 | 64 | 6 |
| 1.000 | 1.000 | 10 |
| 1.000.000 | 1.000.000 | 20 |
| 1.000.000.000 | 1.000.000.000 | 30 |

Eine Milliarde Einträge in 30 Schritten – das ist der Grund, warum sich Sortieren lohnt, sobald man häufig sucht.

## Binäre Suche implementieren

Der Algorithmus verwaltet drei Indizes: `unten` und `oben` begrenzen den Bereich, in dem das Element noch liegen kann, `mitte` ist der Vergleichskandidat. Nach jedem Vergleich wandert eine der beiden Grenzen hinter die Mitte:

```csharp
static int BinaereSuche(IReadOnlyList<int> sortiert, int gesucht)
{
    int unten = 0;
    int oben = sortiert.Count - 1;

    while (unten <= oben)
    {
        int mitte = unten + (oben - unten) / 2;

        if (sortiert[mitte] == gesucht)
        {
            return mitte;
        }
        if (sortiert[mitte] < gesucht)
        {
            unten = mitte + 1; // links von mitte kann es nicht sein
        }
        else
        {
            oben = mitte - 1;  // rechts von mitte kann es nicht sein
        }
    }
    return -1; // nicht gefunden
}
```

Wir berechnen die Mitte als `unten + (oben - unten) / 2` statt `(unten + oben) / 2` – bei sehr großen Listen könnte die Summe sonst den `int`-Bereich überschreiten. Die Schleife endet, sobald `unten` über `oben` hinausläuft, also der Bereich leer ist:

```csharp
List<int> bestenliste = [120, 340, 500, 720, 980, 1150, 1400, 1870, 2300, 3100];
Console.WriteLine(BinaereSuche(bestenliste, 1150)); // 5
Console.WriteLine(BinaereSuche(bestenliste, 800));  // -1
```

<svg viewBox="0 0 710 314" role="img" aria-labelledby="binaere-suche-titel" xmlns="http://www.w3.org/2000/svg" style="max-width:100%;height:auto;font-family:system-ui,sans-serif"><title id="binaere-suche-titel">Binäre Suche nach 1150 in einer sortierten Bestenliste aus zehn Zahlen: In drei Schritten halbiert sich der Suchbereich zwischen den Grenzen unten und oben, bis die Mitte den gesuchten Wert trifft.</title><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle"><text x="120" y="32">0</text><text x="180" y="32">1</text><text x="240" y="32">2</text><text x="300" y="32">3</text><text x="360" y="32">4</text><text x="420" y="32">5</text><text x="480" y="32">6</text><text x="540" y="32">7</text><text x="600" y="32">8</text><text x="660" y="32">9</text></g><g stroke="currentColor" stroke-width="1.5" fill="currentColor" fill-opacity="0.06"><rect x="90" y="40" width="60" height="32"/><rect x="150" y="40" width="60" height="32"/><rect x="210" y="40" width="60" height="32"/><rect x="270" y="40" width="60" height="32"/><rect x="330" y="40" width="60" height="32"/><rect x="390" y="40" width="60" height="32"/><rect x="450" y="40" width="60" height="32"/><rect x="510" y="40" width="60" height="32"/><rect x="570" y="40" width="60" height="32"/><rect x="630" y="40" width="60" height="32"/></g><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle"><text x="120" y="61">120</text><text x="180" y="61">340</text><text x="240" y="61">500</text><text x="300" y="61">720</text><text x="360" y="61">980</text><text x="420" y="61">1150</text><text x="480" y="61">1400</text><text x="540" y="61">1870</text><text x="600" y="61">2300</text><text x="660" y="61">3100</text></g><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12"><text x="8" y="61">sortiert</text><text x="90" y="94">gesucht: 1150</text></g><g stroke="currentColor" stroke-width="1.5" stroke-opacity="0.3" fill="none"></g><g stroke="currentColor" stroke-width="1.5" fill="currentColor" fill-opacity="0.06"><rect x="90" y="110" width="60" height="28"/><rect x="150" y="110" width="60" height="28"/><rect x="210" y="110" width="60" height="28"/><rect x="270" y="110" width="60" height="28"/><rect x="390" y="110" width="60" height="28"/><rect x="450" y="110" width="60" height="28"/><rect x="510" y="110" width="60" height="28"/><rect x="570" y="110" width="60" height="28"/><rect x="630" y="110" width="60" height="28"/></g><rect x="330" y="110" width="60" height="28" stroke="#d33682" stroke-width="1.5" fill="#d33682" fill-opacity="0.12"/><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle"><text x="120" y="129">120</text><text x="180" y="129">340</text><text x="240" y="129">500</text><text x="300" y="129">720</text><text x="420" y="129">1150</text><text x="480" y="129">1400</text><text x="540" y="129">1870</text><text x="600" y="129">2300</text><text x="660" y="129">3100</text></g><text x="360" y="129" fill="#d33682" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle">980</text><text x="8" y="129" fill="currentColor" font-size="13">Schritt 1</text><g stroke="currentColor" stroke-width="1.5"><line x1="120" y1="140" x2="120" y2="146"/><line x1="660" y1="140" x2="660" y2="146"/></g><line x1="360" y1="140" x2="360" y2="146" stroke="#d33682" stroke-width="1.5"/><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle"><text x="120" y="158">unten</text><text x="660" y="158">oben</text></g><text x="360" y="158" fill="#d33682" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle">mitte</text><g stroke="currentColor" stroke-width="1.5" stroke-opacity="0.3" fill="none"><rect x="90" y="175" width="60" height="28"/><rect x="150" y="175" width="60" height="28"/><rect x="210" y="175" width="60" height="28"/><rect x="270" y="175" width="60" height="28"/><rect x="330" y="175" width="60" height="28"/></g><g stroke="currentColor" stroke-width="1.5" fill="currentColor" fill-opacity="0.06"><rect x="390" y="175" width="60" height="28"/><rect x="450" y="175" width="60" height="28"/><rect x="570" y="175" width="60" height="28"/><rect x="630" y="175" width="60" height="28"/></g><rect x="510" y="175" width="60" height="28" stroke="#d33682" stroke-width="1.5" fill="#d33682" fill-opacity="0.12"/><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle"><text x="420" y="194">1150</text><text x="480" y="194">1400</text><text x="600" y="194">2300</text><text x="660" y="194">3100</text></g><text x="540" y="194" fill="#d33682" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle">1870</text><text x="8" y="194" fill="currentColor" font-size="13">Schritt 2</text><g stroke="currentColor" stroke-width="1.5"><line x1="420" y1="205" x2="420" y2="211"/><line x1="660" y1="205" x2="660" y2="211"/></g><line x1="540" y1="205" x2="540" y2="211" stroke="#d33682" stroke-width="1.5"/><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle"><text x="420" y="223">unten</text><text x="660" y="223">oben</text></g><text x="540" y="223" fill="#d33682" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle">mitte</text><g stroke="currentColor" stroke-width="1.5" stroke-opacity="0.3" fill="none"><rect x="90" y="240" width="60" height="28"/><rect x="150" y="240" width="60" height="28"/><rect x="210" y="240" width="60" height="28"/><rect x="270" y="240" width="60" height="28"/><rect x="330" y="240" width="60" height="28"/><rect x="510" y="240" width="60" height="28"/><rect x="570" y="240" width="60" height="28"/><rect x="630" y="240" width="60" height="28"/></g><g stroke="currentColor" stroke-width="1.5" fill="currentColor" fill-opacity="0.06"><rect x="450" y="240" width="60" height="28"/></g><rect x="390" y="240" width="60" height="28" stroke="#d33682" stroke-width="1.5" fill="#d33682" fill-opacity="0.12"/><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle"><text x="480" y="259">1400</text></g><text x="420" y="259" fill="#d33682" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle">1150</text><text x="8" y="259" fill="currentColor" font-size="13">Schritt 3</text><g stroke="currentColor" stroke-width="1.5"><line x1="420" y1="270" x2="420" y2="276"/><line x1="480" y1="270" x2="480" y2="276"/></g><g fill="currentColor" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle"><text x="420" y="288">unten</text><text x="480" y="288">oben</text></g><text x="420" y="304" fill="#d33682" font-family="ui-monospace,monospace" font-size="12" text-anchor="middle">mitte</text></svg>

Der erste Vergleich trifft die `980` an Index 4 – zu klein, also wandert `unten` auf 5 und die linke Hälfte ist mit einem Schlag erledigt. Der zweite Vergleich trifft die `1870` an Index 7 – zu groß, `oben` rückt auf 6. Im dritten Schritt fallen `unten` und `mitte` auf denselben Index 5, und dort steht die gesuchte `1150`. Drei Vergleiche statt sechs bei der linearen Suche, und das bei nur zehn Einträgen; der Abstand wächst mit jeder Verdopplung der Liste um genau einen weiteren Schritt.

Die **Vorbedingung** ist unverhandelbar: Die Liste muss sortiert sein. Auf unsortierten Daten liefert die binäre Suche kein falsches Ergebnis mit Fehlermeldung, sondern schlicht Unsinn – mal einen Treffer, mal `-1`, ohne dass irgendetwas auffällt. Wer die Liste nicht selbst sortiert hat, sollte sie vor der Suche mit `Sort` ordnen oder eine lineare Suche verwenden.
{: .notice--warning}

.NET bringt die Implementierung fertig mit: `bestenliste.BinarySearch(1150)` liefert `5`, und `Array.BinarySearch` tut dasselbe für Arrays. Beide funktionieren für jeden Typ, der [`IComparable<T>`](/modules/icomparable_sortieren/icomparable_sortieren.md) implementiert, und nehmen wahlweise einen `IComparer<T>` entgegen. Fehlt das Element, geben sie eine **negative Zahl** zurück, die verschlüsselt, wo es eingefügt werden müsste – genau das brauchen wir, um einen neuen Punktestand an der richtigen Stelle in die Bestenliste zu schieben, und genau das übt die zweite [Aufgabe](/modules/aufgaben_collections_linq/aufgaben_collections_linq.md).

## Sortieren: Was macht `Array.Sort`?

Um zu verstehen, was Sortieren kostet, hilft ein Blick auf den naivsten Algorithmus. **Selection Sort** sucht das kleinste Element und tauscht es an Position 0, sucht dann das kleinste der verbleibenden und tauscht es an Position 1, und so weiter:

```csharp
static void SelectionSort(int[] daten)
{
    for (int i = 0; i < daten.Length - 1; i++)
    {
        int kleinster = i;
        for (int j = i + 1; j < daten.Length; j++)
        {
            if (daten[j] < daten[kleinster])
            {
                kleinster = j;
            }
        }
        (daten[i], daten[kleinster]) = (daten[kleinster], daten[i]);
    }
}
```

Der Tausch mit Tupeln in der letzten Zeile ersetzt die klassische Hilfsvariable. Der Algorithmus ist leicht zu verstehen, aber die verschachtelten Schleifen verraten das Problem: Für jedes der n Elemente wird der Rest durchsucht, also insgesamt etwa n²/2 Vergleiche. Bei einer Million Elementen sind das 500 Milliarden Vergleiche – Minuten statt Millisekunden. **Bubble Sort**, der wiederholt benachbarte Elemente vertauscht, liegt in derselben Klasse.

`Array.Sort` und `List<T>.Sort` verwenden stattdessen **Introsort**, eine Kombination aus Quicksort, Heapsort und Insertion Sort. Alle drei folgen der Idee „Teilen und Herrschen“ wie die binäre Suche und kommen mit etwa **n · log₂ n** Vergleichen aus – für eine Million Elemente rund 20 Millionen statt 500 Milliarden. Für die Praxis heißt das: Eigene Sortieralgorithmen schreibt man zum Lernen, nicht für den Produktivcode. Wenn wir im Modul [`IComparable<T>`](/modules/icomparable_sortieren/icomparable_sortieren.md) die Gegner nach Entfernung sortieren, ruft `Sort` nur unseren Comparer auf und erledigt den Rest selbst.

## Aufwand grob abschätzen: die O-Notation

Damit man solche Vergleiche nicht jedes Mal ausrechnen muss, beschreibt man das Wachstum eines Algorithmus mit der **O-Notation**. Sie ignoriert Konstanten und kleine Terme und behält nur, wie der Aufwand mit n wächst:

| Notation | Sprich | Beispiel |
| :--- | :--- | :--- |
| O(1) | konstant | Array-Zugriff `a[i]`, `StatischesObjektAn` über das Dictionary |
| O(log n) | logarithmisch | binäre Suche in der Bestenliste |
| O(n) | linear | `ObjektAn` in der ersten Fassung, `foreach` über alle Gegner |
| O(n log n) | linearithmisch | `Array.Sort`, `List<T>.Sort` |
| O(n²) | quadratisch | Selection Sort, Bubble Sort, verschachtelte Schleife über dieselbe Liste |

Das ist eine grobe Faustregel, keine Stoppuhr – für zehn Elemente ist ein O(n²)-Algorithmus oft schneller als ein aufwendiger O(n log n)-Algorithmus, und genau deshalb darf die Gegnersuche linear bleiben. Aber sobald n groß wird, entscheidet allein die Zeile in dieser Tabelle. Wer eine `foreach`-Schleife in eine andere `foreach`-Schleife über dieselbe Liste schreibt, hat O(n²) gebaut – und sollte kurz innehalten, ob ein `Dictionary` oder eine vorherige Sortierung das nicht besser lösen.
{: .notice--primary}

Übung: Baue die alte Fassung von `ObjektAn` mit einer `List<Spielobjekt>` nach und zähle mit einem Zähler mit, wie viele Positionsvergleiche ein einziger Aufruf von `AlsText()` für das Level „Kerker“ auslöst. Vergleiche mit der heutigen Dictionary-Fassung. Ergänze anschließend `BinaereSuche` um einen Zähler und vergleiche die Anzahl der Vergleiche mit einer linearen Suche in einer Bestenliste aus 1.000 Werten. Was passiert mit beiden Zahlen, wenn der gesuchte Punktestand gar nicht vorkommt?
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure); die erste Fassung des Spielfelds steckt im Tag `v01-vererbung`, die Dictionary-Fassung in `v02-interfaces`.

## Weitere Quellen

- [Array.BinarySearch – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.array.binarysearch)
- [Array.Sort – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.array.sort) (Abschnitt „Hinweise“ beschreibt Introsort)
- [List<T>.BinarySearch – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.list-1.binarysearch)
- [Sortieralgorithmen animiert – VisuAlgo](https://visualgo.net/en/sorting) – Selection Sort, Quicksort und Heapsort nebeneinander laufen lassen und die Vergleiche mitzählen
- [Big-O Cheat Sheet](https://www.bigocheatsheet.com/) – eine Tabelle mit dem Aufwand aller gängigen Such-, Sortier- und Collection-Operationen, inklusive Farbskala
