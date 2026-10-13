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
        List<string> namen = new();
        foreach (T ding in inhalt)
        {
            namen.Add(ding.Name);   // Compilerfehler CS1061: "T" enthält keine Definition für "Name"
        }
        return string.Join(", ", namen);
    }
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
        if (inhalt.Count == 0) return "leer";
        List<string> namen = new();
        foreach (T ding in inhalt)
        {
            namen.Add(ding.Name);
        }
        return string.Join(", ", namen);
    }
}
```

Innerhalb der Klasse darf `ding.Name` jetzt stehen, weil der Compiler weiß: Was auch immer `T` sein wird, es ist `ISammelbar`. Von außen prüft er den Constraint an jeder Stelle, an der `T` mit einem konkreten Typ belegt wird:

```csharp
Inventar<Gegenstand> rucksack = new Inventar<Gegenstand>();   // Gegenstand : ISammelbar – in Ordnung
rucksack.Hinzufuegen(new Schluessel(new Koordinate(3, 3)));

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
    foreach (T ding in inhalt)
    {
        if (ding is TArt) return true;
    }
    return false;
}

public bool Entfernen<TArt>() where TArt : T
{
    foreach (T ding in inhalt)
    {
        if (ding is TArt) return inhalt.Remove(ding);
    }
    return false;
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

Die Typinferenz hilft hier übrigens nicht: `TArt` kommt in keinem Parameter vor, sondern nur im Rumpf. Deshalb stehen die spitzen Klammern am Aufruf – `Enthaelt<Schluessel>()` liest sich dafür fast wie ein Satz.

## Alle Constraint-Arten im Überblick

`ISammelbar` ist nur eine Möglichkeit. C# kennt eine Reihe von Bedingungen, die sich an einen Typparameter stellen lassen:

| Constraint | Bedeutung | Erlaubt innerhalb der Klasse |
| :--- | :--- | :--- |
| `where T : struct` | `T` muss ein Werttyp sein (`int`, `double`, eigene `struct`s wie `Koordinate`); Nullable-Typen sind ausgeschlossen | `T` ist nie `null`, `default(T)` ist der Nullwert des Typs |
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

Wie man mit `where T : IComparable<T>` Werte vergleicht, was `default(T)` liefert und welche generischen Interfaces .NET mitbringt, zeigt das Modul [Vergleichen, `default` und generische Interfaces](/modules/generische_interfaces/generische_interfaces.md) (erweitertes Wissen).

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag [`v02-interfaces`](https://github.com/erodner/prog2-adventure/tree/v02-interfaces)).

## Weitere Quellen

- [Einschränkungen für Typparameter – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters)
- [Schnittstellen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/types/interfaces) – Auffrischung zu Interfaces, die hier als Constraint auftreten
- [.NET Fiddle](https://dotnetfiddle.net/) – die Fehlermeldungen CS0311 und CS0315 in zwei Minuten selbst erzeugen, statt sie nur zu lesen
- [SharpLab](https://sharplab.io/) – zeigt neben deinem C#-Code, was der Compiler aus einer Methode mit `where T : ISammelbar` macht: der Constraint steht danach wirklich im erzeugten Code
