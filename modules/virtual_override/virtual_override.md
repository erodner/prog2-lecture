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

Im Modul [Vererbung – Grundlagen](/modules/vererbung_grundlagen/vererbung_grundlagen.md) haben `Wand` und `Spieler` Name, Position und Beschreibung von `Spielobjekt` geerbt. Beim Zeichnen der Karte hört die Gemeinsamkeit aber auf: Eine Wand ist ein `#`, der Held ein `@`. In diesem Modul sehen wir, wie das Spielfeld jedes Objekt richtig zeichnet, obwohl es nur `Spielobjekt` kennt. Dieses Prinzip heißt **Polymorphie**.

## Das Spielfeld

Das `Spielfeld` ist ein Raster aus `Breite` mal `Hoehe` Feldern. Den Spieler merkt es sich in einer eigenen Property, alle anderen Objekte liegen in einer einzigen Liste:

```csharp
public class Spielfeld
{
    public int Breite { get; }
    public int Hoehe { get; }
    public Spieler Spieler { get; }

    private readonly List<Spielobjekt> objekte = new();

    public Spielfeld(int breite, int hoehe, Spieler spieler)
    {
        Breite = breite;
        Hoehe = hoehe;
        Spieler = spieler;
    }

    public void Hinzufuegen(Spielobjekt objekt)
    {
        objekte.Add(objekt);
    }
}
```

Das Raster selbst wird nirgends gespeichert. Ein Feld ist nur eine `Koordinate`; was dort liegt, steht in der Position des jeweiligen Objekts. Ein Raum von 10 × 6 Feldern braucht also kein Array mit 60 Einträgen, sondern nur eine Liste mit den Wänden. Weil die Liste den Typ `List<Spielobjekt>` hat, passt jede Unterklasse hinein, wie wir im Modul [Vererbung – Grundlagen](/modules/vererbung_grundlagen/vererbung_grundlagen.md) gesehen haben.

## Was liegt auf diesem Feld? `ObjektAn`

Zum Zeichnen und für Kollisionen muss das Spielfeld immer wieder dieselbe Frage beantworten: Was liegt an einer bestimmten Koordinate? Das erledigt `ObjektAn`. Die Methode geht die Liste durch und liefert das erste Objekt, dessen Position passt:

```csharp
public Spielobjekt? ObjektAn(Koordinate position)
{
    foreach (Spielobjekt o in objekte)
    {
        if (o.Position == position) return o;
    }
    return null;
}
```

Das Fragezeichen in `Spielobjekt?` bedeutet: Das Ergebnis kann `null` sein, nämlich wenn das Feld leer ist. Den Spieler findet `ObjektAn` nicht, weil er nicht in der Liste steht.

Interessant ist der Rückgabetyp. Wer `ObjektAn` aufruft, bekommt ein `Spielobjekt` zurück und weiß nicht, ob es eine Wand oder etwas ganz anderes ist. Trotzdem muss er es zeichnen können. Er kann also nur fragen: „Spielobjekt, welches Zeichen hast du?“ Damit die Antwort von der Art des Objekts abhängt, braucht es `virtual` und `override`.

## Das Symbol: `virtual` und `override`

`Spielobjekt` erlaubt mit `virtual`, dass Unterklassen das Symbol (und die Passierbarkeit) ersetzen. Die Vorgaben `?` und `false` greifen nur, wenn eine Unterklasse nichts überschreibt:

```csharp
public class Spielobjekt
{
    public virtual char Symbol => '?';
    public virtual bool IstPassierbar => false;
    // Name, Position, Konstruktor, Beschreibung wie bisher
}
```

`Wand` und `Spieler` überschreiben das Symbol mit `override`. Die Signatur muss dabei exakt dieselbe sein wie in der Basisklasse:

```csharp
public sealed class Wand : Spielobjekt
{
    // Konstruktor wie bisher
    public override char Symbol => '#';
}

public class Spieler : Spielobjekt
{
    // Lebenspunkte, Konstruktor, Bewegen wie bisher
    public override char Symbol => '@';
}
```

`virtual` und `override` funktionieren für Properties wie `Symbol` genauso wie für Methoden. `Symbol` ist hier eine *Expression-bodied Property*, also ein `get`-Zugriff mit `=>`.
{: .notice--primary}

## Polymorphie beim Zeichnen

Jetzt kann das Spielfeld die Karte zeichnen. `AlsText` geht alle Felder Zeile für Zeile durch:

```csharp
/// <summary>Zeichnet das Spielfeld als Text – Zeile für Zeile.</summary>
public string AlsText()
{
    StringBuilder sb = new();
    for (int y = 0; y < Hoehe; y++)
    {
        for (int x = 0; x < Breite; x++)
        {
            Koordinate p = new(x, y);
            char zeichen = p == Spieler.Position ? Spieler.Symbol : ObjektAn(p)?.Symbol ?? '.';
            sb.Append(zeichen);
        }
        sb.AppendLine();
    }
    return sb.ToString();
}
```

Die Zeile mit `zeichen` hat drei Fälle. Steht dort der Spieler, wird sein Symbol gezeichnet. Sonst fragt `ObjektAn(p)?.Symbol` das Objekt auf dem Feld nach seinem Symbol; das `?.` liefert `null`, wenn das Feld leer ist. In dem Fall greift `?? '.'` und zeichnet einen Punkt.

Entscheidend ist `ObjektAn(p)?.Symbol`. Der Ausdruck hat den Typ `Spielobjekt`, aber welches `Symbol` läuft, entscheidet das **tatsächliche Objekt** zur Laufzeit. Für eine Wand ist das `#`:

```csharp
Spieler held = new Spieler("Held", new Koordinate(1, 1));
Spielfeld feld = new Spielfeld(10, 6, held);

for (int x = 0; x < feld.Breite; x++)
{
    feld.Hinzufuegen(new Wand(new Koordinate(x, 0)));
    feld.Hinzufuegen(new Wand(new Koordinate(x, feld.Hoehe - 1)));
}
for (int y = 1; y < feld.Hoehe - 1; y++)
{
    feld.Hinzufuegen(new Wand(new Koordinate(0, y)));
    feld.Hinzufuegen(new Wand(new Koordinate(feld.Breite - 1, y)));
}
feld.Hinzufuegen(new Wand(new Koordinate(5, 2)));
feld.Hinzufuegen(new Wand(new Koordinate(5, 3)));

Console.WriteLine(feld.AlsText());
// ##########
// #@.......#
// #....#...#
// #....#...#
// #........#
// ##########
```

Programmierst du mit der Klasse `Koordinate` aus [Vererbung – Grundlagen](/modules/vererbung_grundlagen/vererbung_grundlagen.md) mit, besteht deine Karte nur aus Punkten, und der Held läuft durch alle Wände. Das liegt nicht an der Polymorphie, sondern am Vergleich `o.Position == position` in `ObjektAn`. Warum, klärt das Modul [Die Basisklasse `object`](/modules/object_basisklasse/object_basisklasse.md).
{: .notice--warning}

In `AlsText` steht nirgends das Wort `Wand`. Das Spielfeld fragt jedes Objekt nach seinem Symbol und bekommt die passende Antwort. Man sagt: `Symbol` ist **polymorph**.

Dasselbe gilt für Kollisionen. Bevor der Held einen Schritt macht, fragt `Bewegen` das Spielfeld, ob das Zielfeld frei ist. Dafür gibt es zwei Methoden:

```csharp
public bool IstInnerhalb(Koordinate p)
{
    return p.X >= 0 && p.Y >= 0 && p.X < Breite && p.Y < Hoehe;
}

public bool IstFrei(Koordinate p)
{
    if (!IstInnerhalb(p)) return false;
    Spielobjekt? o = ObjektAn(p);
    return o is null || o.IstPassierbar;
}
```

`IstInnerhalb` prüft nur, ob die Koordinate auf dem Raster liegt: `X` zwischen `0` und `Breite - 1`, `Y` zwischen `0` und `Hoehe - 1`. Ohne diese Prüfung könnte der Held aus einem Raum ohne geschlossene Wand einfach hinauslaufen. `IstFrei` nutzt sie als ersten Schritt. Liegt das Feld auf dem Raster, fragt die Methode `ObjektAn`: Ein leeres Feld ist frei, ein belegtes nur, wenn das Objekt passierbar ist. `o.IstPassierbar` ist wieder eine polymorphe Frage, deren Antwort das tatsächliche Objekt gibt.

`AlsText` und `IstFrei` hängen nur von `Spielobjekt` ab. Kommt später eine `Tuer` dazu, die passierbar ist, sobald sie offen ist, oder ein `Verfolger` mit dem Symbol `V`, funktionieren beide Methoden **ohne Änderung** weiter. Das ist der eigentliche Gewinn der Polymorphie: Bestehender Code bleibt stabil, während neue Objektarten hinzukommen. Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v01-vererbung`).

## Erweitern statt ersetzen: `base`

Auch `Beschreibung()` ist `virtual`. Der Spieler will die geerbte Beschreibung nicht ersetzen, sondern um seine Lebenspunkte ergänzen. Mit `base.Beschreibung()` ruft er die Fassung der Basisklasse auf:

```csharp
public override string Beschreibung()
{
    return base.Beschreibung() + $", {Lebenspunkte} Lebenspunkte";
}
```

```csharp
Console.WriteLine(held.Beschreibung());                        // Held bei (1, 1), 3 Lebenspunkte
Console.WriteLine(new Wand(new Koordinate(5, 2)).Beschreibung()); // Wand bei (5, 2)
```

Ändern wir später `Spielobjekt.Beschreibung`, bekommt der Spieler die Änderung automatisch mit.

## Ohne `virtual` keine Polymorphie

Fehlt in der Basisklasse das `virtual`, meldet der Compiler beim `override` den Fehler CS0506. Lässt man auch das `override` weg und schreibt in `Wand` einfach eine zweite Property `Symbol`, gibt es nur eine Warnung, aber die Mauern bestehen plötzlich aus `?`: `AlsText` fragt über den Typ `Spielobjekt` und bekommt dessen Symbol. Das Mitglied wird dann nicht überschrieben, sondern **versteckt**, siehe [Kompilierzeittyp und Laufzeittyp](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md).
{: .notice--warning}

Im Zweifel gilt: Ein Mitglied, das abgeleitete Klassen sinnvoll anpassen könnten, wird `virtual`. Eines, dessen Verhalten für alle Erben verbindlich sein soll (etwa die geprüfte Schrittlogik in `Bewegen`), bleibt ohne `virtual` oder wird ausdrücklich versiegelt, siehe [`sealed`](/modules/sealed/sealed.md).

Übung: Schreibe eine dritte Klasse `Ausgang : Spielobjekt`, die `: base("Ausgang", position)` aufruft, das Symbol `E` bekommt und `IstPassierbar` mit `=> true` überschreibt – der Held soll den Ausgang schließlich betreten können. Setze einen Ausgang in die rechte Wand des Raums oben und gib das Spielfeld aus. Überlege *vorher*, was passiert, wenn du das `override` bei `IstPassierbar` weglässt: Welche der beiden Methoden `AlsText` und `IstFrei` verhält sich dann anders, und warum musstest du an keiner von beiden etwas ändern, um den Ausgang einzubauen?
{: .notice--info}

## Weitere Quellen

- [Polymorphie – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/object-oriented/polymorphism)
- [`virtual` – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/virtual)
- [`override` – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/override)
- [Game Programming Patterns – Robert Nystrom](https://gameprogrammingpatterns.com/) – ein kostenlos lesbares Buch darüber, wie Objektorientierung in echten Spielen eingesetzt wird (und wo sie schadet).
- [Type Object – Game Programming Patterns](https://gameprogrammingpatterns.com/type-object.html) – zeigt die Kehrseite: Wenn ein Spiel hunderte Objektarten bekommt, wird aus jeder Unterklasse irgendwann besser ein Datensatz.
