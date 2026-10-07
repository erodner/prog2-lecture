---
title: "Kompilierzeittyp und Laufzeittyp"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Seit dem Modul [`virtual` und `override`](/modules/virtual_override/virtual_override.md) wissen wir, dass `ObjektAn(p)?.Symbol` je nach Objekt `#` oder `@` liefert, obwohl `ObjektAn` immer ein `Spielobjekt?` zurückgibt. Um zu verstehen, *warum* das funktioniert, müssen wir bei jeder Variablen zwei Typen auseinanderhalten: den Typ, den der Compiler sieht, und den Typ des Objekts, das zur Laufzeit wirklich dahintersteckt. Diese Unterscheidung ist eine der wichtigsten in der objektorientierten Programmierung. Sie erklärt, was der Compiler erlaubt, welche Methode tatsächlich läuft und wann ein Cast scheitert.

## Zwei Typen für eine Variable

Betrachten wir eine einzige Zeile:

```csharp
Spielobjekt o = new Spieler("Held", new Koordinate(1, 1));
```

- Der **Kompilierzeittyp** (auch *statischer Typ*) ist `Spielobjekt`. Er steht bei der Deklaration der Variablen und ändert sich nie.
- Der **Laufzeittyp** (auch *dynamischer Typ*) ist `Spieler`. Er gehört zum Objekt, das mit `new` erzeugt wurde, und steht erst fest, wenn das Programm läuft.

Erlaubt ist diese Zuweisung, weil ein Spieler ein Spielobjekt *ist* (die Ist-eine-Beziehung aus [Vererbung – Grundlagen](/modules/vererbung_grundlagen/vererbung_grundlagen.md)). Daraus folgt eine feste Regel: Der Laufzeittyp ist immer der Kompilierzeittyp selbst oder eine davon abgeleitete Klasse, nie etwas Allgemeineres.

Eine Analogie: Eine Variable ist wie ein beschrifteter Karton. Auf dem Karton steht „Spielobjekt“, das ist der Kompilierzeittyp. Was tatsächlich drinliegt, ein Spieler oder eine Wand, ist der Laufzeittyp. Der Compiler liest nur die Beschriftung, die Laufzeitumgebung schaut hinein.

Im Adventure begegnen uns die beiden Typen ständig, und fast nie stehen sie so offensichtlich in einer Zeile wie oben:

| Code | Kompilierzeittyp | Laufzeittyp |
| :--- | :--- | :--- |
| `Spielobjekt o = new Wand(...)` | `Spielobjekt` | `Wand` |
| `foreach (Spielobjekt o in objekte)` | `Spielobjekt` | wechselt mit jedem Element: `Wand`, `Wand`, … |
| `Spielobjekt? o = feld.ObjektAn(p)` | `Spielobjekt?` | das Objekt auf dem Feld, oder kein Objekt (`null`) |
| Parameter `objekt` in `Hinzufuegen(Spielobjekt objekt)` | `Spielobjekt` | das, was der Aufrufer übergibt |
| `Spieler held = new Spieler(...)` | `Spieler` | `Spieler` |

Bei einem Parameter oder einem Rückgabewert kann der Compiler den Laufzeittyp gar nicht kennen: `Hinzufuegen` wird mal mit einer Wand und mal mit einer Tür aufgerufen. Deshalb verlässt er sich ausschließlich auf den Kompilierzeittyp.

## Wer entscheidet was?

Beide Typen haben eine klare Aufgabe:

- **Der Kompilierzeittyp entscheidet, was aufgerufen werden darf.** Der Compiler prüft jeden Aufruf gegen die Mitglieder des Kompilierzeittyps.
- **Der Laufzeittyp entscheidet, welche Implementierung läuft**, und zwar bei `virtual`-Mitgliedern.

```csharp
Spielobjekt o = new Spieler("Held", new Koordinate(1, 1));
Console.WriteLine(o.Symbol);                  // @ – erlaubt, jedes Spielobjekt hat ein Symbol
o.Bewegen(Richtung.Rechts, feld);             // Fehler CS1061: 'Spielobjekt' enthält keine
                                              // Definition für 'Bewegen'
```

`o.Symbol` ist erlaubt, weil `Spielobjekt` ein `Symbol` hat. Welches Symbol herauskommt, entscheidet der Laufzeittyp: `@`. `o.Bewegen` verbietet der Compiler, obwohl das Objekt einen Spieler enthält, denn auf dem Karton steht nur „Spielobjekt“.

Wie findet die Laufzeitumgebung (die CLR, *Common Language Runtime*) die richtige Implementierung? Sie beginnt beim Laufzeittyp und sucht nach oben: Hat `Spieler` ein `override` für `Symbol`? Ja, also `@`. Wenn nicht, schaut sie in der Basisklasse nach, dann in deren Basisklasse, bis sie eine Implementierung findet. Ein `Magier : Spieler`, der `Symbol` nicht überschreibt, erscheint deshalb als `@` und nicht als `?`.

Übung: Bestimme für jede Zeile Kompilierzeittyp und Laufzeittyp von `x` und entscheide, ob der Aufruf kompiliert und was er ausgibt: (a) `Spielobjekt x = new Wand(new Koordinate(0, 0)); Console.WriteLine(x.Symbol);` (b) `Spielobjekt x = new Spieler("Held", new Koordinate(1, 1)); Console.WriteLine(x.Lebenspunkte);` (c) `Spieler x = new Spielobjekt("Held", new Koordinate(1, 1));` (d) `Spielobjekt? x = feld.ObjektAn(new Koordinate(1, 1));` für das Spielfeld aus [`virtual` und `override`](/modules/virtual_override/virtual_override.md).
{: .notice--info}

## Den Laufzeittyp abfragen und nutzen

Manchmal muss man wissen, was im Karton liegt. Die Methode `GetType()`, die jedes Objekt von `object` erbt, liefert den Laufzeittyp:

```csharp
Spielobjekt o = new Spieler("Held", new Koordinate(1, 1));
Console.WriteLine(o.GetType().Name);                    // Spieler
Console.WriteLine(o.GetType() == typeof(Spielobjekt));  // False
```

Auch im Debugger sieht man beide Typen: Setze einen Haltepunkt in `AlsText` und schau dir `ObjektAn(p)` in der Überwachung an. Visual Studio zeigt den Kompilierzeittyp `Spielobjekt?` und daneben den Laufzeittyp `Adventure.Kern.Wand`.

Meist will man aber nicht nur den Typ wissen, sondern **mit dem spezielleren Objekt arbeiten**, etwa die Lebenspunkte des Spielers lesen. Dafür braucht man eine Variable mit passendem Kompilierzeittyp, und es gibt drei Wege dorthin:

```csharp
// 1. Pattern Matching mit is: prüfen und gleichzeitig eine typisierte Variable anlegen
if (o is Spieler s)
{
    Console.WriteLine($"{s.Name} hat noch {s.Lebenspunkte} Lebenspunkte.");
    s.Bewegen(Richtung.Rechts, feld);
}

// 2. as: liefert null, wenn der Laufzeittyp nicht passt
Spieler? vielleicht = o as Spieler;
vielleicht?.Bewegen(Richtung.Oben, feld);

// 3. expliziter Cast: wirft eine Exception, wenn der Laufzeittyp nicht passt
Spieler sicher = (Spieler)o;
sicher.Bewegen(Richtung.Unten, feld);
```

Alle drei prüfen den **Laufzeittyp** und liefern eine Variable mit dem Kompilierzeittyp `Spieler`. `is` mit Pattern Matching ist heute die erste Wahl: Prüfung und Umwandlung passieren in einem Schritt, und `s` ist nur im `if`-Block gültig. `as` ist praktisch, wenn man mit `null` weiterarbeiten kann, etwa mit dem `?.`-Operator aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/nullable/nullable/). Der harte Cast passt, wenn ein falscher Typ ein Programmierfehler wäre, der laut auffallen soll:

```csharp
Spielobjekt wand = new Wand(new Koordinate(5, 2));
Spieler p = (Spieler)wand;
// System.InvalidCastException: Unable to cast object of type 'Adventure.Kern.Wand'
// to type 'Adventure.Kern.Spieler'.
```

Der Compiler lässt den Cast durch, weil er nur den Kompilierzeittyp `Spielobjekt` sieht, und ein Spielobjekt *könnte* ein Spieler sein. Erst zur Laufzeit stellt die CLR fest, dass im Karton eine `Wand` liegt.

Wenn du mehrere `is`-Abfragen nacheinander schreibst („wenn Wand, dann `#`, wenn Spieler, dann `@` …“), fehlt meist ein `virtual`-Mitglied. Genau dafür gibt es `Symbol`: Polymorphie erledigt die Fallunterscheidung für dich und vergisst keine der Objektarten, die im Laufe des Semesters noch dazukommen.
{: .notice--primary}

## Sonderfall: versteckte Mitglieder

Die Regel „der Laufzeittyp entscheidet“ gilt nur für `virtual`-Mitglieder, die mit `override` überschrieben werden. Deklariert eine abgeleitete Klasse ein Mitglied mit demselben Namen, aber **ohne** `override`, wird das geerbte Mitglied nicht ersetzt, sondern **versteckt** (englisch *hiding*). Das Schlüsselwort `new` vor dem Mitglied macht das ausdrücklich. Dann entscheidet der **Kompilierzeittyp**, welche Fassung läuft.

Bauen wir den Spieler absichtlich falsch:

```csharp
public class Spieler : Spielobjekt
{
    // FALSCH: versteckt statt überschreibt
    public new string Beschreibung() => base.Beschreibung() + $", {Lebenspunkte} Lebenspunkte";
}
```

```csharp
Spieler held = new Spieler("Held", new Koordinate(1, 1));
Console.WriteLine(held.Beschreibung());   // Held bei (1, 1), 3 Lebenspunkte

Spielobjekt o = held;                     // dasselbe Objekt, anderer Kompilierzeittyp
Console.WriteLine(o.Beschreibung());      // Held bei (1, 1)
```

Beide Variablen zeigen auf dasselbe Objekt, liefern aber verschiedene Beschreibungen. Über `held` (Kompilierzeittyp `Spieler`) läuft die neue Fassung, über `o` (Kompilierzeittyp `Spielobjekt`) die alte. Weil das Spielfeld alle Objekte als `Spielobjekt` behandelt, verschwinden die Lebenspunkte damit überall, wo es darauf ankommt. Dasselbe mit `Symbol` in `Wand` ergibt eine Karte voller `?`, obwohl `new Wand(...).Symbol` weiterhin `#` liefert. **Versteckte Mitglieder sind für Polymorphie unsichtbar.**

Lässt man `override` und `new` beide weg, verhält sich der Code wie mit `new`, aber der Compiler warnt mit CS0114 („blendet den geerbten Member aus …“). Diese Warnung ist fast immer ein vergessenes `override` (oder ein vergessenes `virtual` in der Basisklasse). Nimm sie ernst. Bewusstes Verstecken mit `new` braucht man in sauberem Code praktisch nie.
{: .notice--warning}

Übung: Lege ein Array `Spielobjekt[] objekte = { new Wand(new Koordinate(0, 0)), new Spieler("Held", new Koordinate(1, 1)) };` an. Schreibe eine Schleife, die für jedes Element den Laufzeittyp und das Symbol ausgibt und nur den Spieler einen Schritt nach rechts gehen lässt. Ersetze anschließend `is` durch einen harten Cast: Bei welchem Element fliegt die Exception, und warum erst zur Laufzeit? Baue danach in `Wand` das `override` vor `Symbol` in ein `new` um und beobachte, wie sich die Ausgabe von `feld.AlsText()` verändert, während `new Wand(...).Symbol` unverändert `#` liefert.
{: .notice--info}

## Weitere Quellen

- [Typtests und Umwandlungen (`is`, `as`) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/type-testing-and-cast)
- [`new`-Modifizierer – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/new-modifier)
- [Versionsverwaltung mit `override` und `new` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/versioning-with-the-override-and-new-keywords)
- [Pattern Matching – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/functional/pattern-matching)
- [.NET Fiddle – C# ohne Installation ausführen](https://dotnetfiddle.net/) – der Unterschied zwischen `override` und `new` lässt sich hier in einer Minute selbst nachstellen, ohne ein Projekt anzulegen.
