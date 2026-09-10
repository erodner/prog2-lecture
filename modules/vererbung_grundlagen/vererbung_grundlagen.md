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

Mit Feldern, Properties und Methoden können wir bereits komplexe Dinge modellieren – eine Person mit Namen, Größe und Augenfarbe samt allem, was sie tun kann. Schwierig wird es erst, wenn zwei Klassen sich *ähneln*, aber nicht gleich sind: Ein Stehplatz und ein Sitzplatz im Stadion haben beide einen Preis, aber nur der Sitzplatz hat eine Reihe und eine Nummer. Ohne Vererbung müssten wir den Preis in beiden Klassen pflegen – und jede Änderung an zwei Stellen nachziehen. **Vererbung** erlaubt es, eine Klasse auf einer anderen aufzubauen: Die abgeleitete Klasse übernimmt alles, was die Basisklasse hat, und ergänzt nur das, was sie zusätzlich braucht.

Wir verwenden in dieser Vorlesung ein Beispiel mit zwei Geistern: **Spooky** ist ein ganz normaler Geist, **Schleimi** ist ein Schleimgeist. Ein Schleimgeist kann alles, was ein Geist kann – nur manches macht er zusätzlich oder ein bisschen anders.

## Die Basisklasse

Die Klasse `Geist` kennen wir vom Aufbau her schon aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/klassen/klassen/): ein Konstruktor, eine Property und eine Methode.

```csharp
class Geist
{
    public string Name { get; set; }

    public Geist(string name)
    {
        Name = name;
    }

    public void Spuken()
    {
        Console.WriteLine($"{Name} sagt: 'Buh'");
    }
}
```

Der Schleimgeist soll nun ebenfalls einen Namen haben und spuken können – und zusätzlich eine Schleimspur hinterlassen. Statt die Klasse zu kopieren, leiten wir sie ab.

## Ableiten mit `:`

Die Vererbung wird in C# mit einem Doppelpunkt hinter dem Klassennamen angegeben. `Schleimgeist` **erbt** von `Geist`; man sagt auch: `Schleimgeist` ist von `Geist` **abgeleitet**, und `Geist` ist die **Basisklasse** von `Schleimgeist`.

```csharp
class Schleimgeist : Geist
{
    public Schleimgeist(string name) : base(name)
    {
    }

    public void Schleimen()
    {
        Console.WriteLine($"{Name} hinterlässt eine Schleimspur.");
    }
}
```

Auffällig ist, wie wenig in der Klasse steht: nur ein Konstruktor und die neue Methode `Schleimen`. Trotzdem hat jeder Schleimgeist einen Namen und kann spuken – beides wurde geerbt.

```csharp
Geist spooky = new Geist("Spooky");
spooky.Spuken();          // Spooky sagt: 'Buh'

Schleimgeist schleimi = new Schleimgeist("Schleimi");
Console.WriteLine(schleimi.Name);   // Schleimi
schleimi.Spuken();        // Schleimi sagt: 'Buh'
schleimi.Schleimen();     // Schleimi hinterlässt eine Schleimspur.
```

`Name` und `Spuken` sind in `Schleimgeist` nirgends definiert und trotzdem vorhanden. Das ist der Kern der Wiederverwendung: Alles, was in `Geist` steht, existiert automatisch auch in `Schleimgeist`. Ändern wir später die Ausgabe von `Spuken`, profitieren alle abgeleiteten Klassen sofort davon.

## Die Ist-eine-Beziehung

Vererbung ist mehr als ein Trick zum Codesparen – sie drückt eine fachliche Beziehung aus: Ein Schleimgeist **ist ein** Geist. Deshalb darf ein `Schleimgeist` überall dort verwendet werden, wo ein `Geist` verlangt wird:

```csharp
Geist g = new Schleimgeist("Schleimi");
g.Spuken();               // Schleimi sagt: 'Buh'

static void Vorstellen(Geist geist)
{
    Console.WriteLine($"Hier kommt {geist.Name}!");
}
Vorstellen(schleimi);     // Hier kommt Schleimi!
```

Man nennt das **Substituierbarkeit**: Die abgeleitete Klasse kann die Basisklasse vollständig ersetzen. Über die Variable `g` vom Typ `Geist` können wir allerdings nur das aufrufen, was ein `Geist` kann – `g.Schleimen()` würde der Compiler ablehnen. Warum das so ist und wie man trotzdem an den Schleimgeist herankommt, klären wir im Modul [Laufzeittyp und Verstecken](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md).

Vererbung nur einsetzen, wenn die Ist-eine-Beziehung wirklich stimmt. Ein `Auto` *hat* einen Motor, aber es *ist* kein Motor – hier gehört der Motor als Property in die Klasse `Auto`, nicht als Basisklasse. Wer Vererbung nur benutzt, um an ein paar Methoden heranzukommen, bekommt Hierarchien, die niemand mehr versteht.
{: .notice--warning}

## Konstruktorverkettung mit `base`

Konstruktoren werden **nicht** vererbt. Der Konstruktor von `Schleimgeist` muss deshalb selbst dafür sorgen, dass der Geist-Teil des Objekts initialisiert wird – dafür steht `: base(name)` hinter der Parameterliste. Damit wird der Konstruktor der Basisklasse mit dem Namen aufgerufen, bevor der eigene Konstruktorrumpf beginnt.

Lässt man `: base(...)` weg, ruft C# automatisch den **parameterlosen** Konstruktor der Basisklasse auf. Den gibt es bei `Geist` nicht, weil wir einen eigenen Konstruktor mit Parameter geschrieben haben – der Compiler meldet dann den Fehler CS7036 („Es wurde kein Argument angegeben, das dem erforderlichen Parameter ‚name‘ entspricht“). Die Reihenfolge der Initialisierung ist dabei immer dieselbe: **erst die Basisklasse, dann die abgeleitete Klasse**. Das lässt sich mit zwei Ausgaben sichtbar machen:

```csharp
class Geist
{
    public Geist(string name)
    {
        Name = name;
        Console.WriteLine("Geist-Konstruktor");
    }
    // ...
}

class Schleimgeist : Geist
{
    public Schleimgeist(string name) : base(name)
    {
        Console.WriteLine("Schleimgeist-Konstruktor");
    }
}

new Schleimgeist("Schleimi");
// Geist-Konstruktor
// Schleimgeist-Konstruktor
```

Das ist logisch: Der Schleimgeist-Konstruktor darf sich darauf verlassen, dass `Name` schon gesetzt ist – die Basisklasse hat ihre Arbeit bereits erledigt. Mit `base` greift man übrigens nicht nur auf Konstruktoren zu, sondern auf alle Mitglieder der Basisklasse; das nutzen wir im Modul [`virtual` und `override`](/modules/virtual_override/virtual_override.md).

## `protected` – sichtbar für Erben

Aus Programmierung 1 kennen wir `public` und `private`. `private`-Mitglieder der Basisklasse werden zwar mit vererbt (sie sind Teil des Objekts), sind aber in der abgeleiteten Klasse **nicht zugreifbar**. Dazwischen liegt `protected`: sichtbar in der Klasse selbst und in allen abgeleiteten Klassen, aber nicht von außen.

```csharp
class Geist
{
    protected int Energie { get; set; } = 100;
    // ...
}

class Schleimgeist : Geist
{
    public void Schleimen()
    {
        Energie -= 10;    // erlaubt – Schleimgeist ist ein Erbe
        Console.WriteLine($"{Name} hinterlässt eine Schleimspur. Energie: {Energie}");
    }
}

Schleimgeist s = new Schleimgeist("Schleimi");
s.Schleimen();            // Schleimi hinterlässt eine Schleimspur. Energie: 90
Console.WriteLine(s.Energie);   // Fehler CS0122: 'Energie' ist geschützt
```

`protected` ist ein Versprechen an die Erben: „Das hier dürft ihr benutzen.“ Genau deshalb sollte man es nicht leichtfertig vergeben – jedes `protected`-Mitglied gehört zur Schnittstelle, auf die sich abgeleitete Klassen verlassen.
{: .notice--primary}

## Nur eine Basisklasse

C# kennt nur **Einfachvererbung**: Eine Klasse hat genau eine direkte Basisklasse. `class Schleimgeist : Geist, Monster` ist nicht erlaubt. Hierarchien können aber beliebig tief werden – ein `Schleimkoenig : Schleimgeist` erbt alles von `Schleimgeist` und damit auch alles von `Geist`. Wer Verhalten aus mehreren Quellen kombinieren möchte, greift zu Interfaces; die lernen wir in [Vorlesung 02](/lectures/02/02.md) kennen.

Übung: Erweitere alle Geister um eine Property `Groesse` (in Zentimetern). Wo muss sie stehen, damit sowohl `Geist` als auch `Schleimgeist` sie haben? Erstelle dann einen neuen Schleimgeist mit dem Namen „Smeargol“, setze seine Größe auf 5 und lass ihn spuken und schleimen.
{: .notice--info}

## Weitere Quellen

- [Vererbung in C# – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/object-oriented/inheritance)
- [Vererbung – Tutorial mit Beispielen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/tutorials/inheritance)
- [Zugriffsmodifizierer (`protected`) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers)
