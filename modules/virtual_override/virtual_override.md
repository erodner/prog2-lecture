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

Im Modul [Vererbung – Grundlagen](/modules/vererbung_grundlagen/vererbung_grundlagen.md) haben `Wand` und `Spieler` ihren Namen, ihre Position und ihre Beschreibung von `Spielobjekt` geerbt. Für die Position ist das genau richtig – aber beim Zeichnen der Karte hört die Gemeinsamkeit auf: Eine Wand ist ein `#`, der Held ein `@`, eine Tür ein `D`. Wir wollen eine geerbte Eigenschaft also nicht nur übernehmen, sondern **anpassen** – und zwar so, dass auch Code, der nur `Spielobjekt` kennt, automatisch die angepasste Variante benutzt. Genau das leisten `virtual` und `override`. Dieses Prinzip heißt **Polymorphie** (griechisch für Vielgestaltigkeit) und ist der eigentliche Grund, warum objektorientierte Programmierung so mächtig ist.

## `virtual` – die Basisklasse erlaubt das Überschreiben

Eine Methode oder Property kann in einer abgeleiteten Klasse nur dann ersetzt werden, wenn die Basisklasse das ausdrücklich erlaubt. Dafür wird sie mit `virtual` gekennzeichnet – in `Spielobjekt` betrifft das gleich drei Mitglieder:

```csharp
public class Spielobjekt
{
    public string Name { get; }
    public Position Position { get; protected set; }

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

`virtual` ist lediglich die Erlaubnis: „Erben dürfen das durch etwas Eigenes ersetzen.“ Wird nichts überschrieben, gilt weiterhin die Implementierung der Basisklasse – ein Objekt ohne eigenes Symbol erscheint als `?` auf der Karte und versperrt den Weg. Beides sind bewusst gewählte Vorgaben: Ein neues Spielobjekt fällt beim Testen sofort auf, und es lässt niemanden versehentlich hindurchlaufen.

`virtual` und `override` funktionieren für Methoden (`Beschreibung()`) genauso wie für Properties – `Symbol` und `IstPassierbar` sind hier als *Expression-bodied Property* geschrieben, also als `get`-Zugriff mit `=>`.
{: .notice--primary}

## `override` – die abgeleitete Klasse ersetzt das Mitglied

Die Wand überschreibt nur das Symbol; die geerbte Beschreibung („Wand bei (5, 2)“) passt schon:

```csharp
public sealed class Wand : Spielobjekt
{
    public Wand(Position position) : base("Wand", position)
    {
    }

    public override char Symbol => '#';
}
```

Der Spieler geht einen Schritt weiter: Er hat Lebenspunkte, die in der Beschreibung auftauchen sollen. Die Signatur muss beim Überschreiben exakt dieselbe sein wie in der Basisklasse – gleicher Name, gleiche Parameter, gleicher Rückgabetyp.

```csharp
public class Spieler : Spielobjekt
{
    public int Lebenspunkte { get; private set; } = 3;

    public override char Symbol => '@';

    public override string Beschreibung()
    {
        return base.Beschreibung() + $", {Lebenspunkte} Lebenspunkte";
    }
}
```

Interessant ist `base.Beschreibung()`: Mit `base` greifen wir auf die **Originalimplementierung** der Basisklasse zu. Der Spieler ersetzt die Beschreibung also nicht komplett, sondern *erweitert* sie – erst Name und Position wie bei jedem Spielobjekt, dann die Lebenspunkte. Ob und wann man `base` aufruft, ist eine Designentscheidung: Manchmal will man das Verhalten vollständig austauschen, manchmal nur ergänzen. Hier lohnt es sich: Ergänzen wir in `Spielobjekt.Beschreibung` später die Himmelsrichtung, erbt der Spieler diese Ergänzung automatisch mit.

```csharp
Spieler held = new Spieler("Held", new Position(1, 1));
Console.WriteLine(held.Beschreibung());   // Held bei (1, 1), 3 Lebenspunkte

Wand wand = new Wand(new Position(5, 2));
Console.WriteLine(wand.Beschreibung());   // Wand bei (5, 2)
```

Bis hierhin könnte man einwenden: Das hätte man auch mit zwei verschieden benannten Methoden erreicht. Der Unterschied zeigt sich erst, wenn wir die Objekte über ihre Basisklasse ansprechen – und genau das tut das Spielfeld.

## Polymorphie – die richtige Implementierung zur Laufzeit

Das Spielfeld hält alle Objekte in einer `List<Spielobjekt>` und zeichnet die Karte Zeile für Zeile. Für jedes Feld fragt es das dort liegende Objekt nach seinem Symbol – und zwar über eine Variable vom Typ `Spielobjekt`, nicht `Wand` oder `Spieler`:

```csharp
/// <summary>Zeichnet das Spielfeld als Text – Zeile für Zeile.</summary>
public string AlsText()
{
    StringBuilder sb = new();
    for (int y = 0; y < Hoehe; y++)
    {
        for (int x = 0; x < Breite; x++)
        {
            Position p = new(x, y);
            char zeichen = p == Spieler.Position ? Spieler.Symbol : ObjektAn(p)?.Symbol ?? '.';
            sb.Append(zeichen);
        }
        sb.AppendLine();
    }
    return sb.ToString();
}
```

Beim Aufruf von `ObjektAn(p)?.Symbol` entscheidet **nicht** der Typ der Variablen, sondern das tatsächliche Objekt, welche Implementierung läuft. Legen wir einen kleinen Raum an, kommt dabei genau die Karte heraus, die wir erwarten:

```csharp
Spieler held = new Spieler("Held", new Position(1, 1));
Spielfeld feld = new Spielfeld(10, 6, held);

for (int x = 0; x < feld.Breite; x++)
{
    feld.Hinzufuegen(new Wand(new Position(x, 0)));
    feld.Hinzufuegen(new Wand(new Position(x, feld.Hoehe - 1)));
}
for (int y = 1; y < feld.Hoehe - 1; y++)
{
    feld.Hinzufuegen(new Wand(new Position(0, y)));
    feld.Hinzufuegen(new Wand(new Position(feld.Breite - 1, y)));
}
feld.Hinzufuegen(new Wand(new Position(5, 2)));
feld.Hinzufuegen(new Wand(new Position(5, 3)));

Console.WriteLine(feld.AlsText());
// ##########
// #@.......#
// #....#...#
// #....#...#
// #........#
// ##########
```

In `AlsText` steht nirgends das Wort `Wand`. Die Zeichenroutine weiß nichts von Wänden und muss es auch nicht – sie fragt jedes Objekt nach seinem Symbol und bekommt die passende Antwort. Man sagt: `Symbol` ist **polymorph**. Die Laufzeitumgebung schaut bei jedem Zugriff nach, welches Objekt wirklich dort liegt, und wählt die passende Implementierung.

Dasselbe passiert bei `IstPassierbar`. Die Methode `IstFrei` prüft nur die Spielfeldgrenzen und stellt dann genau eine polymorphe Frage:

```csharp
public bool IstFrei(Position p)
{
    if (!IstInnerhalb(p)) return false;
    Spielobjekt? o = ObjektAn(p);
    return o is null || o.IstPassierbar;
}
```

Warum ist das der Kern der OOP? Weil sich dadurch die Abhängigkeiten umdrehen: `AlsText` und `IstFrei` hängen nur von `Spielobjekt` ab. Kommt morgen eine `Tuer` dazu, die `IstPassierbar` zurückgibt, sobald sie offen ist, oder ein `Verfolger` mit dem Symbol `V`, dann funktionieren beide Methoden **ohne eine einzige Änderung** weiter. Bestehender Code bleibt stabil, während neue Varianten hinzukommen – und genau davon lebt dieses Spiel, das über das Semester um ein gutes Dutzend Objektarten wächst. Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v01-vererbung`).

## Ohne `virtual` keine Polymorphie

Die Erlaubnis der Basisklasse ist keine Formalität. Fehlt das `virtual`, lässt sich das Mitglied nicht überschreiben – der Compiler meldet CS0506 („kann den geerbten Member nicht überschreiben, da er nicht als `virtual`, `abstract` oder `override` markiert ist“). Und wer dann *beide* Schlüsselwörter weglässt und in `Wand` einfach eine zweite Property `Symbol` schreibt, bekommt zwar nur eine Warnung, aber ein anderes Verhalten: Die Mauern des Raums bestünden plötzlich aus `?` statt aus `#`, weil `AlsText` die Objekte über den Typ `Spielobjekt` befragt. Das Mitglied wird dann nicht überschrieben, sondern **versteckt** – was das genau bedeutet, zeigt das Modul [Laufzeittyp und Verstecken](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md).
{: .notice--warning}

Im Zweifel gilt: Ein Mitglied, das abgeleitete Klassen sinnvoll anpassen könnten, wird `virtual`. Eines, dessen Verhalten für alle Erben verbindlich sein soll (etwa die geprüfte Schrittlogik in `Bewegen`), bleibt ohne `virtual` – oder wird ausdrücklich versiegelt, siehe [`sealed`](/modules/sealed/sealed.md).

Übung: Schreibe eine dritte Klasse `Ausgang : Spielobjekt`, die `: base("Ausgang", position)` aufruft, das Symbol `E` bekommt und `IstPassierbar` mit `=> true` überschreibt – der Held soll den Ausgang schließlich betreten können. Setze einen Ausgang in die rechte Wand des Raums oben und gib das Spielfeld aus. Überlege *vorher*, was passiert, wenn du das `override` bei `IstPassierbar` weglässt: Welche der beiden Methoden `AlsText` und `IstFrei` verhält sich dann anders, und warum musstest du an keiner von beiden etwas ändern, um den Ausgang einzubauen?
{: .notice--info}

## Weitere Quellen

- [Polymorphie – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/object-oriented/polymorphism)
- [`virtual` – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/virtual)
- [`override` – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/override)
- [Game Programming Patterns – Robert Nystrom](https://gameprogrammingpatterns.com/) – ein kostenlos lesbares Buch darüber, wie Objektorientierung in echten Spielen eingesetzt wird (und wo sie schadet).
- [Type Object – Game Programming Patterns](https://gameprogrammingpatterns.com/type-object.html) – zeigt die Kehrseite: Wenn ein Spiel hunderte Objektarten bekommt, wird aus jeder Unterklasse irgendwann besser ein Datensatz.
