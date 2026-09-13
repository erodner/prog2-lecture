---
title: "Adapter"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Wer mit einem deutschen Ladegerät nach England reist, steht vor einem bekannten Problem: Das Gerät funktioniert einwandfrei, die Steckdose auch – nur passen die beiden nicht zusammen. Niemand käme auf die Idee, deshalb das Ladegerät umzubauen oder die Steckdose aus der Wand zu reißen. Man kauft einen Reiseadapter. In der Softwareentwicklung passiert dasselbe ständig: Eine fertige Klasse – aus einer Bibliothek, von einem anderen Team, aus einem alten Projekt – liefert genau die Daten, die wir brauchen, aber über eine Schnittstelle, die nicht zu unserem Code passt. Das **Adapter**-Muster (Strukturmuster, auch *Wrapper* genannt) löst das, ohne dass eine der beiden Seiten geändert werden muss.

## Problem: zwei Oberflächen, dieselben vier Richtungen

Das Adventure hat eine einzige Stelle, an der eine Bewegung stattfindet: `feld.SpielerZieht(richtung)`. Der Kern kennt nur den Aufzählungstyp `Richtung` – und das ist gut so, denn er soll weder von der Konsole noch vom Browser etwas wissen. Trotzdem steht in beiden Oberflächen fast derselbe Code. In `Adventure.Konsole` kommt die Eingabe als `ConsoleKey`:

```csharp
ConsoleKey taste = Console.ReadKey(true).Key;

Richtung? richtung = taste switch
{
    ConsoleKey.W or ConsoleKey.UpArrow => Richtung.Oben,
    ConsoleKey.S or ConsoleKey.DownArrow => Richtung.Unten,
    ConsoleKey.A or ConsoleKey.LeftArrow => Richtung.Links,
    ConsoleKey.D or ConsoleKey.RightArrow => Richtung.Rechts,
    _ => null
};
```

In `Adventure.Web` liefert der Browser ein `KeyboardEventArgs`, dessen Property `Key` ein `string` ist:

```csharp
private void TasteGedrueckt(KeyboardEventArgs e)
{
    Richtung? richtung = e.Key switch
    {
        "ArrowUp" or "w" or "W" => Richtung.Oben,
        "ArrowDown" or "s" or "S" => Richtung.Unten,
        "ArrowLeft" or "a" or "A" => Richtung.Links,
        "ArrowRight" or "d" or "D" => Richtung.Rechts,
        _ => null
    };
}
```

Zwei fremde Schnittstellen – `ConsoleKey` gehört zu .NET, `KeyboardEventArgs` zu ASP.NET Core –, und beide müssen dasselbe liefern. Ändern können wir keine von beiden. Und ein Unit-Test, der eine Folge von Zügen abspielen will, hat gar keine Tastatur zur Verfügung. Was fehlt, ist eine eigene Schnittstelle, auf die sich das Spiel stützen kann.

## Lösung

Der Adapter ist eine dritte Klasse, die zwischen beiden Seiten vermittelt. Vier Rollen sind beteiligt:

- **Ziel** (*Target*): die Schnittstelle, die der Client erwartet – die schreiben wir selbst.
- **Client**: der Code, der nur das Ziel kennt – die Spielschleife.
- **Adaptierte Klasse** (*Adaptee*): die vorhandene Klasse mit der unpassenden Schnittstelle – `Console` bzw. `KeyboardEventArgs`.
- **Adapter**: implementiert das Ziel und leitet jeden Aufruf an die adaptierte Klasse weiter, wobei er Parameter und Rückgabewerte umrechnet.

Das Ziel ist ein winziges Interface. Es beschreibt genau das, was die Spielschleife braucht, und nichts sonst:

```csharp
public interface IEingabe
{
    /// <summary>Liefert den nächsten Zug oder null, wenn gerade keiner vorliegt.</summary>
    Richtung? NaechsteRichtung();
}
```

### Variante 1: Objektadapter (Delegation)

Der Objektadapter *hat* seine Quelle: Er implementiert nur das Ziel-Interface und hält die adaptierte Klasse als Feld. `KonsolenEingabe` übersetzt Tastencodes – die Zuordnungstabelle ist dieselbe Delegat-freie Variante, die wir in den [Aufgaben zu Vorlesung 07](/modules/aufgaben_delegates_events/aufgaben_delegates_events.md) entwickelt haben:

```csharp
public sealed class KonsolenEingabe : IEingabe
{
    private static readonly Dictionary<ConsoleKey, Richtung> belegung = new()
    {
        [ConsoleKey.W] = Richtung.Oben,    [ConsoleKey.UpArrow] = Richtung.Oben,
        [ConsoleKey.S] = Richtung.Unten,   [ConsoleKey.DownArrow] = Richtung.Unten,
        [ConsoleKey.A] = Richtung.Links,   [ConsoleKey.LeftArrow] = Richtung.Links,
        [ConsoleKey.D] = Richtung.Rechts,  [ConsoleKey.RightArrow] = Richtung.Rechts,
    };

    public Richtung? NaechsteRichtung()
    {
        ConsoleKey taste = Console.ReadKey(true).Key;
        return belegung.TryGetValue(taste, out Richtung r) ? r : null;
    }
}
```

Der zweite Adapter hat gar keine Tastatur. `SkriptEingabe` bekommt eine fertige Liste von Richtungen hineingereicht und gibt sie der Reihe nach aus – das ist der Objektadapter in Reinform, mit dem Adaptee als Feld:

```csharp
public sealed class SkriptEingabe : IEingabe
{
    private readonly IReadOnlyList<Richtung> zuege;
    private int naechster;

    public SkriptEingabe(params Richtung[] zuege)
    {
        this.zuege = zuege;
    }

    public Richtung? NaechsteRichtung()
    {
        return naechster < zuege.Count ? zuege[naechster++] : null;
    }
}
```

Die Spielschleife kennt jetzt nur noch `IEingabe` und ist damit von der Konsole befreit:

```csharp
static void Spielen(Spielfeld feld, IEingabe eingabe)
{
    while (feld.Status == Spielstatus.Laeuft && eingabe.NaechsteRichtung() is Richtung r)
    {
        feld.SpielerZieht(r);
    }
}

Spielen(feld, new KonsolenEingabe());
Spielen(feld, new SkriptEingabe(Richtung.Rechts, Richtung.Rechts, Richtung.Unten));
```

Derselbe Aufruf, einmal mit Mensch und einmal ohne. Der zweite reicht für einen Test in [Vorlesung 12](/lectures/12/12.md): drei Züge abspielen, danach `feld.Spieler.Position` prüfen – reproduzierbar, in Millisekunden, ohne Tastendruck.

Das Interface `IEingabe` beschreibt ein *Holen* (*pull*): Der Client fragt nach dem nächsten Zug und wartet. Für die Konsole passt das perfekt, weil `Console.ReadKey` genau das tut. Blazor funktioniert umgekehrt: Der Browser *liefert* ein Tastenereignis, wann immer es ihm passt (*push*), und niemand darf darauf warten. Ein Adapter kann Schnittstellen angleichen, aber nicht die Richtung des Kontrollflusses umdrehen – für Blazor ist die Übersetzung von `e.Key` in `Richtung` daher eine eigene, kleine Adapter-Methode, und ein `IEingabe` würde dort nicht helfen.
{: .notice--warning}

### Variante 2: Klassenadapter (Vererbung)

Für die zweite Variante brauchen wir eine adaptierte *Klasse*, von der man erben kann. Nehmen wir einen fremden Temperatursensor, den eine Klimaanlage über unser Interface `ITemperaturQuelle` ansprechen soll:

```csharp
public interface ITemperaturQuelle
{
    double AktuelleTemperaturCelsius();
}

// Fremde Bibliothek – wir haben keinen Zugriff auf den Quellcode.
public class TemperaturSensorLib
{
    public double LeseFahrenheit() => 75.2;
}
```

Der Klassenadapter *ist* ein Sensor und *kann sich* als Temperaturquelle ausgeben: Er erbt von der adaptierten Klasse und implementiert zusätzlich das Ziel-Interface. In der Interface-Methode ruft er die geerbte Methode auf und rechnet um:

```csharp
public class SensorKlassenAdapter : TemperaturSensorLib, ITemperaturQuelle
{
    public double AktuelleTemperaturCelsius()
    {
        double fahrenheit = LeseFahrenheit();      // geerbt
        return (fahrenheit - 32) * 5 / 9;
    }
}
```

Der Objektadapter hätte den Sensor stattdessen als Feld gehalten und im Konstruktor bekommen. Der Unterschied ist die Stelle, an der die adaptierte Klasse entsteht: beim Klassenadapter fest im Adapter, beim Objektadapter von außen hineingereicht. Das ist dieselbe Entscheidung wie im Modul [Schnittstellen- vs. Implementierungsvererbung](/modules/schnittstellen_vs_implementierungsvererbung/schnittstellen_vs_implementierungsvererbung.md): „ist ein“ gegen „hat ein“. Der Klassenadapter funktioniert außerdem nur, weil `TemperaturSensorLib` nicht [`sealed`](/modules/sealed/sealed.md) ist und weil C# eine Basisklasse plus beliebig viele Interfaces erlaubt.

## Klassenadapter oder Objektadapter?

| | Klassenadapter | Objektadapter |
| :--- | :--- | :--- |
| Verbindung | Vererbung | Komposition (Feld) |
| Adaptiert | genau diese eine Klasse | die Klasse *und alle ihre Unterklassen* |
| Geht bei `sealed`-Klassen | nein | ja |
| Zugriff auf `protected`-Mitglieder | ja, kann sogar überschreiben | nein, nur die öffentliche Schnittstelle |
| Objekte zur Laufzeit | eines | zwei (Adapter + Adaptee) |
| Adaptee austauschbar | nein, fest eingebrannt | ja, per Konstruktor |

Im Zweifel: Objektadapter. Er kommt ohne Vererbung aus, funktioniert mit versiegelten Klassen und mit Unterklassen des Adaptees, und man kann dem Adapter im Test ein vorbereitetes Objekt hineinreichen – genau das macht `SkriptEingabe`. Der Klassenadapter lohnt sich nur, wenn man wirklich Methoden der adaptierten Klasse überschreiben muss.
{: .notice--primary}

## Beispiel in .NET

Das bekannteste Adapter-Paar in .NET sind `Stream` und `StreamReader`. Ein `Stream` – egal ob Datei, Netzwerkverbindung oder Speicherbereich – liefert rohe Bytes über `Read(byte[] ...)`. Wer Text lesen will, braucht aber Zeichen und Zeilen. `StreamReader` ist ein Objektadapter: Er bekommt im Konstruktor einen `Stream`, implementiert die Schnittstelle `TextReader` mit `ReadLine()` und `ReadToEnd()` und erledigt dazwischen die Umrechnung von Bytes in Zeichen nach der angegebenen Kodierung. Genau so liest `TextdateiLevelQuelle` in Vorlesung 09 die Levelkarten ein:

```csharp
using Stream roh = File.OpenRead("labyrinth.txt");     // liefert Bytes
using StreamReader reader = new(roh);                  // adaptiert zu Text
string? ersteZeile = reader.ReadLine();
```

Dass der Adapter mit *jedem* `Stream` funktioniert, nicht nur mit `FileStream`, ist genau der Vorteil des Objektadapters aus der Tabelle.

## Vor- und Nachteile

Der Adapter macht zwei Seiten kompatibel, die nichts voneinander wissen, und hält den Client sauber: Die Spielschleife kennt nur `IEingabe`, egal ob dahinter eine Tastatur, ein Testskript oder später ein Netzwerkclient steckt. Weil die Übersetzung an einer Stelle gebündelt ist, lässt sie sich dort auch erweitern – etwa um eine zweite Tastenbelegung – und ein Adapter lässt sich leicht durch einen anderen ersetzen.

Dem stehen zwei Kosten gegenüber. Jeder Aufruf geht durch eine zusätzliche Schicht, und wenn dabei Daten kopiert oder umgewandelt werden müssen, kostet das Zeit – bei einem Tastendruck egal, bei Millionen Datensätzen nicht. Und der Adapter selbst ist kaum wiederverwendbar, weil er genau *dieses* Ziel mit genau *dieser* Klasse verbindet.

Ein Adapter soll Schnittstellen angleichen, nicht Verhalten hinzufügen. Wer in `KonsolenEingabe` anfängt, Züge zu puffern, ungültige Eingaben zu zählen oder Spielregeln zu prüfen, hat eine Klasse geschrieben, die zwei Aufgaben hat – und die zweite versteckt sich hinter dem harmlosen Namen „Eingabe“.
{: .notice--warning}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`).

Übung: Schreibe einen dritten Adapter `ZufallsEingabe : IEingabe`, der bei jedem Aufruf eine zufällige Richtung liefert, und lass damit hundert Runden laufen – wie oft gewinnt der Held? Erweitere anschließend `IEingabe` um eine Property `bool IstAmEnde`, damit `Spielen` zwischen „kein Zug mehr vorhanden“ und „Zug war keine Bewegungstaste“ unterscheiden kann. Warum wäre für `Adventure.Web` ein Klassenadapter auf `KeyboardEventArgs` keine Lösung?
{: .notice--info}

## Weitere Quellen

- [`StreamReader`-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.streamreader)
- [`Stream`-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.stream)
- [Adapter – Refactoring.Guru](https://refactoring.guru/design-patterns/adapter) – Problem, Rollen und Diagramm des Musters, mit Beispielen aus der Praxis.
- [Adapter in C# – Refactoring.Guru](https://refactoring.guru/design-patterns/adapter/csharp/example) – ein vollständiges, lauffähiges C#-Beispiel zum Nachbauen.
