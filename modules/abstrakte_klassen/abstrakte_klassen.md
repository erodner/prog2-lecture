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

Stell dir einen Bauplan für „ein Gebäude“ vor: Er legt fest, dass es ein Fundament, Wände und ein Dach gibt – aber niemand kann nach diesem Plan bauen, weil er nicht sagt, ob es ein Einfamilienhaus oder eine Lagerhalle wird. Trotzdem ist der Plan nützlich: Jeder konkrete Bauplan muss diese Punkte ausfüllen. Genau das leistet eine **abstrakte Klasse** in C#. Sie fasst zusammen, was alle Unterklassen gemeinsam haben, schreibt vor, was jede Unterklasse selbst liefern muss, und lässt sich bewusst nicht instanziieren. In diesem Modul entsteht dabei die Klasse `Figur`, der Kern des Geometrieeditors, der uns durch die ganze Vorlesung begleiten wird.

## Wir müssen nochmal über Copy&Paste reden

Angenommen, wir wollen in einem Zeichenprogramm Rechtecke, Kreise und Dreiecke verwalten. Jede Figur hat einen Namen und eine Position (`X` als Abstand vom linken, `Y` als Abstand vom oberen Rand), und jede kann verschoben werden. Ohne Vererbung sieht das schnell so aus:

```csharp
class Rechteck
{
    public string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Breite { get; set; }
    public double Hoehe { get; set; }

    public void Verschieben(double dx, double dy) { X += dx; Y += dy; }
}

class Kreis
{
    public string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Radius { get; set; }

    public void Verschieben(double dx, double dy) { X += dx; Y += dy; }
}
```

`Name`, `X`, `Y` und `Verschieben` sind zweimal identisch vorhanden – beim Dreieck dann ein drittes Mal. Aus der [Vererbung](/modules/vererbung_grundlagen/vererbung_grundlagen.md) kennen wir die Lösung: Gemeinsames wandert in eine Basisklasse `Figur`. Aber wie sieht diese Basisklasse aus? Was ist die Fläche einer „Figur“, die weder Rechteck noch Kreis ist? Es gibt keine sinnvolle Antwort – und genau das ist der Punkt, an dem eine normale Basisklasse nicht mehr reicht.

## Die abstrakte Klasse `Figur`

Mit dem Schlüsselwort `abstract` erklären wir eine Klasse zu einer reinen Verallgemeinerung: Sie beschreibt keine konkrete Figur, sondern nur, was alle Figuren gemeinsam haben. Und sie darf **abstrakte Methoden** enthalten – Methoden, die nur aus ihrer Signatur bestehen und keinen Rumpf haben.

```csharp
public abstract class Figur
{
    public string Name { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    protected Figur(string name, double x, double y)
    {
        Name = name;
        X = x;
        Y = y;
    }

    public abstract double Flaeche { get; }
    public abstract double Umfang { get; }

    public void Verschieben(double dx, double dy)
    {
        X += dx;
        Y += dy;
    }
}
```

Zwei Dinge fallen auf. Erstens stehen hinter `Flaeche` und `Umfang` keine Berechnungen, nur ein `{ get; }` – die Basisklasse verspricht, dass es diese Properties gibt, überlässt die Berechnung aber den Unterklassen. Zweitens ist der Konstruktor `protected`: Er wird nur von abgeleiteten Klassen über `base(...)` aufgerufen, denn von außen kann ohnehin niemand eine `Figur` erzeugen. Das ist die Datei `Figur.cs` aus dem Geometrieeditor, hier nur um ein paar Zeilen gekürzt, die wir uns im nächsten Modul ansehen.

Das vollständige Projekt findest du im Repository unter `examples/04_blazor/Geometrieeditor`. In der Originaldatei stehen über der Klasse zusätzlich einige `[JsonPolymorphic]`- und `[JsonDerivedType]`-Attribute – die brauchen wir erst in Vorlesung 09, wenn wir Figuren als JSON speichern, und lassen sie bis dahin weg.
{: .notice--primary}

## Keine Objekte aus abstrakten Klassen

Was passiert, wenn man trotzdem versucht, eine `Figur` zu erzeugen?

```csharp
Figur f = new Figur("Irgendwas", 0, 0);
// error CS0144: Eine Instanz der abstrakten Klasse "Figur" kann nicht erstellt werden.
```

Der Compiler verweigert das – und das ist genau gewollt. Ein Objekt, das eine `Flaeche` verspricht, aber keine berechnen kann, wäre ein Widerspruch. Eine abstrakte Klasse ist ein Bauplan für Baupläne: Sie existiert nur, damit andere Klassen von ihr erben.

Eine Klasse, die auch nur ein einziges abstraktes Mitglied enthält, muss selbst als `abstract` markiert sein. Umgekehrt darf eine abstrakte Klasse durchaus ohne abstrakte Mitglieder auskommen – dann drückt `abstract` nur aus, dass Instanzen keinen Sinn ergeben.
{: .notice--warning}

## Abgeleitete Klassen müssen liefern

Wer von `Figur` erbt, übernimmt den Vertrag: Jede nicht-abstrakte Unterklasse **muss** alle abstrakten Mitglieder mit `override` implementieren. Vergisst man eines, kompiliert die Klasse nicht.

```csharp
public class Rechteck : Figur
{
    public double Breite { get; set; }
    public double Hoehe { get; set; }

    public Rechteck(string name, double x, double y, double breite, double hoehe)
        : base(name, x, y)
    {
        Breite = breite;
        Hoehe = hoehe;
    }

    public override double Flaeche => Breite * Hoehe;
    public override double Umfang => 2 * (Breite + Hoehe);
}

public class Kreis : Figur
{
    public double Radius { get; set; }

    public Kreis(string name, double x, double y, double radius)
        : base(name, x, y)
    {
        Radius = radius;
    }

    public override double Flaeche => Math.PI * Radius * Radius;
    public override double Umfang => 2 * Math.PI * Radius;
}
```

Das `override` ist dasselbe Schlüsselwort wie bei [`virtual`-Methoden](/modules/virtual_override/virtual_override.md) – nur dass es hier keine Wahl ist, sondern Pflicht. `Name`, `X`, `Y` und `Verschieben` erben beide Klassen unverändert; der Copy&Paste-Code ist verschwunden. Das `Dreieck` aus dem Geometrieeditor funktioniert nach demselben Muster mit drei Seitenlängen und dem Satz des Heron für die Fläche.

## Polymorphie mit abstrakten Klassen

Der eigentliche Gewinn zeigt sich, sobald wir verschiedene Figuren gemeinsam behandeln. Als Kompilierzeittyp ist `Figur` völlig in Ordnung – nur `new Figur(...)` ist verboten:

```csharp
List<Figur> figuren = new()
{
    new Rechteck("R1", 0, 0, 4, 3),
    new Kreis("K1", 10, 10, 1),
    new Dreieck("D1", 5, 5, 3, 4, 5)
};

foreach (Figur f in figuren)
{
    f.Verschieben(1, 1);
    Console.WriteLine($"{f.Name}: Fläche {f.Flaeche:F2}, Umfang {f.Umfang:F2}");
}
// R1: Fläche 12,00, Umfang 14,00
// K1: Fläche 3,14, Umfang 6,28
// D1: Fläche 6,00, Umfang 12,00
```

Die Schleife weiß nicht, welche konkrete Figur sie gerade in der Hand hat. Trotzdem liefert `f.Flaeche` jedes Mal das richtige Ergebnis, weil – wie beim [Laufzeittyp](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md) besprochen – die Implementierung des tatsächlichen Objekts aufgerufen wird. Eine abstrakte Methode ist damit automatisch polymorph; ein `virtual` braucht sie nicht.

Genau diese `List<Figur>` ist das Herz des Geometrieeditors: Die Klasse `FigurenVerwaltung` hält eine solche Liste, summiert Flächen und sucht Figuren nach Namen – ohne je zu wissen, ob es sich um Rechtecke oder Kreise handelt. In den nächsten Vorlesungen bekommt sie eine grafische Oberfläche, eine Datei zum Speichern und Unit-Tests.

Übung: Ergänze ein `abstract void Zeichnen()` in `Figur` und implementiere es in `Rechteck` und `Kreis` so, dass jeweils eine Zeile wie `Zeichne Rechteck R1` in einer anderen `ConsoleColor` erscheint (`Console.ForegroundColor` setzen, danach `Console.ResetColor()`). Was passiert mit `Dreieck`, wenn du dort das `override` vergisst?
{: .notice--info}

## Weitere Quellen

- [abstract (C#-Referenz) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/abstract)
- [Abstrakte und versiegelte Klassen und Klassenmember – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/abstract-and-sealed-classes-and-class-members)
- [Compilerfehler CS0144 – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/misc/cs0144)
