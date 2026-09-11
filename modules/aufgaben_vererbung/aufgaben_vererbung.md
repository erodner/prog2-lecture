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

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking*: die Fähigkeit, Probleme so zu strukturieren, dass ein Computer sie lösen kann. Dazu gehören Abstraktion, Zerlegung, Mustererkennung und Algorithmenentwurf. Bei Vererbung kommt eine besondere Fähigkeit dazu: vorherzusagen, was ein Programm tut, das man nicht selbst geschrieben hat. Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Algorithmenentwurf

Die CLR wählt bei jedem Methodenaufruf nach festen Regeln die Implementierung aus. Spiele diesen Algorithmus von Hand durch und sage die Ausgabe voraus – **ohne** den Code auszuführen:

```csharp
class Roboter
{
    public virtual void Arbeiten() => Console.WriteLine("Roboter.Arbeiten");
    public void Abschalten() => Console.WriteLine("Roboter.Abschalten");
    public virtual void Piepen() => Console.WriteLine("Roboter.Piepen");
}

class Putzroboter : Roboter
{
    public override void Arbeiten() => Console.WriteLine("Putzroboter.Arbeiten");
    public new void Abschalten() => Console.WriteLine("Putzroboter.Abschalten");
    public new virtual void Piepen() => Console.WriteLine("Putzroboter.Piepen");
}

class Fensterputzroboter : Putzroboter
{
    public override void Arbeiten() { Console.WriteLine("Fensterputzroboter.Arbeiten"); base.Arbeiten(); }
    public override void Piepen() => Console.WriteLine("Fensterputzroboter.Piepen");
}

Roboter a = new Fensterputzroboter();
Putzroboter b = new Fensterputzroboter();
a.Arbeiten(); a.Abschalten(); a.Piepen();
b.Arbeiten(); b.Abschalten(); b.Piepen();
```

- Welche Methoden sind `virtual`/`override` (Laufzeittyp entscheidet), welche versteckt (Kompilierzeittyp entscheidet)?
- Was bewirkt `new virtual` in `Putzroboter` für die Methode `Piepen` – und welche `Piepen`-Methode überschreibt `Fensterputzroboter` damit eigentlich?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die Ketten sortieren:** `Arbeiten` ist eine durchgehende `virtual`/`override`-Kette von `Roboter` bis `Fensterputzroboter`. `Abschalten` ist in `Roboter` nicht `virtual` und wird in `Putzroboter` versteckt. `Piepen` ist tückisch: `Putzroboter` versteckt die Methode von `Roboter` und beginnt mit `new virtual` eine **zweite, unabhängige** Kette. Das `override` in `Fensterputzroboter` überschreibt die nächstgelegene virtuelle Methode – also `Putzroboter.Piepen`, nicht `Roboter.Piepen`.

**Schritt 2 — Aufrufe über `a` (Kompilierzeittyp `Roboter`):**

```
Fensterputzroboter.Arbeiten   // virtual: Laufzeittyp Fensterputzroboter
Putzroboter.Arbeiten          // base.Arbeiten() aus Fensterputzroboter
Roboter.Abschalten            // nicht virtual: Kompilierzeittyp Roboter
Roboter.Piepen                // Kette von Roboter.Piepen hat kein override
```

**Schritt 3 — Aufrufe über `b` (Kompilierzeittyp `Putzroboter`):**

```
Fensterputzroboter.Arbeiten
Putzroboter.Arbeiten
Putzroboter.Abschalten        // Kompilierzeittyp ist jetzt Putzroboter
Fensterputzroboter.Piepen     // Kette von Putzroboter.Piepen, überschrieben
```

**Zentrale Designentscheidungen:**

- **Ein Objekt, zwei Verhalten:** `a` und `b` zeigen auf Objekte desselben Typs, liefern aber bei `Abschalten` und `Piepen` unterschiedliche Ausgaben. Genau deshalb ist Verstecken in Hierarchien ein Wartungsrisiko.
- **`new virtual` startet eine neue Kette:** Wer eine Methode versteckt und gleichzeitig `virtual` macht, koppelt die Basisklasse ab. Für Code, der nur `Roboter` kennt, existiert das Überschreiben in `Fensterputzroboter` nicht.
- **Regel für die Praxis:** Willst du Polymorphie, brauchst du eine ununterbrochene `virtual`/`override`-Kette bis zur Basisklasse.

</details>

## Aufgabe 2 — Abstraktion

Eine Firma verwaltet **Mitarbeiter**, **Manager** und **Praktikanten**. Alle haben Namen und Personalnummer. Ein Mitarbeiter bekommt ein festes Monatsgehalt. Ein Manager bekommt zusätzlich einen Bonus von 5 % pro geführter Person. Ein Praktikant bekommt eine Pauschale pro Monat, die aber gesetzlich mindestens 600 € betragen muss. Die Buchhaltung braucht die monatliche Lohnsumme über *alle* Beschäftigten.

- Welche Klasse ist die Basis, was ist gemeinsam, was ist speziell?
- Welche Methode muss `virtual` sein, damit die Lohnsumme mit einer einzigen Schleife berechnet werden kann?
- Ist ein Praktikant ein Mitarbeiter, obwohl er ein anderes Gehaltsmodell hat? Wo landet die 600-€-Regel?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Gemeinsames abstrahieren:** Name, Personalnummer und die Fähigkeit, ein Monatsgehalt zu liefern, haben alle drei. Das ist die Basisklasse `Mitarbeiter`. Manager und Praktikant *sind* Mitarbeiter – die Buchhaltung behandelt sie identisch, nur die Berechnung unterscheidet sich. Also ist `Monatsgehalt()` die `virtual`-Methode.

**Schritt 2 — Die Hierarchie:**

```csharp
class Mitarbeiter
{
    public string Name { get; }
    public int Personalnummer { get; }
    protected decimal Grundgehalt { get; }

    public Mitarbeiter(string name, int personalnummer, decimal grundgehalt)
    {
        Name = name;
        Personalnummer = personalnummer;
        Grundgehalt = grundgehalt;
    }

    public virtual decimal Monatsgehalt() => Grundgehalt;

    public override string ToString() => $"{Name} ({GetType().Name}): {Monatsgehalt():F2} EUR";
}

class Manager : Mitarbeiter
{
    public List<Mitarbeiter> Team { get; } = new List<Mitarbeiter>();

    public Manager(string name, int personalnummer, decimal grundgehalt)
        : base(name, personalnummer, grundgehalt) { }

    public override decimal Monatsgehalt() => base.Monatsgehalt() * (1 + 0.05m * Team.Count);
}

class Praktikant : Mitarbeiter
{
    public Praktikant(string name, int personalnummer, decimal pauschale)
        : base(name, personalnummer, Math.Max(pauschale, 600m)) { }
}
```

**Schritt 3 — Die Lohnsumme:**

```csharp
List<Mitarbeiter> belegschaft = new List<Mitarbeiter>
{
    new Mitarbeiter("Anna", 1, 3500m),
    new Manager("Ben", 2, 5000m),
    new Praktikant("Cem", 3, 450m)
};
decimal summe = 0;
foreach (Mitarbeiter m in belegschaft)
{
    summe += m.Monatsgehalt();
}
Console.WriteLine(summe);   // 9100.00 – Ben hat noch kein Team, Cem bekommt 600
```

**Zentrale Designentscheidungen:**

- **Die 600-€-Regel im Konstruktor von `Praktikant`:** Sie ist eine Eigenschaft des Praktikantenvertrags, nicht der Gehaltsberechnung. `Praktikant` braucht dadurch kein `override` – die geerbte Methode reicht.
- **`Manager` ruft `base.Monatsgehalt()`:** Ändert sich später die Grundberechnung (etwa um Zulagen), profitiert der Manager automatisch.
- **`ToString` in der Basisklasse mit `GetType().Name`:** Eine einzige Implementierung liefert für alle Erben die richtige Typbezeichnung – kein Kopieren in jede Klasse.
- **`Grundgehalt` ist `protected`:** Erben dürfen es lesen, die Buchhaltung sieht nur `Monatsgehalt()`.

</details>

## Aufgabe 3 — Zerlegung

Gegeben ist `class Punkt { public int X { get; set; } public int Y { get; set; } }`. Ein Programm legt Punkte in ein `HashSet<Punkt>` und stellt fest: `set.Contains(new Punkt { X = 1, Y = 2 })` liefert `false`, obwohl genau dieser Punkt eingefügt wurde.

- Zerlege, was `HashSet<Punkt>.Contains` intern in welcher Reihenfolge tut. An welchem Schritt scheitert es?
- Implementiere `Equals` und `GetHashCode` korrekt. Reicht es, nur eine der beiden zu überschreiben?
- Der Punkt hat `set`-Properties. Was passiert, wenn man `X` ändert, *nachdem* der Punkt im Set liegt?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — `Contains` zerlegen:** (1) `GetHashCode()` des gesuchten Punkts berechnen, (2) den Bucket mit diesem Hashcode auswählen, (3) nur die Objekte in diesem Bucket mit `Equals` vergleichen. Die Standardimplementierung von `GetHashCode` liefert für zwei verschiedene Objekte fast sicher verschiedene Werte – der gesuchte Punkt landet in einem anderen Bucket, und `Equals` wird nie aufgerufen. Selbst wenn es aufgerufen würde, vergleicht das Standard-`Equals` nur Referenzen.

**Schritt 2 — Beide Methoden überschreiben:**

```csharp
class Punkt
{
    public int X { get; }
    public int Y { get; }

    public Punkt(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object? obj)
    {
        return obj is Punkt p && p.GetType() == GetType() && X == p.X && Y == p.Y;
    }

    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"({X}, {Y})";
}

HashSet<Punkt> set = new HashSet<Punkt> { new Punkt(1, 2) };
Console.WriteLine(set.Contains(new Punkt(1, 2)));   // True
```

**Schritt 3 — Nur eine der beiden reicht nicht:** Nur `Equals`: falscher Bucket, `Contains` bleibt `false`. Nur `GetHashCode`: richtiger Bucket, aber der Referenzvergleich schlägt fehl. Der Vertrag lautet: `Equals` gleich ⇒ Hashcode gleich.

**Schritt 4 — Veränderlichkeit:** Wird `X` nach dem Einfügen geändert, liegt der Punkt in einem Bucket, der zu seinem alten Hashcode gehört. Er ist danach weder mit dem alten noch mit dem neuen Wert auffindbar – er ist im Set „verloren“. Deshalb sind in der Lösung die Properties nur lesbar.

**Zentrale Designentscheidungen:**

- **`p.GetType() == GetType()`:** Ohne diese Prüfung wäre ein `Punkt3D : Punkt` mit gleichem X und Y „gleich“ einem `Punkt` – aber `punkt3D.Equals(punkt)` könnte anders entscheiden als `punkt.Equals(punkt3D)`. Gleichheit muss symmetrisch sein.
- **Unveränderlichkeit:** Objekte, die als Schlüssel dienen, sollten sich nach der Erzeugung nicht mehr ändern. Ein Punkt mit anderen Koordinaten ist ein *neuer* Punkt.
- **`HashCode.Combine` statt `X ^ Y`:** Bei `X ^ Y` hätten (1, 2) und (2, 1) denselben Hashcode – erlaubt, aber unnötig viele Kollisionen.

</details>

## Aufgabe 4 — Mustererkennung

Ein Kollege hat für einen Ticketshop drei Klassen geschrieben – per Copy&Paste. `Sitzplatz` hat `Veranstaltung`, `Preis` (mit Prüfung auf positiv), `Reihe`, `Nummer` und `Endpreis()` (Preis plus 10 % Gebühr). `Stehplatz` hat `Veranstaltung`, `Preis` (gleiche Prüfung), `Block` und dasselbe `Endpreis()`. `Logenplatz` hat alles von `Sitzplatz` plus `MitCatering` und ein `Endpreis()`, das zusätzlich 25 € Pauschale aufschlägt. Jede Preisprüfung steht dreimal im Code, `Endpreis()` ebenfalls.

- Welche Mitglieder wiederholen sich exakt, welche variieren nach einem Muster, welche sind einzigartig?
- Skizziere die Hierarchie. Ist `Logenplatz` ein `Sitzplatz` oder nur ein Platz mit zufällig gleichen Feldern?
- Wie sorgst du dafür, dass die 10-%-Regel nur an einer Stelle steht, obwohl `Logenplatz` sie erweitert?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Muster erkennen:** Exakt gleich: `Veranstaltung`, `Preis` mit Prüfung, die Gebührenregel. Nach Muster variierend: `Endpreis()` – die Grundregel bleibt, `Logenplatz` ergänzt sie. Einzigartig: `Reihe`/`Nummer` (Sitzplatz), `Block` (Stehplatz), `MitCatering` (Logenplatz).

**Schritt 2 — Hierarchie:** `Platz` als Basis mit den gemeinsamen Teilen. `Sitzplatz : Platz`, `Stehplatz : Platz`. `Logenplatz : Sitzplatz`, weil ein Logenplatz fachlich ein Sitzplatz mit Reihe und Nummer *ist* – überall, wo ein Sitzplatz gebucht wird, darf eine Loge stehen.

```csharp
class Platz
{
    private decimal preis;
    public string Veranstaltung { get; }
    public decimal Preis
    {
        get => preis;
        set => preis = value > 0 ? value : throw new ArgumentException("Preis muss positiv sein.");
    }

    public Platz(string veranstaltung, decimal preis)
    {
        Veranstaltung = veranstaltung;
        Preis = preis;
    }

    public virtual decimal Endpreis() => Preis * 1.10m;
}

class Sitzplatz : Platz
{
    public int Reihe { get; }
    public int Nummer { get; }

    public Sitzplatz(string veranstaltung, decimal preis, int reihe, int nummer)
        : base(veranstaltung, preis)
    {
        Reihe = reihe;
        Nummer = nummer;
    }
}

class Stehplatz : Platz
{
    public string Block { get; }

    public Stehplatz(string veranstaltung, decimal preis, string block)
        : base(veranstaltung, preis)
    {
        Block = block;
    }
}

class Logenplatz : Sitzplatz
{
    public bool MitCatering { get; }

    public Logenplatz(string veranstaltung, decimal preis, int reihe, int nummer, bool mitCatering)
        : base(veranstaltung, preis, reihe, nummer)
    {
        MitCatering = mitCatering;
    }

    public override decimal Endpreis() => base.Endpreis() + 25m;
}
```

**Zentrale Designentscheidungen:**

- **Die Prüfung steht einmal in `Platz.Preis`:** Der Konstruktor setzt über die Property, damit auch die Erzeugung geprüft wird. Alle Erben bekommen die Regel geschenkt.
- **`Endpreis()` ist `virtual`, aber nur `Logenplatz` überschreibt:** `Sitzplatz` und `Stehplatz` brauchen kein `override` – die Basisimplementierung passt. `Logenplatz` erweitert mit `base.Endpreis()` statt die 10 % abzuschreiben.
- **Drei Ebenen statt zwei:** `Logenplatz` erbt von `Sitzplatz`, nicht von `Platz`, weil `Reihe` und `Nummer` sonst erneut kopiert werden müssten – dasselbe Copy&Paste-Problem eine Ebene tiefer.
- **Prüfstein:** Ändert sich die Gebühr auf 12 %, ist genau eine Zeile betroffen. Das war das Ziel des Refactorings.

</details>
