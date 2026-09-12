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

Ein Steckdosenadapter interessiert sich nicht dafür, welches Gerät du anschließt – Föhn, Laptop oder Ladegerät. Er verlangt nur, dass der Stecker eine bestimmte Form hat. Genau so funktioniert ein **Interface** in C#: Es beschreibt eine Fähigkeit, die eine Klasse anbieten muss, ohne festzulegen, wie sie umgesetzt wird. Und anders als bei einer Basisklasse kann ein Objekt beliebig viele solcher Fähigkeiten haben. Das löst ein Problem, an dem unsere Vererbungshierarchie aus dem vorigen Modul gerade scheitert.

## Eine Tür ist zwei Dinge gleichzeitig

Unser Dungeon soll eine Tür bekommen. Eine Tür steht fest an ihrem Platz, blockiert den Weg und wird auf der Karte gezeichnet – sie ist also ein `StatischesObjekt`, genau wie eine Wand. Gleichzeitig kann der Spieler etwas mit ihr *tun*: davorstehen, drücken, mit einem Schlüssel aufschließen. Dieselbe Fähigkeit braucht die Truhe, die man öffnet und plündert.

Der naheliegende Versuch mit Vererbung: eine weitere Zwischenklasse `InteragierbaresObjekt` mit einer Methode `Interagieren`. Nur – wovon erbt dann die Tür?

```csharp
public sealed class Tuer : StatischesObjekt, InteragierbaresObjekt   // Compilerfehler CS1721
{
}
```

Eine Klasse in C# hat genau **eine** Basisklasse. Mehrfachvererbung gibt es bewusst nicht: Zwei Basisklassen könnten Felder mit gleichem Namen mitbringen oder dieselbe Methode unterschiedlich implementieren, und dann wäre unklar, welche Version im Objekt landet. Wir könnten `InteragierbaresObjekt` natürlich zwischen `StatischesObjekt` und `Tuer` schieben – aber dann hängt die Fähigkeit „ansprechbar“ für immer daran, statisch zu sein. Ein Händler, der über die Karte läuft und mit dem man reden kann, wäre nicht mehr möglich. Und beim Aufheben von Gegenständen wird es noch enger: Ein Schlüssel liegt zwar als statisches Objekt herum, landet aber anschließend im Inventar, wo auch Dinge Platz haben sollen, die nie auf dem Spielfeld lagen.

Was wir brauchen, ist eine Möglichkeit, Fähigkeiten zu beschreiben, die keinen Zustand und keine Implementierung mitbringen und sich deshalb gefahrlos kombinieren lassen.

## Ein Interface definieren

Ein Interface wird mit dem Schlüsselwort `interface` statt `class` definiert. Es enthält nur die Signaturen der Mitglieder, die eine Klasse anbieten muss, wenn sie das Interface implementiert:

```csharp
/// <summary>
/// Etwas, mit dem der Spieler etwas tun kann, wenn er davor steht:
/// eine Tür aufschließen, eine Truhe öffnen.
/// </summary>
public interface IInteragierbar
{
    /// <summary>Führt die Interaktion aus und liefert eine Meldung für den Spieler.</summary>
    string Interagieren(Spieler spieler);
}

/// <summary>Etwas, das der Spieler aufheben und im Inventar tragen kann.</summary>
public interface ISammelbar
{
    string Name { get; }

    /// <summary>Wird aufgerufen, sobald der Spieler das Objekt aufhebt.</summary>
    string Aufheben(Spieler spieler);
}
```

Drei Dinge sind hier anders als bei Klassen. Erstens steht bei den Mitgliedern **keine Sichtbarkeit** – sie sind automatisch `public`, denn ein Interface beschreibt ja gerade, was von außen nutzbar ist. Zweitens haben die Methoden keinen Rumpf, ähnlich wie abstrakte Methoden. Drittens beginnt der Name mit einem großen `I`: Das ist keine Sprachregel, aber eine Konvention, an die sich die gesamte .NET-Welt hält – man erkennt Interfaces im Code sofort.

Ein Interface hat keine Felder und keine Konstruktoren. Es beschreibt ausschließlich, *was* ein Objekt kann – nicht, welche Daten es dafür intern speichert. Das `string Name { get; }` in `ISammelbar` ist deshalb kein Auto-Property, sondern nur die Forderung, dass die Klasse ein lesbares `Name`-Property bereitstellt.
{: .notice--primary}

## Ein Interface implementieren

Die Syntax sieht aus wie Vererbung – nach dem Doppelpunkt steht das Interface, hinter der Basisklasse und durch Komma getrennt. Die Bedeutung ist aber eine andere: Die Klasse erbt nichts, sondern **verpflichtet sich**, alle Mitglieder des Interfaces öffentlich bereitzustellen.

```csharp
public sealed class Tuer : StatischesObjekt, IInteragierbar
{
    public bool IstOffen { get; private set; }

    public Tuer(Position position) : base("Tür", position) { }

    public override char Symbol => IstOffen ? '/' : 'D';
    public override bool IstPassierbar => IstOffen;

    public string Interagieren(Spieler spieler)
    {
        if (IstOffen) return "Die Tür ist schon offen.";
        if (!spieler.Inventar.Enthaelt<Schluessel>())
        {
            return "Die Tür ist verschlossen. Du brauchst einen Schlüssel.";
        }
        spieler.Inventar.Entfernen<Schluessel>();
        IstOffen = true;
        return "Du schließt die Tür auf.";
    }
}
```

Die Tür ist jetzt beides: **ein** statisches Objekt (Vererbung, mit `Name`, `Position` und dem geerbten Vertrag `Symbol`) und sie **kann** Interaktion (Interface). `public` ist bei `Interagieren` Pflicht – das Interface verspricht öffentliche Mitglieder, also muss die Klasse sie auch öffentlich anbieten. Lässt man die Methode weg oder macht sie privat, meldet der Compiler, dass `Tuer` das Interface nicht vollständig implementiert.

Nach demselben Muster wird die `Truhe` interagierbar, und die Gegenstände erfüllen den zweiten Vertrag. Interessant ist dabei, dass nicht die einzelnen Gegenstände `ISammelbar` implementieren, sondern ihre gemeinsame abstrakte Basisklasse:

```csharp
public abstract class Gegenstand : StatischesObjekt, ISammelbar
{
    protected Gegenstand(string name, Position position) : base(name, position) { }

    // Man kann auf einen Gegenstand treten – dabei wird er aufgehoben.
    public override bool IstPassierbar => true;

    public virtual string Aufheben(Spieler spieler) => $"{spieler.Name} hebt {Name} auf.";
}

public sealed class Schluessel : Gegenstand
{
    public Schluessel(Position position) : base("Schlüssel", position) { }
    public override char Symbol => 'k';
}

public sealed class Trank : Gegenstand
{
    public int Heilung { get; }
    public override char Symbol => '!';

    // Ein Trank wird sofort getrunken statt ins Inventar gelegt.
    public override string Aufheben(Spieler spieler)
    {
        spieler.Heilen(Heilung);
        return $"{spieler.Name} trinkt einen Trank (+{Heilung}).";
    }
}
```

`Schluessel` und `Trank` erfüllen `ISammelbar`, ohne es je zu erwähnen: `Name` kommt aus `Spielobjekt`, `Aufheben` aus `Gegenstand`. Ein Interface wird mitvererbt – wer von einer Klasse erbt, die es implementiert, implementiert es ebenfalls.

## Das Interface als Typ

Ein Interface kann überall dort als Typ stehen, wo auch eine Klasse stehen könnte: bei Variablen, Parametern, Rückgabewerten und in Sammlungen. Genau das nutzt die Zugregel des Spielfelds. Wenn der Spieler in eine Richtung zieht, schaut sie zuerst nach, was dort steht:

```csharp
Position ziel = Spieler.Position.Verschoben(richtung);
StatischesObjekt? davor = StatischesObjektAn(ziel);

if (davor is IInteragierbar interagierbar && !davor.IstPassierbar)
{
    // Vor einer verschlossenen Tür oder einer Truhe: interagieren statt gehen.
    meldung.Append(interagierbar.Interagieren(Spieler));
}
else if (Spieler.Bewegen(richtung, this))
{
    if (davor is Gegenstand gegenstand)
    {
        meldung.Append(gegenstand.Aufheben(Spieler));
        // ...
    }
}
```

Das `is`-Muster prüft zur Laufzeit, ob das Objekt die Fähigkeit hat, und liefert gleich eine passend typisierte Variable. Über `interagierbar` ist ausschließlich `Interagieren` erreichbar – `interagierbar.Symbol` würde nicht kompilieren, obwohl dahinter sicher ein `Spielobjekt` steckt. Das kennen wir vom [Kompilierzeittyp](/modules/laufzeittyp_verstecken/laufzeittyp_verstecken.md): Der deklarierte Typ bestimmt, was man sehen darf, der Laufzeittyp bestimmt, was passiert.

Der große Gewinn ist die Offenheit dieser Regel. Sie nennt weder `Tuer` noch `Truhe`. Jede neue Objektart, die `IInteragierbar` implementiert – ein Hebel, ein Brunnen, ein Schalter – funktioniert sofort, ohne dass `SpielerZieht` angefasst werden muss. Dasselbe gilt für das Inventar, das mit beliebigen sammelbaren Dingen arbeitet:

```csharp
public class Inventar<T> : IEnumerable<T> where T : ISammelbar
{
    // ...
}
```

Diese Schreibweise mit dem `T` schauen wir uns in Vorlesung 05 genauer an; hier zählt nur die Aussage: Was ins Inventar darf, wird über ein Interface festgelegt, nicht über eine Basisklasse.

## Interfaces in .NET

Die .NET-Klassenbibliothek ist voll von Interfaces, und du hast einige davon längst benutzt, ohne es zu merken. `foreach` funktioniert über jede Klasse, die `IEnumerable` implementiert – deshalb kann man Arrays, Listen und auch unser `Inventar<T>` mit derselben Schleife durchlaufen. `IComparable` beschreibt, dass sich Objekte vergleichen lassen, was `Sort()` für eigene Klassen möglich macht. Und `IDisposable` kennzeichnet Objekte, die Ressourcen wie Dateien freigeben müssen. Alle drei tauchen in späteren Vorlesungen im Detail auf; hier reicht die Erkenntnis: Ein Interface ist die Art, wie .NET „dieses Objekt kann X“ ausdrückt.

Übung: Schreibe eine Klasse `Hebel : StatischesObjekt, IInteragierbar`, die beim Interagieren zwischen „umgelegt“ und „zurückgestellt“ wechselt, das Symbol entsprechend `'-'` (umgelegt) oder `'|'` (zurückgestellt) liefert und nicht passierbar ist. Teste sie, indem du sie mit `feld.Hinzufuegen(new Hebel(new Position(3, 4)))` ins Spielfeld setzt und davorläufst. Welche Zeile in `Spielfeld.SpielerZieht` musstest du dafür ändern?
{: .notice--info}

## Weitere Quellen

- [Schnittstellen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/types/interfaces)
- [interface (C#-Referenz) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/interface)
- [Mustervergleich mit `is` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/functional/pattern-matching)
