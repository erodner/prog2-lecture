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

## Smartphone, Taschenlampe oder Funkgerät?

Wir wollen elektronische Geräte modellieren. Es gibt aufladbare Geräte wie Smartphones und Taschenlampen, die einen Akkustand haben, und funkfähige Geräte wie Smartphones und Funkgeräte, die eine Reichweite haben. Der Vererbungsansatz liegt nahe: eine Basisklasse `AufladbaresGeraet` mit `Akkustand` und eine Basisklasse `FunkfaehigesGeraet` mit `Reichweite`. Nur: Wovon erbt dann `Smartphone`?

```csharp
class Smartphone : AufladbaresGeraet, FunkfaehigesGeraet   // Compilerfehler CS1721
{
}
```

Eine Klasse in C# hat genau **eine** Basisklasse. Mehrfachvererbung gibt es bewusst nicht: Zwei Basisklassen könnten Felder mit gleichem Namen mitbringen oder dieselbe Methode unterschiedlich implementieren, und dann wäre unklar, welche Version im Objekt landet. Das Problem ist aber real – ein Smartphone *ist* aufladbar *und* funkfähig, während eine Taschenlampe nur aufladbar und ein einfaches Funkgerät mit Batterien nur funkfähig ist. Was wir brauchen, ist eine Möglichkeit, Fähigkeiten zu beschreiben, die keinen Zustand und keine Implementierung mitbringen und sich deshalb gefahrlos kombinieren lassen.

## Ein Interface definieren

Ein Interface wird mit dem Schlüsselwort `interface` statt `class` definiert. Es enthält die Signaturen von Properties und Methoden, die eine Klasse anbieten muss, wenn sie das Interface implementiert:

```csharp
interface IAufladbar
{
    int Akkustand { get; set; }
    void Aufladen();
}

interface IFunkfaehig
{
    double Reichweite { get; set; }
}
```

Drei Dinge sind hier anders als bei Klassen. Erstens steht bei den Mitgliedern **keine Sichtbarkeit** – sie sind automatisch `public`, denn ein Interface beschreibt ja gerade, was von außen nutzbar ist. Zweitens haben die Methoden keinen Rumpf, ähnlich wie abstrakte Methoden. Drittens beginnt der Name mit einem großen `I`: Das ist keine Sprachregel, aber eine Konvention, an die sich die gesamte .NET-Welt hält – man erkennt Interfaces im Code sofort.

Ein Interface hat keine Felder und keine Konstruktoren. Es beschreibt ausschließlich, *was* ein Objekt kann – nicht, welche Daten es dafür intern speichert. Ein `{ get; set; }` im Interface ist deshalb kein Auto-Property, sondern nur die Forderung, dass die Klasse ein Property mit Getter und Setter bereitstellt.
{: .notice--primary}

## Ein Interface implementieren

Die Syntax sieht aus wie Vererbung – nach dem Doppelpunkt steht das Interface. Die Bedeutung ist aber eine andere: Die Klasse erbt nichts, sondern **verpflichtet sich**, alle Mitglieder des Interfaces öffentlich bereitzustellen.

```csharp
class Taschenlampe : IAufladbar
{
    public int Akkustand { get; set; }

    public void Aufladen()
    {
        Akkustand = 100;
    }
}
```

`public` ist hier Pflicht: Das Interface verspricht öffentliche Mitglieder, also muss die Klasse sie auch öffentlich anbieten. Lässt man `Aufladen` weg oder macht `Akkustand` privat, meldet der Compiler, dass `Taschenlampe` das Interface nicht vollständig implementiert. Und jetzt kommt der entscheidende Schritt – mehrere Interfaces werden einfach durch Komma getrennt:

```csharp
class Smartphone : IAufladbar, IFunkfaehig
{
    public int Akkustand { get; set; }
    public double Reichweite { get; set; }

    public void Aufladen()
    {
        Akkustand = 100;
    }
}

class Funkgeraet : IFunkfaehig
{
    public double Reichweite { get; set; }
}
```

`Smartphone` erfüllt beide Verträge, `Taschenlampe` und `Funkgeraet` je einen – das Funkgerät läuft mit Batterien und hat deshalb keinen Akkustand. Hätte `Smartphone` zusätzlich eine echte Basisklasse, etwa `Geraet` mit einem Namen und einem Gewicht, stünde sie als erste in der Liste: `class Smartphone : Geraet, IAufladbar, IFunkfaehig`. Eine Basisklasse, beliebig viele Interfaces – das ist die Regel.

## Das Interface als Typ

Ein Interface kann überall dort als Typ stehen, wo auch eine Klasse stehen könnte: bei Variablen, Parametern, Rückgabewerten und in Sammlungen. Damit lassen sich Objekte ganz verschiedener Klassen gemeinsam behandeln, solange sie dieselbe Fähigkeit haben:

```csharp
IAufladbar[] aufladbareGeraete =
{
    new Smartphone { Akkustand = 15, Reichweite = 30 },
    new Taschenlampe { Akkustand = 40 }
};

foreach (IAufladbar geraet in aufladbareGeraete)
{
    int vorher = geraet.Akkustand;
    geraet.Aufladen();
    Console.WriteLine($"{geraet.GetType().Name}: {vorher} % -> {geraet.Akkustand} %");
}
// Smartphone: 15 % -> 100 %
// Taschenlampe: 40 % -> 100 %
```

Die Objekte werden hier mit einem *Objektinitialisierer* erzeugt – `new Taschenlampe { Akkustand = 40 }` ruft den parameterlosen Konstruktor auf und setzt anschließend die Properties. Über eine Variable vom Typ `IAufladbar` sind nur die Mitglieder des Interfaces erreichbar: `geraet.Reichweite` würde nicht kompilieren, obwohl das erste Objekt ein `Smartphone` ist. Das kennen wir vom [Kompilierzeittyp](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md): Der deklarierte Typ bestimmt, was man sehen darf, der Laufzeittyp bestimmt, was passiert.

Ob ein Objekt eine bestimmte Fähigkeit hat, prüft man wie bei Klassen mit `is` – inklusive Musterabgleich, der gleich die passende Variable liefert:

```csharp
object[] alles = { new Smartphone { Reichweite = 30 }, new Taschenlampe(), new Funkgeraet { Reichweite = 5 } };

foreach (object o in alles)
{
    if (o is IFunkfaehig funk)
        Console.WriteLine($"{o.GetType().Name} funkt bis {funk.Reichweite} km weit");
}
// Smartphone funkt bis 30 km weit
// Funkgeraet funkt bis 5 km weit
```

## Interfaces in .NET

Die .NET-Klassenbibliothek ist voll von Interfaces, und du hast einige davon längst benutzt, ohne es zu merken. `foreach` funktioniert über jede Klasse, die `IEnumerable` implementiert – deshalb kann man Arrays, Listen und Dictionaries mit derselben Schleife durchlaufen. `IComparable` beschreibt, dass sich Objekte vergleichen lassen, was `Sort()` für eigene Klassen möglich macht. Und `IDisposable` kennzeichnet Objekte, die Ressourcen wie Dateien freigeben müssen. Alle drei tauchen in späteren Vorlesungen im Detail auf; hier reicht die Erkenntnis: Ein Interface ist die Art, wie .NET „dieses Objekt kann X“ ausdrückt.

Übung: Definiere ein Interface `IHatSpeicher` mit `double Speicherplatz { get; set; }` (in Gigabyte) und einer Methode `bool IstVoll()`. Welche der Klassen `Smartphone`, `Taschenlampe` und `Funkgeraet` sollten es implementieren? Schreibe dann eine Methode `Gesamtspeicher(object[] geraete)`, die den Speicherplatz aller Geräte mit Speicher im Array zusammenzählt.
{: .notice--info}

## Weitere Quellen

- [Schnittstellen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/types/interfaces)
- [interface (C#-Referenz) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/interface)
- [Objekt- und Auflistungsinitialisierer – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers)
