---
title: "Interfaces – Grundlagen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ein Steckdosenadapter interessiert sich nicht dafür, welches Gerät du anschließt – Föhn, Laptop oder Ladegerät. Er verlangt nur, dass der Stecker eine bestimmte Form hat. Genau so funktioniert ein **Interface** in C#: Es beschreibt eine Fähigkeit, die eine Klasse anbieten muss, ohne festzulegen, wie sie umgesetzt wird. Und anders als bei einer Basisklasse kann ein Objekt beliebig viele solcher Fähigkeiten haben. Das löst ein Problem, an dem Vererbung allein scheitert.

## Kaffee oder Tee? Oder doch lieber Cola?

Wir wollen Getränke modellieren. Es gibt Heißgetränke wie Kaffee und Tee, die eine Temperatur haben, und koffeinhaltige Getränke wie Kaffee und Cola, die einen Koffeingehalt haben. Der Vererbungsansatz liegt nahe: eine Basisklasse `Heissgetraenk` mit `Temperatur` und eine Basisklasse `KoffeinhaltigesGetraenk` mit `Koffein`. Nur: Wovon erbt dann `Kaffee`?

```csharp
class Kaffee : Heissgetraenk, KoffeinhaltigesGetraenk   // Compilerfehler CS1721
{
}
```

Eine Klasse in C# hat genau **eine** Basisklasse. Mehrfachvererbung gibt es bewusst nicht: Zwei Basisklassen könnten Felder mit gleichem Namen mitbringen oder dieselbe Methode unterschiedlich implementieren, und dann wäre unklar, welche Version im Objekt landet. Das Problem ist aber real – Kaffee *ist* heiß *und* koffeinhaltig. Was wir brauchen, ist eine Möglichkeit, Fähigkeiten zu beschreiben, die keinen Zustand und keine Implementierung mitbringen und sich deshalb gefahrlos kombinieren lassen.

## Ein Interface definieren

Ein Interface wird mit dem Schlüsselwort `interface` statt `class` definiert. Es enthält die Signaturen von Properties und Methoden, die eine Klasse anbieten muss, wenn sie das Interface implementiert:

```csharp
interface IHeissgetraenk
{
    int Temperatur { get; set; }
    void Abkuehlen(int grad);
}

interface IHatKoffein
{
    double Koffein { get; set; }
}
```

Drei Dinge sind hier anders als bei Klassen. Erstens steht bei den Mitgliedern **keine Sichtbarkeit** – sie sind automatisch `public`, denn ein Interface beschreibt ja gerade, was von außen nutzbar ist. Zweitens haben die Methoden keinen Rumpf, ähnlich wie abstrakte Methoden. Drittens beginnt der Name mit einem großen `I`: Das ist keine Sprachregel, aber eine Konvention, an die sich die gesamte .NET-Welt hält – man erkennt Interfaces im Code sofort.

Ein Interface hat keine Felder und keine Konstruktoren. Es beschreibt ausschließlich, *was* ein Objekt kann – nicht, welche Daten es dafür intern speichert. Ein `{ get; set; }` im Interface ist deshalb kein Auto-Property, sondern nur die Forderung, dass die Klasse ein Property mit Getter und Setter bereitstellt.
{: .notice--primary}

## Ein Interface implementieren

Die Syntax sieht aus wie Vererbung – nach dem Doppelpunkt steht das Interface. Die Bedeutung ist aber eine andere: Die Klasse erbt nichts, sondern **verpflichtet sich**, alle Mitglieder des Interfaces öffentlich bereitzustellen.

```csharp
class Tee : IHeissgetraenk
{
    public int Temperatur { get; set; }

    public void Abkuehlen(int grad)
    {
        Temperatur -= grad;
    }
}
```

`public` ist hier Pflicht: Das Interface verspricht öffentliche Mitglieder, also muss die Klasse sie auch öffentlich anbieten. Lässt man `Abkuehlen` weg oder macht `Temperatur` privat, meldet der Compiler, dass `Tee` das Interface nicht vollständig implementiert. Und jetzt kommt der entscheidende Schritt – mehrere Interfaces werden einfach durch Komma getrennt:

```csharp
class Kaffee : IHeissgetraenk, IHatKoffein
{
    public int Temperatur { get; set; }
    public double Koffein { get; set; }

    public void Abkuehlen(int grad)
    {
        Temperatur -= grad;
    }
}

class Cola : IHatKoffein
{
    public double Koffein { get; set; }
}
```

`Kaffee` erfüllt beide Verträge, `Tee` und `Cola` je einen. Hätte `Kaffee` zusätzlich eine echte Basisklasse, etwa `Getraenk` mit einem Namen und einer Füllmenge, stünde sie als erste in der Liste: `class Kaffee : Getraenk, IHeissgetraenk, IHatKoffein`. Eine Basisklasse, beliebig viele Interfaces – das ist die Regel.

## Das Interface als Typ

Ein Interface kann überall dort als Typ stehen, wo auch eine Klasse stehen könnte: bei Variablen, Parametern, Rückgabewerten und in Sammlungen. Damit lassen sich Objekte ganz verschiedener Klassen gemeinsam behandeln, solange sie dieselbe Fähigkeit haben:

```csharp
IHeissgetraenk[] heisseGetraenke =
{
    new Kaffee { Temperatur = 90, Koffein = 80 },
    new Tee { Temperatur = 95 }
};

foreach (IHeissgetraenk getraenk in heisseGetraenke)
{
    getraenk.Abkuehlen(10);
    Console.WriteLine($"{getraenk.GetType().Name}: {getraenk.Temperatur} °C");
}
// Kaffee: 80 °C
// Tee: 85 °C
```

Die Objekte werden hier mit einem *Objektinitialisierer* erzeugt – `new Tee { Temperatur = 95 }` ruft den parameterlosen Konstruktor auf und setzt anschließend die Properties. Über eine Variable vom Typ `IHeissgetraenk` sind nur die Mitglieder des Interfaces erreichbar: `getraenk.Koffein` würde nicht kompilieren, obwohl das erste Objekt ein `Kaffee` ist. Das kennen wir vom [Kompilierzeittyp](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md): Der deklarierte Typ bestimmt, was man sehen darf, der Laufzeittyp bestimmt, was passiert.

Ob ein Objekt eine bestimmte Fähigkeit hat, prüft man wie bei Klassen mit `is` – inklusive Musterabgleich, der gleich die passende Variable liefert:

```csharp
object[] alles = { new Kaffee { Koffein = 80 }, new Tee(), new Cola { Koffein = 35 } };

foreach (object o in alles)
{
    if (o is IHatKoffein wach)
        Console.WriteLine($"{o.GetType().Name} enthält {wach.Koffein} mg Koffein");
}
// Kaffee enthält 80 mg Koffein
// Cola enthält 35 mg Koffein
```

## Interfaces in .NET

Die .NET-Klassenbibliothek ist voll von Interfaces, und du hast einige davon längst benutzt, ohne es zu merken. `foreach` funktioniert über jede Klasse, die `IEnumerable` implementiert – deshalb kann man Arrays, Listen und Dictionaries mit derselben Schleife durchlaufen. `IComparable` beschreibt, dass sich Objekte vergleichen lassen, was `Sort()` für eigene Klassen möglich macht. Und `IDisposable` kennzeichnet Objekte, die Ressourcen wie Dateien freigeben müssen. Alle drei tauchen in späteren Vorlesungen im Detail auf; hier reicht die Erkenntnis: Ein Interface ist die Art, wie .NET „dieses Objekt kann X“ ausdrückt.

Übung: Definiere ein Interface `IZuckerhaltig` mit `double Zucker { get; set; }` und einer Methode `bool IstZuckerfrei()`. Welche der Klassen `Kaffee`, `Tee` und `Cola` sollten es implementieren? Schreibe dann eine Methode `Zuckergehalt(object[] getraenke)`, die den Gesamtzucker aller zuckerhaltigen Getränke im Array berechnet.
{: .notice--info}

## Weitere Quellen

- [Schnittstellen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/types/interfaces)
- [interface (C#-Referenz) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/interface)
- [Objekt- und Auflistungsinitialisierer – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers)
