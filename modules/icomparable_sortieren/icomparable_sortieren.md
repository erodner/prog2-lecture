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

`Array.Sort` sortiert ein `int[]` und ein `string[]` ohne Rückfragen – die Reihenfolge von Zahlen und Wörtern ist klar. Aber wie soll ein Sortieralgorithmus eine Liste von `Temperatur`-Objekten oder `Figur`-Objekten ordnen? Ist ein Kreis „kleiner“ als ein Rechteck? Der Algorithmus selbst weiß es nicht, und er muss es auch nicht wissen: Er braucht nur eine einzige Auskunft, nämlich für zwei beliebige Elemente, welches zuerst kommt. Genau diese Auskunft beschreibt das Interface `IComparable<T>`. Sobald ein Typ es implementiert, funktionieren `Sort`, `Min`, `Max` und alle sortierten Collections mit ihm – ein schönes Beispiel dafür, wie ein [Interface](/modules/interfaces_grundlagen/interfaces_grundlagen.md) einen Algorithmus von den Daten entkoppelt.

## Die Methode `CompareTo`

`IComparable<T>` enthält genau eine Methode: `int CompareTo(T? other)`. Der Rückgabewert ist keine Zahl im Sinne von „um wie viel größer“, sondern ein Signal mit drei Bedeutungen:

- **kleiner als 0:** Das aktuelle Objekt kommt in der Sortierreihenfolge **vor** `other`.
- **0:** Beide stehen an derselben Position – sie gelten als gleich groß.
- **größer als 0:** Das aktuelle Objekt kommt **nach** `other`.

Die eingebauten Typen implementieren das Interface bereits, und wir können `CompareTo` direkt aufrufen:

```csharp
Console.WriteLine(3.CompareTo(7));           // -1
Console.WriteLine("Berlin".CompareTo("Aachen")); // 1
Console.WriteLine(2.5.CompareTo(2.5));       // 0
```

Für `int` liefert `CompareTo` tatsächlich immer -1, 0 oder 1 – darauf verlassen darf man sich aber nicht. Der Vertrag sagt nur „negativ, null oder positiv“, und genau so sollte man das Ergebnis auch abfragen: mit `< 0` statt `== -1`.

## Eine eigene Klasse vergleichbar machen

Nehmen wir eine Klasse `Temperatur`, die einen Wert in Grad Celsius kapselt. Damit sie sortierbar wird, implementiert sie `IComparable<Temperatur>` und delegiert den Vergleich an den eingebauten `double`-Vergleich:

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

## Alternative Ordnungen mit `IComparer<T>`

`IComparable<T>` legt **eine** natürliche Ordnung fest – für `Temperatur` aufsteigend nach `Grad`. Was aber, wenn wir dieselben Objekte einmal absteigend, einmal nach Betrag sortieren wollen? Die Klasse kann nicht mehrere `CompareTo`-Methoden haben. Für solche Fälle gibt es das zweite Interface `IComparer<T>`: Der Vergleich wandert aus der Klasse heraus in ein eigenes, kleines Vergleichsobjekt.

```csharp
public class NachBetragComparer : IComparer<Temperatur>
{
    public int Compare(Temperatur? x, Temperatur? y)
    {
        if (x is null || y is null)
        {
            return (x is null ? 0 : 1) - (y is null ? 0 : 1);
        }
        return Math.Abs(x.Grad).CompareTo(Math.Abs(y.Grad));
    }
}
```

Ein solcher Comparer wird dem Sortieraufruf oder der Collection mitgegeben, ohne dass `Temperatur` davon etwas wissen muss:

```csharp
Array.Sort(messungen, new NachBetragComparer());
// -3 °C, 21.5 °C, 30.2 °C  (nach Betrag: 3 < 21.5 < 30.2)

SortedSet<Temperatur> nachBetrag = new SortedSet<Temperatur>(new NachBetragComparer());
```

Der Unterschied in einem Satz: `IComparable<T>` sagt „ich weiß, wie ich mich mit anderen vergleiche“, `IComparer<T>` sagt „ich weiß, wie ich zwei andere vergleiche“. Das erste gehört in die Klasse, das zweite ist austauschbar.

Für einen einzigen, schnellen Sonderfall wirkt eine ganze Comparer-Klasse schwerfällig. `List<T>.Sort` akzeptiert deshalb auch ein `Comparison<T>` – das ist ein **Delegat**, im Kern eine Methode als Wert, die man direkt übergeben kann. Was Delegaten sind und wie man sie mit Lambda-Ausdrücken in einer Zeile schreibt, ist das Thema der nächsten Vorlesung ([Delegaten](/modules/delegaten/delegaten.md)).
{: .notice--primary}

Übung: Erweitere die Klasse `Figur` aus dem Geometrieeditor gedanklich um `IComparable<Figur>`, sodass Figuren nach ihrer Fläche sortiert werden. Welche Methode nutzt du für den Vergleich? Schreibe anschließend einen `IComparer<Figur>`, der alphabetisch nach `Name` sortiert, und überlege: Welche der beiden Ordnungen gehört in die Klasse, welche daneben – und warum?
{: .notice--info}

## Weitere Quellen

- [IComparable<T> – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.icomparable-1)
- [IComparer<T> – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.icomparer-1)
- [Array.Sort – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.array.sort)
- [SortedSet<T> – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.collections.generic.sortedset-1)
