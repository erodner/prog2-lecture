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

Ein Typparameter `T` ohne weitere Angaben ist ein Versprechen an alle: „Diese Klasse funktioniert mit jedem Typ.“ Der Preis für dieses Versprechen ist hoch – innerhalb der Klasse darf man mit `T` nur das tun, was **jeder** Typ kann, und das ist fast nichts. Sobald ein Algorithmus Elemente vergleichen, ein neues Objekt erzeugen oder eine bestimmte Property lesen möchte, muss man das Versprechen einschränken: „Diese Klasse funktioniert mit jedem Typ, der aufgesammelt werden kann.“ Genau das sind **Constraints** – Bedingungen an den Typparameter, die der Compiler bei jeder Verwendung prüft und die im Gegenzug innerhalb der Klasse mehr Möglichkeiten freischalten. Wie ein Etikett am Rucksack des Helden: „Nur Aufsammelbares – dafür weiß jedes Ding hier drin, wie es heißt.“

## Das Problem: `T` kann fast nichts

Im Modul [Generische Typen](/modules/generische_typen/generische_typen.md) haben wir das `Inventar<T>` gebaut. Seine `ToString`-Methode soll die Namen aller getragenen Dinge auflisten – und genau daran scheitert eine Fassung ohne Constraint:

```csharp
public class Inventar<T>
{
    private readonly List<T> inhalt = new();

    public override string ToString()
    {
        return string.Join(", ", inhalt.Select(d => d.Name));   // Compilerfehler CS1061:
    }                                                           // "T" enthält keine Definition für "Name"
}
```

Der Compiler weiß nur, dass `T` irgendein Typ ist. Ob dieser Typ eine Property `Name` hat, kann er nicht wissen – bei `Schluessel` ja, bei `int` nein. Also lehnt er den Zugriff ab. Dasselbe passiert mit `<` und `>`: Operatoren lassen sich in C# nicht als Constraint fordern, weshalb Vergleiche über das Interface `IComparable<T>` laufen.

## Die Lösung: `where T : ISammelbar`

Ein Constraint steht hinter der Klassendeklaration und beginnt mit `where`. Wir fordern, dass jeder Typ, der für `T` eingesetzt wird, das Interface `ISammelbar` aus [Vorlesung 02](/lectures/02/02.md) implementiert – und damit eine Property `Name` besitzt:

```csharp
public interface ISammelbar
{
    string Name { get; }
    string Aufheben(Spieler spieler);
}

public class Inventar<T> : IEnumerable<T> where T : ISammelbar
{
    private readonly List<T> inhalt = new();

    public int Anzahl => inhalt.Count;

    public void Hinzufuegen(T ding)
    {
        inhalt.Add(ding);
    }

    public override string ToString()
    {
        return inhalt.Count == 0 ? "leer" : string.Join(", ", inhalt.Select(d => d.Name));
    }
}
```

Innerhalb der Klasse darf `d.Name` jetzt stehen, weil der Compiler weiß: Was auch immer `T` sein wird, es ist `ISammelbar`. Von außen prüft er den Constraint an jeder Stelle, an der `T` mit einem konkreten Typ belegt wird:

```csharp
Inventar<Gegenstand> rucksack = new Inventar<Gegenstand>();   // Gegenstand : ISammelbar – in Ordnung
rucksack.Hinzufuegen(new Schluessel(new Position(3, 3)));

Inventar<Wand> unsinn = new Inventar<Wand>();
// Compilerfehler CS0311: Der Typ "Wand" kann nicht als Typparameter "T" verwendet werden.
// Es gibt keine implizite Verweiskonvertierung von "Wand" in "ISammelbar".
```

Der Fehler erscheint bereits beim Erzeugen des Objekts, nicht erst beim `Hinzufuegen`. Ein `Inventar<string>` ist aus demselben Grund unmöglich – Zeichenketten kann man nicht aufheben.

Ein Constraint schränkt ein **und** erweitert: Er schränkt ein, welche Typen erlaubt sind, und erweitert, was die Klasse mit `T` tun darf. Je mehr Constraints, desto mächtiger der Code innerhalb der Klasse – aber desto weniger Typen passen hinein.
{: .notice--primary}

## Ein zweiter Typparameter an der Methode

Die Tür im Kerker will wissen, ob der Held einen Schlüssel dabeihat. Eine Methode `EnthaeltSchluessel()` wäre die schnelle Lösung – aber dann bräuchte das Inventar für jede Art von Gegenstand eine eigene Methode. Besser ist eine **generische Methode innerhalb der generischen Klasse**, die die gesuchte Art als eigenen Typparameter bekommt:

```csharp
public bool Enthaelt<TArt>() where TArt : T
{
    return inhalt.Any(d => d is TArt);
}

public bool Entfernen<TArt>() where TArt : T
{
    T? treffer = inhalt.FirstOrDefault(d => d is TArt);
    return treffer is not null && inhalt.Remove(treffer);
}
```

Hier steckt der interessanteste Constraint des ganzen Moduls: `where TArt : T` verlangt, dass der zweite Typparameter vom ersten abgeleitet ist. Für ein `Inventar<Gegenstand>` heißt das, dass `TArt` ein `Schluessel`, ein `Trank` oder ein `Schatz` sein darf – aber niemals eine `Wand`. Ohne diesen Constraint könnte man `rucksack.Enthaelt<Wand>()` schreiben; die Abfrage würde brav kompilieren und immer `false` liefern, weil in einem Inventar voller Gegenstände nie eine Wand liegt. Der Constraint macht aus einer sinnlosen Frage einen Compilerfehler. Die `Tuer` nutzt beide Methoden genau so:

```csharp
public string Interagieren(Spieler spieler)
{
    if (IstOffen)
    {
        return "Die Tür ist schon offen.";
    }
    if (!spieler.Inventar.Enthaelt<Schluessel>())
    {
        return "Die Tür ist verschlossen. Du brauchst einen Schlüssel.";
    }
    spieler.Inventar.Entfernen<Schluessel>();
    Aufschliessen();
    return "Du schließt die Tür auf.";
}
```

Die Typinferenz hilft hier übrigens nicht: `TArt` kommt in keinem Parameter vor, sondern nur im Rumpf. Deshalb stehen die spitzen Klammern am Aufruf – `Enthaelt<Schluessel>()` liest sich dafür fast wie ein Satz. Die Methoden `Any` und `FirstOrDefault` sind LINQ-Abfragen, die wir in der nächsten Vorlesung kennenlernen; hier tun sie nichts anderes, als die interne Liste einmal zu durchlaufen.

## Alle Constraint-Arten im Überblick

`ISammelbar` ist nur eine Möglichkeit. C# kennt eine Reihe von Bedingungen, die sich an einen Typparameter stellen lassen:

| Constraint | Bedeutung | Erlaubt innerhalb der Klasse |
| :--- | :--- | :--- |
| `where T : struct` | `T` muss ein Werttyp sein (`int`, `double`, eigene `struct`s wie `Position`); Nullable-Typen sind ausgeschlossen | `T` ist nie `null`, `default(T)` ist der Nullwert des Typs |
| `where T : class` | `T` muss ein Referenztyp sein (Klasse, Interface, Delegat, Array) | Vergleich mit `null`, Referenzgleichheit |
| `where T : new()` | `T` muss einen öffentlichen parameterlosen Konstruktor haben; bei mehreren Constraints immer als letztes | `new T()` |
| `where T : Spielobjekt` | `T` muss `Spielobjekt` sein oder davon erben | Zugriff auf alle Mitglieder von `Spielobjekt`, z. B. `Position` und `Symbol` |
| `where T : ISammelbar` | `T` muss das Interface implementieren | Aufruf der Interface-Mitglieder, z. B. `Name` |
| `where T : notnull` | `T` darf kein nullable Typ sein (weder `string?` noch `int?`) | Verwendung als Dictionary-Schlüssel |
| `where TArt : T` | `TArt` muss ein anderer Typparameter `T` oder davon abgeleitet sein | Zuweisung von `TArt` an `T`, Typprüfung mit `is` |

Mehrere Bedingungen lassen sich mit Kommas kombinieren, und für jeden Typparameter gibt es eine eigene `where`-Klausel. Eine Methode, die eine ganze Reihe gleichartiger Gegner erzeugt und auf dem Spielfeld verteilt, braucht zum Beispiel zwei Bedingungen gleichzeitig:

```csharp
static List<T> Erzeugen<T>(int anzahl) where T : Spielobjekt, new()
{
    var liste = new List<T>();
    for (int i = 0; i < anzahl; i++)
    {
        T objekt = new T();
        Console.WriteLine(objekt.Symbol);   // erlaubt, weil T : Spielobjekt
        liste.Add(objekt);
    }
    return liste;
}
```

Der Constraint `Spielobjekt` erlaubt den Zugriff auf `Symbol`, der Constraint `new()` erlaubt `new T()`. Die Reihenfolge ist vorgeschrieben: Basisklasse zuerst, dann Interfaces, `new()` immer zuletzt.

`new()` verlangt einen **parameterlosen** Konstruktor. Alle Klassen unseres Spiels bekommen im Konstruktor mindestens eine `Position` und wären für `Erzeugen<T>` deshalb nicht zugelassen – ein Constraint kann keinen Konstruktor mit Argumenten fordern. In der Praxis ist das kein Verlust: Eine Fabrikmethode, die `new Wache(position)` aufruft, ist ohnehin ehrlicher.
{: .notice--warning}

## Vergleichen mit `where T : IComparable<T>`

Ein zweites häufiges Muster ist der Vergleich. Für eine Bestenliste möchten wir wissen, welcher Wert der größte ist – und zwar unabhängig davon, ob es um Punkte, Rundenzahlen oder Spielernamen geht:

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

`Groesstes` wirft bei einer leeren Liste eine Exception – eine bewusste Entscheidung, denn „das größte von nichts“ gibt es nicht. Manchmal möchte man an dieser Stelle aber lieber einen „leeren“ Wert von `T` zurückgeben, ohne zu wissen, ob `T` ein Wert- oder ein Referenztyp ist. `null` funktioniert nicht, denn ein `int` kann nicht `null` sein. Dafür gibt es `default(T)`: Es liefert `0` für Zahlen, `false` für `bool`, `null` für Referenztypen und eine Struktur mit lauter Nullwerten für eigene `struct`s – für `Position` also `(0, 0)`:

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
Console.WriteLine(ErstesOderStandard(new Position[0]));          // (0, 0)
Gegner? ersterGegner = ErstesOderStandard(new Gegner[0]);        // null
```

Mit eingeschaltetem Nullable-Kontext warnt der Compiler, dass `default(T)` bei Referenztypen `null` ist – deshalb steht `Gegner?` als Variablentyp. Meist genügt die Kurzform `default` ohne Klammern, weil der Compiler den Typ aus dem Kontext kennt.

Übung: Schreibe eine generische Methode `Kleinstes<T>(IEnumerable<T> elemente) where T : IComparable<T>`. Was soll bei einer leeren Folge passieren – `default(T)` zurückgeben oder eine Exception werfen? Begründe deine Entscheidung und überlege, was `default` für `Position` bedeutet, wenn `(0, 0)` ein völlig normales Feld auf der Karte ist.
{: .notice--info}

## Generische Interfaces als Brücke

`IComparable<T>` ist selbst ein generisches Interface: Der Typparameter legt fest, **womit** ein Objekt verglichen werden kann. Ein `Gegner` würde `IComparable<Gegner>` implementieren und bekäme dadurch eine typsichere Methode `CompareTo(Gegner other)` – ohne Casts, wie sie das alte nicht-generische `IComparable` mit `CompareTo(object other)` nötig machte. Nach demselben Muster sind die wichtigsten Interfaces von .NET aufgebaut:

| Interface | Verspricht | Wird gebraucht von |
| :--- | :--- | :--- |
| `IComparable<T>` | Elemente lassen sich ordnen (`CompareTo`) | `List<T>.Sort()`, `Array.Sort`, `SortedSet<T>` |
| `IEquatable<T>` | Elemente lassen sich typsicher auf Gleichheit prüfen (`Equals(T)`) | `List<T>.Contains`, `Dictionary<TKey, TValue>` |
| `IEnumerable<T>` | Elemente lassen sich der Reihe nach durchlaufen | `foreach`, LINQ – und unser `Inventar<T>` |

Damit schließt sich der Kreis: Die `List<Gegner>` im `Spielfeld` ist eine generische Klasse, `foreach` über das Inventar funktioniert, weil `Inventar<Gegenstand>` das Interface `IEnumerable<Gegenstand>` implementiert, und `Position` taugt als Dictionary-Schlüssel, weil es als `record struct` `Equals` und `GetHashCode` mitbringt. Wie Vergleichen, Gleichheit und Sortieren im Detail zusammenspielen, ist Thema der nächsten Vorlesung.

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`).

## Weitere Quellen

- [Einschränkungen für Typparameter – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)
- [IComparable&lt;T&gt;-Schnittstelle – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.icomparable-1)
- [default-Wert-Ausdrücke – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/default)
- [Generische Schnittstellen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/generics/generic-interfaces)
