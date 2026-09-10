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

Wer in einem Telefonbuch den Namen „Meier“ sucht, blättert nicht auf Seite 1 los und liest jeden Eintrag. Man schlägt es in der Mitte auf, sieht „K“, weiß, dass „M“ dahinter liegt, und halbiert den Rest – nach einer Handvoll Schritte ist man am Ziel. Diese Strategie funktioniert nur, weil das Telefonbuch **sortiert** ist. Suchen und Sortieren gehören deshalb zusammen: Das eine macht das andere schnell. In diesem Modul vergleichen wir die naive Suche mit der binären Suche, schauen, was `Array.Sort` intern tut, und lernen, den Aufwand eines Algorithmus grob abzuschätzen, ohne ihn auszuführen.

## Lineare Suche

Die einfachste Suche prüft ein Element nach dem anderen, bis der Treffer gefunden ist. Aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/arrays_schleifen/arrays_schleifen/) kennen wir das Muster mit `foreach` und `break`:

```csharp
List<string> kunden = ["Anna Ahrens", "Bela Brandt", "Zoe Ziegler"];
bool gefunden = false;

foreach (string kunde in kunden)
{
    if (kunde == "Zoe Ziegler")
    {
        gefunden = true;
        break;
    }
}
Console.WriteLine(gefunden); // True
```

Steht der gesuchte Eintrag ganz hinten oder fehlt er, wird die gesamte Liste durchlaufen. Bei n Elementen sind das im schlimmsten Fall n Vergleiche – bei einer Million Einträge eine Million Vergleiche. `List<T>.Contains` und `IndexOf` arbeiten genau so. Für kleine oder unsortierte Listen ist das völlig in Ordnung, und der Code hat einen großen Vorteil: Er setzt nichts voraus.

## Binäre Suche: Teilen und Herrschen

Wenn die Daten sortiert sind, geht es dramatisch besser. Denken wir an ein Ratespiel: Jemand denkt sich eine Zahl zwischen 1 und 64, und wir dürfen fragen „größer oder kleiner als …?“. Die beste Strategie ist, immer die **Mitte** des verbleibenden Bereichs zu nennen:

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
static int BinaereSuche(int[] sortiert, int gesucht)
{
    int unten = 0;
    int oben = sortiert.Length - 1;

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

Wir berechnen die Mitte als `unten + (oben - unten) / 2` statt `(unten + oben) / 2` – bei sehr großen Arrays könnte die Summe sonst den `int`-Bereich überschreiten. Die Schleife endet, sobald `unten` über `oben` hinausläuft, also der Bereich leer ist:

```csharp
int[] zahlen = [2, 5, 8, 12, 16, 23, 38, 56, 72, 91];
Console.WriteLine(BinaereSuche(zahlen, 23)); // 5
Console.WriteLine(BinaereSuche(zahlen, 7));  // -1
```

Die **Vorbedingung** ist unverhandelbar: Das Array muss sortiert sein. Auf unsortierten Daten liefert die binäre Suche kein falsches Ergebnis mit Fehlermeldung, sondern schlicht Unsinn – mal einen Treffer, mal `-1`, ohne dass irgendetwas auffällt. Wer das Array nicht selbst sortiert hat, sollte es vor der Suche mit `Array.Sort` tun oder eine lineare Suche verwenden.
{: .notice--warning}

.NET bringt die Implementierung fertig mit: `Array.BinarySearch(zahlen, 23)` liefert `5`, und `List<T>.BinarySearch` tut dasselbe für Listen. Beide funktionieren für jeden Typ, der [`IComparable<T>`](/modules/icomparable_sortieren/icomparable_sortieren.md) implementiert. Fehlt das Element, geben sie eine **negative Zahl** zurück, die verschlüsselt, wo es eingefügt werden müsste – dazu mehr in den [Aufgaben](/modules/aufgaben_collections_linq/aufgaben_collections_linq.md).

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

`Array.Sort` und `List<T>.Sort` verwenden stattdessen **Introsort**, eine Kombination aus Quicksort, Heapsort und Insertion Sort. Alle drei folgen der Idee „Teilen und Herrschen“ wie die binäre Suche und kommen mit etwa **n · log₂ n** Vergleichen aus – für eine Million Elemente rund 20 Millionen statt 500 Milliarden. Für die Praxis heißt das: Eigene Sortieralgorithmen schreibt man zum Lernen, nicht für den Produktivcode.

## Aufwand grob abschätzen: die O-Notation

Damit man solche Vergleiche nicht jedes Mal ausrechnen muss, beschreibt man das Wachstum eines Algorithmus mit der **O-Notation**. Sie ignoriert Konstanten und kleine Terme und behält nur, wie der Aufwand mit n wächst:

| Notation | Sprich | Beispiel |
| :--- | :--- | :--- |
| O(1) | konstant | Array-Zugriff `a[i]`, `Dictionary`-Lookup |
| O(log n) | logarithmisch | binäre Suche |
| O(n) | linear | lineare Suche, `foreach` |
| O(n log n) | linearithmisch | `Array.Sort` |
| O(n²) | quadratisch | Selection Sort, Bubble Sort, verschachtelte Schleife über dieselbe Liste |

Das ist eine grobe Faustregel, keine Stoppuhr – für zehn Elemente ist ein O(n²)-Algorithmus oft schneller als ein aufwendiger O(n log n)-Algorithmus. Aber sobald n groß wird, entscheidet allein die Zeile in dieser Tabelle. Wer eine `foreach`-Schleife in eine andere `foreach`-Schleife über dieselbe Liste schreibt, hat O(n²) gebaut – und sollte kurz innehalten, ob ein `Dictionary` oder eine vorherige Sortierung das nicht besser lösen.
{: .notice--primary}

Übung: Ergänze `BinaereSuche` um einen Zähler, der die Anzahl der Vergleiche mitzählt, und teste mit einem sortierten Array aus 1.000 Zufallszahlen. Vergleiche mit der Anzahl der Vergleiche einer linearen Suche nach demselben Element. Was passiert mit beiden Zahlen, wenn das gesuchte Element nicht enthalten ist?
{: .notice--info}

## Weitere Quellen

- [Array.BinarySearch – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.array.binarysearch)
- [Array.Sort – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.array.sort) (Abschnitt „Hinweise“ beschreibt Introsort)
- [List<T>.BinarySearch – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.list-1.binarysearch)
