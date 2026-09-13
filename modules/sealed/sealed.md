---
title: "Versiegeln mit sealed"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Vererbung ist mächtig – und genau deshalb manchmal gefährlich. Wer von einer Klasse erbt und ein `virtual`-Mitglied überschreibt, kann ihr Verhalten beliebig verändern: versehentlich, weil man die Semantik nicht verstanden hat, oder absichtlich, um eine Prüfung zu umgehen. Bei einer Klasse `Konto` ist das kein Spaß mehr, und selbst in unserem Spiel wäre eine Wand, durch die man plötzlich hindurchlaufen kann, kein Feature, sondern ein Fehler. Mit dem Schlüsselwort `sealed` lässt sich Vererbung an einer Stelle der Hierarchie gezielt **beenden** – die Klasse ist dann versiegelt, man könnte auch sagen: enterbt.

## Eine Wand ist eine Wand

Im Adventure gibt es genau eine versiegelte Klasse, und das ist kein Zufall:

```csharp
public sealed class Wand : Spielobjekt
{
    public Wand(Position position) : base("Wand", position)
    {
    }

    public override char Symbol => '#';
}
```

Eine Wand ist ein **abgeschlossenes Konzept**: Sie steht auf einem Feld, sie wird als `#` gezeichnet, und man kommt nicht hindurch – das ist ihre komplette Bedeutung im Spiel. Für alles, was sich anders verhalten soll, gibt es bereits die passende Stelle in der Hierarchie: Eine Tür, die sich öffnen lässt, ist keine Spezialwand, sondern ein eigenes `Spielobjekt` mit eigenem Symbol. Eine `Glaswand : Wand`, die `IstPassierbar` auf `true` setzt, wäre dagegen eine Zeitbombe – jede Wegfindung im Spiel geht davon aus, dass `#` blockiert.

```csharp
sealed class Wand : Spielobjekt { /* ... */ }

class Geheimwand : Wand                 // Fehler CS0509
{
    public override bool IstPassierbar => true;
}
```

Der Compiler meldet CS0509: „kann nicht von dem versiegelten Typ `Wand` abgeleitet werden.“ Wer eine `Wand` in der Hand hält, weiß damit garantiert, womit er es zu tun hat – und wenn jemand später doch eine durchlässige Mauer braucht, muss er eine bewusste Entscheidung treffen und eine eigene Klasse neben `Wand` stellen, statt heimlich deren Bedeutung zu verbiegen. Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v01-vererbung`).

## Versiegeln, damit Prüfungen halten

Der zweite klassische Grund ist Sicherheit. Als Ausgangspunkt dient eine Klasse `Konto`, deren Methoden bewusst überschreibbar sind, damit spezielle Kontoarten eigene Regeln ergänzen können:

```csharp
class Konto
{
    public decimal Kontostand { get; protected set; }

    public virtual void Einzahlen(decimal betrag)
    {
        Kontostand += betrag;
    }

    public virtual void Abheben(decimal betrag)
    {
        Kontostand -= betrag;
    }
}
```

Ein `SicheresKonto` überschreibt beide Methoden mit Prüfungen. Damit niemand diese Prüfungen durch eine weitere Ableitung wieder aushebeln kann, wird die Klasse versiegelt:

```csharp
sealed class SicheresKonto : Konto
{
    public override void Einzahlen(decimal betrag)
    {
        if (betrag <= 0)
        {
            throw new ArgumentException("Betrag muss positiv sein.");
        }
        base.Einzahlen(betrag);
    }

    public override void Abheben(decimal betrag)
    {
        if (betrag > Kontostand)
        {
            throw new InvalidOperationException("Deckung reicht nicht.");
        }
        base.Abheben(betrag);
    }
}
```

`SicheresKonto` selbst erbt ganz normal von `Konto` und nutzt `base`, um die eigentliche Buchung zu erledigen – das Siegel wirkt nur nach unten. Ein `ManipuliertesKonto : SicheresKonto`, das in `Abheben` einfach `Kontostand -= betrag` schreiben und die Deckungsprüfung überspringen will, scheitert wieder an CS0509.

## Versiegelte Methoden

Manchmal soll die Klasse weiterhin erweiterbar bleiben, aber ein *bestimmtes* Mitglied nicht mehr verändert werden. Dafür kombiniert man `sealed` mit `override`: Die Methode wird ein letztes Mal überschrieben und gleichzeitig für alle weiteren Erben geschlossen.

```csharp
class Girokonto : Konto
{
    public sealed override void Abheben(decimal betrag)
    {
        if (betrag > Kontostand)
        {
            throw new InvalidOperationException("Deckung reicht nicht.");
        }
        base.Abheben(betrag);
    }
}

class Studentenkonto : Girokonto
{
    public override void Einzahlen(decimal betrag)    // erlaubt
    {
        base.Einzahlen(betrag);
        Console.WriteLine("Danke – Studierende zahlen keine Gebühren.");
    }

    public override void Abheben(decimal betrag)      // Fehler CS0239
    {
        // ...
    }
}
```

`Studentenkonto` darf `Einzahlen` überschreiben, aber nicht `Abheben` – dort meldet der Compiler CS0239 („kann den geerbten Member nicht überschreiben, da er versiegelt ist“). `sealed` an einem Mitglied ist nur zusammen mit `override` erlaubt; eine Methode, die gar nicht `virtual` ist, braucht kein Siegel, weil sie ohnehin nicht überschrieben werden kann.

## Statische Klassen

Von `static`-Klassen kann grundsätzlich nicht geerbt werden – sie sind implizit versiegelt. Das ist konsequent: Eine statische Klasse wie `Math` oder eine eigene `Zinsrechner`-Klasse hat keine Objekte, und ohne Objekte gibt es auch keine Ist-eine-Beziehung.

```csharp
static class Zinsrechner
{
    public static decimal Zinsen(decimal betrag, decimal satz) => betrag * satz / 100;
}

class MeinZinsrechner : Zinsrechner   // Fehler CS0709
{
}
```

## Wann versiegeln?

Es gibt zwei gute Gründe für `sealed`:

- **Sicherheit und Klarheit:** Die Klasse hat eine feste Bedeutung, die niemand durch Überschreiben verändern soll – wie bei `Wand` oder `SicheresKonto`. Auch in .NET selbst sind viele Klassen versiegelt, allen voran `string`: Ein String, der sich beim Vergleichen plötzlich anders verhält, wäre eine Katastrophe für jedes Programm.
- **Performance:** Ruft man ein `virtual`-Mitglied auf, muss die Laufzeitumgebung nachschauen, welche Implementierung zum tatsächlichen Objekt gehört (mehr dazu im Modul [Laufzeittyp und Verstecken](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md)). Bei einer versiegelten Klasse kann es nur eine Implementierung geben – der JIT-Compiler darf den Aufruf dann direkt auflösen und die Methode sogar inlinen. Für die meisten Programme ist dieser Effekt unmessbar klein; in einer Schleife, die bei jedem Zeichnen der Karte für jedes Feld `Symbol` abfragt, ist er immerhin messbar.

Im Zweifel nicht `sealed` – außer du hast einen Grund. Jedes Siegel nimmt späteren Erweiterungen eine Option weg, und die beste Erweiterung ist oft eine, an die man beim Schreiben der Klasse noch nicht gedacht hat. `Spielobjekt` und `Spieler` bleiben deshalb offen: Aus `Spieler` könnte später ein `Magier` mit eigenen Fähigkeiten werden. Wenn du aber weißt, dass eine Klasse ein abgeschlossenes Konzept ist, dann versiegle sie und dokumentiere damit deine Absicht.
{: .notice--primary}

Übung: Die Klasse `Spieler` soll erweiterbar bleiben, aber ihre Property `Symbol` soll in keiner abgeleiteten Klasse noch einmal überschrieben werden können – der Held ist immer `@`. Wie erreichst du das, ohne die Klasse selbst zu versiegeln? Schreibe anschließend eine Klasse `Magier : Spieler`, die `Beschreibung()` um die Zahl der Zaubersprüche erweitert, und versuche darin trotzdem, `Symbol` zu überschreiben: Welchen Fehler meldet der Compiler?
{: .notice--info}

## Weitere Quellen

- [`sealed` – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/sealed)
- [Statische Klassen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members)
- [Vererbung in C# – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/object-oriented/inheritance)
- [Liskovsches Substitutionsprinzip – Wikipedia](https://de.wikipedia.org/wiki/Liskovsches_Substitutionsprinzip) – die theoretische Begründung für das Siegel: Eine `Glaswand`, durch die man laufen kann, verletzt genau dieses Prinzip.
