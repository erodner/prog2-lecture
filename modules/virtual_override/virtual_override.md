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

Im Modul [Vererbung – Grundlagen](/modules/vererbung_grundlagen/vererbung_grundlagen.md) hat der Schleimgeist das Spuken einfach geerbt und spukt genau wie jeder andere Geist. Eigentlich spukt ein Schleimgeist aber *anders*: Er hinterlässt dabei eine Schleimspur. Wir wollen also eine geerbte Methode nicht nur übernehmen, sondern **anpassen** – und zwar so, dass auch Code, der nur einen `Geist` kennt, automatisch die angepasste Variante aufruft. Genau das leisten `virtual` und `override`. Dieses Prinzip heißt **Polymorphie** (griechisch für Vielgestaltigkeit) und ist der eigentliche Grund, warum objektorientierte Programmierung so mächtig ist.

## `virtual` – die Basisklasse erlaubt das Überschreiben

Eine Methode kann in einer abgeleiteten Klasse nur dann ersetzt werden, wenn die Basisklasse das ausdrücklich erlaubt. Dafür wird sie mit `virtual` gekennzeichnet:

```csharp
class Geist
{
    public string Name { get; set; }

    public Geist(string name)
    {
        Name = name;
    }

    public virtual void Spuken()
    {
        Console.WriteLine($"{Name} sagt: 'Buh'");
    }
}
```

An der Verwendung von `Geist` ändert sich dadurch nichts – Spooky spukt wie bisher. `virtual` ist lediglich die Erlaubnis: „Erben dürfen diese Methode durch eine eigene ersetzen.“ Wird die Methode in einer abgeleiteten Klasse nicht überschrieben, gilt weiterhin die Implementierung der Basisklasse.

## `override` – die abgeleitete Klasse ersetzt die Methode

Der Schleimgeist überschreibt `Spuken` mit `override`. Die Signatur muss exakt dieselbe sein wie in der Basisklasse – gleicher Name, gleiche Parameter, gleicher Rückgabetyp.

```csharp
class Schleimgeist : Geist
{
    public Schleimgeist(string name) : base(name)
    {
    }

    public override void Spuken()
    {
        Schleimen();
        base.Spuken();
    }

    public void Schleimen()
    {
        Console.WriteLine($"{Name} hinterlässt eine Schleimspur.");
    }
}
```

Interessant ist die Zeile `base.Spuken()`: Mit `base` greifen wir auf die **Originalmethode** der Basisklasse zu. Der Schleimgeist ersetzt das Spuken also nicht komplett, sondern *erweitert* es – erst schleimen, dann ganz normal „Buh“ sagen. Ob und wann man `base` aufruft, ist eine Designentscheidung: Manchmal will man das Verhalten vollständig austauschen, manchmal nur ergänzen.

```csharp
Schleimgeist schleimi = new Schleimgeist("Schleimi");
schleimi.Spuken();
// Schleimi hinterlässt eine Schleimspur.
// Schleimi sagt: 'Buh'
```

Bis hierhin könnte man einwenden: Das hätte man auch mit einer eigenen Methode `SchleimigSpuken` erreicht. Der Unterschied zeigt sich erst, wenn wir Geister über ihre Basisklasse ansprechen.

## Polymorphie – die richtige Methode zur Laufzeit

Dank der Ist-eine-Beziehung dürfen wir Schleimgeister in eine Liste von Geistern legen. Beim Aufruf von `Spuken` entscheidet **nicht** der Typ der Variablen, sondern das tatsächliche Objekt, welche Implementierung läuft:

```csharp
List<Geist> geister = new List<Geist>
{
    new Geist("Spooky"),
    new Schleimgeist("Schleimi"),
    new Schleimgeist("Smeargol")
};

foreach (Geist g in geister)
{
    g.Spuken();
}
// Spooky sagt: 'Buh'
// Schleimi hinterlässt eine Schleimspur.
// Schleimi sagt: 'Buh'
// Smeargol hinterlässt eine Schleimspur.
// Smeargol sagt: 'Buh'
```

In der Schleife steht überall nur `g.Spuken()` – und trotzdem spuken die Schleimgeister schleimig. Die Schleife weiß nichts von Schleimgeistern und muss es auch nicht. Man sagt: Die Methode `Spuken` ist **polymorph**. Die Laufzeitumgebung schaut bei jedem Aufruf nach, welches Objekt sich wirklich hinter `g` verbirgt, und wählt die passende Methode.

Warum ist das der Kern der OOP? Weil sich dadurch die Abhängigkeiten umdrehen: Die Schleife hängt nur von `Geist` ab. Kommt morgen ein `Poltergeist` dazu, der `Spuken` auf seine Weise überschreibt, funktioniert die Schleife **ohne eine einzige Änderung** weiter. Bestehender Code bleibt stabil, während neue Varianten hinzukommen. Das gleiche Muster steckt im durchgehenden Beispiel des Kurses: Im Geometrieeditor deklariert die Basisklasse `Figur` eine Methode `public virtual string Beschreibung()`, und `Kreis` überschreibt sie mit `base.Beschreibung() + $" (r = {Radius})"`. Eine Liste von Figuren kann so jede Figur beschreiben lassen, ohne zu wissen, ob es ein Kreis, Rechteck oder Dreieck ist. Das vollständige Projekt findest du im Repository unter `examples/03_blazor/Geometrieeditor`.

`virtual` und `override` funktionieren nicht nur für Methoden, sondern genauso für Properties: `public virtual string Beschreibung => ...` in der Basisklasse, `public override string Beschreibung => ...` in der abgeleiteten Klasse.
{: .notice--primary}

## Ohne `virtual` keine Polymorphie

Die Erlaubnis der Basisklasse ist keine Formalität. Fehlt das `virtual`, lässt sich die Methode nicht überschreiben – der Compiler meldet CS0506 („kann den geerbten Member nicht überschreiben, da er nicht als `virtual`, `abstract` oder `override` markiert ist“). Und wer dann *beide* Schlüsselwörter weglässt und in `Schleimgeist` einfach eine zweite Methode `Spuken` schreibt, bekommt zwar nur eine Warnung, aber ein anderes Verhalten: In der Liste oben würden plötzlich alle Geister nur noch „Buh“ sagen. Die Methode wird dann nicht überschrieben, sondern **versteckt** – was das genau bedeutet, zeigt das Modul [Laufzeittyp und Verstecken](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md).
{: .notice--warning}

Im Zweifel gilt: Eine Methode, die abgeleitete Klassen sinnvoll anpassen könnten, wird `virtual`. Eine Methode, deren Verhalten für alle Erben verbindlich sein soll (etwa eine Prüfung, die niemand umgehen darf), bleibt ohne `virtual` – oder wird ausdrücklich versiegelt, siehe [`sealed`](/modules/sealed/sealed.md).

Übung: Schreibe eine Klasse `Poltergeist`, die beim Spuken zuerst „… rumpelt laut“ ausgibt und danach ganz normal spukt. Füge einen Poltergeist zur Liste oben hinzu und überlege, *bevor* du das Programm startest, welche Ausgabe entsteht. Ändere dann `override` in `Poltergeist` zu `new` – welche Zeile der Ausgabe ändert sich?
{: .notice--info}

## Weitere Quellen

- [Polymorphie – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/object-oriented/polymorphism)
- [`virtual` – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/virtual)
- [`override` – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/override)
