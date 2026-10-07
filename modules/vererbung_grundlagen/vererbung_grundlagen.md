---
title: "Vererbung – Grundlagen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Mit Feldern, Properties und Methoden können wir bereits komplexe Dinge modellieren. Schwierig wird es erst, wenn zwei Klassen sich *ähneln*, aber nicht gleich sind. Genau in diese Situation laufen wir sofort, wenn wir mit dem durchgehenden Beispiel dieses Kurses beginnen: dem **Adventure**, einem rundenbasierten Dungeon-Spiel. Auf dem Spielfeld liegen Wände, später kommen Türen, Truhen, Schlüssel, Tränke und Gegner dazu, und mittendrin steht der Held. All diese Dinge sind völlig verschieden – und haben doch dasselbe Grundgerüst: Jedes von ihnen hat einen **Namen**, steht auf einer **Position**, wird mit einem **Zeichen** gezeichnet und kann vom Spieler betreten werden oder eben nicht.

Ohne Vererbung müssten wir `Name` und `Position` in jeder dieser Klassen erneut schreiben – und jede Änderung in einem Dutzend Dateien nachziehen. **Vererbung** erlaubt es, eine Klasse auf einer anderen aufzubauen: Die abgeleitete Klasse übernimmt alles, was die Basisklasse hat, und ergänzt nur das, was sie zusätzlich braucht.

## Die Basisklasse `Spielobjekt`

Bevor wir Objekte platzieren können, brauchen wir einen Typ für ein Feld auf der Karte. `Koordinate` ist eine kleine Klasse mit den Werten `X` (nach rechts) und `Y` (nach unten):

```csharp
public class Koordinate
{
    public int X { get; }
    public int Y { get; }

    public Koordinate(int x, int y)
    {
        X = x;
        Y = y;
    }

    /// <summary>Liefert das Nachbarfeld in der angegebenen Richtung.</summary>
    public Koordinate Verschoben(Richtung richtung)
    {
        if (richtung == Richtung.Oben)
        {
            return new Koordinate(X, Y - 1);
        }
        else if (richtung == Richtung.Unten)
        {
            return new Koordinate(X, Y + 1);
        }
        else if (richtung == Richtung.Links)
        {
            return new Koordinate(X - 1, Y);
        }
        else
        {
            return new Koordinate(X + 1, Y);
        }
    }

    public override string ToString() => $"({X}, {Y})";
}
```

`Verschoben` verändert die Koordinate nicht, sondern liefert eine neue. Mit `ToString` legen wir fest, dass eine Position als `(3, 4)` ausgegeben wird; was das `override` davor bedeutet, klärt das Modul [`virtual` und `override`](/modules/virtual_override/virtual_override.md). Diese erste Fassung hat noch einen Haken, den wir im Modul [Die Basisklasse `object`](/modules/object_basisklasse/object_basisklasse.md) aufdecken.

Auf `Koordinate` baut die gemeinsame Basisklasse auf. Sie enthält genau das, was *alles* hat, was auf dem Spielfeld liegt – vom Aufbau her ist das eine ganz normale Klasse, wie wir sie aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/klassen/klassen/) kennen:

```csharp
public class Spielobjekt
{
    public string Name { get; }
    public Koordinate Position { get; protected set; }

    public Spielobjekt(string name, Koordinate position)
    {
        Name = name;
        Position = position;
    }

    /// <summary>Das Zeichen, mit dem das Objekt auf der Karte gezeichnet wird.</summary>
    public virtual char Symbol => '?';

    /// <summary>Kann der Spieler dieses Feld betreten?</summary>
    public virtual bool IstPassierbar => false;

    public virtual string Beschreibung()
    {
        return $"{Name} bei {Position}";
    }
}
```

Das Schlüsselwort `virtual` heißt: „Erben dürfen das anders machen.“ Wie das funktioniert, ist Thema des Moduls [`virtual` und `override`](/modules/virtual_override/virtual_override.md); hier interessiert uns erst einmal das Erben selbst.

## Ableiten mit `:`

Die Vererbung wird in C# mit einem Doppelpunkt hinter dem Klassennamen angegeben. `Wand` **erbt** von `Spielobjekt`; man sagt auch: `Wand` ist von `Spielobjekt` **abgeleitet**, und `Spielobjekt` ist die **Basisklasse** von `Wand`.

```csharp
public sealed class Wand : Spielobjekt
{
    public Wand(Koordinate position) : base("Wand", position)
    {
    }

    public override char Symbol => '#';
}
```

Auffällig ist, wie wenig in der Klasse steht: ein Konstruktor und ein Zeichen. Trotzdem hat jede Wand einen Namen, eine Position und eine Beschreibung – alles geerbt. Das `sealed` bedeutet, dass von `Wand` niemand mehr erben darf; die Begründung liefert das Modul [Versiegeln mit `sealed`](/modules/sealed/sealed.md).

Der Spieler braucht mehr: Er hat Lebenspunkte und kann sich bewegen.

```csharp
public class Spieler : Spielobjekt
{
    public int Lebenspunkte { get; private set; } = 3;

    public Spieler(string name, Koordinate position) : base(name, position)
    {
    }

    public override char Symbol => '@';

    /// <summary>Versucht einen Schritt; bleibt stehen, wenn das Zielfeld belegt ist.</summary>
    public bool Bewegen(Richtung richtung, Spielfeld feld)
    {
        Koordinate ziel = Position.Verschoben(richtung);
        if (!feld.IstFrei(ziel))
        {
            return false;
        }
        Position = ziel;
        return true;
    }
}
```

In `Bewegen` steckt der ganze Gewinn der Vererbung: `Position` ist nirgends in `Spieler` deklariert und wird trotzdem gelesen *und* geschrieben. Ändern wir später die Basisklasse – etwa um jedem Spielobjekt eine Kennnummer zu geben –, haben Wand, Spieler und alle künftigen Gegner sie sofort.

## Die Ist-eine-Beziehung

Vererbung ist mehr als ein Trick zum Codesparen – sie drückt eine fachliche Beziehung aus: Eine Wand **ist ein** Spielobjekt, ein Spieler **ist ein** Spielobjekt. Deshalb darf eine `Wand` überall dort verwendet werden, wo ein `Spielobjekt` verlangt wird. Genau davon lebt das Spielfeld, das seine Objekte in einer einzigen Liste hält:

```csharp
public class Spielfeld
{
    private readonly List<Spielobjekt> objekte = new();

    public void Hinzufuegen(Spielobjekt objekt)
    {
        objekte.Add(objekt);
    }
}
```

```csharp
Spielfeld feld = new Spielfeld(10, 6, held);
feld.Hinzufuegen(new Wand(new Koordinate(0, 0)));
feld.Hinzufuegen(new Wand(new Koordinate(5, 2)));
```

Man nennt das **Substituierbarkeit**: Die abgeleitete Klasse kann die Basisklasse vollständig ersetzen. `Hinzufuegen` muss keine einzige Zeile ändern, wenn morgen eine Tür oder ein Gegner dazukommt – solange beide von `Spielobjekt` erben. Über eine Variable vom Typ `Spielobjekt` können wir allerdings nur das aufrufen, was jedes Spielobjekt kann; `objekte[0].Bewegen(...)` würde der Compiler ablehnen, selbst wenn dort tatsächlich der Spieler steht. Warum das so ist und wie man trotzdem an den Spieler herankommt, klären wir im Modul [Kompilierzeittyp und Laufzeittyp](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md).

Vererbung nur einsetzen, wenn die Ist-eine-Beziehung wirklich stimmt. Der Spieler *hat* ein Spielfeld, auf dem er sich bewegt, aber er *ist* kein Spielfeld – deshalb ist das Spielfeld ein Parameter von `Bewegen` und keine Basisklasse. Wer Vererbung nur benutzt, um an ein paar Methoden heranzukommen, bekommt Hierarchien, die niemand mehr versteht.
{: .notice--warning}

## Konstruktorverkettung mit `base`

Konstruktoren werden **nicht** vererbt. Der Konstruktor von `Wand` muss deshalb selbst dafür sorgen, dass der Spielobjekt-Teil initialisiert wird – dafür steht `: base("Wand", position)` hinter der Parameterliste. Damit wird der Konstruktor der Basisklasse aufgerufen, bevor der eigene Konstruktorrumpf beginnt. Schön zu sehen ist hier, dass eine abgeleitete Klasse Werte auch **festlegen** darf: Eine Wand heißt immer „Wand“, ihr Konstruktor braucht dafür keinen Parameter. Der Spieler reicht seinen Namen dagegen durch (`: base(name, position)`), denn der Held darf heißen, wie er möchte.

Lässt man `: base(...)` weg, ruft C# automatisch den **parameterlosen** Konstruktor der Basisklasse auf. Den gibt es bei `Spielobjekt` nicht, weil wir einen eigenen Konstruktor mit Parametern geschrieben haben – der Compiler meldet dann den Fehler CS7036 („Es wurde kein Argument angegeben, das dem erforderlichen Parameter ‚name‘ entspricht“). Die Reihenfolge der Initialisierung ist dabei immer dieselbe: **erst die Basisklasse, dann die abgeleitete Klasse**. Das lässt sich mit zwei Ausgaben sichtbar machen:

```csharp
public class Spielobjekt
{
    public Spielobjekt(string name, Koordinate position)
    {
        Name = name;
        Position = position;
        Console.WriteLine("Spielobjekt-Konstruktor");
    }
    // ...
}

public class Spieler : Spielobjekt
{
    public Spieler(string name, Koordinate position) : base(name, position)
    {
        Console.WriteLine("Spieler-Konstruktor");
    }
}

new Spieler("Held", new Koordinate(1, 1));
// Spielobjekt-Konstruktor
// Spieler-Konstruktor
```

Das ist logisch: Der Spieler-Konstruktor darf sich darauf verlassen, dass `Name` und `Position` schon gesetzt sind – die Basisklasse hat ihre Arbeit bereits erledigt. Mit `base` greift man übrigens nicht nur auf Konstruktoren zu, sondern auf alle Mitglieder der Basisklasse; das nutzen wir im Modul [`virtual` und `override`](/modules/virtual_override/virtual_override.md).

## `protected` – sichtbar für Erben

Aus Programmierung 1 kennen wir `public` und `private`. `private`-Mitglieder der Basisklasse werden zwar mit vererbt (sie sind Teil des Objekts), sind aber in der abgeleiteten Klasse **nicht zugreifbar**. Dazwischen liegt `protected`: sichtbar in der Klasse selbst und in allen abgeleiteten Klassen, aber nicht von außen.

Genau deshalb steht in `Spielobjekt` die Zeile `public Koordinate Position { get; protected set; }`. Ein zusammengesetzter Zugriffsmodifizierer am Setter: Lesen darf jeder – das Spielfeld muss schließlich wissen, wo ein Objekt steht –, aber **verschieben darf sich ein Objekt nur selbst**. Die Karte und die Konsolenausgabe können keinem Gegner heimlich eine neue Position zuweisen.

```csharp
Spieler held = new Spieler("Held", new Koordinate(1, 1));
Console.WriteLine(held.Position);        // (1, 1) – lesen ist erlaubt
held.Position = new Koordinate(5, 5);      // Fehler CS0272: der Setter ist nicht zugreifbar
held.Bewegen(Richtung.Rechts, feld);     // so herum: geprüfter Schritt auf (2, 1)
```

`protected` ist ein Versprechen an die Erben: „Das hier dürft ihr benutzen.“ Genau deshalb sollte man es nicht leichtfertig vergeben – jedes `protected`-Mitglied gehört zur Schnittstelle, auf die sich abgeleitete Klassen verlassen.
{: .notice--primary}

## Nur eine Basisklasse

C# kennt nur **Einfachvererbung**: Eine Klasse hat genau eine direkte Basisklasse. Hierarchien können aber beliebig tief werden – in der nächsten Vorlesung schieben wir zwischen `Spielobjekt` und die konkreten Klassen noch eine Ebene ein (`StatischesObjekt` für alles, was liegen bleibt, `BeweglichesObjekt` für Spieler und Gegner), und alles, was dort steht, gilt automatisch auch weiter unten. Wer Verhalten aus mehreren Quellen kombinieren möchte, greift zu Interfaces; die lernen wir in [Vorlesung 02](/lectures/02/02.md) kennen.

Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v01-vererbung`).

Übung: Lege eine Klasse `Tuer : Spielobjekt` an, die im Konstruktor nur eine `Koordinate` bekommt, dort `: base("Tür", position)` aufruft und zusätzlich eine Property `bool IstOffen { get; private set; }` sowie eine Methode `Oeffnen()` erhält. Füge eine Tür zum Spielfeld hinzu und gib `tuer.Beschreibung()` aus – woher kommt diese Methode? Versuche anschließend, in `Oeffnen()` die Zeile `Position = new Koordinate(0, 0);` zu schreiben: Warum ist das erlaubt, obwohl der Setter `protected` ist?
{: .notice--info}

## Weitere Quellen

- [Vererbung in C# – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/object-oriented/inheritance)
- [Vererbung – Tutorial mit Beispielen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/tutorials/inheritance)
- [Zugriffsmodifizierer (`protected`) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers)
- [Roguelike – Wikipedia](https://de.wikipedia.org/wiki/Roguelike) – ordnet unser Adventure in ein Spielgenre ein, das seit 1980 von genau solchen Rastern, Symbolen und rundenbasierten Zügen lebt.
- [RogueBasin – Wiki der Roguelike-Entwicklung](https://www.roguebasin.com/index.php/Main_Page) – eine Fundgrube an Artikeln zu Levelerzeugung, Sichtlinien und Gegner-KI, wenn du das Spiel über die Vorlesung hinaus weiterbauen willst.
