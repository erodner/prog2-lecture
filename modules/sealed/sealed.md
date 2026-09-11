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

Vererbung ist mächtig – und genau deshalb manchmal gefährlich. Wer von einer Klasse erbt und eine `virtual`-Methode überschreibt, kann ihr Verhalten beliebig verändern: versehentlich, weil man die Semantik nicht verstanden hat, oder absichtlich, um eine Prüfung zu umgehen. Bei einer Klasse `Konto` ist das kein Spaß mehr. Mit dem Schlüsselwort `sealed` lässt sich Vererbung an einer Stelle der Hierarchie gezielt **beenden** – die Klasse ist dann versiegelt, man könnte auch sagen: enterbt.

## Versiegelte Klassen

Als Ausgangspunkt dient eine Klasse `Konto`, deren Methoden bewusst überschreibbar sind, damit spezielle Kontoarten eigene Regeln ergänzen können:

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

Ein `SicheresKonto` überschreibt beide Methoden mit Prüfungen. Damit niemand diese Prüfungen durch eine weitere Ableitung wieder aushebeln kann, wird die Klasse mit `sealed` versiegelt:

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

`SicheresKonto` selbst erbt ganz normal von `Konto` und nutzt `base`, um die eigentliche Buchung zu erledigen. Der Unterschied zeigt sich erst, wenn jemand versucht, *von* `SicheresKonto` zu erben:

```csharp
class ManipuliertesKonto : SicheresKonto     // Fehler CS0509
{
    public override void Abheben(decimal betrag)
    {
        Kontostand -= betrag;    // Prüfung umgangen – geht zum Glück nicht
    }
}
```

Der Compiler meldet CS0509: „kann nicht von dem versiegelten Typ `SicheresKonto` abgeleitet werden.“ Die Semantik von `SicheresKonto` ist damit garantiert – wer ein `SicheresKonto` in der Hand hat, weiß, dass die Prüfungen gelten.

## Versiegelte Methoden

Manchmal soll die Klasse weiterhin erweiterbar bleiben, aber eine *bestimmte* Methode nicht mehr verändert werden. Dafür kombiniert man `sealed` mit `override`: Die Methode wird ein letztes Mal überschrieben und gleichzeitig für alle weiteren Erben geschlossen.

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

`Studentenkonto` darf `Einzahlen` überschreiben, aber nicht `Abheben` – dort meldet der Compiler CS0239 („kann den geerbten Member nicht überschreiben, da er versiegelt ist“). `sealed` an einer Methode ist nur zusammen mit `override` erlaubt; eine Methode, die gar nicht `virtual` ist, braucht kein Siegel, weil sie ohnehin nicht überschrieben werden kann.

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

- **Sicherheit und Klarheit:** Die Klasse hat eine feste Bedeutung, die niemand durch Überschreiben verändern soll – wie beim `SicheresKonto`. Auch in .NET selbst sind viele Klassen versiegelt, allen voran `string`: Ein String, der sich beim Vergleichen plötzlich anders verhält, wäre eine Katastrophe für jedes Programm.
- **Performance:** Ruft man eine `virtual`-Methode auf, muss die Laufzeitumgebung nachschauen, welche Implementierung zum tatsächlichen Objekt gehört (mehr dazu im Modul [Laufzeittyp und Verstecken](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md)). Bei einer versiegelten Klasse kann es nur eine Implementierung geben – der JIT-Compiler darf den Aufruf dann direkt auflösen und die Methode sogar inlinen. Für die meisten Programme ist dieser Effekt unmessbar klein, in engen Schleifen kann er aber spürbar sein.

Im Zweifel nicht `sealed` – außer du hast einen Grund. Jedes Siegel nimmt späteren Erweiterungen eine Option weg, und die beste Erweiterung ist oft eine, an die man beim Schreiben der Klasse noch nicht gedacht hat. Wenn du aber weißt, dass eine Klasse ein abgeschlossenes Konzept ist (ein Wert wie `Bruch`, ein Sicherheitsbaustein wie `SicheresKonto`), dann versiegle sie und dokumentiere damit deine Absicht.
{: .notice--primary}

Übung: Die Klasse `Roboter` aus den vorigen Modulen soll erweiterbar bleiben, aber ihre `Name`-Property soll in keiner abgeleiteten Klasse überschrieben werden können. Wie erreichst du das, ohne die Klasse selbst zu versiegeln? Und was passiert, wenn du `Putzroboter` versiegelst und anschließend `Fensterputzroboter : Putzroboter` schreibst?
{: .notice--info}

## Weitere Quellen

- [`sealed` – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/sealed)
- [Statische Klassen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members)
- [Vererbung in C# – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/object-oriented/inheritance)
