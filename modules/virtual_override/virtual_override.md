---
title: "virtual und override"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Im Modul [Vererbung – Grundlagen](/modules/vererbung_grundlagen/vererbung_grundlagen.md) hat der Putzroboter das Arbeiten einfach geerbt und arbeitet genau wie jeder andere Roboter. Eigentlich arbeitet ein Putzroboter aber *anders*: Er wischt dabei den Boden. Wir wollen also eine geerbte Methode nicht nur übernehmen, sondern **anpassen** – und zwar so, dass auch Code, der nur einen `Roboter` kennt, automatisch die angepasste Variante aufruft. Genau das leisten `virtual` und `override`. Dieses Prinzip heißt **Polymorphie** (griechisch für Vielgestaltigkeit) und ist der eigentliche Grund, warum objektorientierte Programmierung so mächtig ist.

## `virtual` – die Basisklasse erlaubt das Überschreiben

Eine Methode kann in einer abgeleiteten Klasse nur dann ersetzt werden, wenn die Basisklasse das ausdrücklich erlaubt. Dafür wird sie mit `virtual` gekennzeichnet:

```csharp
class Roboter
{
    public string Name { get; set; }

    public Roboter(string name)
    {
        Name = name;
    }

    public virtual void Arbeiten()
    {
        Console.WriteLine($"{Name} arbeitet.");
    }
}
```

An der Verwendung von `Roboter` ändert sich dadurch nichts – Robbi arbeitet wie bisher. `virtual` ist lediglich die Erlaubnis: „Erben dürfen diese Methode durch eine eigene ersetzen.“ Wird die Methode in einer abgeleiteten Klasse nicht überschrieben, gilt weiterhin die Implementierung der Basisklasse.

## `override` – die abgeleitete Klasse ersetzt die Methode

Der Putzroboter überschreibt `Arbeiten` mit `override`. Die Signatur muss exakt dieselbe sein wie in der Basisklasse – gleicher Name, gleiche Parameter, gleicher Rückgabetyp.

```csharp
class Putzroboter : Roboter
{
    public Putzroboter(string name) : base(name)
    {
    }

    public override void Arbeiten()
    {
        Wischen();
        base.Arbeiten();
    }

    public void Wischen()
    {
        Console.WriteLine($"{Name} wischt den Boden.");
    }
}
```

Interessant ist die Zeile `base.Arbeiten()`: Mit `base` greifen wir auf die **Originalmethode** der Basisklasse zu. Der Putzroboter ersetzt das Arbeiten also nicht komplett, sondern *erweitert* es – erst wischen, dann ganz normal arbeiten. Ob und wann man `base` aufruft, ist eine Designentscheidung: Manchmal will man das Verhalten vollständig austauschen, manchmal nur ergänzen.

```csharp
Putzroboter wischi = new Putzroboter("Wischi");
wischi.Arbeiten();
// Wischi wischt den Boden.
// Wischi arbeitet.
```

Bis hierhin könnte man einwenden: Das hätte man auch mit einer eigenen Methode `WischenUndArbeiten` erreicht. Der Unterschied zeigt sich erst, wenn wir Roboter über ihre Basisklasse ansprechen.

## Polymorphie – die richtige Methode zur Laufzeit

Dank der Ist-eine-Beziehung dürfen wir Putzroboter in eine Liste von Robotern legen. Beim Aufruf von `Arbeiten` entscheidet **nicht** der Typ der Variablen, sondern das tatsächliche Objekt, welche Implementierung läuft:

```csharp
List<Roboter> alleRoboter = new List<Roboter>
{
    new Roboter("Robbi"),
    new Putzroboter("Wischi"),
    new Putzroboter("Putzi")
};

foreach (Roboter r in alleRoboter)
{
    r.Arbeiten();
}
// Robbi arbeitet.
// Wischi wischt den Boden.
// Wischi arbeitet.
// Putzi wischt den Boden.
// Putzi arbeitet.
```

In der Schleife steht überall nur `r.Arbeiten()` – und trotzdem wischen die Putzroboter dabei den Boden. Die Schleife weiß nichts von Putzrobotern und muss es auch nicht. Man sagt: Die Methode `Arbeiten` ist **polymorph**. Die Laufzeitumgebung schaut bei jedem Aufruf nach, welches Objekt sich wirklich hinter `r` verbirgt, und wählt die passende Methode.

Warum ist das der Kern der OOP? Weil sich dadurch die Abhängigkeiten umdrehen: Die Schleife hängt nur von `Roboter` ab. Kommt morgen ein `Rasenroboter` dazu, der `Arbeiten` auf seine Weise überschreibt, funktioniert die Schleife **ohne eine einzige Änderung** weiter. Bestehender Code bleibt stabil, während neue Varianten hinzukommen. Das gleiche Muster steckt im durchgehenden Beispiel des Kurses: Im Geometrieeditor deklariert die Basisklasse `Figur` eine Methode `public virtual string Beschreibung()`, und `Kreis` überschreibt sie mit `base.Beschreibung() + $" (r = {Radius})"`. Eine Liste von Figuren kann so jede Figur beschreiben lassen, ohne zu wissen, ob es ein Kreis, Rechteck oder Dreieck ist. Das vollständige Projekt findest du im Repository unter `examples/04_blazor/Geometrieeditor`.

`virtual` und `override` funktionieren nicht nur für Methoden, sondern genauso für Properties: `public virtual string Beschreibung => ...` in der Basisklasse, `public override string Beschreibung => ...` in der abgeleiteten Klasse.
{: .notice--primary}

## Ohne `virtual` keine Polymorphie

Die Erlaubnis der Basisklasse ist keine Formalität. Fehlt das `virtual`, lässt sich die Methode nicht überschreiben – der Compiler meldet CS0506 („kann den geerbten Member nicht überschreiben, da er nicht als `virtual`, `abstract` oder `override` markiert ist“). Und wer dann *beide* Schlüsselwörter weglässt und in `Putzroboter` einfach eine zweite Methode `Arbeiten` schreibt, bekommt zwar nur eine Warnung, aber ein anderes Verhalten: In der Liste oben würden plötzlich alle Roboter nur noch „arbeitet.“ ausgeben. Die Methode wird dann nicht überschrieben, sondern **versteckt** – was das genau bedeutet, zeigt das Modul [Laufzeittyp und Verstecken](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md).
{: .notice--warning}

Im Zweifel gilt: Eine Methode, die abgeleitete Klassen sinnvoll anpassen könnten, wird `virtual`. Eine Methode, deren Verhalten für alle Erben verbindlich sein soll (etwa eine Prüfung, die niemand umgehen darf), bleibt ohne `virtual` – oder wird ausdrücklich versiegelt, siehe [`sealed`](/modules/sealed/sealed.md).

Übung: Schreibe eine Klasse `Rasenroboter`, die eine Methode `Maehen()` bekommt („… mäht den Rasen.“) und beim Arbeiten zuerst mäht und danach ganz normal arbeitet. Füge einen Rasenroboter zur Liste oben hinzu und überlege, *bevor* du das Programm startest, welche Ausgabe entsteht. Ändere dann `override` in `Rasenroboter` zu `new` – welche Zeile der Ausgabe ändert sich?
{: .notice--info}

## Weitere Quellen

- [Polymorphie – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/object-oriented/polymorphism)
- [`virtual` – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/virtual)
- [`override` – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/override)
