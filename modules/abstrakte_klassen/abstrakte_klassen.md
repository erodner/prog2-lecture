---
title: "Abstrakte Klassen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Stell dir einen Bauplan für „ein Gebäude“ vor: Er legt fest, dass es ein Fundament, Wände und ein Dach gibt – aber niemand kann nach diesem Plan bauen, weil er nicht sagt, ob es ein Einfamilienhaus oder eine Lagerhalle wird. Trotzdem ist der Plan nützlich: Jeder konkrete Bauplan muss diese Punkte ausfüllen. Genau das leistet eine **abstrakte Klasse** in C#. Sie fasst zusammen, was alle Unterklassen gemeinsam haben, schreibt vor, was jede Unterklasse selbst liefern muss, und lässt sich bewusst nicht instanziieren. In diesem Modul bauen wir damit die Klasse `Spielobjekt` aus unserem Dungeon-Spiel um – die Wurzel der Hierarchie, die uns bis zum Ende des Semesters begleitet.

## Das Problem mit `new Spielobjekt(...)`

In der [letzten Vorlesung](/lectures/01/01.md) ist `Spielobjekt` als ganz normale Basisklasse entstanden: Alles, was auf dem Spielfeld liegt, hat einen Namen, eine Position, ein Zeichen für die Karte und die Information, ob man darüberlaufen darf.

```csharp
public class Spielobjekt
{
    public string Name { get; }
    public Position Position { get; protected set; }

    public Spielobjekt(string name, Position position)
    {
        Name = name;
        Position = position;
    }

    public virtual char Symbol => '?';          // Notnagel!
    public virtual bool IstPassierbar => false;
}
```

Das `'?'` ist der wunde Punkt. Es steht da nur, weil ein `virtual`-Property einen Rumpf braucht – eine sinnvolle Antwort gibt es nicht, denn „irgendein Spielobjekt“ hat kein Zeichen. Schlimmer noch: Der Compiler erlaubt damit Code, der fachlich Unsinn ist.

```csharp
Spielobjekt ding = new Spielobjekt("Ding", new Position(3, 4));
feld.Hinzufuegen(ding);   // liegt jetzt als '?' im Dungeon herum
```

Ein Objekt, das weder Wand noch Tür noch Trank ist, kann es im Spiel nicht geben. Und wenn jemand später eine neue Objektart schreibt und das `override` für `Symbol` vergisst, fällt das nicht beim Kompilieren auf – es erscheint einfach ein `?` auf der Karte. Beide Probleme lösen wir mit einem einzigen Schlüsselwort.

## Die abstrakte Klasse `Spielobjekt`

Mit `abstract` erklären wir eine Klasse zu einer reinen Verallgemeinerung: Sie beschreibt kein konkretes Objekt, sondern nur, was alle gemeinsam haben. Und sie darf **abstrakte Mitglieder** enthalten – Methoden oder Properties, die nur aus ihrer Signatur bestehen und keinen Rumpf haben.

```csharp
public abstract class Spielobjekt
{
    public string Name { get; }
    public Position Position { get; protected set; }

    protected Spielobjekt(string name, Position position)
    {
        Name = name;
        Position = position;
    }

    /// <summary>Das Zeichen, mit dem das Objekt auf der Karte gezeichnet wird.</summary>
    public abstract char Symbol { get; }

    /// <summary>Darf ein bewegliches Objekt dieses Feld betreten?</summary>
    public virtual bool IstPassierbar => false;

    public virtual string Beschreibung() => $"{Name} bei {Position}";

    public override string ToString() => Beschreibung();
}
```

Zwei Dinge fallen auf. Erstens steht hinter `Symbol` keine Berechnung mehr, nur ein `{ get; }` – die Basisklasse verspricht, dass es dieses Property gibt, überlässt die Antwort aber den Unterklassen. Zweitens ist der Konstruktor `protected`: Er wird nur noch von abgeleiteten Klassen über `base(...)` aufgerufen, denn von außen kann ohnehin niemand ein `Spielobjekt` erzeugen. `IstPassierbar` und `Beschreibung()` bleiben dagegen `virtual`, weil „Wände blockieren, alles andere überschreibt bei Bedarf“ eine brauchbare Standardantwort ist – mehr dazu im [nächsten Modul](/modules/abstrakte_mitglieder/abstrakte_mitglieder.md).

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v02-interfaces`).
{: .notice--primary}

## Keine Objekte aus abstrakten Klassen

Was passiert jetzt mit dem Codestück von oben?

```csharp
Spielobjekt ding = new Spielobjekt("Ding", new Position(3, 4));
// error CS0144: Eine Instanz des abstrakten Typs oder der abstrakten Schnittstelle
//               "Spielobjekt" kann nicht erstellt werden.
```

Der Compiler verweigert das – und das ist genau gewollt. Aus einem Laufzeitproblem („warum steht da ein `?`“) ist ein Kompilierzeitfehler geworden. `Spielobjekt` existiert nur noch, damit andere Klassen von ihr erben; als Typ für Variablen, Parameter und Sammlungen bleibt sie uneingeschränkt erlaubt.

Eine Klasse, die auch nur ein einziges abstraktes Mitglied enthält, muss selbst als `abstract` markiert sein. Umgekehrt darf eine abstrakte Klasse durchaus ohne abstrakte Mitglieder auskommen – dann drückt `abstract` nur aus, dass Instanzen keinen Sinn ergeben.
{: .notice--warning}

## Zwei abstrakte Zwischenklassen

Genau dieser Fall tritt in unserem Spiel sofort ein. Die Objekte zerfallen in zwei Gruppen: Manche liegen fest an ihrem Platz, andere laufen über die Karte. Diese Unterscheidung braucht das Spielfeld ständig – Wände werden in einem `Dictionary<Position, StatischesObjekt>` abgelegt, Gegner in einer Liste. Also bekommt sie zwei eigene Klassen:

```csharp
/// <summary>Objekte, die sich nie bewegen: Wände, Türen, Truhen, Gegenstände auf dem Boden.</summary>
public abstract class StatischesObjekt : Spielobjekt
{
    protected StatischesObjekt(string name, Position position) : base(name, position)
    {
    }
}

/// <summary>Objekte, die sich über das Spielfeld bewegen: der Spieler und alle Gegner.</summary>
public abstract class BeweglichesObjekt : Spielobjekt
{
    protected BeweglichesObjekt(string name, Position position) : base(name, position)
    {
    }

    /// <summary>Versucht einen Schritt; bleibt stehen, wenn das Zielfeld nicht frei ist.</summary>
    public bool Bewegen(Richtung richtung, Spielfeld feld)
    {
        Position ziel = Position.Verschoben(richtung);
        if (!feld.IstFrei(ziel)) return false;
        Position = ziel;
        return true;
    }
}
```

`StatischesObjekt` enthält nichts als einen Konstruktor und ist trotzdem `abstract` – sie ist eine reine Einordnung, und „ein statisches Objekt“ ohne genauere Art gibt es nicht. `BeweglichesObjekt` bringt zusätzlich echten Code mit, den sich Spieler und Gegner teilen. Beide erben das abstrakte `Symbol` weiter, ohne es zu implementieren: Eine abstrakte Klasse darf einen abstrakten Vertrag an ihre Erben durchreichen.

## Abgeleitete Klassen müssen liefern

Erst die konkreten Klassen am Ende der Kette lösen das Versprechen ein. Jede nicht-abstrakte Unterklasse **muss** alle geerbten abstrakten Mitglieder mit `override` implementieren – vergisst man eines, kompiliert die Klasse nicht.

```csharp
public sealed class Wand : StatischesObjekt
{
    public Wand(Position position) : base("Wand", position) { }

    public override char Symbol => '#';
}

public sealed class Ausgang : StatischesObjekt
{
    public Ausgang(Position position) : base("Ausgang", position) { }

    public override char Symbol => 'E';
    public override bool IstPassierbar => true;
}
```

Das `override` ist dasselbe Schlüsselwort wie bei [`virtual`-Methoden](/modules/virtual_override/virtual_override.md) – nur dass es hier keine Wahl ist, sondern Pflicht. Schön sichtbar wird der Unterschied an `Ausgang`: `Symbol` **muss** überschrieben werden, `IstPassierbar` **darf** überschrieben werden, weil man durch den Ausgang hindurchlaufen können soll. Die `Wand` verzichtet darauf und erbt die Standardantwort `false`. Und `sealed` sorgt dafür, dass an dieser Stelle Schluss ist – von einer Wand muss niemand mehr erben.

## Polymorphie mit abstrakten Klassen

Der Gewinn zeigt sich, sobald das Spielfeld alle Objekte gemeinsam behandelt. Beim Zeichnen der Karte fragt es jedes Feld nach dem Objekt, das dort liegt, und holt sich dessen `Symbol`:

```csharp
public string AlsText()
{
    StringBuilder sb = new();
    for (int y = 0; y < Hoehe; y++)
    {
        for (int x = 0; x < Breite; x++)
        {
            sb.Append(ObjektAn(new Position(x, y))?.Symbol ?? '.');
        }
        sb.AppendLine();
    }
    return sb.ToString();
}
```

`ObjektAn` liefert ein `Spielobjekt?` – welche Art dahintersteckt, weiß die Schleife nicht. Trotzdem erscheint für eine Wand ein `#`, für den Spieler ein `@` und für eine offene Tür ein `/`, weil – wie beim [Laufzeittyp](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md) besprochen – die Implementierung des tatsächlichen Objekts aufgerufen wird. Eine abstrakte Methode ist damit automatisch polymorph; ein zusätzliches `virtual` braucht sie nicht.

```
####################
#@.....#...........#
#......#.....W.....#
#..k...#...........#
#......D...........#
```

Der entscheidende Punkt: Das `?` kann in dieser Ausgabe nicht mehr auftauchen. Der Compiler hat jede Objektart gezwungen, sich für ein Zeichen zu entscheiden – und jede neue Objektart, die wir in den nächsten Modulen ergänzen, wird beim Kompilieren daran erinnert.

Übung: Ergänze eine Klasse `Statue : StatischesObjekt` mit dem Symbol `'S'`, die nicht passierbar ist. Lass zuerst das `override` bei `Symbol` weg und lies die Fehlermeldung genau. Versuche danach, `StatischesObjekt` direkt zu instanziieren, und vergleiche die Fehlernummer mit CS0144. Welche der beiden Fehlermeldungen hättest du bei der alten, nicht-abstrakten Version aus Vorlesung 01 bekommen?
{: .notice--info}

## Weitere Quellen

- [abstract (C#-Referenz) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/abstract)
- [Abstrakte und versiegelte Klassen und Klassenmember – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/abstract-and-sealed-classes-and-class-members)
- [Compilerfehler CS0144 – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/misc/cs0144)
- [Type Object – Game Programming Patterns](https://gameprogrammingpatterns.com/type-object.html) – freies Buch über Spielarchitektur; das Kapitel zeigt, wann eine Klassenhierarchie für Gegnerarten an ihre Grenzen stößt.
- [dotnetfiddle.net](https://dotnetfiddle.net/) – C# im Browser ausprobieren: ideal, um CS0144 und ein vergessenes `override` in zwei Minuten selbst zu provozieren.
