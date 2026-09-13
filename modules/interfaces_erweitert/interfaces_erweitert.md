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

Verträge im echten Leben bauen aufeinander auf: Ein Mietvertrag für eine Wohnung mit Garage enthält alles aus dem normalen Mietvertrag plus ein paar Klauseln mehr. Und manchmal steht in einem Vertrag eine Standardregelung, die gilt, solange nichts anderes vereinbart ist. Interfaces in C# kennen beides – und noch ein paar Feinheiten, die beim Lesen von Bibliothekscode und beim Entwurf größerer Programme wichtig werden. Am Ende dieses Moduls sehen wir mit `ILevelQuelle` ein Interface, das nicht mehr eine Fähigkeit beschreibt, sondern eine Grenze zwischen zwei Teilen des Programms zieht.

## Interfaces erben von Interfaces

Ein Interface kann ein anderes erweitern. Wer das größere Interface implementiert, muss dann auch alle Mitglieder des kleineren bereitstellen. Nehmen wir an, unser Held soll nicht mehr beliebig viel schleppen können – dann braucht alles Tragbare ein Gewicht:

```csharp
public interface ISammelbar
{
    string Name { get; }
    string Aufheben(Spieler spieler);
}

public interface ITragbar : ISammelbar
{
    double Gewicht { get; }
}
```

Eine Klasse, die `ITragbar` implementiert, verpflichtet sich damit automatisch auch zu `Name` und `Aufheben`. Und jedes `ITragbar`-Objekt lässt sich einer `ISammelbar`-Variablen zuweisen – dieselbe Ersetzbarkeit wie zwischen Basisklasse und Unterklasse, nur ohne geerbten Code. Interfaces dürfen dabei von mehreren Interfaces erben, was Klassen mit Basisklassen verwehrt bleibt: `interface ITragbarUndBrennbar : ITragbar, IBrennbar` wäre erlaubt.

Warum nicht gleich `Gewicht` in `ISammelbar` aufnehmen? Weil das alle bestehenden Implementierungen brechen würde – `Gegenstand` und jede Klasse außerhalb unseres Projekts müssten sofort ein `Gewicht` liefern. Ein neues, engeres Interface lässt den alten Vertrag unangetastet.
{: .notice--primary}

## Explizite Implementierung

Normalerweise implementiert eine Klasse ein Interface-Mitglied als ganz normales öffentliches Mitglied. Es gibt aber eine zweite Form, bei der der Interface-Name vorangestellt wird. Das ist praktisch für Dinge, die im Inventar liegen dürfen, ohne je auf dem Spielfeld gelegen zu haben – etwa ein Auftrag, den der Spieler annimmt:

```csharp
public class Auftrag : ISammelbar
{
    public string Name { get; }
    public bool Erfuellt { get; private set; }

    public Auftrag(string name) => Name = name;

    string ISammelbar.Aufheben(Spieler spieler) => $"{spieler.Name} nimmt den Auftrag „{Name}“ an.";
}
```

Bei einer **expliziten Implementierung** steht keine Sichtbarkeit, und das Mitglied ist nur über den Interface-Typ erreichbar – nicht über die Klasse selbst:

```csharp
Auftrag a = new Auftrag("Finde den Schlüssel");
a.Aufheben(spieler);                       // Compilerfehler: Auftrag hat kein Aufheben
((ISammelbar)a).Aufheben(spieler);         // funktioniert
ISammelbar s = a;
s.Aufheben(spieler);                       // funktioniert
```

Wozu ist das gut? Zum einen, wenn zwei Interfaces ein Mitglied mit gleichem Namen verlangen, das unterschiedlich umgesetzt werden soll – dann kann die Klasse beide explizit implementieren und hält sie auseinander. Zum anderen, um die öffentliche Schnittstelle einer Klasse schlank zu halten: „Aufheben“ ist etwas, das das Inventar mit einem `ISammelbar` tut, kein Befehl, den man einem Auftrag direkt gibt.

## Default-Implementierungen

Seit C# 8 darf ein Interface-Mitglied einen Rumpf haben. Eine Klasse, die das Interface implementiert, kann diese Methode übernehmen, ohne sie selbst zu schreiben:

```csharp
public interface ISammelbar
{
    string Name { get; }
    string Aufheben(Spieler spieler);

    string Beschreibung() => $"{Name} – liegt hier und kann aufgehoben werden.";
}
```

Der Hauptzweck ist, ein Interface nachträglich um ein Mitglied erweitern zu können, ohne alle existierenden Implementierungen zu brechen – bei Bibliotheken mit vielen Nutzern ein echtes Problem. Der `Auftrag` von oben bekommt seine `Beschreibung()` damit geschenkt. Bei unseren Gegenständen passiert dagegen etwas Interessantes:

```csharp
ISammelbar auftrag = new Auftrag("Finde den Schlüssel");
Console.WriteLine(auftrag.Beschreibung());
// Finde den Schlüssel – liegt hier und kann aufgehoben werden.

ISammelbar schluessel = new Schluessel(new Position(3, 4));
Console.WriteLine(schluessel.Beschreibung());
// Schlüssel bei (3, 4)
```

`Schluessel` erbt über `Gegenstand` und `StatischesObjekt` die Methode `public virtual string Beschreibung()` aus `Spielobjekt`. Sie passt auf die Signatur im Interface und wird deshalb als Implementierung gewertet – die Default-Version kommt gar nicht erst zum Zug. Eine Default-Implementierung ist immer nur die Rückfallebene für Klassen, die nichts Eigenes anbieten. Zwei weitere Dinge sollte man wissen: Default-Mitglieder verhalten sich sonst wie explizit implementierte, sind also nur über den Interface-Typ erreichbar. Und ein Interface bleibt zustandslos – es kann keine Felder anlegen, sondern nur mit den Mitgliedern arbeiten, die es selbst deklariert (oben `Name`).

Default-Implementierungen machen ein Interface nicht zur abstrakten Klasse. Sie sind ein Werkzeug für die Weiterentwicklung von Schnittstellen, kein Ersatz für eine Basisklasse mit gemeinsamem Code. Wer ein Interface mit fünf Default-Methoden schreibt, braucht vermutlich eine abstrakte Klasse.
{: .notice--warning}

## Interface-Mitglieder und Überschreiben

Ein Punkt, der gern übersehen wird: Eine Methode, die ein Interface implementiert, ist **nicht automatisch virtuell**. Beim Interface selbst ist der Aufruf polymorph – über `ISammelbar` landet man immer bei der Klasse, die das Interface implementiert. Aber wenn von dieser Klasse wiederum eine Unterklasse erbt und die Methode verfeinern will, gilt dieselbe Regel wie in [`virtual` und `override`](/modules/virtual_override/virtual_override.md). Deshalb steht in `Gegenstand` ein bewusstes `virtual`:

```csharp
public abstract class Gegenstand : StatischesObjekt, ISammelbar
{
    public virtual string Aufheben(Spieler spieler) => $"{spieler.Name} hebt {Name} auf.";
}

public sealed class Schatz : Gegenstand
{
    public int Wert { get; }
    public override char Symbol => '$';

    public override string Aufheben(Spieler spieler)
    {
        spieler.SchatzEinsammeln(this);
        return $"{spieler.Name} findet einen Schatz im Wert von {Wert}!";
    }
}
```

Ohne das `virtual` in `Gegenstand` gäbe es kein `override` in `Schatz` – bestenfalls ein `new`, mit dem Ergebnis, dass `ISammelbar s = new Schatz(pos, 100)` beim Aufruf von `s.Aufheben(spieler)` die farblose Standardmeldung von `Gegenstand` liefert und niemand Punkte bekommt. Wer Interface-Implementierungen in einer Klassenhierarchie weiter verfeinern will, markiert sie als `virtual` (oder `abstract`, wenn die Klasse selbst abstrakt ist).

## Ein Vertrag zwischen Schichten: `ILevelQuelle`

Zum Schluss ein Interface, das keine Fähigkeit von Spielobjekten beschreibt, sondern eine Grenze im Programm. Unser Spiel muss seine Level irgendwo herbekommen – vorerst fest im Code, später aus einer Textdatei und in Vorlesung 09 sogar über HTTP von einem Server. Der Spielkern soll davon nichts wissen. Also legt er per Interface fest, was eine Levelquelle können muss:

```csharp
/// <summary>Ein Level ist eine Karte aus Textzeilen plus ein Name.</summary>
public record Level(string Name, IReadOnlyList<string> Zeilen);

/// <summary>Woher die Level kommen, ist dem Spiel egal – Datei, Netz oder fest im Code.</summary>
public interface ILevelQuelle
{
    IReadOnlyList<string> LevelNamen { get; }
    Level Laden(string name);
}
```

Die einfachste Implementierung hält zwei Karten als Stringliste im Code – mehr können wir im Moment noch nicht, weil wir weder Dateien lesen noch JSON verarbeiten:

```csharp
public class EingebauteLevel : ILevelQuelle
{
    private static readonly Dictionary<string, string[]> level = new()
    {
        ["Kerker"] = new[]
        {
            "####################",
            "#@.....#...........#",
            "#......#.....W.....#",
            "#..k...#...........#",
            "#......D...........#",
            "#......#...V.......#",
            "#......#.......T...#",
            "#..!...#..........E#",
            "####################",
        },
        // "Katakomben" ...
    };

    public IReadOnlyList<string> LevelNamen => level.Keys.ToList();

    public Level Laden(string name)
    {
        if (!level.TryGetValue(name, out string[]? zeilen))
        {
            throw new KeyNotFoundException($"Es gibt kein Level namens '{name}'.");
        }
        return new Level(name, zeilen);
    }
}
```

Das Hauptprogramm arbeitet ausschließlich mit dem Interface-Typ – und genau deshalb lässt sich die Quelle später austauschen, ohne eine Zeile Spiellogik anzufassen:

```csharp
ILevelQuelle quelle = new EingebauteLevel();
Level level = quelle.Laden(quelle.LevelNamen[0]);
Spielfeld feld = LevelParser.Parsen(level);
```

In Vorlesung 04 zerlegen wir das Projekt genau an dieser Linie in Schichten: `ILevelQuelle` bleibt im Kern, die Implementierungen wandern in ein eigenes Projekt `Adventure.Daten`, und die Blazor-Oberfläche bekommt die passende Quelle per Dependency Injection hineingereicht. Dass das ohne Umbau funktioniert, liegt allein an diesem Interface. Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`).

Übung: Erweitere `ISammelbar` wie oben um ein Interface `ITragbar` mit `double Gewicht { get; }`, implementiere es in `Gegenstand` (Schlüssel 0,5 kg, Trank 0,3 kg, Schatz 2 kg) und schreibe eine Methode `double Traglast(Inventar<Gegenstand> inventar)`, die alle Gewichte summiert. Schreibe danach eine Klasse `ZaehlendeLevelQuelle : ILevelQuelle`, die eine andere `ILevelQuelle` im Konstruktor entgegennimmt, alles an sie weiterreicht und zusätzlich mitzählt, wie oft geladen wurde. Warum funktioniert das ohne jede Änderung am übrigen Programm?
{: .notice--info}

## Weitere Quellen

- [Explizite Schnittstellenimplementierung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/interfaces/explicit-interface-implementation)
- [Schnittstellen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/types/interfaces)
- [Schnittstellen mit Standardmethoden sicher aktualisieren – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/tutorials/default-interface-methods-versions) – Tutorial zum Mitprogrammieren: ein veröffentlichtes Interface wird Schritt für Schritt erweitert, ohne bestehende Implementierungen zu brechen.
- [Adapter – Refactoring Guru](https://refactoring.guru/design-patterns/adapter) – dasselbe Prinzip wie bei `ILevelQuelle`: Der Nutzer kennt nur den Vertrag, dahinter darf stecken, was will; mit [C#-Beispiel](https://refactoring.guru/design-patterns/adapter/csharp/example).
