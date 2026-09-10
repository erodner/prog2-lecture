---
title: "Generische Constraints"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ein Typparameter `T` ohne weitere Angaben ist ein Versprechen an alle: „Diese Klasse funktioniert mit jedem Typ.“ Der Preis für dieses Versprechen ist hoch – innerhalb der Klasse darf man mit `T` nur das tun, was **jeder** Typ kann, und das ist fast nichts. Sobald ein Algorithmus Elemente vergleichen, ein neues Objekt erzeugen oder eine bestimmte Methode aufrufen möchte, muss man das Versprechen einschränken: „Diese Klasse funktioniert mit jedem Typ, der vergleichbar ist.“ Genau das sind **Constraints** – Bedingungen an den Typparameter, die der Compiler bei jeder Verwendung prüft und die im Gegenzug innerhalb der Klasse mehr Möglichkeiten freischalten. Wie ein Container, auf dem steht: „Nur für Glas – aber dafür dürfen Sie hier auch Scherben einwerfen.“

## Das Problem: `T` kann fast nichts

Im Modul [Generische Typen](/modules/generische_typen/generische_typen.md) haben wir den `Muellcontainer<T>` gebaut. Nun soll ein Container seinen Inhalt automatisch sortieren – die kleinsten Elemente unten, die größten oben. Dazu müssen wir beim Einfügen zwei Elemente vergleichen:

```csharp
class SortierterMuellcontainer<T>
{
    private T[] inhalt;
    private int anzahl;

    public void Push(T element)
    {
        if (inhalt[anzahl - 1].CompareTo(element) > 0)   // Compilerfehler CS1061:
        {                                                 // "T" enthält keine Definition für "CompareTo"
            // ...
        }
    }
}
```

Der Compiler weiß nur, dass `T` irgendein Typ ist. Ob dieser Typ eine Methode `CompareTo` hat, kann er nicht wissen – bei `int` und `string` ja, bei `Figur` nein. Also lehnt er den Aufruf ab. Dasselbe passiert mit `<` und `>`: Operatoren lassen sich in C# nicht als Constraint fordern, weshalb Vergleiche über das Interface `IComparable<T>` laufen.

## Die Lösung: `where T : IComparable<T>`

Ein Constraint steht hinter der Klassendeklaration und beginnt mit `where`. Wir fordern, dass jeder Typ, der für `T` eingesetzt wird, das generische Interface `IComparable<T>` implementiert – also eine Methode `int CompareTo(T other)` besitzt. Damit darf die Klasse diese Methode aufrufen:

```csharp
class SortierterMuellcontainer<T> where T : IComparable<T>
{
    private T[] inhalt;
    private int anzahl;

    public SortierterMuellcontainer(int kapazitaet)
    {
        inhalt = new T[kapazitaet];
    }

    public void Push(T element)
    {
        if (anzahl == inhalt.Length)
        {
            throw new InvalidOperationException("Der Container ist voll.");
        }

        // Von oben nach unten: alle größeren Elemente eine Position nach oben schieben
        int position = anzahl;
        while (position > 0 && inhalt[position - 1].CompareTo(element) > 0)
        {
            inhalt[position] = inhalt[position - 1];
            position--;
        }
        inhalt[position] = element;
        anzahl++;
    }

    public T Pop()
    {
        if (anzahl == 0)
        {
            throw new InvalidOperationException("Der Container ist leer.");
        }
        anzahl--;
        return inhalt[anzahl];
    }
}
```

Das Einfügen funktioniert wie beim Sortieren von Spielkarten in der Hand: Man schiebt alle größeren Karten eine Position weiter und legt die neue an die frei gewordene Stelle. Das ist im Kern der Einfügeschritt von *Insertion Sort*, den wir im Modul [Such- und Sortieralgorithmen](/modules/such_und_sortieralgorithmen/such_und_sortieralgorithmen.md) noch genauer betrachten. `Pop` liefert dann immer das größte Element:

```csharp
var zahlen = new SortierterMuellcontainer<int>(10);
zahlen.Push(7);
zahlen.Push(2);
zahlen.Push(9);
zahlen.Push(4);
Console.WriteLine(zahlen.Pop()); // 9
Console.WriteLine(zahlen.Pop()); // 7

var woerter = new SortierterMuellcontainer<string>(10);
woerter.Push("Papier");
woerter.Push("Glas");
Console.WriteLine(woerter.Pop()); // Papier
```

`int` und `string` implementieren `IComparable<int>` bzw. `IComparable<string>`, daher akzeptiert der Compiler beide. Mit `Figur` sieht das anders aus:

```csharp
var figuren = new SortierterMuellcontainer<Figur>(5);
// Compilerfehler CS0311: Der Typ "Figur" kann nicht als Typparameter "T" verwendet werden.
// Es gibt keine implizite Verweiskonvertierung von "Figur" in "IComparable<Figur>".
```

Der Fehler erscheint bereits beim Erzeugen des Objekts, nicht erst beim `Push`. Der Compiler prüft den Constraint an jeder Stelle, an der `T` mit einem konkreten Typ belegt wird. Wollten wir Figuren sortieren, müssten wir `IComparable<Figur>` in `Figur` implementieren – wie das geht, sehen wir im Modul [IComparable und Sortieren](/modules/icomparable_sortieren/icomparable_sortieren.md).

Ein Constraint schränkt ein **und** erweitert: Er schränkt ein, welche Typen erlaubt sind, und erweitert, was die Klasse mit `T` tun darf. Je mehr Constraints, desto mächtiger der Code innerhalb der Klasse – aber desto weniger Typen passen hinein.
{: .notice--primary}

## Alle Constraint-Arten im Überblick

`IComparable<T>` ist nur eine Möglichkeit. C# kennt eine Reihe von Bedingungen, die sich an einen Typparameter stellen lassen:

| Constraint | Bedeutung | Erlaubt innerhalb der Klasse |
| :--- | :--- | :--- |
| `where T : struct` | `T` muss ein Werttyp sein (`int`, `double`, eigene `struct`s); Nullable-Typen sind ausgeschlossen | `T` ist nie `null`, `default(T)` ist der Nullwert des Typs |
| `where T : class` | `T` muss ein Referenztyp sein (Klasse, Interface, Delegat, Array) | Vergleich mit `null`, Referenzgleichheit |
| `where T : new()` | `T` muss einen öffentlichen parameterlosen Konstruktor haben; bei mehreren Constraints immer als letztes | `new T()` |
| `where T : Figur` | `T` muss `Figur` sein oder davon erben | Zugriff auf alle Mitglieder von `Figur` |
| `where T : IComparable<T>` | `T` muss das Interface implementieren | Aufruf der Interface-Methoden |
| `where T : notnull` | `T` darf kein nullable Typ sein (weder `string?` noch `int?`) | Verwendung als Dictionary-Schlüssel |
| `where T : U` | `T` muss ein anderer Typparameter `U` oder davon abgeleitet sein | Zuweisung von `T` an `U` |

Mehrere Bedingungen lassen sich mit Kommas kombinieren, und für jeden Typparameter gibt es eine eigene `where`-Klausel. Eine Methode, die eine Liste beliebiger Figuren anlegt und mit frisch erzeugten Objekten füllt, braucht zwei Bedingungen gleichzeitig:

```csharp
static List<T> Erzeugen<T>(int anzahl) where T : Figur, new()
{
    var liste = new List<T>();
    for (int i = 0; i < anzahl; i++)
    {
        T figur = new T();
        figur.Name = $"Figur {i}";   // erlaubt, weil T : Figur
        liste.Add(figur);
    }
    return liste;
}
```

Der Constraint `Figur` erlaubt den Zugriff auf `Name`, der Constraint `new()` erlaubt `new T()`. Die Reihenfolge ist vorgeschrieben: Basisklasse zuerst, dann Interfaces, `new()` immer zuletzt.

`new()` verlangt einen **parameterlosen** Konstruktor. Die Klassen `Kreis` und `Rechteck` des Geometrieeditors haben nur Konstruktoren mit Parametern und wären für `Erzeugen<T>` deshalb nicht zugelassen – ein Constraint kann keinen Konstruktor mit Argumenten fordern.
{: .notice--warning}

## `default(T)` – der Nullwert eines unbekannten Typs

Manchmal braucht man in generischem Code einen „leeren“ Wert von `T`, ohne zu wissen, ob `T` ein Wert- oder ein Referenztyp ist. `null` funktioniert nicht, denn ein `int` kann nicht `null` sein. Dafür gibt es `default(T)`: Es liefert `0` für Zahlen, `false` für `bool`, `null` für Referenztypen und eine Struktur mit lauter Nullwerten für eigene `struct`s:

```csharp
static T ErstesOderStandard<T>(T[] feld)
{
    if (feld.Length == 0)
    {
        return default(T);
    }
    return feld[0];
}

int[] leer = { };
Console.WriteLine(ErstesOderStandard(leer));          // 0
Console.WriteLine(ErstesOderStandard(new[] { 5 }));   // 5
string? text = ErstesOderStandard(new string[0]);     // null
```

Mit eingeschaltetem Nullable-Kontext warnt der Compiler, dass `default(T)` bei Referenztypen `null` ist – deshalb steht `string?` als Variablentyp. Meist genügt die Kurzform `default` ohne Klammern, weil der Compiler den Typ aus dem Kontext kennt.

Übung: Schreibe eine generische Methode `Groesstes<T>(T[] feld) where T : IComparable<T>`, die das größte Element eines Arrays liefert. Was soll bei einem leeren Array passieren – `default(T)` zurückgeben oder eine Exception werfen? Begründe deine Entscheidung.
{: .notice--info}

## Generische Interfaces als Brücke

`IComparable<T>` ist selbst ein generisches Interface: Der Typparameter legt fest, **womit** ein Objekt verglichen werden kann. Ein `Rechteck` würde `IComparable<Rechteck>` implementieren und bekommt dadurch eine typsichere Methode `CompareTo(Rechteck other)` – ohne Casts, wie sie das alte nicht-generische `IComparable` mit `CompareTo(object other)` nötig machte. Nach demselben Muster sind die wichtigsten Interfaces von .NET aufgebaut:

| Interface | Verspricht | Wird gebraucht von |
| :--- | :--- | :--- |
| `IComparable<T>` | Elemente lassen sich ordnen (`CompareTo`) | `List<T>.Sort()`, `Array.Sort`, `SortierterMuellcontainer<T>` |
| `IEquatable<T>` | Elemente lassen sich typsicher auf Gleichheit prüfen (`Equals(T)`) | `List<T>.Contains`, `Dictionary<TKey, TValue>` |
| `IEnumerable<T>` | Elemente lassen sich der Reihe nach durchlaufen | `foreach`, LINQ |

Damit schließt sich der Kreis: Die `List<Figur>` aus der `FigurenVerwaltung` ist eine generische Klasse, `foreach` darüber funktioniert, weil sie `IEnumerable<Figur>` implementiert, und sobald `Figur` zusätzlich `IComparable<Figur>` implementiert, könnten wir sie mit `Sort()` ordnen. Wie Vergleichen, Gleichheit und Sortieren im Detail zusammenspielen, ist Thema der nächsten Vorlesung.

## Weitere Quellen

- [Einschränkungen für Typparameter – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)
- [IComparable&lt;T&gt;-Schnittstelle – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.icomparable-1)
- [default-Wert-Ausdrücke – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/default)
- [Generische Schnittstellen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/generics/generic-interfaces)
