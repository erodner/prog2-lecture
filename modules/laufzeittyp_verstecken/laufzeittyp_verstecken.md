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

Seit dem Modul [`virtual` und `override`](/modules/virtual_override/virtual_override.md) wissen wir, dass `r.Arbeiten()` je nach Objekt unterschiedlich ausfällt, obwohl `r` immer den Typ `Roboter` hat. Um wirklich zu verstehen, *warum* das so ist – und warum es manchmal nicht klappt –, müssen wir zwei Typen auseinanderhalten, die bei jeder Variablen im Spiel sind: den Typ, den der Compiler sieht, und den Typ, der zur Laufzeit tatsächlich im Speicher liegt. Wer diesen Unterschied verinnerlicht hat, versteht auch, was das Schlüsselwort `new` vor einer Methode anrichtet und wann ein Cast scheitert.

## Kompilierzeittyp und Laufzeittyp

Betrachten wir eine einzige Zeile:

```csharp
Roboter r = new Putzroboter("Wischi");
```

Links steht der **Kompilierzeittyp** (auch statischer Typ): `Roboter`. Er ist der Typ der Variablen und legt fest, welche Methoden und Properties man über `r` aufrufen *darf*. Rechts vom `new` steht der **Laufzeittyp** (dynamischer Typ): `Putzroboter`. Er beschreibt, welches Objekt sich zur Laufzeit wirklich hinter der Variablen verbirgt. Der Laufzeittyp ist immer der Kompilierzeittyp selbst oder eine davon abgeleitete Klasse – niemals etwas Allgemeineres.

```csharp
r.Arbeiten();     // erlaubt – Roboter kann arbeiten
r.Wischen();      // Fehler CS1061: 'Roboter' enthält keine Definition für 'Wischen'
```

Der Compiler kennt nur den Kompilierzeittyp. Dass hinter `r` in Wirklichkeit ein Putzroboter steckt, könnte er in diesem einfachen Fall zwar erraten – bei einem Parameter oder einem Element aus einer `List<Roboter>` aber nicht. Deshalb gilt konsequent: **Was aufgerufen werden darf, entscheidet der Kompilierzeittyp. Welche Implementierung läuft, entscheidet der Laufzeittyp** – jedenfalls bei `virtual`-Methoden.

## Wie die CLR die Methode findet

Beim Aufruf einer `virtual`-Methode sucht die Laufzeitumgebung (die CLR, *Common Language Runtime*) **im Laufzeittyp beginnend hierarchisch nach oben**: Hat `Putzroboter` ein `override` für `Arbeiten`? Ja, dann wird es aufgerufen. Wenn nicht, schaut sie in der Basisklasse nach, dann in deren Basisklasse, bis sie eine Implementierung findet. Ein `Fensterputzroboter : Putzroboter`, der `Arbeiten` nicht überschreibt, arbeitet also wie ein Putzroboter – nicht wie ein einfacher Roboter.

Bei einer Methode **ohne** `virtual` findet diese Suche nicht statt. Der Compiler bindet den Aufruf fest an die Methode des Kompilierzeittyps. Und genau hier lauert eine Falle.

## Verstecken mit `new` statt Überschreiben

Was passiert, wenn eine abgeleitete Klasse eine Methode mit demselben Namen deklariert, aber `override` fehlt? Dann wird die geerbte Methode nicht ersetzt, sondern **versteckt** (englisch *hiding*). Das Schlüsselwort `new` vor der Methode macht diese Absicht explizit. Der folgende Vergleich zeigt beide Varianten nebeneinander:

```csharp
class Roboter
{
    public virtual void Arbeiten() => Console.WriteLine("Roboter arbeitet");
    public void Abschalten() => Console.WriteLine("Roboter schaltet ab");
}

class Putzroboter : Roboter
{
    public override void Arbeiten() => Console.WriteLine("Putzroboter wischt den Boden");
    public new void Abschalten() => Console.WriteLine("Putzroboter fährt in die Ladestation");
}

Putzroboter p = new Putzroboter();
p.Arbeiten();          // Putzroboter wischt den Boden
p.Abschalten();        // Putzroboter fährt in die Ladestation

Roboter r = p;         // dasselbe Objekt, anderer Kompilierzeittyp
r.Arbeiten();          // Putzroboter wischt den Boden
r.Abschalten();        // Roboter schaltet ab
```

Über die Variable `p` verhalten sich beide Methoden gleich. Der Unterschied zeigt sich bei `r`: `Arbeiten` ist `virtual`/`override`, also entscheidet der Laufzeittyp – der Putzroboter wischt. `Abschalten` ist versteckt, also entscheidet der Kompilierzeittyp – und der ist `Roboter`. Ein und dasselbe Objekt schaltet sich je nach Variable anders ab. Für eine `List<Roboter>` bedeutet das: Versteckte Methoden sind für Polymorphie unsichtbar.

Lässt man sowohl `override` als auch `new` weg, verhält sich der Code exakt wie mit `new` – aber der Compiler warnt mit CS0108 („blendet den geerbten Member aus; verwenden Sie das Schlüsselwort `new`, wenn das Ausblenden beabsichtigt war“). Diese Warnung ist fast immer ein Zeichen für ein vergessenes `override` (oder ein vergessenes `virtual` in der Basisklasse). Nimm sie ernst: Das Programm kompiliert, tut aber nicht, was du meinst. Bewusstes Verstecken mit `new` ist in sauberem Code sehr selten nötig.
{: .notice--warning}

## Den Laufzeittyp herausfinden

Manchmal muss man wissen, was sich hinter einer Referenz verbirgt. Die Methode `GetType()`, die jedes Objekt von `object` erbt, liefert den Laufzeittyp:

```csharp
Roboter r = new Putzroboter("Wischi");
Console.WriteLine(r.GetType().Name);              // Putzroboter
Console.WriteLine(r.GetType() == typeof(Roboter));  // False
```

`GetType()` ist nützlich für Ausgaben und im Debugger. Im Code will man aber meist nicht den Typ vergleichen, sondern **mit dem spezielleren Objekt arbeiten** – etwa `Wischen` aufrufen. Dafür gibt es drei Werkzeuge:

```csharp
// 1. Pattern Matching mit is: prüfen und gleichzeitig eine typisierte Variable anlegen
if (r is Putzroboter putzroboter)
{
    putzroboter.Wischen();    // Wischi wischt den Boden.
}

// 2. as: liefert null, wenn der Laufzeittyp nicht passt
Putzroboter? vielleicht = r as Putzroboter;
vielleicht?.Wischen();

// 3. expliziter Cast: wirft eine Exception, wenn der Laufzeittyp nicht passt
Putzroboter sicher = (Putzroboter)r;
sicher.Wischen();
```

`is` mit Pattern Matching ist heute die erste Wahl: Die Prüfung und die Umwandlung passieren in einem Schritt, und die neue Variable ist nur im `if`-Block gültig. `as` ist praktisch, wenn man mit `null` weiterarbeiten kann – etwa mit dem `?.`-Operator, den wir aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/nullable/nullable/) kennen. Der harte Cast ist dann angebracht, wenn ein falscher Typ ein Programmierfehler wäre, der laut auffallen soll:

```csharp
Roboter robbi = new Roboter("Robbi");
Putzroboter p = (Putzroboter)robbi;
// System.InvalidCastException: Unable to cast object of type 'Roboter' to type 'Putzroboter'.
```

Der Compiler lässt den Cast durch, weil er zur Kompilierzeit nicht wissen kann, was in `robbi` steckt. Zur Laufzeit stellt die CLR fest, dass ein `Roboter` eben kein `Putzroboter` ist – die Ist-eine-Beziehung gilt nur in eine Richtung.

Wenn du in einer `foreach`-Schleife über eine `List<Roboter>` mehrere `is`-Abfragen nacheinander schreibst („wenn Putzroboter, dann …, wenn Rasenroboter, dann …“), ist das meist ein Zeichen, dass eine `virtual`-Methode fehlt. Polymorphie erledigt die Fallunterscheidung für dich – und vergisst keinen neuen Robotertyp.
{: .notice--primary}

Übung: Gegeben ist `Roboter[] alleRoboter = { new Roboter("Robbi"), new Putzroboter("Wischi") };`. Schreibe eine Schleife, die für jedes Element den Laufzeittyp ausgibt und nur die Putzroboter zusätzlich wischen lässt. Ersetze anschließend `is` durch einen harten Cast – bei welchem Element fliegt die Exception, und warum erst zur Laufzeit?
{: .notice--info}

## Weitere Quellen

- [Typtests und Umwandlungen (`is`, `as`) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/operators/type-testing-and-cast)
- [`new`-Modifizierer – C#-Referenz – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/new-modifier)
- [Versionsverwaltung mit `override` und `new` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/versioning-with-the-override-and-new-keywords)
- [Pattern Matching – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/functional/pattern-matching)
