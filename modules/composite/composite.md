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

Ein Ordner auf der Festplatte enthält Dateien – und andere Ordner, die wiederum Dateien und Ordner enthalten. Trotzdem kannst du einen Ordner genauso verschieben, umbenennen oder löschen wie eine einzelne Datei, und der Explorer fragt nicht nach, ob es sich um ein Blatt oder einen Ast handelt. Im Adventure stellt sich dieselbe Frage beim Bauen von Leveln: Eine Wand ist ein einzelnes Objekt, ein Raum besteht aus zwanzig Wänden, ein Kerker aus mehreren Räumen – und trotzdem möchte man einen Raum als *ein* Bauteil behandeln, das sich als Ganzes verschieben und aufs Spielfeld setzen lässt. Dieses Prinzip – **Einzelobjekte und Gruppen von Objekten über dieselbe Schnittstelle behandeln** – heißt **Composite** (Kompositum, Strukturmuster). Es ist das Muster hinter jeder Baumstruktur, die uns in der Praxis begegnet: Dateisysteme, Menüs mit Untermenüs, Organigramme, GUI-Oberflächen.

## Problem

Bisher entstehen unsere Level Zeichen für Zeichen: `LevelParser.Parsen` liest die Textkarte und ruft für jedes `#` ein `feld.Hinzufuegen(new Wand(position))` auf. Für eine handgebaute Schatzkammer heißt das zwanzig einzelne Aufrufe, und wenn die Kammer drei Felder weiter rechts liegen soll, muss man zwanzig Positionen anfassen. Schlimmer wird es, sobald Räume geschachtelt sind: Ein Kerker enthält Räume, ein Raum enthält Nischen und Möblierung. Ohne Muster müsste der Code bei jeder Operation unterscheiden: Ist das ein einzelnes Objekt? Dann setze es. Ist das eine Gruppe? Dann gehe die Liste durch – und für jedes Element wieder dieselbe Frage. Diese Fallunterscheidung wiederholt sich in jeder Operation und wird mit jeder neuen Bauteilart länger.

Naheliegend wäre eine Klasse `Raum : Spielobjekt`, damit ein Raum überall dort hinpasst, wo ein Spielobjekt erwartet wird. Das funktioniert hier aber nicht: `Spielobjekt` hat *eine* `Position` und *ein* `Symbol`, `StatischesObjekt` ist ausdrücklich „bewegt sich nie“, und `Spielfeld` legt statische Objekte in einem `Dictionary<Position, StatischesObjekt>` ab – ein Raum hat weder eine einzelne Rasterzelle noch ein einzelnes Zeichen. Die Komponente des Composite ist deshalb ein **eigenes, neues Interface**, das nur die Bauoperationen beschreibt. Ein Muster zwingt man nicht in eine vorhandene Vererbungshierarchie.
{: .notice--warning}

## Lösung

Das Composite-Muster löst das mit drei Rollen:

- **Komponente** (*Component*): eine gemeinsame Schnittstelle mit den Operationen, die für *alle* Knoten sinnvoll sind – hier Verschieben und Aufs-Feld-Setzen.
- **Blatt** (*Leaf*): ein einfaches Objekt ohne Kinder, das die Operationen direkt ausführt.
- **Kompositum** (*Composite*): ein Objekt, das eine Liste von Komponenten hält und jede Operation an seine Kinder weiterreicht. Da die Kinder selbst Komposita sein können, entsteht daraus eine **rekursive Baumstruktur**.

Der Client sieht nur die Komponente und muss nie wissen, ob er es mit einem Blatt oder einem ganzen Teilbaum zu tun hat. In C# ist die Komponente am besten ein Interface, wie wir es im Modul [Interfaces](/modules/interfaces_grundlagen/interfaces_grundlagen.md) kennengelernt haben:

```csharp
public interface IBauteil
{
    void Verschieben(int dx, int dy);
    void AufFeldSetzen(Spielfeld feld);
}
```

Das Blatt ist eine gewöhnliche Klasse, die das Interface für sich selbst erfüllt. Weil ein `StatischesObjekt` seine Position nach dem Erzeugen nicht mehr ändert, merkt sich der `Baustein` nur *wo* und *was* gebaut werden soll – das „was“ ist ein `Func<Position, StatischesObjekt>`, also eine Fabrikfunktion, wie wir sie im Modul [`Func` und `Action`](/modules/func_action/func_action.md) kennengelernt haben:

```csharp
public sealed class Baustein : IBauteil
{
    private readonly Func<Position, StatischesObjekt> erzeugen;
    private Position position;

    public Baustein(Position position, Func<Position, StatischesObjekt> erzeugen)
    {
        this.position = position;
        this.erzeugen = erzeugen;
    }

    public void Verschieben(int dx, int dy) => position = new Position(position.X + dx, position.Y + dy);

    public void AufFeldSetzen(Spielfeld feld) => feld.Hinzufuegen(erzeugen(position));
}
```

Ein einzelner Baustein ist damit `new Baustein(new Position(3, 2), p => new Wand(p))` oder `new Baustein(new Position(4, 1), p => new Truhe(p, wert: 100))`. Das Kompositum implementiert *dasselbe* Interface, tut aber selbst nichts außer Weiterreichen. Es hält eine `List<IBauteil>` – nicht `List<Baustein>`, denn nur so passen auch andere Gruppen hinein:

```csharp
public sealed class Objektgruppe : IBauteil
{
    private readonly List<IBauteil> teile = new();

    public string Name { get; }

    public Objektgruppe(string name) => Name = name;

    public void Hinzufuegen(IBauteil teil) => teile.Add(teil);
    public void Entfernen(IBauteil teil) => teile.Remove(teil);

    public void Verschieben(int dx, int dy)
    {
        foreach (IBauteil teil in teile)
            teil.Verschieben(dx, dy);
    }

    public void AufFeldSetzen(Spielfeld feld)
    {
        foreach (IBauteil teil in teile)
            teil.AufFeldSetzen(feld);
    }
}
```

Beide Methoden sind rekursiv, ohne dass es so aussieht: Ist ein `teil` selbst eine `Objektgruppe`, ruft `teil.Verschieben(dx, dy)` wieder diese Methode auf, eine Ebene tiefer. Die Rekursion endet automatisch bei den Bausteinen. Ein Raum ist damit nur noch eine Gruppe, die jemand mit Wänden füllt:

```csharp
static Objektgruppe Raum(string name, Position ecke, int breite, int hoehe)
{
    Objektgruppe raum = new(name);
    for (int x = 0; x < breite; x++)
        for (int y = 0; y < hoehe; y++)
            if (x == 0 || y == 0 || x == breite - 1 || y == hoehe - 1)
                raum.Hinzufuegen(new Baustein(new Position(ecke.X + x, ecke.Y + y), p => new Wand(p)));
    return raum;
}
```

Jetzt baut der Client ein ganzes Level als Baum und arbeitet nur noch mit der Wurzel:

```csharp
Objektgruppe kammer = Raum("Schatzkammer", new Position(0, 0), 5, 4);
kammer.Hinzufuegen(new Baustein(new Position(2, 2), p => new Truhe(p, wert: 100)));

Objektgruppe wachstube = Raum("Wachstube", new Position(0, 0), 4, 3);
wachstube.Verschieben(6, 0);

Objektgruppe kerker = new("Kerker");
kerker.Hinzufuegen(kammer);
kerker.Hinzufuegen(wachstube);
kerker.Verschieben(1, 1);            // ein Aufruf, 25 Bauteile auf zwei Ebenen

Spielfeld feld = new(12, 6, new Spieler("Held", new Position(2, 2)));
kerker.AufFeldSetzen(feld);
Console.Write(feld.AlsText());
// ............
// .#####.####.
// .#@..#.#..#.
// .#.T.#.####.
// .#####......
// ............
```

Ein einziger Aufruf `kerker.Verschieben(1, 1)` hat beide Räume samt Truhe verschoben, ein einziges `kerker.AufFeldSetzen(feld)` hat alles gesetzt. `kerker` weiß nicht, wie tief der Baum ist – und muss es auch nicht wissen. Eine dritte Ebene (eine Nische in der Schatzkammer) kostet keine Zeile im Kompositum.

Beachte, dass `Hinzufuegen` und `Entfernen` nur in der `Objektgruppe` stehen, nicht im Interface. Die Gang of Four hat sie in die gemeinsame Basisklasse gelegt, damit der Client wirklich *alles* einheitlich behandeln kann. Der Preis: Ein `Baustein` müsste `Hinzufuegen` dann auch anbieten und beim Aufruf eine Exception werfen – ein Untertyp, der Methoden seines Obertyps verweigert, verstößt gegen das *Liskov-Substitutionsprinzip*, das uns beim Quadrat-Rechteck-Problem in den [Aufgaben zu Vorlesung 02](/modules/aufgaben_abstrakt_interfaces/aufgaben_abstrakt_interfaces.md) begegnet ist. Im Zweifel: Ins Interface gehört nur, was *jeder* Knoten sinnvoll kann.
{: .notice--primary}

## Zweites Beispiel: E-Mail-Empfänger

Das Muster ist nicht auf Spielfelder beschränkt. Eine E-Mail geht an eine Person – oder an eine Verteilerliste, die Personen und weitere Verteilerlisten enthält. Für den Absender soll beides ein *Empfänger* sein:

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

Die Struktur ist Zeile für Zeile dieselbe wie bei den Bauteilen – nur die Namen sind andere. Genau das ist gemeint, wenn ein Muster als *wiederverwendbares Schema* bezeichnet wird: Man erkennt es am Aufbau, nicht an der Anwendung. Eine Verteilerliste `alle` mit den Unterlisten `studierende` und `lehrende` stellt mit einem Aufruf `alle.Zustellen("Prüfungstermine")` jedem einzelnen Mitglied zu.

## Beispiel in .NET: der Komponentenbaum von Blazor

Jede Blazor-Seite ist ein Composite – gleich zweimal. Das HTML, das sie erzeugt, ist ein Baum: Ein `<div class="arbeitsbereich">` enthält das Spielfeld-`div`, das seinerseits ein `<div class="feld">` pro Rasterzelle enthält, und der Browser hält diesen Baum als **DOM** (Document Object Model), in dem jedes Element seine Kinder kennt. Und die Anwendung selbst ist ein Baum aus Razor-Komponenten: `App` enthält das `MainLayout`, das die Seite `Home` enthält, die die `Statusleiste` und den `SpielEndeDialog` enthält. Wenn Blazor rendert, beginnt es an der Wurzel, und jede Komponente reicht das Rendern an ihre Kinder weiter, bis alle Blätter ihr HTML geliefert haben. Das Layoutsystem aus dem Modul [Layout in Blazor](/modules/blazor_layout/blazor_layout.md) ist eine rekursive `Zeichnen`-Operation im großen Stil.

## Vor- und Nachteile

Der Client wird radikal einfach: Er ruft eine Methode an der Wurzel auf und der Baum kümmert sich um den Rest. Neue Blattarten lassen sich hinzufügen, ohne dass Kompositum oder Client geändert werden müssen, und beliebig tiefe Strukturen entstehen ohne zusätzlichen Code.

Die Nachteile sind die Kehrseite derselben Einheitlichkeit. Sobald man Knoten doch *unterschiedlich* behandeln will – etwa nur Truhen zufällig neu befüllen – hilft das Interface nicht weiter, und man landet bei Typprüfungen mit `is`, die das Muster gerade vermeiden wollte. Jede Änderung am Interface zieht Änderungen in allen Blatt- und Kompositum-Klassen nach sich. Und wenn es viele verschiedene Blätter und mehrere Arten von Komposita gibt, wird die Klassenlandschaft schnell unübersichtlich.

Eine `Objektgruppe`, die sich selbst enthält (`kerker.Hinzufuegen(kerker)`), lässt sich mit dem Interface nicht verhindern. `kerker.AufFeldSetzen(feld)` ruft sich dann endlos selbst auf, bis eine `StackOverflowException` das Programm beendet. Wer Zyklen ausschließen will, braucht eine Prüfung in `Hinzufuegen`.
{: .notice--warning}

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`); `IBauteil`, `Baustein` und `Objektgruppe` bauen wir im Workshop darauf auf.

Übung: Erweitere `IBauteil` um `int AnzahlObjekte()`. Wie berechnet die `Objektgruppe` ihre Anzahl, ohne zu wissen, wie tief der Baum ist, und was liefert eine leere Gruppe? Ergänze anschließend eine Methode `Spiegeln(int achseX)`, die jedes Bauteil an einer senkrechten Achse spiegelt – warum genügt es, sie in `Baustein` und `Objektgruppe` zu implementieren, obwohl ein Kerker aus Räumen aus Wänden besteht?
{: .notice--info}

## Weitere Quellen

- [Composite – Refactoring.Guru](https://refactoring.guru/de/design-patterns/composite/csharp/example)
- [Razor-Komponenten in ASP.NET Core – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/components/)
- [Document Object Model (DOM) – MDN Web Docs](https://developer.mozilla.org/de/docs/Web/API/Document_Object_Model)
