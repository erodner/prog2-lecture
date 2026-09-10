---
title: "Schnittstellen- vs. Implementierungsvererbung"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Abstrakte Klassen und Interfaces sehen sich auf den ersten Blick ähnlich: Beide geben vor, was eine Klasse können muss, beide erlauben polymorphe Aufrufe, und beide lassen sich nicht instanziieren. Trotzdem stehst du bei jedem Entwurf vor der Frage, welches von beiden du nimmst – und eine falsche Entscheidung ist später nur mühsam zu korrigieren. Dieses Modul stellt die beiden Formen der Vererbung gegenüber und gibt dir eine Entscheidungshilfe an die Hand, die in den meisten Fällen trägt.

## Zwei Arten von Vererbung

Was wir in der [Vererbung](/modules/vererbung_grundlagen/vererbung_grundlagen.md) und bei [abstrakten Klassen](/modules/abstrakte_klassen/abstrakte_klassen.md) gesehen haben, heißt **Implementierungsvererbung**: Die Unterklasse übernimmt Code – Felder, Properties, fertige Methoden – und passt ihn punktuell mit `override` an. Sie erbt eine Implementierung.

Ein Interface dagegen vererbt nur eine Schnittstelle, deshalb **Schnittstellenvererbung**: Die implementierende Klasse übernimmt Signaturen, keinen Code (von Default-Methoden abgesehen). Sie verspricht, etwas zu können, und muss selbst dafür sorgen.

| | Abstrakte Klasse | Interface |
| :--- | :--- | :--- |
| Zustand (Felder) | ja | nein |
| Konstruktoren | ja, für Unterklassen über `base(...)` | nein |
| Fertige Implementierung | ja, beliebig viel | nur Default-Methoden (C# 8+) |
| Mehrfach kombinierbar | nein, genau eine Basisklasse | ja, beliebig viele |
| Sichtbarkeit der Mitglieder | frei wählbar (`protected` möglich) | implizit `public` |
| Polymorphie | über `abstract`/`virtual` und `override` | jede Implementierung ist über den Interface-Typ polymorph |
| Nachträglich erweitern | neue `virtual`-Methode bricht nichts | neues Mitglied bricht alle Implementierungen (außer mit Default) |
| Beziehung | „ist ein“ | „kann das“ |

Die letzte Zeile ist die wichtigste. `Rechteck : Figur` sagt: Ein Rechteck *ist eine* Figur, in jeder Hinsicht. `Kaffee : IHatKoffein` sagt: Kaffee *kann* koffeinhaltig sein – neben vielen anderen Dingen, die er auch ist.

## Entscheidungshilfe

Die Frage „abstrakte Klasse oder Interface?“ lässt sich meist mit drei Gegenfragen beantworten.

**Gibt es gemeinsamen Code oder gemeinsamen Zustand?** `Figur` hat `Name`, `X`, `Y`, einen Konstruktor, `Verschieben` und `Beschreibung()` – das ist eine Menge Substanz, die alle Figuren teilen. Ein Interface könnte das alles nur fordern, nicht liefern; jede Figur müsste `Verschieben` selbst schreiben. Hier ist die abstrakte Klasse richtig.

**Beschreibst du eine Fähigkeit, die quer durch eine Hierarchie auftaucht?** Vergleichbarkeit, Speicherbarkeit, Koffeingehalt – solche Eigenschaften haben Objekte, die sonst nichts miteinander zu tun haben. Ein `Kaffee` und eine `Cola`, eine `Figur` und ein `Bankkonto`. Dafür kann es keine gemeinsame Basisklasse geben, also Interface.

**Brauchen die Nutzer der Abstraktion die Implementierung überhaupt?** `FigurenVerwaltung` will nur speichern und laden. Was der Speicher intern tut, ist ihr egal – und es soll ihr egal sein, damit man ihn austauschen kann. `IFigurSpeicher` als Interface hält die Verwaltung frei von jedem Wissen über Dateien. Eine abstrakte Klasse `FigurSpeicher` hätte nichts Gemeinsames zu bieten und würde jeder Implementierung ihre einzige Basisklasse wegnehmen.

Im Zweifel Interface. Ein Interface verpflichtet zu nichts außer der Schnittstelle, lässt jeder Klasse ihre Basisklasse frei und lässt sich später immer noch um eine abstrakte Hilfsklasse ergänzen, die Teile davon implementiert. Umgekehrt ist es schwieriger: Wer sich früh auf eine abstrakte Basisklasse festlegt, blockiert die Vererbungslinie aller Unterklassen. In der .NET-Bibliothek ist dieses Muster überall zu sehen – `IEnumerable<T>` als Vertrag, dazu Klassen wie `List<T>`, die ihn erfüllen.
{: .notice--primary}

## Beides zusammen

Die beiden Konstrukte schließen sich nicht aus, im Gegenteil: Ein typischer Entwurf definiert zuerst ein Interface als Vertrag und bietet dann eine abstrakte Klasse an, die den mühsamen Teil davon schon erledigt. Wer die Basisklasse nutzen kann, spart Arbeit; wer eine andere Basisklasse braucht, implementiert das Interface direkt.

```csharp
interface IFigur
{
    string Name { get; }
    double Flaeche { get; }
    void Verschieben(double dx, double dy);
}

abstract class Figur : IFigur
{
    public string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    protected Figur(string name, double x, double y) { Name = name; X = x; Y = y; }

    public abstract double Flaeche { get; }
    public void Verschieben(double dx, double dy) { X += dx; Y += dy; }
}

class Rechteck : Figur { /* wie bisher */ }

class Bildfigur : IFigur   // braucht eine andere Basisklasse, erfüllt aber den Vertrag
{
    // Name, Flaeche, Verschieben selbst implementiert
}
```

Der Rest des Programms arbeitet mit `IFigur` und merkt nicht, welche Objekte über die bequeme Basisklasse und welche direkt kommen. Für den Geometrieeditor haben wir uns diese Zwischenschicht gespart, weil alle Figuren problemlos von `Figur` erben können – aber sobald das nicht mehr gilt, ist das Interface der Ausweg.

## Komposition statt Vererbung

Eine dritte Möglichkeit wird gern vergessen: gar nicht erben, sondern **enthalten**. Wenn ein `Kaffee` eine Temperatur verwalten soll, muss er dafür nicht von einer Klasse `Heissgetraenk` erben – er kann ein Objekt `Waermespeicher` als Feld halten und die Arbeit delegieren:

```csharp
class Waermespeicher
{
    public int Temperatur { get; set; }
    public void Abkuehlen(int grad) => Temperatur -= grad;
}

class Kaffee : IHeissgetraenk
{
    private readonly Waermespeicher waerme = new();

    public int Temperatur
    {
        get => waerme.Temperatur;
        set => waerme.Temperatur = value;
    }

    public void Abkuehlen(int grad) => waerme.Abkuehlen(grad);
}
```

Nach außen erfüllt `Kaffee` das Interface, innen nutzt er fertige Logik – ohne seine einzige Basisklasse zu verbrauchen. Dieses Prinzip „Komposition vor Vererbung“ bevorzugen viele Entwickler, weil eine Vererbungsbeziehung schwer zu ändern ist, ein Feld dagegen jederzeit gegen ein anderes Objekt getauscht werden kann. Genau so arbeitet auch `FigurenVerwaltung`: Sie *hat* einen `IFigurSpeicher`, statt einer zu sein. In Vorlesung 07 begegnen uns mit Adapter und Composite Entwurfsmuster, die ganz auf Komposition beruhen.

Übung: Entscheide für jede Situation, ob abstrakte Klasse, Interface oder Komposition passt, und begründe: (a) Fahrzeuge `Pkw`, `Lkw`, `Motorrad` mit Kennzeichen, Kilometerstand und einer je nach Typ verschiedenen Mautberechnung; (b) Objekte, die sich als Text protokollieren lassen – Figuren, Konten, Benutzer; (c) ein `Roboter`, der mit verschiedenen Greifern ausgestattet werden kann; (d) Sensoren `Temperatursensor` und `Drucksensor`, die alle einen Messwert liefern, einen Namen haben und im Fehlerfall gleich reagieren sollen.
{: .notice--info}

## Weitere Quellen

- [Vererbung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/object-oriented/inheritance)
- [Schnittstellen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/types/interfaces)
- [Objektorientierte Programmierung in C# – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/tutorials/oop)
