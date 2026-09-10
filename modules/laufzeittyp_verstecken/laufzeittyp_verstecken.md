---
title: "Laufzeittyp und Verstecken"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Seit dem Modul [`virtual` und `override`](/modules/virtual_override/virtual_override.md) wissen wir, dass `g.Spuken()` je nach Objekt unterschiedlich ausfällt, obwohl `g` immer den Typ `Geist` hat. Um wirklich zu verstehen, *warum* das so ist – und warum es manchmal nicht klappt –, müssen wir zwei Typen auseinanderhalten, die bei jeder Variablen im Spiel sind: den Typ, den der Compiler sieht, und den Typ, der zur Laufzeit tatsächlich im Speicher liegt. Wer diesen Unterschied verinnerlicht hat, versteht auch, was das Schlüsselwort `new` vor einer Methode anrichtet und wann ein Cast scheitert.

## Kompilierzeittyp und Laufzeittyp

Betrachten wir eine einzige Zeile:

```csharp
Geist g = new Schleimgeist("Schleimi");
```

Links steht der **Kompilierzeittyp** (auch statischer Typ): `Geist`. Er ist der Typ der Variablen und legt fest, welche Methoden und Properties man über `g` aufrufen *darf*. Rechts vom `new` steht der **Laufzeittyp** (dynamischer Typ): `Schleimgeist`. Er beschreibt, welches Objekt sich zur Laufzeit wirklich hinter der Variablen verbirgt. Der Laufzeittyp ist immer der Kompilierzeittyp selbst oder eine davon abgeleitete Klasse – niemals etwas Allgemeineres.

```csharp
g.Spuken();       // erlaubt – Geist kann spuken
g.Schleimen();    // Fehler CS1061: 'Geist' enthält keine Definition für 'Schleimen'
```

Der Compiler kennt nur den Kompilierzeittyp. Dass hinter `g` in Wirklichkeit ein Schleimgeist steckt, könnte er in diesem einfachen Fall zwar erraten – bei einem Parameter oder einem Element aus einer `List<Geist>` aber nicht. Deshalb gilt konsequent: **Was aufgerufen werden darf, entscheidet der Kompilierzeittyp. Welche Implementierung läuft, entscheidet der Laufzeittyp** – jedenfalls bei `virtual`-Methoden.

## Wie die CLR die Methode findet

Beim Aufruf einer `virtual`-Methode sucht die Laufzeitumgebung (die CLR, *Common Language Runtime*) **im Laufzeittyp beginnend hierarchisch nach oben**: Hat `Schleimgeist` ein `override` für `Spuken`? Ja, dann wird es aufgerufen. Wenn nicht, schaut sie in der Basisklasse nach, dann in deren Basisklasse, bis sie eine Implementierung findet. Ein `Schleimkoenig : Schleimgeist`, der `Spuken` nicht überschreibt, spukt also wie ein Schleimgeist – nicht wie ein einfacher Geist.

Bei einer Methode **ohne** `virtual` findet diese Suche nicht statt. Der Compiler bindet den Aufruf fest an die Methode des Kompilierzeittyps. Und genau hier lauert eine Falle.

## Verstecken mit `new` statt Überschreiben

Was passiert, wenn eine abgeleitete Klasse eine Methode mit demselben Namen deklariert, aber `override` fehlt? Dann wird die geerbte Methode nicht ersetzt, sondern **versteckt** (englisch *hiding*). Das Schlüsselwort `new` vor der Methode macht diese Absicht explizit. Der folgende Vergleich zeigt beide Varianten nebeneinander:

```csharp
class Geist
{
    public virtual void Spuken() => Console.WriteLine("Geist spukt");
    public void Verschwinden() => Console.WriteLine("Geist verschwindet");
}

class Schleimgeist : Geist
{
    public override void Spuken() => Console.WriteLine("Schleimgeist spukt schleimig");
    public new void Verschwinden() => Console.WriteLine("Schleimgeist verschwindet mit Blubb");
}

Schleimgeist s = new Schleimgeist();
s.Spuken();          // Schleimgeist spukt schleimig
s.Verschwinden();    // Schleimgeist verschwindet mit Blubb

Geist g = s;         // dasselbe Objekt, anderer Kompilierzeittyp
g.Spuken();          // Schleimgeist spukt schleimig
g.Verschwinden();    // Geist verschwindet
```

Über die Variable `s` verhalten sich beide Methoden gleich. Der Unterschied zeigt sich bei `g`: `Spuken` ist `virtual`/`override`, also entscheidet der Laufzeittyp – schleimig. `Verschwinden` ist versteckt, also entscheidet der Kompilierzeittyp – und der ist `Geist`. Ein und dasselbe Objekt verschwindet je nach Variable anders. Für eine `List<Geist>` bedeutet das: Versteckte Methoden sind für Polymorphie unsichtbar.

Lässt man sowohl `override` als auch `new` weg, verhält sich der Code exakt wie mit `new` – aber der Compiler warnt mit CS0108 („blendet den geerbten Member aus; verwenden Sie das Schlüsselwort `new`, wenn das Ausblenden beabsichtigt war“). Diese Warnung ist fast immer ein Zeichen für ein vergessenes `override` (oder ein vergessenes `virtual` in der Basisklasse). Nimm sie ernst: Das Programm kompiliert, tut aber nicht, was du meinst. Bewusstes Verstecken mit `new` ist in sauberem Code sehr selten nötig.
{: .notice--warning}

## Den Laufzeittyp herausfinden

Manchmal muss man wissen, was sich hinter einer Referenz verbirgt. Die Methode `GetType()`, die jedes Objekt von `object` erbt, liefert den Laufzeittyp:

```csharp
Geist g = new Schleimgeist("Schleimi");
Console.WriteLine(g.GetType().Name);              // Schleimgeist
Console.WriteLine(g.GetType() == typeof(Geist));  // False
```

`GetType()` ist nützlich für Ausgaben und im Debugger. Im Code will man aber meist nicht den Typ vergleichen, sondern **mit dem spezielleren Objekt arbeiten** – etwa `Schleimen` aufrufen. Dafür gibt es drei Werkzeuge:

```csharp
// 1. Pattern Matching mit is: prüfen und gleichzeitig eine typisierte Variable anlegen
if (g is Schleimgeist schleimgeist)
{
    schleimgeist.Schleimen();    // Schleimi hinterlässt eine Schleimspur.
}

// 2. as: liefert null, wenn der Laufzeittyp nicht passt
Schleimgeist? vielleicht = g as Schleimgeist;
vielleicht?.Schleimen();

// 3. expliziter Cast: wirft eine Exception, wenn der Laufzeittyp nicht passt
Schleimgeist sicher = (Schleimgeist)g;
sicher.Schleimen();
```

`is` mit Pattern Matching ist heute die erste Wahl: Die Prüfung und die Umwandlung passieren in einem Schritt, und die neue Variable ist nur im `if`-Block gültig. `as` ist praktisch, wenn man mit `null` weiterarbeiten kann – etwa mit dem `?.`-Operator, den wir aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/nullable/nullable/) kennen. Der harte Cast ist dann angebracht, wenn ein falscher Typ ein Programmierfehler wäre, der laut auffallen soll:

```csharp
Geist spooky = new Geist("Spooky");
Schleimgeist s = (Schleimgeist)spooky;
// System.InvalidCastException: Unable to cast object of type 'Geist' to type 'Schleimgeist'.
```

Der Compiler lässt den Cast durch, weil er zur Kompilierzeit nicht wissen kann, was in `spooky` steckt. Zur Laufzeit stellt die CLR fest, dass ein `Geist` eben kein `Schleimgeist` ist – die Ist-eine-Beziehung gilt nur in eine Richtung.

Wenn du in einer `foreach`-Schleife über eine `List<Geist>` mehrere `is`-Abfragen nacheinander schreibst („wenn Schleimgeist, dann …, wenn Poltergeist, dann …“), ist das meist ein Zeichen, dass eine `virtual`-Methode fehlt. Polymorphie erledigt die Fallunterscheidung für dich – und vergisst keinen neuen Geistertyp.
{: .notice--primary}

Übung: Gegeben ist `Geist[] geister = { new Geist("Spooky"), new Schleimgeist("Schleimi") };`. Schreibe eine Schleife, die für jedes Element den Laufzeittyp ausgibt und nur die Schleimgeister zusätzlich schleimen lässt. Ersetze anschließend `is` durch einen harten Cast – bei welchem Element fliegt die Exception, und warum erst zur Laufzeit?
{: .notice--info}

## Weitere Quellen

- [Typtests und Umwandlungen (`is`, `as`) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/type-testing-and-cast)
- [`new`-Modifizierer – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/new-modifier)
- [Versionsverwaltung mit `override` und `new` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/versioning-with-the-override-and-new-keywords)
- [Pattern Matching – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/functional/pattern-matching)
