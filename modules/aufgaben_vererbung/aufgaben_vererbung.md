---
title: "🧩 Aufgaben und Beispiele: Vererbung"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Bei Vererbung kommt eine besondere Fähigkeit dazu: vorherzusagen, was ein Programm tut, das man nicht selbst geschrieben hat. Alle Aufgaben spielen im Adventure, dem durchgehenden Beispiel des Kurses – das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v01-vererbung`). Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Algorithmenentwurf

Die CLR wählt bei jedem Methodenaufruf nach festen Regeln die Implementierung aus. Spiele diesen Algorithmus von Hand durch und sage die Ausgabe voraus – **ohne** den Code auszuführen. (Im echten Spiel ist `Wand` versiegelt und taugt deshalb nicht für eine dreistufige Hierarchie; wir nehmen den Spieler und einen gedachten `Magier`.)

```csharp
class Spielobjekt
{
    public virtual void Beschreiben() => Console.WriteLine("Spielobjekt.Beschreiben");
    public void Betreten() => Console.WriteLine("Spielobjekt.Betreten");
    public virtual void Melden() => Console.WriteLine("Spielobjekt.Melden");
}

class Spieler : Spielobjekt
{
    public override void Beschreiben() => Console.WriteLine("Spieler.Beschreiben");
    public new void Betreten() => Console.WriteLine("Spieler.Betreten");
    public new virtual void Melden() => Console.WriteLine("Spieler.Melden");
}

class Magier : Spieler
{
    public override void Beschreiben() { Console.WriteLine("Magier.Beschreiben"); base.Beschreiben(); }
    public override void Melden() => Console.WriteLine("Magier.Melden");
}

Spielobjekt a = new Magier();
Spieler b = new Magier();
a.Beschreiben(); a.Betreten(); a.Melden();
b.Beschreiben(); b.Betreten(); b.Melden();
```

- Welche Mitglieder sind `virtual`/`override` (Laufzeittyp entscheidet), welche versteckt (Kompilierzeittyp entscheidet)?
- Was bewirkt `new virtual` in `Spieler` für die Methode `Melden` – und welche `Melden`-Methode überschreibt `Magier` damit eigentlich?
- Was bedeutet das für eine Schleife über die `List<Spielobjekt>` des Spielfelds?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Ketten sortieren:** `Beschreiben` ist eine durchgehende `virtual`/`override`-Kette von `Spielobjekt` bis `Magier`. `Betreten` ist in `Spielobjekt` nicht `virtual` und wird in `Spieler` versteckt. `Melden` ist tückisch: `Spieler` versteckt die Methode von `Spielobjekt` und beginnt mit `new virtual` eine **zweite, unabhängige** Kette. Das `override` in `Magier` überschreibt die nächstgelegene virtuelle Methode – also `Spieler.Melden`, nicht `Spielobjekt.Melden`.

**Schritt 2 — Aufrufe über `a` (Kompilierzeittyp `Spielobjekt`):**

```
Magier.Beschreiben        // virtual: Laufzeittyp Magier
Spieler.Beschreiben       // base.Beschreiben() aus Magier
Spielobjekt.Betreten      // nicht virtual: Kompilierzeittyp Spielobjekt
Spielobjekt.Melden        // Kette von Spielobjekt.Melden hat kein override
```

**Schritt 3 — Aufrufe über `b` (Kompilierzeittyp `Spieler`):**

```
Magier.Beschreiben
Spieler.Beschreiben
Spieler.Betreten          // Kompilierzeittyp ist jetzt Spieler
Magier.Melden             // Kette von Spieler.Melden, überschrieben
```

**Zentrale Designentscheidungen:**

- **Ein Objekt, zwei Verhalten:** `a` und `b` zeigen auf Objekte desselben Typs, liefern aber bei `Betreten` und `Melden` unterschiedliche Ausgaben. Genau deshalb ist Verstecken in Hierarchien ein Wartungsrisiko.
- **`new virtual` startet eine neue Kette:** Wer ein Mitglied versteckt und gleichzeitig `virtual` macht, koppelt die Basisklasse ab. Für das Spielfeld, das nur `Spielobjekt` kennt, existiert das Überschreiben in `Magier` nicht – in einer `List<Spielobjekt>` käme immer nur `Spielobjekt.Melden` heraus.
- **Regel für die Praxis:** Willst du Polymorphie, brauchst du eine ununterbrochene `virtual`/`override`-Kette bis zur Basisklasse. Genau deshalb sind `Symbol`, `IstPassierbar` und `Beschreibung()` im Spiel sauber durchgängig `virtual`.

</details>

## Aufgabe 2 — Abstraktion

Das Spielfeld soll um **Gegenstände** erweitert werden. Ein **Schlüssel** (`k`) wird eingesammelt und später gebraucht, um eine Tür zu öffnen. Ein **Trank** (`!`) heilt beim Einsammeln zwei Lebenspunkte, aber höchstens bis zum Maximum von fünf. Ein **Schatz** (`$`) bringt Punkte. Alle drei liegen auf einem Feld, tragen einen Namen, werden gezeichnet, dürfen vom Helden **betreten** werden und verschwinden beim Einsammeln vom Spielfeld.

- Welche Klasse ist die Basis, was ist gemeinsam, was ist speziell?
- Welches Mitglied muss `virtual` sein, damit das Einsammeln mit einer einzigen Codestelle im Spielfeld funktioniert?
- Wohin gehört die Regel „höchstens fünf Lebenspunkte“ – in den Trank, in den Spieler oder ins Spielfeld?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Gemeinsames abstrahieren:** Name, Position, Symbol und Beschreibung haben alle Spielobjekte – die stecken schon in `Spielobjekt`. Neu und allen drei Gegenständen gemeinsam sind zwei Dinge: Sie sind passierbar, und sie *tun etwas*, wenn der Held sie aufnimmt. Das rechtfertigt eine Zwischenklasse `Gegenstand` zwischen `Spielobjekt` und den konkreten Typen. Die Wirkung ist das, was variiert – also wird `Einsammeln` die `virtual`-Methode.

**Schritt 2 — Die Hierarchie:**

```csharp
class Gegenstand : Spielobjekt
{
    public int Punktwert { get; }

    protected Gegenstand(string name, Position position, int punktwert)
        : base(name, position)
    {
        Punktwert = punktwert;
    }

    public override bool IstPassierbar => true;

    public virtual void Einsammeln(Spieler spieler)
    {
        spieler.PunkteGutschreiben(Punktwert);
    }

    public override string Beschreibung() => base.Beschreibung() + " – kann aufgenommen werden";
}

sealed class Schluessel : Gegenstand
{
    public Schluessel(Position position) : base("Schlüssel", position, 0) { }

    public override char Symbol => 'k';
}

sealed class Schatz : Gegenstand
{
    public Schatz(Position position, int punktwert) : base("Schatz", position, punktwert) { }

    public override char Symbol => '$';
}

sealed class Trank : Gegenstand
{
    public Trank(Position position) : base("Trank", position, 0) { }

    public override char Symbol => '!';

    public override void Einsammeln(Spieler spieler)
    {
        base.Einsammeln(spieler);
        spieler.Heilen(2);
    }
}
```

**Schritt 3 — Eine Codestelle im Spielfeld:**

```csharp
public void SpielerZieht(Richtung richtung)
{
    if (!Spieler.Bewegen(richtung, this)) return;

    if (ObjektAn(Spieler.Position) is Gegenstand g)
    {
        g.Einsammeln(Spieler);
        Entfernen(g);
    }
}
```

**Zentrale Designentscheidungen:**

- **`IstPassierbar => true` steht einmal in `Gegenstand`:** Kein konkreter Gegenstand muss daran denken. Ein vergessenes `override` hätte sonst einen Schlüssel zur Folge, der wie eine Wand blockiert.
- **Die 5-Lebenspunkte-Grenze gehört in `Spieler.Heilen`:** Sie ist eine Eigenschaft des Helden, nicht des Tranks. Sonst müsste jeder heilende Gegenstand die Regel kennen und kopieren – und beim Einführen eines zweiten Trankes stünde sie zweimal im Code.
- **`Trank` ruft `base.Einsammeln(spieler)`:** Kommen später Punkte für jedes aufgenommene Objekt hinzu, profitiert der Trank automatisch davon.
- **`protected` beim Konstruktor von `Gegenstand`:** Ein „Gegenstand“ ohne nähere Bestimmung soll gar nicht erzeugbar sein. In [Vorlesung 02](/lectures/02/02.md) wird daraus eine `abstract class` – und aus dem Einsammeln ein Interface `ISammelbar`, damit auch Dinge einsammelbar sein können, die keine Gegenstände sind.
- **Genau eine `is`-Abfrage:** `is Gegenstand g` fragt nach der *Kategorie*, nicht nach dem konkreten Typ. Drei Abfragen (`is Schluessel`, `is Trank`, `is Schatz`) wären das Anzeichen für eine fehlende `virtual`-Methode.

</details>

## Aufgabe 3 — Zerlegung

Das Spiel soll sich merken, welche Felder der Held schon gesehen hat, damit die Karte nach und nach aufgedeckt wird. Ein Kollege schreibt dafür `class Feld { public int X { get; set; } public int Y { get; set; } }` und legt die besuchten Felder in ein `HashSet<Feld>`. Beim Testen stellt er fest: `besucht.Contains(new Feld { X = 1, Y = 2 })` liefert `false`, obwohl der Held genau dort war. Mit dem `readonly record struct Position` aus dem Spiel funktioniert dasselbe Programm sofort.

- Zerlege, was `HashSet<Feld>.Contains` intern in welcher Reihenfolge tut. An welchem Schritt scheitert es?
- Implementiere `Equals` und `GetHashCode` korrekt. Reicht es, nur eine der beiden zu überschreiben?
- `Feld` hat `set`-Properties. Was passiert, wenn man `X` ändert, *nachdem* das Feld im Set liegt?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — `Contains` zerlegen:** (1) `GetHashCode()` des gesuchten Felds berechnen, (2) den Bucket mit diesem Hashcode auswählen, (3) nur die Objekte in diesem Bucket mit `Equals` vergleichen. Die von `object` geerbte `GetHashCode`-Implementierung liefert für zwei verschiedene Objekte fast sicher verschiedene Werte – das gesuchte Feld landet in einem anderen Bucket, und `Equals` wird nie aufgerufen. Selbst wenn es aufgerufen würde, vergleicht das Standard-`Equals` nur Referenzen.

**Schritt 2 — Beide Methoden überschreiben:**

```csharp
class Feld
{
    public int X { get; }
    public int Y { get; }

    public Feld(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Feld f && f.GetType() == GetType() && X == f.X && Y == f.Y;
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"({X}, {Y})";
}

HashSet<Feld> besucht = new HashSet<Feld> { new Feld(1, 2) };
Console.WriteLine(besucht.Contains(new Feld(1, 2)));   // True
```

**Schritt 3 — Nur eine der beiden reicht nicht:** Nur `Equals`: falscher Bucket, `Contains` bleibt `false`. Nur `GetHashCode`: richtiger Bucket, aber der Referenzvergleich schlägt fehl. Der Vertrag lautet: `Equals` gleich ⇒ Hashcode gleich.

**Schritt 4 — Veränderlichkeit:** Wird `X` nach dem Einfügen geändert, liegt das Feld in einem Bucket, der zu seinem alten Hashcode gehört. Es ist danach weder mit den alten noch mit den neuen Koordinaten auffindbar – es ist im Set „verloren“. Deshalb sind in der Lösung die Properties nur lesbar.

**Zentrale Designentscheidungen:**

- **`readonly record struct` statt Handarbeit:** Genau diese vier Methoden (`Equals`, `GetHashCode`, `==`, `!=`) erzeugt der Compiler für `Position` automatisch. Deshalb funktioniert `o.Position == position` im Spielfeld, und deshalb kann `Position` später ohne weiteres Schlüssel eines `Dictionary` werden.
- **`f.GetType() == GetType()`:** Ohne diese Prüfung wäre ein `Feld3D : Feld` mit gleichem X und Y „gleich“ einem `Feld` – aber `feld3D.Equals(feld)` könnte anders entscheiden als `feld.Equals(feld3D)`. Gleichheit muss symmetrisch sein.
- **Unveränderlichkeit:** Objekte, die als Schlüssel dienen, sollten sich nach der Erzeugung nicht mehr ändern. Ein Feld mit anderen Koordinaten ist ein *neues* Feld – genau das drückt `Position.Verschoben` aus, das eine neue Position zurückgibt, statt die alte zu verändern.
- **`HashCode.Combine` statt `X ^ Y`:** Bei `X ^ Y` hätten (1, 2) und (2, 1) denselben Hashcode – erlaubt, aber auf einem quadratischen Spielfeld unnötig viele Kollisionen.

</details>

## Aufgabe 4 — Mustererkennung

Ein Kollege hat die ersten beiden Objektarten des Spiels per Copy&Paste geschrieben, bevor er von Vererbung wusste:

```csharp
class Wand
{
    public string Name = "Wand";
    public int X;
    public int Y;
    public char Symbol = '#';
    public bool IstPassierbar = false;

    public string Beschreibung() => $"{Name} bei ({X}, {Y})";
    public int Entfernung(int x, int y) => Math.Abs(X - x) + Math.Abs(Y - y);
}

class Tuer
{
    public string Name = "Tür";
    public int X;
    public int Y;
    public char Symbol = 'D';
    public bool IstOffen = false;

    public string Beschreibung() => $"{Name} bei ({X}, {Y})" + (IstOffen ? " (offen)" : " (verschlossen)");
    public int Entfernung(int x, int y) => Math.Abs(X - x) + Math.Abs(Y - y);
}
```

Das Spielfeld hält zwei Listen, `List<Wand>` und `List<Tuer>`, und zeichnet die Karte mit zwei fast identischen Schleifen. Für Truhen soll jetzt eine dritte Liste dazukommen.

- Welche Mitglieder wiederholen sich exakt, welche variieren nach einem Muster, welche sind einzigartig?
- Skizziere die Hierarchie. Was passiert mit `X`, `Y` und `Entfernung`?
- Wie wird aus den zwei Listen und zwei Schleifen genau eine – und wie sorgst du dafür, dass die Tür ihre Sonderregel behält, ohne dass `IstOffen` in die Basisklasse wandert?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Muster erkennen:** Exakt gleich: `Name`, `X`, `Y`, `Entfernung` und das Grundgerüst von `Beschreibung`. Nach Muster variierend: `Symbol` (`#` gegen `D`), `IstPassierbar` (fest `false` gegen „nur wenn offen“) und `Beschreibung` (die Tür hängt einen Zusatz an). Einzigartig: `IstOffen` und das Öffnen der Tür.

**Schritt 2 — Zwei Abstraktionen, nicht eine:** `X` und `Y` treten immer paarweise auf, und `Entfernung` rechnet nur mit ihnen – das ist gar kein Spielobjekt-Thema, sondern ein eigener **Wert**. Er wird zur `Position`, und `Entfernung` wandert dorthin. Der Rest wird zur Basisklasse `Spielobjekt`. Aus Feldern werden dabei Properties: `Name` ist nach der Erzeugung fest, `Position` darf nur das Objekt selbst ändern.

```csharp
public readonly record struct Position(int X, int Y)
{
    public int Entfernung(Position andere) => Math.Abs(X - andere.X) + Math.Abs(Y - andere.Y);
    public override string ToString() => $"({X}, {Y})";
}

public class Spielobjekt
{
    public string Name { get; }
    public Position Position { get; protected set; }

    public Spielobjekt(string name, Position position)
    {
        Name = name;
        Position = position;
    }

    public virtual char Symbol => '?';
    public virtual bool IstPassierbar => false;
    public virtual string Beschreibung() => $"{Name} bei {Position}";
}

public sealed class Wand : Spielobjekt
{
    public Wand(Position position) : base("Wand", position) { }

    public override char Symbol => '#';
}

public class Tuer : Spielobjekt
{
    public bool IstOffen { get; private set; }

    public Tuer(Position position) : base("Tür", position) { }

    public override char Symbol => 'D';
    public override bool IstPassierbar => IstOffen;

    public void Oeffnen() => IstOffen = true;

    public override string Beschreibung()
        => base.Beschreibung() + (IstOffen ? " (offen)" : " (verschlossen)");
}
```

**Schritt 3 — Eine Liste, eine Schleife:**

```csharp
private readonly List<Spielobjekt> objekte = new();

public void Hinzufuegen(Spielobjekt objekt) => objekte.Add(objekt);

public bool IstFrei(Position p)
{
    Spielobjekt? o = ObjektAn(p);
    return o is null || o.IstPassierbar;
}
```

**Zentrale Designentscheidungen:**

- **`IstOffen` bleibt in `Tuer`:** Eine Wand hat keinen Öffnungszustand. In die Basisklasse gehört nur, was *jedes* Spielobjekt hat – sonst bekommt man eine aufgeblähte Basisklasse voller Felder, die die meisten Erben ignorieren.
- **Aus einem Feld wird eine `virtual`-Property:** `bool IstPassierbar = false` lässt sich nicht überschreiben, `public virtual bool IstPassierbar => false` schon. Erst dadurch kann die Tür ihre Durchlässigkeit *berechnen*, statt sie zu speichern – und `IstFrei` muss nie wissen, worum es sich handelt.
- **`Position` als eigener Typ:** Zwei zusammengehörige `int`-Felder, die überall zusammen weitergereicht werden, sind fast immer ein verstecktes Konzept. Der `record struct` liefert obendrein `==` und `GetHashCode` gratis (siehe [Die Basisklasse `object`](/modules/object_basisklasse/object_basisklasse.md)).
- **Prüfstein:** Die Truhe kostet jetzt eine kleine Klasse und keine Zeile im Spielfeld. Genau das war das Ziel des Refactorings – und genau dieser Stand ist der Ausgangspunkt der nächsten Vorlesung.

</details>
