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

Wir verwenden in dieser Vorlesung ein Beispiel mit zwei Robotern: **Robbi** ist ein ganz normaler Roboter, **Wischi** ist ein Putzroboter. Ein Putzroboter kann alles, was ein Roboter kann – nur manches macht er zusätzlich oder ein bisschen anders.

## Die Basisklasse

Die Klasse `Roboter` kennen wir vom Aufbau her schon aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/klassen/klassen/): ein Konstruktor, eine Property und eine Methode.

```csharp
class Roboter
{
    public string Name { get; set; }

    public Roboter(string name)
    {
        Name = name;
    }

    public void Arbeiten()
    {
        Console.WriteLine($"{Name} arbeitet.");
    }
}
```

Der Putzroboter soll nun ebenfalls einen Namen haben und arbeiten können – und zusätzlich den Boden wischen. Statt die Klasse zu kopieren, leiten wir sie ab.

## Ableiten mit `:`

Die Vererbung wird in C# mit einem Doppelpunkt hinter dem Klassennamen angegeben. `Putzroboter` **erbt** von `Roboter`; man sagt auch: `Putzroboter` ist von `Roboter` **abgeleitet**, und `Roboter` ist die **Basisklasse** von `Putzroboter`.

```csharp
class Putzroboter : Roboter
{
    public Putzroboter(string name) : base(name)
    {
    }

    public void Wischen()
    {
        Console.WriteLine($"{Name} wischt den Boden.");
    }
}
```

Auffällig ist, wie wenig in der Klasse steht: nur ein Konstruktor und die neue Methode `Wischen`. Trotzdem hat jeder Putzroboter einen Namen und kann arbeiten – beides wurde geerbt.

```csharp
Roboter robbi = new Roboter("Robbi");
robbi.Arbeiten();         // Robbi arbeitet.

Putzroboter wischi = new Putzroboter("Wischi");
Console.WriteLine(wischi.Name);   // Wischi
wischi.Arbeiten();        // Wischi arbeitet.
wischi.Wischen();         // Wischi wischt den Boden.
```

`Name` und `Arbeiten` sind in `Putzroboter` nirgends definiert und trotzdem vorhanden. Das ist der Kern der Wiederverwendung: Alles, was in `Roboter` steht, existiert automatisch auch in `Putzroboter`. Ändern wir später die Ausgabe von `Arbeiten`, profitieren alle abgeleiteten Klassen sofort davon.

## Die Ist-eine-Beziehung

Vererbung ist mehr als ein Trick zum Codesparen – sie drückt eine fachliche Beziehung aus: Ein Putzroboter **ist ein** Roboter. Deshalb darf ein `Putzroboter` überall dort verwendet werden, wo ein `Roboter` verlangt wird:

```csharp
Roboter r = new Putzroboter("Wischi");
r.Arbeiten();             // Wischi arbeitet.

static void Vorstellen(Roboter roboter)
{
    Console.WriteLine($"Hier kommt {roboter.Name}!");
}
Vorstellen(wischi);       // Hier kommt Wischi!
```

Man nennt das **Substituierbarkeit**: Die abgeleitete Klasse kann die Basisklasse vollständig ersetzen. Über die Variable `r` vom Typ `Roboter` können wir allerdings nur das aufrufen, was ein `Roboter` kann – `r.Wischen()` würde der Compiler ablehnen. Warum das so ist und wie man trotzdem an den Putzroboter herankommt, klären wir im Modul [Laufzeittyp und Verstecken](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md).

Vererbung nur einsetzen, wenn die Ist-eine-Beziehung wirklich stimmt. Ein `Auto` *hat* einen Motor, aber es *ist* kein Motor – hier gehört der Motor als Property in die Klasse `Auto`, nicht als Basisklasse. Wer Vererbung nur benutzt, um an ein paar Methoden heranzukommen, bekommt Hierarchien, die niemand mehr versteht.
{: .notice--warning}

## Konstruktorverkettung mit `base`

Konstruktoren werden **nicht** vererbt. Der Konstruktor von `Putzroboter` muss deshalb selbst dafür sorgen, dass der Roboter-Teil des Objekts initialisiert wird – dafür steht `: base(name)` hinter der Parameterliste. Damit wird der Konstruktor der Basisklasse mit dem Namen aufgerufen, bevor der eigene Konstruktorrumpf beginnt.

Lässt man `: base(...)` weg, ruft C# automatisch den **parameterlosen** Konstruktor der Basisklasse auf. Den gibt es bei `Roboter` nicht, weil wir einen eigenen Konstruktor mit Parameter geschrieben haben – der Compiler meldet dann den Fehler CS7036 („Es wurde kein Argument angegeben, das dem erforderlichen Parameter ‚name‘ entspricht“). Die Reihenfolge der Initialisierung ist dabei immer dieselbe: **erst die Basisklasse, dann die abgeleitete Klasse**. Das lässt sich mit zwei Ausgaben sichtbar machen:

```csharp
class Roboter
{
    public Roboter(string name)
    {
        Name = name;
        Console.WriteLine("Roboter-Konstruktor");
    }
    // ...
}

class Putzroboter : Roboter
{
    public Putzroboter(string name) : base(name)
    {
        Console.WriteLine("Putzroboter-Konstruktor");
    }
}

new Putzroboter("Wischi");
// Roboter-Konstruktor
// Putzroboter-Konstruktor
```

Das ist logisch: Der Putzroboter-Konstruktor darf sich darauf verlassen, dass `Name` schon gesetzt ist – die Basisklasse hat ihre Arbeit bereits erledigt. Mit `base` greift man übrigens nicht nur auf Konstruktoren zu, sondern auf alle Mitglieder der Basisklasse; das nutzen wir im Modul [`virtual` und `override`](/modules/virtual_override/virtual_override.md).

## `protected` – sichtbar für Erben

Aus Programmierung 1 kennen wir `public` und `private`. `private`-Mitglieder der Basisklasse werden zwar mit vererbt (sie sind Teil des Objekts), sind aber in der abgeleiteten Klasse **nicht zugreifbar**. Dazwischen liegt `protected`: sichtbar in der Klasse selbst und in allen abgeleiteten Klassen, aber nicht von außen.

```csharp
class Roboter
{
    protected int Energie { get; set; } = 100;
    // ...
}

class Putzroboter : Roboter
{
    public void Wischen()
    {
        Energie -= 10;    // erlaubt – Putzroboter ist ein Erbe
        Console.WriteLine($"{Name} wischt den Boden. Energie: {Energie}");
    }
}

Putzroboter p = new Putzroboter("Wischi");
p.Wischen();              // Wischi wischt den Boden. Energie: 90
Console.WriteLine(p.Energie);   // Fehler CS0122: 'Energie' ist geschützt
```

`protected` ist ein Versprechen an die Erben: „Das hier dürft ihr benutzen.“ Genau deshalb sollte man es nicht leichtfertig vergeben – jedes `protected`-Mitglied gehört zur Schnittstelle, auf die sich abgeleitete Klassen verlassen.
{: .notice--primary}

## Nur eine Basisklasse

C# kennt nur **Einfachvererbung**: Eine Klasse hat genau eine direkte Basisklasse. `class Putzroboter : Roboter, Maschine` ist nicht erlaubt. Hierarchien können aber beliebig tief werden – ein `Fensterputzroboter : Putzroboter` erbt alles von `Putzroboter` und damit auch alles von `Roboter`. Wer Verhalten aus mehreren Quellen kombinieren möchte, greift zu Interfaces; die lernen wir in [Vorlesung 02](/lectures/02/02.md) kennen.

Übung: Erweitere alle Roboter um eine Property `Groesse` (in Zentimetern). Wo muss sie stehen, damit sowohl `Roboter` als auch `Putzroboter` sie haben? Leite dann eine weitere Klasse `Rasenroboter` von `Roboter` ab, die zusätzlich eine Methode `Maehen()` bekommt. Erstelle einen Rasenroboter mit dem Namen „Rasi“, setze seine Größe auf 30 und lass ihn arbeiten und mähen.
{: .notice--info}

## Weitere Quellen

- [Vererbung in C# – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/object-oriented/inheritance)
- [Vererbung – Tutorial mit Beispielen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/tutorials/inheritance)
- [Zugriffsmodifizierer (`protected`) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/access-modifiers)
