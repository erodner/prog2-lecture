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

## Problem

Unsere Anwendung steuert eine Klimaanlage und arbeitet mit einem eigenen Interface für Temperaturquellen. Alles, was Temperaturen liefert, soll Grad Celsius zurückgeben:

```csharp
public interface ITemperaturQuelle
{
    double AktuelleTemperaturCelsius();
}

public class Klimaanlage
{
    private readonly ITemperaturQuelle quelle;

    public Klimaanlage(ITemperaturQuelle quelle)
    {
        this.quelle = quelle;
    }

    public void Regeln()
    {
        if (quelle.AktuelleTemperaturCelsius() > 24.0)
            Console.WriteLine("Kühlung an");
    }
}
```

Nun sollen die Sensoren eines Herstellers angebunden werden, der eine fertige Bibliothek mitliefert. Die darin enthaltene Klasse misst korrekt, kennt unser Interface aber nicht und rechnet in Fahrenheit:

```csharp
// Fremde Bibliothek – wir haben keinen Zugriff auf den Quellcode.
public class TemperaturSensorLib
{
    public double LeseFahrenheit() => 75.2;
}
```

`new Klimaanlage(new TemperaturSensorLib())` kompiliert nicht, weil `TemperaturSensorLib` kein `ITemperaturQuelle` ist. Die Bibliothek können wir nicht ändern, und die Klimaanlage soll weiterhin nur mit unserem Interface arbeiten – sonst müssten wir für jeden weiteren Hersteller wieder die Klimaanlage anfassen.

## Lösung

Der Adapter ist eine dritte Klasse, die zwischen beiden vermittelt. Vier Rollen sind beteiligt:

- **Ziel** (*Target*): die Schnittstelle, die der Client erwartet – hier `ITemperaturQuelle`.
- **Client**: der Code, der nur das Ziel kennt – die `Klimaanlage`.
- **Adaptierte Klasse** (*Adaptee*): die vorhandene Klasse mit der unpassenden Schnittstelle – `TemperaturSensorLib`.
- **Adapter**: implementiert das Ziel und leitet jeden Aufruf an die adaptierte Klasse weiter, wobei er Parameter und Rückgabewerte umrechnet.

Für die Verbindung zwischen Adapter und adaptierter Klasse gibt es zwei Möglichkeiten, und genau daran unterscheiden sich die beiden Varianten des Musters.

### Variante 1: Klassenadapter (Vererbung)

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

Klimaanlage anlage = new(new SensorKlassenAdapter());
anlage.Regeln(); // Kühlung an   (75,2 °F sind 24 °C)
```

Die Klimaanlage bekommt eine `ITemperaturQuelle` und ist zufrieden. Dass dahinter ein Fahrenheit-Sensor steckt, sieht sie nicht. Der Klassenadapter ist knapp – aber er funktioniert nur, weil `TemperaturSensorLib` nicht [`sealed`](/modules/sealed/sealed.md) ist und weil C# eine Basisklasse plus beliebig viele Interfaces erlaubt. Wäre `ITemperaturQuelle` eine abstrakte Klasse, ginge diese Variante gar nicht.

### Variante 2: Objektadapter (Delegation)

Der Objektadapter *hat* einen Sensor: Er implementiert nur das Ziel-Interface und hält die adaptierte Klasse als Feld. Jede Interface-Methode delegiert an dieses Objekt:

```csharp
public class SensorObjektAdapter : ITemperaturQuelle
{
    private readonly TemperaturSensorLib sensor;

    public SensorObjektAdapter(TemperaturSensorLib sensor)
    {
        this.sensor = sensor;
    }

    public double AktuelleTemperaturCelsius()
    {
        return (sensor.LeseFahrenheit() - 32) * 5 / 9;
    }
}

TemperaturSensorLib sensor = new();
Klimaanlage anlage = new(new SensorObjektAdapter(sensor));
anlage.Regeln(); // Kühlung an
```

Der Unterschied ist die Stelle, an der der Sensor entsteht: Beim Klassenadapter steckt er im Adapter selbst, beim Objektadapter wird er von außen hineingereicht. Das ist dieselbe Entscheidung wie im Modul [Schnittstellen- vs. Implementierungsvererbung](/modules/schnittstellen_vs_implementierungsvererbung/schnittstellen_vs_implementierungsvererbung.md): „ist ein“ gegen „hat ein“.

## Klassenadapter oder Objektadapter?

| | Klassenadapter | Objektadapter |
| :--- | :--- | :--- |
| Verbindung | Vererbung | Komposition (Feld) |
| Adaptiert | genau diese eine Klasse | die Klasse *und alle ihre Unterklassen* |
| Geht bei `sealed`-Klassen | nein | ja |
| Zugriff auf `protected`-Mitglieder | ja, kann sogar überschreiben | nein, nur die öffentliche Schnittstelle |
| Objekte zur Laufzeit | eines | zwei (Adapter + Adaptee) |
| Adaptee austauschbar | nein, fest eingebrannt | ja, per Konstruktor |

Im Zweifel: Objektadapter. Er kommt ohne Vererbung aus, funktioniert mit versiegelten Klassen und mit Unterklassen des Adaptees, und man kann dem Adapter im Test ein vorbereitetes Objekt hineinreichen. Der Klassenadapter lohnt sich nur, wenn man wirklich Methoden der adaptierten Klasse überschreiben muss.
{: .notice--primary}

## Beispiel in .NET

Das bekannteste Adapter-Paar in .NET sind `Stream` und `StreamReader`. Ein `Stream` – egal ob Datei, Netzwerkverbindung oder Speicherbereich – liefert rohe Bytes über `Read(byte[] ...)`. Wer Text lesen will, braucht aber Zeichen und Zeilen. `StreamReader` ist ein Objektadapter: Er bekommt im Konstruktor einen `Stream`, implementiert die Schnittstelle `TextReader` mit `ReadLine()` und `ReadToEnd()` und erledigt dazwischen die Umrechnung von Bytes in Zeichen nach der angegebenen Kodierung:

```csharp
using Stream roh = File.OpenRead("messwerte.txt");     // liefert Bytes
using StreamReader reader = new(roh);                  // adaptiert zu Text
string? ersteZeile = reader.ReadLine();
```

Dass der Adapter mit *jedem* `Stream` funktioniert, nicht nur mit `FileStream`, ist genau der Vorteil des Objektadapters aus der Tabelle. Mehr zu Streams kommt in Vorlesung 09.

## Vor- und Nachteile

Der Adapter macht zwei Klassen kompatibel, die nichts voneinander wissen, und hält den Client sauber: Die `Klimaanlage` kennt nur `ITemperaturQuelle`, egal wie viele Hersteller später hinzukommen. Weil die Umrechnung an einer Stelle gebündelt ist, lässt sie sich dort auch erweitern – etwa um Messwerte zu glätten oder unplausible Werte auszufiltern – und ein Adapter lässt sich leicht durch einen anderen ersetzen.

Dem stehen zwei Kosten gegenüber. Jeder Aufruf geht durch eine zusätzliche Schicht, und wenn dabei Daten kopiert oder umgewandelt werden müssen, kostet das Zeit – bei einem Sensorwert egal, bei Millionen Datensätzen nicht. Und der Adapter selbst ist kaum wiederverwendbar, weil er genau *dieses* Ziel mit genau *dieser* Klasse verbindet.

Ein Adapter soll Schnittstellen angleichen, nicht Verhalten hinzufügen. Wer im Adapter anfängt, Werte zu cachen, Fehler zu verschlucken oder Geschäftslogik einzubauen, hat eine Klasse geschrieben, die zwei Aufgaben hat – und die zweite versteckt sich hinter dem harmlosen Namen „Adapter“.
{: .notice--warning}

Übung: Der Geometrieeditor soll Figuren aus einer alten Bibliothek anzeigen, deren Klasse `AltesRechteck` die Ecken als `int Links, Oben, Rechts, Unten` speichert und keine Fläche kennt. Schreibe einen Objektadapter `AltesRechteckAdapter : Figur`, der `Flaeche`, `Umfang` und die Position aus diesen Werten berechnet. Warum ist hier ein Klassenadapter unmöglich?
{: .notice--info}

## Weitere Quellen

- [`StreamReader`-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.streamreader)
- [`Stream`-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.io.stream)
- [Adapter – Refactoring.Guru](https://refactoring.guru/de/design-patterns/adapter/csharp/example)
