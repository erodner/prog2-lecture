---
title: "Vergleichen, default und generische Interfaces"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

**➕ Erweitertes Wissen.** Wie man einen Typparameter mit `where` einschränkt, kennst du aus [Generische Constraints](/modules/generische_constraints/generische_constraints.md). Dieses Modul zeigt drei Anwendungen, die dir in Bibliotheken oft begegnen. Die folgenden Module setzen es nicht voraus.
{: .notice--success}

## Vergleichen mit `where T : IComparable<T>`

Ein häufiges Muster ist der Vergleich. Für eine Bestenliste möchten wir wissen, welcher Wert der größte ist – und zwar unabhängig davon, ob es um Punkte, Rundenzahlen oder Spielernamen geht:

```csharp
static T Groesstes<T>(IReadOnlyList<T> elemente) where T : IComparable<T>
{
    if (elemente.Count == 0)
    {
        throw new InvalidOperationException("Die Liste ist leer.");
    }

    T groesstes = elemente[0];
    foreach (T element in elemente)
    {
        if (element.CompareTo(groesstes) > 0)
        {
            groesstes = element;
        }
    }
    return groesstes;
}

Console.WriteLine(Groesstes(new[] { 120, 340, 90 }));            // 340
Console.WriteLine(Groesstes(new[] { "Held", "Wache", "Ada" }));  // Wache
```

`int` und `string` implementieren `IComparable<int>` bzw. `IComparable<string>`, daher akzeptiert der Compiler beide. Mit `Groesstes(feld.Gegner)` sähe es anders aus: `Gegner` implementiert `IComparable<Gegner>` nicht, und der Compiler lehnt den Aufruf mit CS0311 ab – zu Recht, denn „der größte Gegner“ ist ohne weitere Angabe keine sinnvolle Frage. Wie man Objekte vergleichbar macht und wie man mehrere Ordnungen nebeneinander definiert, ist Thema des Moduls [`IComparable<T>` und Sortieren](/modules/icomparable_sortieren/icomparable_sortieren.md) in der nächsten Vorlesung.

## `default(T)` – der Nullwert eines unbekannten Typs

`Groesstes` wirft bei einer leeren Liste eine Exception – eine bewusste Entscheidung, denn „das größte von nichts“ gibt es nicht. Manchmal möchte man an dieser Stelle aber lieber einen „leeren“ Wert von `T` zurückgeben, ohne zu wissen, ob `T` ein Wert- oder ein Referenztyp ist. `null` funktioniert nicht, denn ein `int` kann nicht `null` sein. Dafür gibt es `default(T)`: Es liefert `0` für Zahlen, `false` für `bool`, `null` für Referenztypen und eine Struktur mit lauter Nullwerten für eigene `struct`s – für `Koordinate` also `(0, 0)`:

```csharp
static T ErstesOderStandard<T>(T[] feld)
{
    if (feld.Length == 0)
    {
        return default(T);
    }
    return feld[0];
}

Console.WriteLine(ErstesOderStandard(new int[0]));               // 0
Console.WriteLine(ErstesOderStandard(new Koordinate[0]));          // (0, 0)
Gegner? ersterGegner = ErstesOderStandard(new Gegner[0]);        // null
```

Mit eingeschaltetem Nullable-Kontext warnt der Compiler, dass `default(T)` bei Referenztypen `null` ist – deshalb steht `Gegner?` als Variablentyp. Meist genügt die Kurzform `default` ohne Klammern, weil der Compiler den Typ aus dem Kontext kennt.

Übung: Schreibe eine generische Methode `Kleinstes<T>(IEnumerable<T> elemente) where T : IComparable<T>`. Was soll bei einer leeren Folge passieren – `default(T)` zurückgeben oder eine Exception werfen? Begründe deine Entscheidung und überlege, was `default` für `Koordinate` bedeutet, wenn `(0, 0)` ein völlig normales Feld auf der Karte ist.
{: .notice--info}

## Generische Interfaces als Brücke

`IComparable<T>` ist selbst ein generisches Interface: Der Typparameter legt fest, **womit** ein Objekt verglichen werden kann. Ein `Gegner` würde `IComparable<Gegner>` implementieren und bekäme dadurch eine typsichere Methode `CompareTo(Gegner other)` – ohne Casts, wie sie das alte nicht-generische `IComparable` mit `CompareTo(object other)` nötig machte. Nach demselben Muster sind die wichtigsten Interfaces von .NET aufgebaut:

| Interface | Verspricht | Wird gebraucht von |
| :--- | :--- | :--- |
| `IComparable<T>` | Elemente lassen sich ordnen (`CompareTo`) | `List<T>.Sort()`, `Array.Sort`, `SortedSet<T>` |
| `IEquatable<T>` | Elemente lassen sich typsicher auf Gleichheit prüfen (`Equals(T)`) | `List<T>.Contains`, `Dictionary<TKey, TValue>` |
| `IEnumerable<T>` | Elemente lassen sich der Reihe nach durchlaufen | `foreach`, LINQ – und unser `Inventar<T>` |

Damit schließt sich der Kreis: Die `List<Gegner>` im `Spielfeld` ist eine generische Klasse, `foreach` über das Inventar funktioniert, weil `Inventar<Gegenstand>` das Interface `IEnumerable<Gegenstand>` implementiert, und `Koordinate` taugt als Dictionary-Schlüssel, weil es als `record struct` `Equals` und `GetHashCode` mitbringt. Wie Vergleichen, Gleichheit und Sortieren im Detail zusammenspielen, ist Thema der nächsten Vorlesung.

## Weitere Quellen

- [IComparable&lt;T&gt;-Schnittstelle – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.icomparable-1)
- [default-Wert-Ausdrücke – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/default)
- [Generische Schnittstellen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/generics/generic-interfaces)
