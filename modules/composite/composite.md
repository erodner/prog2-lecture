---
title: "Composite"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ein Ordner auf der Festplatte enthält Dateien – und andere Ordner, die wiederum Dateien und Ordner enthalten. Trotzdem kannst du einen Ordner genauso verschieben, umbenennen oder löschen wie eine einzelne Datei, und der Explorer fragt nicht nach, ob es sich um ein Blatt oder einen Ast handelt. Im Zeichenprogramm markierst du drei Figuren, gruppierst sie, und ab dann verhält sich die Gruppe wie eine einzige Figur: Sie lässt sich verschieben, skalieren, erneut gruppieren. Dieses Prinzip – **Einzelobjekte und Gruppen von Objekten über dieselbe Schnittstelle behandeln** – heißt **Composite** (Kompositum, Strukturmuster). Es ist das Muster hinter jeder Baumstruktur, die uns in der Praxis begegnet: Dateisysteme, Menüs mit Untermenüs, Organigramme, GUI-Oberflächen.

## Problem

Wir wollen Grafiken zeichnen und verschieben. Es gibt einfache Grafiken wie eine Ellipse und zusammengesetzte Grafiken, die aus mehreren Teilen bestehen. Ohne Muster müsste der Client bei jeder Operation unterscheiden: Ist das eine Ellipse? Dann zeichne sie. Ist das eine Gruppe? Dann gehe die Liste durch – und für jedes Element wieder dieselbe Frage, denn eine Gruppe kann Gruppen enthalten. Die Fallunterscheidung wiederholt sich in jeder Operation, und jede neue Grafikart macht sie länger.

## Lösung

Das Composite-Muster löst das mit drei Rollen:

- **Komponente** (*Component*): eine gemeinsame Schnittstelle mit den Operationen, die für *alle* Knoten sinnvoll sind – Zeichnen, Verschieben.
- **Blatt** (*Leaf*): ein einfaches Objekt ohne Kinder, das die Operationen direkt ausführt.
- **Kompositum** (*Composite*): ein Objekt, das eine Liste von Komponenten hält und jede Operation an seine Kinder weiterreicht. Da die Kinder selbst Komposita sein können, entsteht daraus eine **rekursive Baumstruktur**.

Der Client sieht nur die Komponente und muss nie wissen, ob er es mit einem Blatt oder einem ganzen Teilbaum zu tun hat. In C# ist die Komponente am besten ein Interface, wie wir es im Modul [Interfaces](/modules/interfaces_grundlagen/interfaces_grundlagen.md) kennengelernt haben:

```csharp
public interface IGrafik
{
    void Zeichnen();
    void Verschieben(double dx, double dy);
}
```

Das Blatt ist eine gewöhnliche Klasse, die das Interface für sich selbst erfüllt:

```csharp
public class Ellipse : IGrafik
{
    public double X { get; private set; }
    public double Y { get; private set; }

    public Ellipse(double x, double y) { X = x; Y = y; }

    public void Zeichnen() => Console.WriteLine($"Ellipse bei ({X}, {Y})");

    public void Verschieben(double dx, double dy) { X += dx; Y += dy; }
}
```

Das Kompositum implementiert *dasselbe* Interface, tut aber selbst nichts außer Weiterreichen. Es hält eine `List<IGrafik>` – nicht `List<Ellipse>`, denn nur so passen auch andere Komposita hinein:

```csharp
public class ZusammengesetzteGrafik : IGrafik
{
    private readonly List<IGrafik> teile = new();

    public void Hinzufuegen(IGrafik grafik) => teile.Add(grafik);
    public void Entfernen(IGrafik grafik) => teile.Remove(grafik);

    public void Zeichnen()
    {
        foreach (IGrafik teil in teile)
            teil.Zeichnen();
    }

    public void Verschieben(double dx, double dy)
    {
        foreach (IGrafik teil in teile)
            teil.Verschieben(dx, dy);
    }
}
```

`Zeichnen` und `Verschieben` sind hier rekursiv, ohne dass es so aussieht: Ist ein `teil` selbst eine `ZusammengesetzteGrafik`, ruft `teil.Zeichnen()` wieder diese Methode auf, eine Ebene tiefer. Die Rekursion endet automatisch bei den Blättern. Der Client baut einen Baum und arbeitet nur mit der Wurzel:

```csharp
ZusammengesetzteGrafik bild = new();
ZusammengesetzteGrafik gruppe = new();
gruppe.Hinzufuegen(new Ellipse(1, 1));
gruppe.Hinzufuegen(new Ellipse(2, 2));

bild.Hinzufuegen(new Ellipse(0, 0));
bild.Hinzufuegen(gruppe);

bild.Verschieben(10, 0);
bild.Zeichnen();
// Ellipse bei (10, 0)
// Ellipse bei (11, 1)
// Ellipse bei (12, 2)
```

Ein einziger Aufruf `bild.Verschieben(10, 0)` hat drei Ellipsen auf zwei Ebenen verschoben. `bild` weiß nicht, wie tief der Baum ist – und muss es auch nicht wissen.

Beachte, dass `Hinzufuegen` und `Entfernen` nur im Kompositum stehen, nicht im Interface. Die Gang of Four hat sie in die gemeinsame Basisklasse gelegt, damit der Client wirklich *alles* einheitlich behandeln kann. Der Preis: Eine Ellipse müsste `Hinzufuegen` dann auch anbieten und beim Aufruf eine Exception werfen – ein Untertyp, der Methoden seines Obertyps verweigert, verstößt gegen das *Liskov-Substitutionsprinzip*, das uns beim Quadrat-Rechteck-Problem in den [Aufgaben zu Vorlesung 02](/modules/aufgaben_abstrakt_interfaces/aufgaben_abstrakt_interfaces.md) begegnet ist. Im Zweifel: Ins Interface gehört nur, was *jeder* Knoten sinnvoll kann.
{: .notice--primary}

## Zweites Beispiel: E-Mail-Empfänger

Das Muster ist nicht auf Grafik beschränkt. Eine E-Mail geht an eine Person – oder an eine Verteilerliste, die Personen und weitere Verteilerlisten enthält. Für den Absender soll beides ein *Empfänger* sein:

```csharp
public interface IEmpfaenger
{
    void Zustellen(string betreff);
}

public class Person : IEmpfaenger
{
    private readonly string adresse;
    public Person(string adresse) => this.adresse = adresse;
    public void Zustellen(string betreff) => Console.WriteLine($"{adresse}: {betreff}");
}

public class Verteilerliste : IEmpfaenger
{
    private readonly List<IEmpfaenger> mitglieder = new();
    public void Aufnehmen(IEmpfaenger e) => mitglieder.Add(e);

    public void Zustellen(string betreff)
    {
        foreach (IEmpfaenger m in mitglieder)
            m.Zustellen(betreff);
    }
}
```

Die Struktur ist Zeile für Zeile dieselbe wie bei den Grafiken – nur die Namen sind andere. Genau das ist gemeint, wenn ein Muster als *wiederverwendbares Schema* bezeichnet wird: Man erkennt es am Aufbau, nicht an der Anwendung. Eine Verteilerliste `alle` mit den Unterlisten `studierende` und `lehrende` stellt mit einem Aufruf `alle.Zustellen("Prüfungstermine")` jedem einzelnen Mitglied zu.

## Composite im Geometrieeditor

Im Geometrieeditor aus Vorlesung 03 ist `Figur` die abstrakte Basisklasse, `Rechteck`, `Kreis` und `Dreieck` sind die Blätter. Was fehlt, ist das Kompositum – eine `Figurengruppe`, die selbst eine `Figur` ist:

```csharp
public class Figurengruppe : Figur
{
    private readonly List<Figur> teile = new();

    public Figurengruppe(string name) : base(name, 0, 0) { }

    public void Hinzufuegen(Figur figur) => teile.Add(figur);

    public override double Flaeche => teile.Sum(f => f.Flaeche);
    public override double Umfang => teile.Sum(f => f.Umfang);
}
```

`Flaeche` und `Umfang` sind in `Figur` abstrakt (siehe Modul [Abstrakte Mitglieder](/modules/abstrakte_mitglieder/abstrakte_mitglieder.md)), also überschreiben wir sie mit der Summe der Teile. Beim Verschieben ist Vorsicht geboten: Das geerbte `Figur.Verschieben` ändert nur `X` und `Y` der Gruppe – die enthaltenen Figuren blieben, wo sie sind. Deshalb ist die Methode in `Figur` als `virtual` freigegeben, und die Gruppe überschreibt sie, um das Verschieben an ihre Teile weiterzureichen – genau wie im Modul [`virtual` und `override`](/modules/virtual_override/virtual_override.md) gesehen. Danach kann `FigurenVerwaltung` eine Gruppe wie jede andere Figur hinzufügen, und `GesamtFlaeche()` zählt die Fläche der Gruppe automatisch mit. Das vollständige Projekt findest du im Repository unter `examples/03_blazor/Geometrieeditor`.

## Beispiel in .NET: der Komponentenbaum von Blazor

Jede Blazor-Seite ist ein Composite – gleich zweimal. Das HTML, das sie erzeugt, ist ein Baum: Ein `<div>` enthält eine `<ul>`, die `<li>`-Elemente enthält, und der Browser hält diesen Baum als **DOM** (Document Object Model), in dem jedes Element seine Kinder kennt. Und die Anwendung selbst ist ein Baum aus Razor-Komponenten: `App` enthält das `MainLayout`, das die Seite `Home` enthält, die den `NeueFigurDialog` enthält. Wenn Blazor rendert, beginnt es an der Wurzel, und jede Komponente reicht das Rendern an ihre Kinder weiter, bis alle Blätter ihr HTML geliefert haben. Das Layoutsystem aus dem Modul [Layout in Blazor](/modules/blazor_layout/blazor_layout.md) ist eine rekursive `Zeichnen`-Operation im großen Stil.

## Vor- und Nachteile

Der Client wird radikal einfach: Er ruft eine Methode an der Wurzel auf und der Baum kümmert sich um den Rest. Neue Blattarten lassen sich hinzufügen, ohne dass Kompositum oder Client geändert werden müssen, und beliebig tiefe Strukturen entstehen ohne zusätzlichen Code.

Die Nachteile sind die Kehrseite derselben Einheitlichkeit. Sobald man Knoten doch *unterschiedlich* behandeln will – etwa nur Ellipsen rot färben – hilft das Interface nicht weiter, und man landet bei Typprüfungen mit `is`, die das Muster gerade vermeiden wollte. Jede Änderung am Interface zieht Änderungen in allen Blatt- und Kompositum-Klassen nach sich. Und wenn es viele verschiedene Blätter und mehrere Arten von Komposita gibt, wird die Klassenlandschaft schnell unübersichtlich.

Eine `ZusammengesetzteGrafik`, die sich selbst enthält (`bild.Hinzufuegen(bild)`), lässt sich mit dem Interface nicht verhindern. `bild.Zeichnen()` ruft sich dann endlos selbst auf, bis eine `StackOverflowException` das Programm beendet. Wer Zyklen zulassen muss, braucht eine Prüfung in `Hinzufuegen`.
{: .notice--warning}

Übung: Erweitere `IGrafik` um `double Flaeche()`. Wie berechnet die `ZusammengesetzteGrafik` ihre Fläche? Füge dann eine Methode `int AnzahlBlaetter()` hinzu, die zählt, wie viele Ellipsen im gesamten Baum stecken – ohne `is` zu verwenden. Was gibt eine leere Gruppe zurück?
{: .notice--info}

## Weitere Quellen

- [Composite – Refactoring.Guru](https://refactoring.guru/de/design-patterns/composite/csharp/example)
- [Razor-Komponenten in ASP.NET Core – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/)
- [Document Object Model (DOM) – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/API/Document_Object_Model)
