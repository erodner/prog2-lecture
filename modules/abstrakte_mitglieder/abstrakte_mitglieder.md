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

Eine abstrakte Klasse ist kein leeres Gerüst. Sie ist eher wie eine Rezeptvorlage, in der die meisten Schritte schon ausformuliert sind – nur an einzelnen Stellen steht „hier die Hauptzutat einsetzen“. Die Vorlage kann den ganzen Ablauf beschreiben, obwohl sie die Zutat nicht kennt, weil sie sich darauf verlässt, dass jedes konkrete Rezept sie liefert. In diesem Modul schauen wir uns an, was in einer abstrakten Klasse alles abstrakt sein darf, wie konkreter Code in der Basisklasse abstrakte Mitglieder nutzen kann und wann `virtual` statt `abstract` die bessere Wahl ist.

## Nicht nur Methoden: abstrakte Properties

Im [vorigen Modul](/modules/abstrakte_klassen/abstrakte_klassen.md) haben wir es schon benutzt, ohne groß darüber zu reden: `Flaeche` und `Umfang` in `Figur` sind keine Methoden, sondern **abstrakte Properties**. Alles, was in einer Klasse überschreibbar sein kann, darf auch abstrakt sein – Methoden, Properties, Indexer und Ereignisse.

```csharp
public abstract class Figur
{
    // ...
    public abstract double Flaeche { get; }
    public abstract double Umfang { get; }
}
```

Das `{ get; }` legt fest, dass die Unterklasse einen Getter liefern muss – und nur einen Getter. Eine berechnete Fläche kann man schließlich nicht setzen. Im `Dreieck` des Geometrieeditors sieht die Implementierung so aus:

```csharp
public class Dreieck : Figur
{
    public double SeiteA { get; set; }
    public double SeiteB { get; set; }
    public double SeiteC { get; set; }

    // Konstruktor mit base(name, x, y) und Prüfung der Dreiecksungleichung ...

    public override double Umfang => SeiteA + SeiteB + SeiteC;

    // Satz des Heron
    public override double Flaeche
    {
        get
        {
            double s = Umfang / 2;
            return Math.Sqrt(s * (s - SeiteA) * (s - SeiteB) * (s - SeiteC));
        }
    }
}
```

Ob die Unterklasse einen Ausdrucksrumpf (`=>`) oder einen vollständigen Getter-Block verwendet, ist ihr überlassen – die Basisklasse schreibt nur die Signatur vor. Interessant ist, dass `Flaeche` hier selbst auf `Umfang` zugreift: Innerhalb der Klasse sind beide ganz normale Properties.

## Abstrakt und konkret gemischt

Der eigentliche Reiz abstrakter Klassen liegt darin, dass sie beides enthalten können: Versprechen *und* fertigen Code. `Verschieben` ist so ein Fall – die Verschiebung funktioniert für alle Figuren gleich, also steht sie fertig in der Basisklasse. Noch interessanter ist `Beschreibung()`:

```csharp
public abstract class Figur
{
    // ...
    public void Verschieben(double dx, double dy)
    {
        X += dx;
        Y += dy;
    }

    public virtual string Beschreibung()
    {
        return $"{Name} bei ({X}, {Y}) mit Fläche {Flaeche:F2}";
    }

    public override string ToString() => Beschreibung();
}
```

`Beschreibung()` verwendet `Flaeche` – ein Property, das in `Figur` keinen Rumpf hat. Wie kann das funktionieren? Die Antwort liegt im Laufzeittyp: `Beschreibung()` wird nie auf einer „reinen“ `Figur` aufgerufen, denn die kann es nicht geben. Zur Laufzeit steckt immer ein `Rechteck`, ein `Kreis` oder ein `Dreieck` dahinter, und dessen `Flaeche` wird verwendet.

```csharp
Figur f = new Kreis("K1", 2, 3, 1);
Console.WriteLine(f.Beschreibung());
// K1 bei (2, 3) mit Fläche 3,14 (r = 1)
```

Die Ausgabe enthält am Ende `(r = 1)`, weil `Kreis` die Methode überschreibt und `base.Beschreibung()` um den Radius ergänzt. Das Grundgerüst der Beschreibung liegt in der Basisklasse, die variablen Teile liefern die Unterklassen. Dieses Muster – die Basisklasse definiert den Ablauf, die Unterklassen füllen einzelne Schritte – begegnet uns in Vorlesung 07 unter dem Namen *Template Method* wieder.

Eine abstrakte Klasse darf abstrakte Mitglieder in ihrem eigenen Code aufrufen, weil zum Zeitpunkt des Aufrufs garantiert ein konkretes Objekt existiert, das sie überschrieben hat. Der Compiler prüft das: Eine nicht-abstrakte Unterklasse ohne vollständige `override`s kompiliert nicht.
{: .notice--primary}

## Konkrete Methoden mit `Figur` als Parameter

Da `Figur` ein ganz normaler Typ ist, kann die Basisklasse auch Methoden anbieten, die andere Figuren entgegennehmen. Als Beispiel ergänzen wir eine einfache Kollisionsprüfung. Dafür braucht die Basisklasse eine Ausdehnung jeder Figur – die kennt sie nicht, also fordert sie diese abstrakt ein:

```csharp
public abstract class Figur
{
    // ...
    public abstract double Ausdehnung { get; }   // Kantenlänge des umschließenden Quadrats

    public bool KollidiertMit(Figur andere)
    {
        double abstandX = Math.Abs(X - andere.X);
        double abstandY = Math.Abs(Y - andere.Y);
        double grenze = (Ausdehnung + andere.Ausdehnung) / 2;
        return abstandX < grenze && abstandY < grenze;
    }
}

// in Rechteck:  public override double Ausdehnung => Math.Max(Breite, Hoehe);
// in Kreis:     public override double Ausdehnung => 2 * Radius;
```

`KollidiertMit` ist vollständig implementiert und funktioniert für jede Kombination von Figuren: Rechteck gegen Kreis, Kreis gegen Dreieck. Die Methode weiß nur, dass beide Beteiligten eine `Ausdehnung` haben – woher die kommt, entscheidet der jeweilige Laufzeittyp. Diese Erweiterung ist nicht Teil des Geometrieeditors im Repository, sondern zeigt das Prinzip.

```csharp
Figur r = new Rechteck("R1", 0, 0, 4, 2);
Figur k = new Kreis("K1", 3, 1, 1);
Console.WriteLine(r.KollidiertMit(k)); // True
k.Verschieben(5, 0);
Console.WriteLine(r.KollidiertMit(k)); // False
```

## `virtual` oder `abstract`?

Beide Schlüsselwörter erlauben `override` in Unterklassen, und beide führen zu polymorphen Aufrufen. Der Unterschied liegt darin, ob die Basisklasse selbst eine sinnvolle Implementierung anbieten kann:

| | `virtual` | `abstract` |
| :--- | :--- | :--- |
| Rumpf in der Basisklasse | ja, eine Standardimplementierung | nein, nur die Signatur |
| Unterklasse muss überschreiben | nein, darf | ja, muss |
| Erlaubt in normalen Klassen | ja | nein, nur in `abstract class` |
| `base.Methode()` aus dem `override` aufrufbar | ja | nein, es gibt keinen Rumpf |

Im Geometrieeditor ist `Beschreibung()` `virtual`, weil eine allgemeine Beschreibung mit Name, Position und Fläche für alle Figuren brauchbar ist – `Dreieck` überschreibt sie gar nicht. `Flaeche` dagegen ist `abstract`, weil es keine Standardformel gibt, die für irgendeine Figur richtig wäre. Eine Faustregel: Wenn dir für die Basisklasse nur ein Rumpf wie `return 0;` oder `throw new NotImplementedException()` einfällt, ist das Mitglied in Wahrheit abstrakt.

Ein häufiger Fehler ist, ein Mitglied `virtual` mit einem sinnlosen Rumpf zu machen, um die Basisklasse „instanziierbar“ zu halten. Dann kompiliert zwar `new Figur(...)`, aber jedes vergessene `override` in einer Unterklasse fällt erst zur Laufzeit als falsches Ergebnis auf – statt sofort als Compilerfehler.
{: .notice--warning}

Übung: Verschiebe `Beschreibung()` gedanklich von `virtual` nach `abstract`. Welche Klassen müssten sich ändern, und was ginge mit `base.Beschreibung()` in `Rechteck` und `Kreis` verloren? Implementiere anschließend ein `abstract void Zeichnen()` in `Figur` und eine konkrete Methode `ZeichneVor(Figur andere)`, die zuerst `andere.Zeichnen()` und dann `Zeichnen()` aufruft – welche Methoden laufen bei `kreis.ZeichneVor(rechteck)`?
{: .notice--info}

## Weitere Quellen

- [Abstrakte und versiegelte Klassen und Klassenmember – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/abstract-and-sealed-classes-and-class-members)
- [virtual (C#-Referenz) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/virtual)
- [Eigenschaften – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/properties)
