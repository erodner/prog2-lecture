---
title: "Abstrakte Mitglieder und konkrete Implementierung"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Eine abstrakte Klasse ist kein leeres Gerüst. Sie ist eher wie eine Rezeptvorlage, in der die meisten Schritte schon ausformuliert sind – nur an einzelnen Stellen steht „hier die Hauptzutat einsetzen“. Die Vorlage kann den ganzen Ablauf beschreiben, obwohl sie die Zutat nicht kennt, weil sie sich darauf verlässt, dass jedes konkrete Rezept sie liefert. In diesem Modul schauen wir uns an, was in einer abstrakten Klasse alles abstrakt sein darf, wie fertiger Code in der Basisklasse abstrakte Mitglieder benutzt und wann `virtual` statt `abstract` die bessere Wahl ist.

## Nicht nur Methoden: abstrakte Properties

Im [vorigen Modul](/modules/abstrakte_klassen/abstrakte_klassen.md) haben wir es schon benutzt, ohne groß darüber zu reden: `Symbol` in `Spielobjekt` ist keine Methode, sondern ein **abstraktes Property**. Alles, was in einer Klasse überschreibbar sein kann, darf auch abstrakt sein – Methoden, Properties, Indexer und Ereignisse.

```csharp
public abstract class Spielobjekt
{
    // ...
    public abstract char Symbol { get; }
}
```

Das `{ get; }` legt fest, dass die Unterklasse einen Getter liefern muss – und nur einen Getter. Ein Symbol von außen zu setzen, wäre auch sinnlos: Eine Wand ist ein `#`, Punkt. Wie die Unterklasse den Getter schreibt, bleibt ihr überlassen. `Wand` antwortet mit einer Konstanten, `Tuer` rechnet:

```csharp
public sealed class Tuer : StatischesObjekt, IInteragierbar
{
    public bool IstOffen { get; private set; }

    public override char Symbol => IstOffen ? '/' : 'D';
    public override bool IstPassierbar => IstOffen;
    // ...
}
```

Beide Properties der Tür hängen vom selben Feld ab – eine offene Tür sieht anders aus *und* verhält sich anders. Für die Basisklasse ist das völlig unsichtbar: Sie hat nur die Signatur `char Symbol { get; }` gefordert.

## Abstrakt und konkret gemischt

Der eigentliche Reiz abstrakter Klassen liegt darin, dass sie beides enthalten können: Versprechen *und* fertigen Code. Der schönste Fall in unserem Spiel ist `BeweglichesObjekt.Bewegen` – eine vollständig implementierte Methode, die über Umwege von einem abstrakten Mitglied abhängt.

```csharp
public abstract class BeweglichesObjekt : Spielobjekt
{
    public bool Bewegen(Richtung richtung, Spielfeld feld)
    {
        Position ziel = Position.Verschoben(richtung);
        if (!feld.IstFrei(ziel)) return false;
        Position = ziel;
        return true;
    }
}
```

`Bewegen` fragt das Spielfeld, ob das Zielfeld frei ist. Und `IstFrei` beantwortet das, indem es das dort liegende Objekt nach `IstPassierbar` fragt:

```csharp
public bool IstFrei(Position p)
{
    if (!IstInnerhalb(p)) return false;
    StatischesObjekt? s = StatischesObjektAn(p);
    if (s is not null && !s.IstPassierbar) return false;
    if (p == Spieler.Position) return false;
    return gegner.All(g => g.Position != p);
}
```

Damit ist die Wirkungskette komplett: Ein Gegner ruft eine geerbte Methode auf, die eine Regel des Spielfelds nutzt, die wiederum das überschriebene Property des Objekts befragt, das zufällig im Weg liegt. Eine verschlossene Tür blockiert, eine offene nicht, ein Trank nie – ohne dass `Bewegen` eine dieser Objektarten kennt. Genau das ist Polymorphie im Alltag.

Eine abstrakte Klasse darf abstrakte Mitglieder in ihrem eigenen Code aufrufen, weil zum Zeitpunkt des Aufrufs garantiert ein konkretes Objekt existiert, das sie überschrieben hat. Der Compiler stellt das sicher: Eine nicht-abstrakte Unterklasse ohne vollständige `override`s kompiliert nicht.
{: .notice--primary}

## Die Basisklasse gibt den Ablauf vor

Noch deutlicher wird das Muster bei den Gegnern. Alle Gegner haben gemeinsam, dass sie einmal pro Runde ziehen – *wohin*, entscheidet jede Art anders. Also steht der gemeinsame Teil in einer abstrakten Klasse, der variable Teil ist ein abstraktes Mitglied:

```csharp
/// <summary>Alle Gegner bewegen sich einmal pro Runde – wie, entscheidet jede Art selbst.</summary>
public abstract class Gegner : BeweglichesObjekt
{
    protected Gegner(string name, Position position) : base(name, position) { }

    /// <summary>Liefert die Richtung für diese Runde oder null, wenn der Gegner stehen bleibt.</summary>
    public abstract Richtung? NaechsterZug(Spielfeld feld);
}
```

Die `Wache` läuft stur geradeaus und dreht um, wenn sie anstößt. Sie braucht dafür einen eigenen Zustand – ihre aktuelle Laufrichtung:

```csharp
public sealed class Wache : Gegner
{
    public Richtung Laufrichtung { get; private set; }

    public override char Symbol => 'W';

    public override Richtung? NaechsterZug(Spielfeld feld)
    {
        if (!feld.IstFrei(Position.Verschoben(Laufrichtung)))
        {
            Laufrichtung = Umkehren(Laufrichtung);
        }
        return feld.IstFrei(Position.Verschoben(Laufrichtung)) ? Laufrichtung : null;
    }
}
```

Der `Verfolger` beantwortet dieselbe Frage völlig anders: Er prüft erst, ob der Spieler nah genug und in Sichtlinie ist, und läuft dann auf ihn zu.

```csharp
public sealed class Verfolger : Gegner
{
    public int Sichtweite { get; }

    public override char Symbol => 'V';

    public override Richtung? NaechsterZug(Spielfeld feld)
    {
        Position ziel = feld.Spieler.Position;
        if (Position.Entfernung(ziel) > Sichtweite || !feld.HatSichtlinie(Position, ziel))
        {
            return null;   // noch nicht entdeckt – stehen bleiben
        }
        // ... Richtung zum Spieler bestimmen
    }
}
```

Das Spielfeld interessiert sich für keinen dieser Unterschiede. Es kennt nur den Vertrag aus `Gegner` und arbeitet ihn für alle Gegner der Reihe nach ab:

```csharp
private void GegnerZiehen(StringBuilder meldung)
{
    foreach (Gegner g in gegner)
    {
        Richtung? zug = g.NaechsterZug(this);
        if (zug is Richtung r)
        {
            Position ziel = g.Position.Verschoben(r);
            if (ziel == Spieler.Position)
            {
                Spieler.SchadenNehmen();
                meldung.Append($" {g.Name} erwischt dich!");
            }
            else
            {
                g.Bewegen(r, this);
            }
        }
    }
    // ...
}
```

Die Regel „wer auf das Feld des Spielers ziehen würde, greift ihn stattdessen an“ steht damit **einmal** im Code und gilt für jede Gegnerart – auch für die, die wir noch gar nicht geschrieben haben. Dieses Muster – die Basisklasse (oder eine aufrufende Klasse) definiert den Ablauf, die Unterklassen füllen einzelne Schritte – begegnet uns in Vorlesung 08 unter dem Namen *Template Method* wieder.

## `virtual` oder `abstract`?

Beide Schlüsselwörter erlauben `override` in Unterklassen, und beide führen zu polymorphen Aufrufen. Der Unterschied liegt darin, ob die Basisklasse selbst eine sinnvolle Implementierung anbieten kann:

| | `virtual` | `abstract` |
| :--- | :--- | :--- |
| Rumpf in der Basisklasse | ja, eine Standardimplementierung | nein, nur die Signatur |
| Unterklasse muss überschreiben | nein, darf | ja, muss |
| Erlaubt in normalen Klassen | ja | nein, nur in `abstract class` |
| `base.Methode()` aus dem `override` aufrufbar | ja | nein, es gibt keinen Rumpf |

In `Spielobjekt` sind alle drei Varianten vertreten, und jede Entscheidung hat einen Grund. `Symbol` ist `abstract`, weil es kein Zeichen gibt, das für irgendein Objekt richtig wäre. `IstPassierbar` ist `virtual` mit der Standardantwort `false`, weil die meisten Objekte blockieren – nur `Ausgang`, `Gegenstand` und die offene `Tuer` überschreiben es. `Beschreibung()` ist ebenfalls `virtual`, und der `Spieler` nutzt die Freiheit, sie zu erweitern:

```csharp
public override string Beschreibung()
{
    return $"{Name} bei {Position}, {Lebenspunkte}/{MaxLebenspunkte} Lebenspunkte, " +
           $"{Punkte} Punkte, Inventar: {Inventar}";
}
```

Eine Faustregel für die Wahl: Wenn dir für die Basisklasse nur ein Rumpf wie `return 0;`, `=> '?'` oder `throw new NotImplementedException()` einfällt, ist das Mitglied in Wahrheit abstrakt. Genau das war der Fehler in der Version aus Vorlesung 01.

Ein häufiger Fehler ist, ein Mitglied `virtual` mit einem sinnlosen Rumpf zu machen, um die Basisklasse „instanziierbar“ zu halten. Dann kompiliert zwar `new Spielobjekt(...)`, aber jedes vergessene `override` in einer Unterklasse fällt erst zur Laufzeit als falsches Ergebnis auf – statt sofort als Compilerfehler.
{: .notice--warning}

Übung: Schreibe eine dritte Gegnerart `Schleicher : Gegner`, die sich nur in jeder zweiten Runde bewegt und sonst `null` zurückgibt (ein `private int runden`-Zähler reicht). Muss `Spielfeld.GegnerZiehen` dafür geändert werden? Überlege anschließend, was passieren würde, wenn `NaechsterZug` statt `abstract` ein `virtual` mit dem Rumpf `return null;` wäre – welcher Fehler fiele dann nicht mehr auf?
{: .notice--info}

## Weitere Quellen

- [Abstrakte und versiegelte Klassen und Klassenmember – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/abstract-and-sealed-classes-and-class-members)
- [virtual (C#-Referenz) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/virtual)
- [Eigenschaften – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/properties)
