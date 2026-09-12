---
title: "IComparable<T> – Objekte sortierbar machen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

`Array.Sort` sortiert ein `int[]` und ein `string[]` ohne Rückfragen – die Reihenfolge von Zahlen und Wörtern ist klar. Aber wie soll ein Sortieralgorithmus eine Liste von `Temperatur`-Objekten oder von `Gegner`-Objekten ordnen? Ist eine Wache „kleiner“ als ein Verfolger? Der Algorithmus selbst weiß es nicht, und er muss es auch nicht wissen: Er braucht nur eine einzige Auskunft, nämlich für zwei beliebige Elemente, welches zuerst kommt. Genau diese Auskunft beschreibt das Interface `IComparable<T>`. Sobald ein Typ es implementiert, funktionieren `Sort`, `Min`, `Max` und alle sortierten Collections mit ihm – ein schönes Beispiel dafür, wie ein [Interface](/modules/interfaces_grundlagen/interfaces_grundlagen.md) einen Algorithmus von den Daten entkoppelt.

## Die Methode `CompareTo`

`IComparable<T>` enthält genau eine Methode: `int CompareTo(T? other)`. Der Rückgabewert ist keine Zahl im Sinne von „um wie viel größer“, sondern ein Signal mit drei Bedeutungen:

- **kleiner als 0:** Das aktuelle Objekt kommt in der Sortierreihenfolge **vor** `other`.
- **0:** Beide stehen an derselben Position – sie gelten als gleich groß.
- **größer als 0:** Das aktuelle Objekt kommt **nach** `other`.

Die eingebauten Typen implementieren das Interface bereits, und wir können `CompareTo` direkt aufrufen:

```csharp
Console.WriteLine(3.CompareTo(7));               // -1
Console.WriteLine("Wache".CompareTo("Ausgang")); // 1
Console.WriteLine(2.5.CompareTo(2.5));           // 0
```

Für `int` liefert `CompareTo` tatsächlich immer -1, 0 oder 1 – darauf verlassen darf man sich aber nicht. Der Vertrag sagt nur „negativ, null oder positiv“, und genau so sollte man das Ergebnis auch abfragen: mit `< 0` statt `== -1`.

## Eine eigene Klasse vergleichbar machen

Bleiben wir für den Einstieg bei einem Beispiel außerhalb des Spiels, bei dem die Ordnung völlig unstrittig ist. Eine Klasse `Temperatur` kapselt einen Wert in Grad Celsius; damit sie sortierbar wird, implementiert sie `IComparable<Temperatur>` und delegiert den Vergleich an den eingebauten `double`-Vergleich:

```csharp
public class Temperatur : IComparable<Temperatur>
{
    public double Grad { get; }

    public Temperatur(double grad)
    {
        Grad = grad;
    }

    public int CompareTo(Temperatur? other)
    {
        if (other is null)
        {
            return 1; // null sortiert vor jedem echten Wert
        }
        return Grad.CompareTo(other.Grad);
    }

    public override string ToString() => $"{Grad} °C";
}
```

Der `null`-Fall ist Konvention: Ein echtes Objekt gilt als größer als `null`, damit `null`-Einträge beim Sortieren nach vorn wandern statt eine Exception auszulösen. Der eigentliche Vergleich ist eine Zeile – wir müssen die Ordnung nicht selbst erfinden, sondern nur festlegen, **welches Feld** sie bestimmt. Damit lässt sich ein Array von Temperaturen genauso sortieren wie ein `int[]`:

```csharp
Temperatur[] messungen =
{
    new Temperatur(21.5), new Temperatur(-3.0), new Temperatur(30.2)
};
Array.Sort(messungen);
Console.WriteLine(string.Join(", ", messungen)); // nutzt ToString jedes Elements
// -3 °C, 21.5 °C, 30.2 °C

List<Temperatur> liste = new List<Temperatur>(messungen);
liste.Sort();
Console.WriteLine(liste[^1]); // 30.2 °C
```

`Array.Sort` und `List<T>.Sort()` rufen intern nur `CompareTo` auf – welcher Algorithmus dahintersteckt, sehen wir im Modul zu den [Such- und Sortieralgorithmen](/modules/such_und_sortieralgorithmen/such_und_sortieralgorithmen.md). Wichtig ist hier: Fehlt die Implementierung, kompiliert der Aufruf zwar, zur Laufzeit gibt es aber eine `InvalidOperationException` mit dem Hinweis, dass kein Vergleich möglich ist.

Wer `CompareTo` überschreibt, sollte auch `Equals` und `GetHashCode` konsistent halten: Wenn `a.CompareTo(b) == 0` gilt, sollte in der Regel auch `a.Equals(b)` `true` sein. Sonst verhalten sich ein `SortedSet` (nutzt `CompareTo`) und ein `HashSet` (nutzt `Equals`/`GetHashCode`) für dieselben Objekte unterschiedlich – ein schwer zu findender Fehler. Die Details zu `Equals` stehen im Modul [Hashcodes und Equals](/modules/hashcodes_equals/hashcodes_equals.md).
{: .notice--warning}

## Sortierte Collections brauchen Vergleichbarkeit

`Sort()` ist nur ein Nutzer von `IComparable<T>`. Die Collections `SortedList<K, V>`, `SortedDictionary<K, V>` und `SortedSet<T>` halten ihre Elemente **dauerhaft** in sortierter Reihenfolge – und müssen dafür bei jedem Einfügen vergleichen. Als Schlüssel beziehungsweise Element ist deshalb nur ein Typ brauchbar, der `IComparable<T>` implementiert:

```csharp
SortedSet<Temperatur> sortiert = new SortedSet<Temperatur>();
sortiert.Add(new Temperatur(30.2));
sortiert.Add(new Temperatur(-3.0));
sortiert.Add(new Temperatur(21.5));

Console.WriteLine(sortiert.Min); // -3 °C
Console.WriteLine(sortiert.Max); // 30.2 °C
```

Die Reihenfolge entsteht hier nicht durch ein nachträgliches `Sort()`, sondern beim Einfügen. Das kostet pro `Add` etwas Zeit, dafür ist die Menge jederzeit sortiert. Ein Vorgeschmack auf die Abwägung, die im Modul [Collections im Überblick](/modules/collections_ueberblick/collections_ueberblick.md) systematisch wird.

## Warum `Gegner` kein `IComparable<Gegner>` ist

Jetzt zum Spiel. Die Anzeige soll die Gegner nach ihrer Entfernung zum Helden auflisten – der gefährlichste zuerst. Die naheliegende Idee wäre, `Gegner` einfach `IComparable<Gegner>` implementieren zu lassen. Dabei fällt sofort auf, dass das nicht funktionieren kann:

```csharp
public int CompareTo(Gegner? other)
{
    // Entfernung wozu? Der Gegner kennt den Spieler nicht.
}
```

Ein Gegner hat keine *natürliche* Ordnung. „Näher am Helden“ ist keine Eigenschaft des Gegners, sondern eine Beziehung zwischen zwei Objekten – und sie ändert sich mit jeder Runde. Genau deshalb existiert ein zweites Interface: `IComparer<T>`. Der Vergleich wandert aus der Klasse heraus in ein eigenes, kleines Vergleichsobjekt, das die fehlende Information als Feld mitbringt.

```csharp
public class NachEntfernungComparer : IComparer<Gegner>
{
    private readonly Position bezug;

    public NachEntfernungComparer(Position bezug)
    {
        this.bezug = bezug;
    }

    public int Compare(Gegner? x, Gegner? y)
    {
        if (x is null || y is null)
        {
            return (x is null ? 0 : 1) - (y is null ? 0 : 1);
        }
        return x.Position.Entfernung(bezug).CompareTo(y.Position.Entfernung(bezug));
    }
}
```

`Position.Entfernung` liefert die Manhattan-Entfernung, also die Anzahl der Schritte ohne Diagonalen – ein `int`, dessen `CompareTo` wir direkt weiterreichen. Benutzt wird der Comparer beim Sortieren einer Liste:

```csharp
Spielfeld feld = LevelParser.Parsen(new EingebauteLevelQuelle().Laden("Kerker"));

List<Gegner> nachNaehe = new List<Gegner>(feld.Gegner);
nachNaehe.Sort(new NachEntfernungComparer(feld.Spieler.Position));

foreach (Gegner g in nachNaehe)
{
    Console.WriteLine($"{g.Name} bei {g.Position}: {g.Position.Entfernung(feld.Spieler.Position)} Schritte");
}
// Wache bei (13, 2): 13 Schritte
// Verfolger bei (11, 5): 14 Schritte
```

Die erste Zeile ist kein Zufall: `feld.Gegner` hat den Typ `IReadOnlyList<Gegner>` und besitzt deshalb gar keine `Sort`-Methode – das Spielfeld gibt seine interne Liste bewusst nur lesend nach außen. Wir kopieren die Referenzen in eine eigene `List<Gegner>` und sortieren diese. Die Gegner selbst werden dabei nicht verändert, nur unsere Sicht auf sie.

Der Unterschied in einem Satz: `IComparable<T>` sagt „ich weiß, wie ich mich mit anderen vergleiche“, `IComparer<T>` sagt „ich weiß, wie ich zwei andere vergleiche“. Das erste gehört in die Klasse und taugt nur für **eine** natürliche Ordnung; das zweite ist austauschbar und darf zusätzlichen Kontext mitbringen.
{: .notice--primary}

Für einen einzigen, schnellen Sonderfall wirkt eine ganze Comparer-Klasse schwerfällig. `List<T>.Sort` akzeptiert deshalb auch ein `Comparison<T>` – das ist ein **Delegat**, im Kern eine Methode als Wert, die man direkt übergeben kann:

```csharp
static int NachSymbol(Gegner a, Gegner b) => a.Symbol.CompareTo(b.Symbol);

nachNaehe.Sort(NachSymbol);   // Methodenname statt Comparer-Objekt: erst alle V, dann alle W
```

Was Delegaten genau sind und wie man sie mit Lambda-Ausdrücken in einer Zeile schreibt – `nachNaehe.Sort((a, b) => a.Name.CompareTo(b.Name))` –, ist das Thema der nächsten Vorlesung ([Delegaten](/modules/delegaten/delegaten.md)).

Übung: Der `Schatz` hat eine Property `Wert`, der `Trank` eine Property `Heilung`. Implementiere `IComparable<Schatz>` in `Schatz`, sodass Schätze nach ihrem Wert sortiert werden, und sortiere damit eine Liste von Schätzen mit `Sort()`. Schreibe anschließend einen `IComparer<Gegenstand>`, der alle Gegenstände alphabetisch nach `Name` ordnet, und überlege: Welche der beiden Ordnungen gehört in die Klasse, welche daneben – und warum kann der Comparer etwas, was `CompareTo` nicht kann?
{: .notice--info}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

## Weitere Quellen

- [IComparable<T> – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.icomparable-1)
- [IComparer<T> – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.icomparer-1)
- [Array.Sort – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.array.sort)
- [SortedSet<T> – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.sortedset-1)
