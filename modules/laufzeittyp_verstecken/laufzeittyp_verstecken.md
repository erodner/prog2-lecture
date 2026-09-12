---
title: "Laufzeittyp und Verstecken"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Seit dem Modul [`virtual` und `override`](/modules/virtual_override/virtual_override.md) wissen wir, dass `ObjektAn(p)?.Symbol` je nach Objekt `#`, `@` oder `?` liefert, obwohl die Variable immer den Typ `Spielobjekt` hat. Um wirklich zu verstehen, *warum* das so ist – und warum es manchmal nicht klappt –, müssen wir zwei Typen auseinanderhalten, die bei jeder Variablen im Spiel sind: den Typ, den der Compiler sieht, und den Typ, der zur Laufzeit tatsächlich im Speicher liegt. Wer diesen Unterschied verinnerlicht hat, versteht auch, was das Schlüsselwort `new` vor einem Mitglied anrichtet und wann ein Cast scheitert.

## Kompilierzeittyp und Laufzeittyp

Betrachten wir eine einzige Zeile:

```csharp
Spielobjekt o = new Spieler("Held", new Position(1, 1));
```

Links steht der **Kompilierzeittyp** (auch statischer Typ): `Spielobjekt`. Er ist der Typ der Variablen und legt fest, welche Mitglieder man über `o` aufrufen *darf*. Rechts vom `new` steht der **Laufzeittyp** (dynamischer Typ): `Spieler`. Er beschreibt, welches Objekt sich zur Laufzeit wirklich hinter der Variablen verbirgt. Der Laufzeittyp ist immer der Kompilierzeittyp selbst oder eine davon abgeleitete Klasse – niemals etwas Allgemeineres.

```csharp
Console.WriteLine(o.Symbol);                  // @ – erlaubt, jedes Spielobjekt hat ein Symbol
o.Bewegen(Richtung.Rechts, feld);             // Fehler CS1061: 'Spielobjekt' enthält keine
                                              // Definition für 'Bewegen'
```

Der Compiler kennt nur den Kompilierzeittyp. Dass hinter `o` in Wirklichkeit ein Spieler steckt, könnte er in diesem einfachen Fall zwar erraten – bei einem Parameter oder beim Rückgabewert von `ObjektAn` aber nicht. Deshalb gilt konsequent: **Was aufgerufen werden darf, entscheidet der Kompilierzeittyp. Welche Implementierung läuft, entscheidet der Laufzeittyp** – jedenfalls bei `virtual`-Mitgliedern.

## Wie die CLR die Methode findet

Beim Aufruf eines `virtual`-Mitglieds sucht die Laufzeitumgebung (die CLR, *Common Language Runtime*) **im Laufzeittyp beginnend hierarchisch nach oben**: Hat `Spieler` ein `override` für `Symbol`? Ja, also `@`. Wenn nicht, schaut sie in der Basisklasse nach, dann in deren Basisklasse, bis sie eine Implementierung findet. Ein `Magier : Spieler`, der `Symbol` nicht überschreibt, erscheint deshalb als `@` – nicht als `?`.

Bei einem Mitglied **ohne** `virtual` findet diese Suche nicht statt. Der Compiler bindet den Aufruf fest an das Mitglied des Kompilierzeittyps. Und genau hier lauert eine Falle.

## Verstecken mit `new` statt Überschreiben

Was passiert, wenn eine abgeleitete Klasse ein Mitglied mit demselben Namen deklariert, aber `override` fehlt? Dann wird das geerbte Mitglied nicht ersetzt, sondern **versteckt** (englisch *hiding*). Das Schlüsselwort `new` davor macht diese Absicht explizit. Bauen wir den Spieler einmal absichtlich falsch:

```csharp
public class Spielobjekt
{
    public virtual string Beschreibung() => $"{Name} bei {Position}";

    public override string ToString() => Beschreibung();
}

public class Spieler : Spielobjekt
{
    // FALSCH: versteckt statt überschreibt
    public new string Beschreibung() => base.Beschreibung() + $", {Lebenspunkte} Lebenspunkte";
}
```

```csharp
Spieler held = new Spieler("Held", new Position(1, 1));
Console.WriteLine(held.Beschreibung());   // Held bei (1, 1), 3 Lebenspunkte

Spielobjekt o = held;                     // dasselbe Objekt, anderer Kompilierzeittyp
Console.WriteLine(o.Beschreibung());      // Held bei (1, 1)
Console.WriteLine(held);                  // Held bei (1, 1)
```

Über die Variable `held` sieht alles richtig aus – deshalb ist dieser Fehler so tückisch. Der Unterschied zeigt sich bei `o`: `Beschreibung` ist nicht überschrieben, also entscheidet der Kompilierzeittyp, und der ist `Spielobjekt`. Besonders unangenehm ist die letzte Zeile: `ToString()` steht in `Spielobjekt` und ruft dort `Beschreibung()` auf – aus Sicht dieser Methode gibt es nur die eigene Fassung. Die Lebenspunkte verschwinden also überall dort, wo der Spieler als Spielobjekt behandelt wird: in `Console.WriteLine(held)`, in jeder Statusausgabe, in jeder Schleife über die Objektliste des Spielfelds. Dasselbe Muster mit `Symbol` in `Wand` hätte eine Karte zur Folge, deren Mauern aus `?` bestehen, obwohl `wand.Symbol` im Debugger brav `#` liefert – denn `AlsText` fragt über `ObjektAn(p)`, und das ist eine Variable vom Typ `Spielobjekt?`. Für eine `List<Spielobjekt>` gilt: **Versteckte Mitglieder sind für Polymorphie unsichtbar.**

Lässt man sowohl `override` als auch `new` weg, verhält sich der Code exakt wie mit `new` – aber der Compiler warnt mit CS0108 („blendet den geerbten Member aus; verwenden Sie das Schlüsselwort `new`, wenn das Ausblenden beabsichtigt war“). Diese Warnung ist fast immer ein Zeichen für ein vergessenes `override` (oder ein vergessenes `virtual` in der Basisklasse). Nimm sie ernst: Das Programm kompiliert, tut aber nicht, was du meinst. Bewusstes Verstecken mit `new` ist in sauberem Code sehr selten nötig.
{: .notice--warning}

## Den Laufzeittyp herausfinden

Manchmal muss man wissen, was sich hinter einer Referenz verbirgt. Die Methode `GetType()`, die jedes Objekt von `object` erbt, liefert den Laufzeittyp:

```csharp
Spielobjekt o = new Spieler("Held", new Position(1, 1));
Console.WriteLine(o.GetType().Name);                    // Spieler
Console.WriteLine(o.GetType() == typeof(Spielobjekt));  // False
```

`GetType()` ist nützlich für Ausgaben und im Debugger – setze einen Haltepunkt in `AlsText` und schau dir an, was die Watch-Ansicht für `ObjektAn(p)` anzeigt: dort steht `Wand`, obwohl die Variable `Spielobjekt?` heißt. Im Code will man aber meist nicht den Typ vergleichen, sondern **mit dem spezielleren Objekt arbeiten** – etwa die Lebenspunkte des Spielers lesen. Dafür gibt es drei Werkzeuge:

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

`is` mit Pattern Matching ist heute die erste Wahl: Die Prüfung und die Umwandlung passieren in einem Schritt, und die neue Variable ist nur im `if`-Block gültig. `as` ist praktisch, wenn man mit `null` weiterarbeiten kann – etwa mit dem `?.`-Operator, den wir aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/nullable/nullable/) kennen. Der harte Cast ist dann angebracht, wenn ein falscher Typ ein Programmierfehler wäre, der laut auffallen soll:

```csharp
Spielobjekt wand = new Wand(new Position(5, 2));
Spieler p = (Spieler)wand;
// System.InvalidCastException: Unable to cast object of type 'Wand' to type 'Spieler'.
```

Der Compiler lässt den Cast durch, weil er zur Kompilierzeit nicht wissen kann, was in `wand` steckt. Zur Laufzeit stellt die CLR fest, dass eine `Wand` eben kein `Spieler` ist – die Ist-eine-Beziehung gilt nur in eine Richtung.

Wenn du beim Zeichnen der Karte mehrere `is`-Abfragen nacheinander schreibst („wenn Wand, dann `#`, wenn Spieler, dann `@` …“), ist das meist ein Zeichen, dass ein `virtual`-Mitglied fehlt. Genau dafür gibt es `Symbol`: Polymorphie erledigt die Fallunterscheidung für dich – und vergisst keine der Objektarten, die im Laufe des Semesters noch dazukommen.
{: .notice--primary}

Übung: Lege ein Array `Spielobjekt[] objekte = { new Wand(new Position(0, 0)), new Spieler("Held", new Position(1, 1)) };` an. Schreibe eine Schleife, die für jedes Element den Laufzeittyp und das Symbol ausgibt und nur den Spieler einen Schritt nach rechts gehen lässt. Ersetze anschließend `is` durch einen harten Cast – bei welchem Element fliegt die Exception, und warum erst zur Laufzeit? Baue danach in `Wand` das `override` vor `Symbol` in ein `new` um und beobachte, wie sich die Ausgabe von `feld.AlsText()` verändert, während `new Wand(...).Symbol` unverändert `#` liefert.
{: .notice--info}

## Weitere Quellen

- [Typtests und Umwandlungen (`is`, `as`) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/type-testing-and-cast)
- [`new`-Modifizierer – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/new-modifier)
- [Versionsverwaltung mit `override` und `new` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/versioning-with-the-override-and-new-keywords)
- [Pattern Matching – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/functional/pattern-matching)
