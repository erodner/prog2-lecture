---
title: "Interfaces – Erweiterte Konzepte"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Verträge im echten Leben bauen aufeinander auf: Ein Mietvertrag für eine Wohnung mit Garage enthält alles aus dem normalen Mietvertrag plus ein paar Klauseln mehr. Und manchmal steht in einem Vertrag eine Standardregelung, die gilt, solange nichts anderes vereinbart ist. Interfaces in C# kennen beides – und noch ein paar Feinheiten, die beim Lesen von Bibliothekscode und beim Entwurf größerer Programme wichtig werden. Am Ende dieses Moduls sehen wir mit `IFigurSpeicher`, wie ein Interface zwei Teile des Geometrieeditors voneinander entkoppelt.

## Interfaces erben von Interfaces

Ein Interface kann ein anderes erweitern. Wer das größere Interface implementiert, muss dann auch alle Mitglieder des kleineren bereitstellen:

```csharp
interface IGeraet
{
    string Name { get; }
    double Gewicht { get; set; }
}

interface IAufladbar : IGeraet
{
    int Akkustand { get; set; }
    void Aufladen();
}
```

Eine Klasse, die `IAufladbar` implementiert, verpflichtet sich damit automatisch auch zu `Name` und `Gewicht`. Und jedes `IAufladbar`-Objekt lässt sich einer `IGeraet`-Variablen zuweisen – dieselbe Ersetzbarkeit wie bei Basisklasse und Unterklasse, nur ohne geerbten Code. Interfaces dürfen dabei von mehreren Interfaces erben, was Klassen mit Basisklassen verwehrt bleibt.

## Explizite Implementierung

Normalerweise implementiert eine Klasse ein Interface-Mitglied als ganz normales öffentliches Mitglied. Es gibt aber eine zweite Form, bei der der Interface-Name vorangestellt wird:

```csharp
class Taschenlampe : IAufladbar
{
    public string Name => "Taschenlampe";
    public double Gewicht { get; set; }
    public int Akkustand { get; set; }

    void IAufladbar.Aufladen()
    {
        Akkustand = 100;
    }
}
```

Bei einer **expliziten Implementierung** steht keine Sichtbarkeit, und die Methode ist nur über den Interface-Typ erreichbar – nicht über die Klasse selbst:

```csharp
Taschenlampe lampe = new Taschenlampe { Akkustand = 20 };
lampe.Aufladen();                         // Compilerfehler: Taschenlampe hat kein Aufladen
((IAufladbar)lampe).Aufladen();           // funktioniert
IAufladbar a = lampe;
a.Aufladen();                             // funktioniert
Console.WriteLine(lampe.Akkustand);       // 100
```

Wozu ist das gut? Zum einen, wenn zwei Interfaces eine Methode mit gleichem Namen verlangen, die unterschiedlich umgesetzt werden soll – dann kann die Klasse beide explizit implementieren und hält sie auseinander. Zum anderen, um die öffentliche Schnittstelle einer Klasse schlank zu halten: Was nur für ein Framework über das Interface interessant ist, muss nicht in der IntelliSense-Liste der Klasse auftauchen.

## Default-Implementierungen

Seit C# 8 darf eine Interface-Methode einen Rumpf haben. Eine Klasse, die das Interface implementiert, kann diese Methode übernehmen, ohne sie selbst zu schreiben:

```csharp
interface IAufladbar : IGeraet
{
    int Akkustand { get; set; }

    void Aufladen()
    {
        Akkustand = 100;
    }

    bool IstFastLeer() => Akkustand < 10;
}

class Smartphone : IAufladbar, IFunkfaehig
{
    public string Name => "Smartphone";
    public double Gewicht { get; set; }
    public int Akkustand { get; set; }
    public double Reichweite { get; set; }
    // Aufladen und IstFastLeer kommen aus dem Interface
}
```

Der Hauptzweck ist, ein Interface nachträglich um eine Methode erweitern zu können, ohne alle existierenden Implementierungen zu brechen – bei Bibliotheken mit vielen Nutzern ein echtes Problem. Zwei Dinge sollte man wissen: Default-Mitglieder verhalten sich wie explizit implementierte, sind also nur über den Interface-Typ erreichbar (`smartphone.IstFastLeer()` geht nicht, `((IAufladbar)smartphone).IstFastLeer()` schon). Und ein Interface bleibt trotzdem zustandslos – es kann keine Felder anlegen, sondern nur mit den Properties arbeiten, die es selbst deklariert.

Default-Implementierungen machen ein Interface nicht zur abstrakten Klasse. Sie sind ein Werkzeug für die Weiterentwicklung von Schnittstellen, kein Ersatz für eine Basisklasse mit gemeinsamem Code. Wer ein Interface mit fünf Default-Methoden schreibt, braucht vermutlich eine abstrakte Klasse.
{: .notice--warning}

## Interface-Methoden und Überschreiben

Ein Punkt, der gern übersehen wird: Eine Methode, die ein Interface implementiert, ist **nicht automatisch virtuell**. Beim Interface selbst ist der Aufruf polymorph – über `IAufladbar` landet man immer bei der Klasse, die das Interface implementiert. Aber wenn von dieser Klasse wiederum eine Unterklasse erbt und die Methode überschreiben will, gilt dieselbe Regel wie in [`virtual` und `override`](/modules/virtual_override/virtual_override.md):

```csharp
class Smartphone : IAufladbar
{
    public int Akkustand { get; set; }
    public virtual void Aufladen() => Akkustand = 100;   // virtual!
}

class Outdoorhandy : Smartphone
{
    public override void Aufladen() => Akkustand = 80;   // schont den Akku, lädt nur bis 80 %
}
```

Ohne das `virtual` in `Smartphone` gäbe es kein `override` in `Outdoorhandy` – bestenfalls ein `new`, mit dem Ergebnis, dass `IAufladbar a = new Outdoorhandy()` beim Aufruf von `a.Aufladen()` die Smartphone-Version verwendet und bis 100 % lädt. Wer also Interface-Implementierungen in einer Klassenhierarchie weiter verfeinern will, markiert sie als `virtual` (oder `abstract`, wenn die Klasse selbst abstrakt ist).

## Ein Vertrag zwischen Schichten: `IFigurSpeicher`

Zum Schluss ein Interface, das nicht Fähigkeiten von Objekten beschreibt, sondern die Grenze zwischen zwei Teilen eines Programms. Der Geometrieeditor muss seine Figuren irgendwo ablegen – vorerst im Arbeitsspeicher, später in einer JSON-Datei. Die Verwaltungslogik soll davon nichts wissen. Also legt das Fachkonzept per Interface fest, was ein Speicher können muss:

```csharp
public interface IFigurSpeicher
{
    void Speichern(IEnumerable<Figur> figuren);
    List<Figur> Laden();
}
```

Die `FigurenVerwaltung` arbeitet ausschließlich mit diesem Interface und bekommt die konkrete Implementierung im Konstruktor übergeben:

```csharp
public class FigurenVerwaltung
{
    private readonly List<Figur> figuren = new();
    private readonly IFigurSpeicher speicher;

    public FigurenVerwaltung(IFigurSpeicher speicher)
    {
        this.speicher = speicher;
    }

    public void Speichern() => speicher.Speichern(figuren);

    public void Laden()
    {
        figuren.Clear();
        figuren.AddRange(speicher.Laden());
    }
    // Hinzufuegen, Entfernen, Suchen, GesamtFlaeche ...
}
```

Die einfachste Implementierung, `ArbeitsspeicherFigurSpeicher`, kopiert die Liste nur in ein Feld. Ob dahinter eine Datei, eine Datenbank oder ein Testdummy steckt, ist für die Verwaltung unsichtbar: `new FigurenVerwaltung(new ArbeitsspeicherFigurSpeicher())` und `new FigurenVerwaltung(new JsonFigurSpeicher("figuren.json"))` verhalten sich aus ihrer Sicht identisch. Das ist der Kern der Schichtenarchitektur, die wir in der nächsten Vorlesung mit einer grafischen Oberfläche aufbauen – und der Grund, warum sich die Verwaltung später ohne Dateizugriff testen lässt. Das vollständige Projekt findest du im Repository unter `examples/04_blazor/Geometrieeditor`.

Übung: Schreibe eine Klasse `ZaehlenderFigurSpeicher : IFigurSpeicher`, die intern einen `ArbeitsspeicherFigurSpeicher` benutzt und zusätzlich zählt, wie oft gespeichert und geladen wurde. Kann die `FigurenVerwaltung` unverändert damit arbeiten? Warum ist das ein Argument für Interfaces gegenüber einer konkreten Klasse als Konstruktorparameter?
{: .notice--info}

## Weitere Quellen

- [Explizite Schnittstellenimplementierung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/interfaces/explicit-interface-implementation)
- [Standardschnittstellenmethoden – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/advanced-topics/interface-implementation/default-interface-methods-versions)
- [Schnittstellen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/types/interfaces)
