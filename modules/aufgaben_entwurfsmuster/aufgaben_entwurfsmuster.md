---
title: "🧩 Aufgaben und Beispiele: Entwurfsmuster"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen — sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Bei Entwurfsmustern ist die Mustererkennung wörtlich gemeint: Die Kunst besteht weniger darin, ein Muster zu implementieren, als darin, es in fremdem Code wiederzuerkennen, das passende auszuwählen – und zu merken, wenn eines fehl am Platz ist. Alle vier Aufgaben spielen im Adventure. Nimm dir für jede Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Mustererkennung

Die folgenden fünf Ausschnitte stammen aus verschiedenen Ausbaustufen des Adventure. Bestimme für jeden, welches Entwurfsmuster aus dieser Vorlesung er umsetzt – oder ob er nur so aussieht.

```csharp
// Ausschnitt A
public sealed class Spielkonfiguration
{
    private static readonly Lazy<Spielkonfiguration> halter = new(() => new Spielkonfiguration());
    private Spielkonfiguration() { }
    public static Spielkonfiguration Instanz => halter.Value;
    public int Sichtweite { get; set; } = 5;
}
```

```csharp
// Ausschnitt B
public sealed class Objektgruppe : IBauteil
{
    private readonly List<IBauteil> teile = new();
    public void Hinzufuegen(IBauteil teil) => teile.Add(teil);

    public void Verschieben(int dx, int dy)
    {
        foreach (IBauteil teil in teile)
            teil.Verschieben(dx, dy);
    }
}
```

```csharp
// Ausschnitt C
public sealed class GamepadEingabe : IEingabe
{
    private readonly Gamepad gamepad;                 // fremde Bibliothek
    public GamepadEingabe(Gamepad gamepad) => this.gamepad = gamepad;

    public Richtung? NaechsteRichtung() => gamepad.LiesStick() switch
    {
        StickLage.Hoch => Richtung.Oben,
        StickLage.Runter => Richtung.Unten,
        StickLage.Links => Richtung.Links,
        StickLage.Rechts => Richtung.Rechts,
        _ => null
    };
}
```

```csharp
// Ausschnitt D
public class Spielfeld
{
    private readonly List<Gegner> gegner = new();

    public IEnumerable<Gegner> Wachsame(int maxEntfernung)
    {
        foreach (Gegner g in gegner)
            if (g.Position.Entfernung(Spieler.Position) <= maxEntfernung)
                yield return g;
    }
}
```

```csharp
// Ausschnitt E
public static class LevelParser
{
    private static Spielobjekt ObjektFuer(char zeichen, Position pos) => zeichen switch
    {
        '#' => new Wand(pos),
        'D' => new Tuer(pos),
        'T' => new Truhe(pos, wert: 100),
        'W' => new Wache(pos),
        _ => throw new ArgumentException($"Unbekanntes Zeichen '{zeichen}' bei {pos}.")
    };
}
```

Leitfragen:
- Woran erkennst du das Muster – an einem Schlüsselwort, an einer Struktur, an den beteiligten Rollen?
- Welche Rolle des Musters spielt die gezeigte Klasse, und welche Rollen fehlen im Ausschnitt?
- Gibt es Ausschnitte, bei denen du ohne den fehlenden Kontext nicht sicher sein kannst?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Ausschnitt A: Singleton.**

Die drei Erkennungsmerkmale sind vollständig da: privater Konstruktor, statisches Feld für die einzige Instanz, statische Zugriffs-Property. Das `Lazy<T>` verrät zusätzlich, dass die threadsichere Variante aus dem [Singleton-Modul](/modules/singleton/singleton.md) verwendet wird, und `sealed` schließt Unterklassen aus. Was der Ausschnitt nicht zeigt, ist der Preis: Jede Klasse, die `Spielkonfiguration.Instanz.Sichtweite` liest, hat eine unsichtbare globale Abhängigkeit – im echten Adventure bekommt ein `Verfolger` seine Sichtweite stattdessen im Konstruktor.

**Schritt 2 — Ausschnitt B: Composite.**

Merkmal: Eine Klasse implementiert ein Interface *und* hält eine Liste desselben Interface-Typs, und ihre Operation besteht nur aus dem Weiterreichen an die Kinder. `Objektgruppe` ist das Kompositum, `IBauteil` die Komponente. Das Blatt (`Baustein`) ist nicht zu sehen, muss aber existieren, sonst würde die Rekursion nie enden. Dass `Hinzufuegen` nur in der `Objektgruppe` steht und nicht im Interface, ist die LSP-freundliche Variante aus dem [Composite-Modul](/modules/composite/composite.md).

**Schritt 3 — Ausschnitt C: Objektadapter.**

Merkmal: Die Klasse implementiert ein Ziel-Interface (`IEingabe`), hält ein Objekt einer fremden Klasse (`Gamepad`) als Feld und übersetzt in der Interface-Methode dessen Aufruf (`LiesStick`) in die erwartete Form (`Richtung?`). Der Client, der nur `IEingabe` kennt – die Spielschleife –, fehlt im Ausschnitt. Hier ist Vorsicht angebracht: Dieselbe Struktur – Interface plus Feld plus Weiterleitung – hat auch ein *Decorator* oder ein *Proxy*. Der Unterschied liegt darin, dass beim Adapter das Feld einen *anderen* Typ als das Interface hat (`Gamepad` ist kein `IEingabe`), während Decorator und Proxy dasselbe Interface wie ihr inneres Objekt implementieren. Der Typunterschied ist das entscheidende Indiz.

**Schritt 4 — Ausschnitt D: Iterator.**

Merkmal: `yield return` in einer Methode mit Rückgabetyp `IEnumerable<T>`. Das ist ein *spezifischer Iterator* wie `NachbarFelder` aus dem [Iterator-Modul](/modules/iterator/iterator.md): Der Client durchläuft die nahen Gegner per `foreach`, ohne zu wissen, dass dahinter eine `List<Gegner>` steckt. Die Enumerator-Klasse erzeugt der Compiler. Bemerkenswert ist, was *nicht* da ist: `Spielfeld` implementiert `IEnumerable<T>` nicht – ein Iterator muss nicht die ganze Klasse durchlaufbar machen.

**Schritt 5 — Ausschnitt E: keines der fünf Muster.**

Der Reflex „hier wird etwas umgewandelt, also Adapter“ trügt. Ein Adapter passt eine *Schnittstelle* an, hier wird aus einem Zeichen ein *neues Objekt* erzeugt – es gibt kein adaptiertes Objekt, das weiterlebt. `ObjektFuer` ist eine **Factory Method** (Erzeugungsmuster, nicht in dieser Vorlesung): eine Stelle, die entscheidet, welche konkrete Klasse hinter einem Zeichen steckt, während der Rest von `LevelParser` nur `Spielobjekt` kennt. Genau deshalb kostet eine neue Objektart genau eine Zeile.

**Zentrale Designentscheidungen:**

- **Struktur schlägt Namen:** Kein Ausschnitt enthält das Wort „Adapter“ oder „Composite“ im Klassennamen. Muster erkennt man an den Rollen und Beziehungen, nicht an der Benennung.
- **Ähnliche Strukturen, verschiedene Muster:** Adapter, Decorator und Proxy sehen auf den ersten Blick gleich aus. Erst die Frage „welchen Typ hat das innere Objekt?“ trennt sie.
- **Nicht jedes Umwandeln ist ein Adapter:** Wer Objekte *erzeugt*, baut eine Fabrik; wer Aufrufe *weiterleitet*, baut einen Adapter.
- **Der Kontext gehört dazu:** Ein Muster besteht aus mehreren Rollen. Wer nur eine Klasse sieht, sollte benennen, welche Rollen er voraussetzt.

</details>

## Aufgabe 2 — Abstraktion

Level sollen sich künftig nicht nur aus Textkarten, sondern auch im Code aus Bauteilen zusammensetzen lassen: einzelne Wände und Truhen, daraus Räume, daraus ein ganzer Kerker. Ein Raum muss sich als *Ganzes* verschieben und auf ein `Spielfeld` setzen lassen, und zusätzlich soll sich für jedes Bauteil die **Ausdehnung** abfragen lassen – das kleinste Rechteck, in das es passt.

- Welches Muster passt, und welche Rollen übernehmen Wand und Raum?
- Warum ist `class Raum : Spielobjekt` hier der falsche Weg? Sieh dir dafür `Spielobjekt` und `Spielfeld.Hinzufuegen` an.
- Was gehört in die gemeinsame Schnittstelle, was nur ins Kompositum?
- Wie berechnet eine Gruppe ihre Ausdehnung, ohne zu wissen, wie tief sie verschachtelt ist – und was liefert eine leere Gruppe?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Muster wählen:**

Einzelne Bauteile und Gruppen sollen über dieselben Operationen angesprochen werden, und Gruppen können Gruppen enthalten – eine rekursive Baumstruktur. Das ist Composite: `IBauteil` ist die Komponente, `Baustein` das Blatt, `Objektgruppe` das Kompositum, und ein „Raum“ ist nur eine mit Wänden gefüllte `Objektgruppe`.

**Schritt 2 — Warum kein `Raum : Spielobjekt`?**

`Spielobjekt` hat genau eine `Position` und genau ein `Symbol`; `StatischesObjekt` heißt ausdrücklich „bewegt sich nie“, und `Spielfeld.Hinzufuegen` legt statische Objekte in einem `Dictionary<Position, StatischesObjekt>` ab – ein Eintrag pro Rasterzelle. Ein Raum hat weder eine einzelne Zelle noch ein einzelnes Zeichen; er würde in das Dictionary nicht hineinpassen, und `Verschieben` gäbe es in der Basisklasse gar nicht. Die Komponente des Composite ist deshalb ein neues, eigenes Interface neben der bestehenden Hierarchie.

**Schritt 3 — Schnittstelle festlegen:**

In das Interface gehört nur, was *jedes* Bauteil sinnvoll kann. `Hinzufuegen` gehört nicht dazu – eine einzelne Wand kann nichts aufnehmen, und eine Methode, die dort immer eine Exception wirft, würde das Substitutionsprinzip verletzen.

```csharp
public readonly record struct Ausdehnung(Position LinksOben, Position RechtsUnten);

public interface IBauteil
{
    void Verschieben(int dx, int dy);
    void AufFeldSetzen(Spielfeld feld);
    Ausdehnung Umriss();
}
```

**Schritt 4 — Blatt:**

Der `Baustein` merkt sich, *wo* und *was* gebaut werden soll. Das „was“ ist eine Fabrikfunktion, weil ein `StatischesObjekt` seine Position nach dem Erzeugen nicht mehr ändert – erzeugt wird also erst beim Setzen. Seine Ausdehnung ist ein Rechteck aus einer einzigen Zelle.

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

    public Ausdehnung Umriss() => new(position, position);
}
```

**Schritt 5 — Kompositum mit rekursivem Umriss:**

Die Gruppe reicht `Verschieben` und `AufFeldSetzen` einfach durch. Für `Umriss` fragt sie jedes Kind nach dessen Rechteck und bildet das umschließende – ob ein Kind ein Baustein oder wieder eine Gruppe ist, spielt keine Rolle, weil `teil.Umriss()` im zweiten Fall dieselbe Methode eine Ebene tiefer aufruft.

```csharp
public sealed class Objektgruppe : IBauteil
{
    private readonly List<IBauteil> teile = new();

    public string Name { get; }
    public Objektgruppe(string name) => Name = name;

    public void Hinzufuegen(IBauteil teil)
    {
        if (ReferenceEquals(teil, this))
            throw new ArgumentException("Eine Gruppe kann sich nicht selbst enthalten.");
        teile.Add(teil);
    }

    public void Verschieben(int dx, int dy)
    {
        foreach (IBauteil teil in teile) teil.Verschieben(dx, dy);
    }

    public void AufFeldSetzen(Spielfeld feld)
    {
        foreach (IBauteil teil in teile) teil.AufFeldSetzen(feld);
    }

    public Ausdehnung Umriss()
    {
        if (teile.Count == 0)
            throw new InvalidOperationException("Eine leere Gruppe hat keinen Umriss.");

        IEnumerable<Ausdehnung> kinder = teile.Select(t => t.Umriss());
        return new Ausdehnung(
            new Position(kinder.Min(a => a.LinksOben.X), kinder.Min(a => a.LinksOben.Y)),
            new Position(kinder.Max(a => a.RechtsUnten.X), kinder.Max(a => a.RechtsUnten.Y)));
    }
}
```

**Schritt 6 — Verwenden:**

```csharp
Objektgruppe kammer = new("Schatzkammer");
for (int x = 0; x < 5; x++)
    kammer.Hinzufuegen(new Baustein(new Position(x, 0), p => new Wand(p)));
kammer.Hinzufuegen(new Baustein(new Position(2, 2), p => new Truhe(p, wert: 100)));

Objektgruppe kerker = new("Kerker");
kerker.Hinzufuegen(kammer);
kerker.Verschieben(1, 1);

Console.WriteLine(kerker.Umriss());
// Ausdehnung { LinksOben = (1, 1), RechtsUnten = (5, 3) }
```

Ein Aufruf an der Wurzel hat sechs Bauteile auf zwei Ebenen verschoben. `kerker` kennt nur sein eines Kind und weiß nicht, wie tief der Baum ist.

**Zentrale Designentscheidungen:**

- **Neues Interface statt Vererbung:** Das Muster wird neben die bestehende `Spielobjekt`-Hierarchie gelegt, nicht hineingezwängt.
- **`Hinzufuegen` nur im Kompositum:** Das Interface bleibt auf Operationen beschränkt, die für Blatt und Gruppe gleichermaßen gelten.
- **Rekursion ohne Tiefenwissen:** Jede Gruppe kennt nur ihre direkten Kinder; die Gesamttiefe ergibt sich aus der Verschachtelung der Aufrufe.
- **Leere Gruppe als Sonderfall:** Bei `AnzahlObjekte()` wäre `0` die natürliche Antwort, beim Umriss gibt es keine – ein leeres Rechteck bei (0, 0) wäre eine Lüge, deshalb die Exception.
- **Zyklen abfangen:** `ReferenceEquals` fängt den direkten Selbstbezug. Indirekte Zyklen (A enthält B enthält A) fängt die Prüfung nicht und führen zu einer `StackOverflowException`.

</details>

## Aufgabe 3 — Algorithmenentwurf

Die folgende Iterator-Methode soll die Gegner liefern, die dem Helden nicht weiter als `reichweite` Felder entfernt sind (Manhattan-Entfernung über `Position.Entfernung`).

```csharp
static IEnumerable<Gegner> InSichtweite(List<Gegner> gegner, Position held, int reichweite)
{
    foreach (Gegner g in gegner)
    {
        Console.WriteLine($"  pruefe {g.Name} bei {g.Position}");
        if (g.Position.Entfernung(held) <= reichweite)
            yield return g;
    }
}

Position held = new(1, 1);
List<Gegner> gegner =
[
    new Wache(new Position(3, 1)),
    new Verfolger(new Position(8, 6)),
    new Wache(new Position(1, 4)),
    new Verfolger(new Position(2, 2)),
];

IEnumerable<Gegner> nah = InSichtweite(gegner, held, 3);
Console.WriteLine("Auswahl definiert");
gegner.Add(new Wache(new Position(1, 2)));

foreach (Gegner g in nah)
    Console.WriteLine($"{g.Name} {g.Position}");
```

- Sage die vollständige Konsolenausgabe voraus, Zeile für Zeile.
- Erscheint die Wache bei (1, 2) in der Ausgabe, obwohl sie erst *nach* der Definition von `nah` hinzugefügt wurde?
- Was ändert sich, wenn die `foreach`-Schleife durch `Console.WriteLine(nah.First().Name)` ersetzt wird?
- Was passiert, wenn man direkt hinter der Schleife noch `Console.WriteLine(nah.Count())` aufruft – und was, wenn das `gegner.Add(...)` *innerhalb* der Schleife stünde?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Entfernungen berechnen:**

| Gegner | Position | Entfernung zu (1, 1) | ≤ 3? |
| :--- | :--- | :--- | :--- |
| Wache | (3, 1) | 2 + 0 = 2 | ja |
| Verfolger | (8, 6) | 7 + 5 = 12 | nein |
| Wache | (1, 4) | 0 + 3 = 3 | ja |
| Verfolger | (2, 2) | 1 + 1 = 2 | ja |
| Wache | (1, 2) | 0 + 1 = 1 | ja |

**Schritt 2 — Verzögerte Ausführung erkennen:**

`InSichtweite(gegner, held, 3)` führt *keine* Zeile des Rumpfs aus – die Methode enthält `yield`, also liefert sie nur ein Iterator-Objekt, das eine Referenz auf die Liste hält. Deshalb erscheint „Auswahl definiert“ als erste Zeile, ohne dass vorher ein „pruefe“ ausgegeben wird. Und deshalb gehört die Wache bei (1, 2) zur Iteration: Als die Schleife den Iterator startet, hat die Liste fünf Elemente.

**Schritt 3 — Ausgabe Schritt für Schritt:**

Der Iterator läuft bei jedem `MoveNext()` bis zum nächsten `yield return`, danach übernimmt der Rumpf der `foreach`-Schleife wieder.

```
Auswahl definiert
  pruefe Wache bei (3, 1)
Wache (3, 1)
  pruefe Verfolger bei (8, 6)
  pruefe Wache bei (1, 4)
Wache (1, 4)
  pruefe Verfolger bei (2, 2)
Verfolger (2, 2)
  pruefe Wache bei (1, 2)
Wache (1, 2)
```

Neun Zeilen, davon vier Gegner. Prüfzeilen und Treffer sind *verzahnt* – nicht erst alle Prüfungen, dann alle Namen. Das ist das elementweise Verhalten aus dem Modul [verzögerte Ausführung](/modules/linq_deferred_execution/linq_deferred_execution.md).

**Schritt 4 — `First()`:**

`First()` ruft `MoveNext()` genau einmal und bricht dann ab. Der Iterator läuft bis zum ersten `yield return` und nicht weiter:

```
Auswahl definiert
  pruefe Wache bei (3, 1)
Wache
```

Die übrigen vier Gegner werden nie geprüft. Das ist der praktische Nutzen der Faulheit: Bei einem Feld mit tausend Gegnern hört `First()` nach dem ersten Treffer auf.

**Schritt 5 — `Count()` nach der Schleife:**

`Count()` muss alle Elemente zählen und startet dafür einen *neuen* Enumerator – der Rumpf läuft komplett ein zweites Mal, alle fünf „pruefe“-Zeilen erscheinen erneut, und danach steht `4` in der Konsole. Wer das Ergebnis mehrfach braucht, sollte es einmal mit `ToList()` einsammeln.

**Schritt 6 — Ändern während der Iteration:**

Stünde `gegner.Add(...)` *innerhalb* der `foreach`-Schleife, würde der nächste `MoveNext()`-Aufruf eine `InvalidOperationException` werfen („Collection was modified“). Der innere `foreach` über `gegner` merkt sich beim Start eine Versionsnummer der Liste; jedes `Add` erhöht sie. Änderungen *vor* dem Start sind dagegen harmlos, weil der Iterator die Liste erst dann anfasst.

**Zentrale Designentscheidungen:**

- **Der Aufruf einer Iterator-Methode ist kostenlos:** Er erzeugt nur das Iterator-Objekt. Arbeit passiert erst beim Durchlaufen – und bei jedem Durchlaufen erneut.
- **Der Iterator sieht die Quelle zur Laufzeit:** Er hält eine Referenz auf `gegner`, keine Kopie.
- **Als Methode auf `Spielfeld` gehört die Sichtlinie dazu:** Eine echte `GegnerInSichtweite` würde zusätzlich `HatSichtlinie(g.Position, Spieler.Position)` prüfen – der teure Bresenham-Lauf passiert dann nur für die Gegner, die der Aufrufer wirklich abholt.
- **Gleicher Effekt mit LINQ:** `gegner.Where(g => g.Position.Entfernung(held) <= reichweite)` verhält sich identisch, weil `Where` selbst eine Iterator-Methode ist. Das `Console.WriteLine` im Rumpf macht das Verhalten nur sichtbar.

</details>

## Aufgabe 4 — Zerlegung

Ein Kommilitone findet es umständlich, das `Spielfeld` überall herumzureichen, und baut es zum Singleton um:

```csharp
public sealed class Spielfeld
{
    private static readonly Lazy<Spielfeld> halter =
        new(() => LevelParser.Parsen(new EingebauteLevelQuelle().Laden("Kerker")));

    public static Spielfeld Instanz => halter.Value;

    private Spielfeld(int breite, int hoehe, Spieler spieler) { /* ... */ }

    public bool IstFrei(Position p) { /* ... */ }
    public void SpielerZieht(Richtung richtung) { /* ... */ }
}

public sealed class Wache : Gegner
{
    public override Richtung? NaechsterZug()          // kein Parameter mehr!
    {
        if (!Spielfeld.Instanz.IstFrei(Position.Verschoben(Laufrichtung)))
            Laufrichtung = Umkehren(Laufrichtung);
        return Spielfeld.Instanz.IstFrei(Position.Verschoben(Laufrichtung)) ? Laufrichtung : null;
    }
}
```

Kurzfristig ist das bequem: `Gegner.NaechsterZug` braucht keinen Parameter mehr, und die Konsole kommt ohne Variable aus. Dann sollen die Tests aus Vorlesung 12 geschrieben werden – und nichts davon funktioniert mehr.

- Welche konkreten Tests lassen sich mit dieser Version nicht mehr schreiben? Sieh dir an, wie `SpielfeldTests` seine Spielfelder baut.
- Was passiert, wenn zwei Browser gleichzeitig `Adventure.Web` öffnen?
- Zerlege die Abhängigkeit: Wer soll entscheiden, welches Spielfeld eine Wache benutzt?
- `Adventure.Web/Program.cs` registriert die `ILevelQuelle` als Singleton – warum ist das in Ordnung, das `Spielfeld`-Singleton aber nicht?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das Problem benennen:**

`SpielfeldTests` baut für jeden Test ein winziges, exakt zugeschnittenes Level:

```csharp
private static Spielfeld Feld(params string[] zeilen) => LevelParser.Parsen(new Level("t", zeilen));

[Test] public void Wand_Blockiert()
{
    Spielfeld f = Feld("#####", "#@#..", "#####");
    f.SpielerZieht(Richtung.Rechts);
    Assert.That(f.Spieler.Position, Is.EqualTo(new Position(1, 1)));
}
```

Mit dem Singleton ist diese Zeile unmöglich: Der Konstruktor ist privat, `LevelParser.Parsen` kann kein Feld mehr erzeugen, und `Spielfeld.Instanz` liefert immer den großen eingebauten Kerker. Schlimmer ist der zweite Effekt: Alle Tests teilen sich *dasselbe* Objekt. Ein Test, der den Helden zum Ausgang laufen lässt, hinterlässt `Status == Gewonnen`, und der nächste Test bekommt ein beendetes Spiel – das Ergebnis hängt davon ab, in welcher Reihenfolge NUnit die Tests ausführt. Das sind exakt die Nachteile „versteckte Abhängigkeit“, „globaler Zustand“ und „schwer testbar“ aus dem [Singleton-Modul](/modules/singleton/singleton.md).

In der Weboberfläche ist es kein Testproblem, sondern ein Fehler: Zwei Browsersitzungen spielen auf demselben Spielfeld. Der Held springt für beide hin und her, und wer zuerst den Ausgang erreicht, beendet das Spiel für alle.

**Schritt 2 — Die Abhängigkeit sichtbar machen:**

Eine Wache braucht nicht *das* Spielfeld, sondern *ein* Spielfeld – nämlich das, auf dem sie gerade steht. Die einfachste Form von Dependency Injection ist hier kein Konstruktor und kein Container, sondern schlicht ein Parameter. Genau so sieht die echte Signatur im Adventure aus:

```csharp
public abstract class Gegner : BeweglichesObjekt
{
    /// <summary>Liefert die Richtung für diese Runde oder null, wenn der Gegner stehen bleibt.</summary>
    public abstract Richtung? NaechsterZug(Spielfeld feld);
}

public sealed class Wache : Gegner
{
    public override Richtung? NaechsterZug(Spielfeld feld)
    {
        if (!feld.IstFrei(Position.Verschoben(Laufrichtung)))
            Laufrichtung = Umkehren(Laufrichtung);
        return feld.IstFrei(Position.Verschoben(Laufrichtung)) ? Laufrichtung : null;
    }
}
```

Die Signatur verrät jetzt vollständig, was die Methode braucht. Und wer `Spielfeld.GegnerZiehen` liest, sieht, wer das Feld liefert: `g.NaechsterZug(this)` – das Feld reicht sich selbst herein.

**Schritt 3 — Den Konstruktor zurückgeben:**

`Spielfeld` wird wieder eine gewöhnliche Klasse mit `public Spielfeld(int breite, int hoehe, Spieler spieler)`, und `LevelParser.Parsen` erzeugt so viele Felder, wie jemand haben möchte. Wie viele es zur Laufzeit gibt, entscheidet der Aufrufer:

```csharp
// Konsole: genau eines pro Programmlauf
Spielfeld feld = LevelParser.Parsen(level);

// Blazor: eines pro Komponente, also pro Browsersitzung
private void NeuStarten() => feld = LevelParser.Parsen(LevelQuelle.Laden(levelName));

// Test: eines pro Testmethode, frisch und winzig
Spielfeld f = Feld("#####", "#@#..", "#####");
```

Jeder Test beginnt damit in einem definierten Zustand und ist von den anderen unabhängig – die Voraussetzung dafür, dass ein roter Test etwas bedeutet.

**Schritt 4 — Warum die `ILevelQuelle` ein Singleton sein darf:**

```csharp
builder.Services.AddSingleton<ILevelQuelle>(_ =>
    new TextdateiLevelQuelle(Path.Combine(AppContext.BaseDirectory, "levels")));
```

Der Unterschied liegt im Zustand und in der Sichtbarkeit. Eine `TextdateiLevelQuelle` liest Dateien und ändert dabei nichts – zwei Sitzungen können sie gefahrlos teilen, und eine zweite Instanz wäre nur Verschwendung. Vor allem aber *holt* sich `Home.razor` die Quelle nicht mit `TextdateiLevelQuelle.Instanz`, sondern bekommt sie mit `@inject ILevelQuelle LevelQuelle` hineingereicht und sieht nur das Interface. Im Test steht dort eine `EingebauteLevelQuelle` oder eine eigene Testquelle. Einmaligkeit ist damit eine Entscheidung der *Anwendung*, keine Eigenschaft der Klasse – und das Spielfeld, das sich bei jedem Zug ändert, ist ohnehin nichts, was man teilen möchte.

**Zentrale Designentscheidungen:**

- **Abhängigkeiten in die Signatur:** Was eine Methode braucht, steht in ihren Parametern. `NaechsterZug(Spielfeld feld)` ist selbsterklärend, `NaechsterZug()` mit verstecktem `Spielfeld.Instanz` nicht.
- **Veränderlicher Zustand wird nicht geteilt:** Ein Singleton ist höchstens für zustandslose oder unveränderliche Dienste vertretbar. Spielstand, Sitzungsdaten und Warenkörbe gehören nie dazu.
- **Einmaligkeit ist eine Entscheidung des Aufrufers:** Ob es ein oder zehn Spielfelder gibt, entscheidet der Code, der `new` aufruft – nicht die Klasse selbst.
- **Testbarkeit ist ein Entwurfsindikator:** Wenn sich eine Klasse nur mit einem laufenden Server, einer echten Datei oder einem globalen Objekt testen lässt, stimmt meistens der Entwurf nicht – nicht der Test.

</details>
