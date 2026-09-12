---
title: "Schnittstellen- vs. Implementierungsvererbung"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Abstrakte Klassen und Interfaces sehen sich auf den ersten Blick ähnlich: Beide geben vor, was eine Klasse können muss, beide erlauben polymorphe Aufrufe, und beide lassen sich nicht instanziieren. Trotzdem stehst du bei jedem Entwurf vor der Frage, welches von beiden du nimmst – und eine falsche Entscheidung ist später nur mühsam zu korrigieren. Dieses Modul stellt die beiden Formen der Vererbung gegenüber und gibt dir eine Entscheidungshilfe an die Hand, die in den meisten Fällen trägt. Das Dungeon-Spiel liefert dabei für jede Variante ein Beispiel, denn es benutzt beide nebeneinander.

## Zwei Arten von Vererbung

Was wir in der [Vererbung](/modules/vererbung_grundlagen/vererbung_grundlagen.md) und bei [abstrakten Klassen](/modules/abstrakte_klassen/abstrakte_klassen.md) gesehen haben, heißt **Implementierungsvererbung**: Die Unterklasse übernimmt Code – Felder, Properties, fertige Methoden – und passt ihn punktuell mit `override` an. `Wache` erbt von `Gegner` den Namen, die Position und die komplette Methode `Bewegen`; sie erbt eine Implementierung.

Ein Interface dagegen vererbt nur eine Schnittstelle, deshalb **Schnittstellenvererbung**: Die implementierende Klasse übernimmt Signaturen, keinen Code (von Default-Methoden abgesehen). `Tuer : IInteragierbar` verspricht, `Interagieren` anzubieten, und muss selbst dafür sorgen.

| | Abstrakte Klasse | Interface |
| :--- | :--- | :--- |
| Zustand (Felder) | ja | nein |
| Konstruktoren | ja, für Unterklassen über `base(...)` | nein |
| Fertige Implementierung | ja, beliebig viel | nur Default-Methoden (C# 8+) |
| Mehrfach kombinierbar | nein, genau eine Basisklasse | ja, beliebig viele |
| Sichtbarkeit der Mitglieder | frei wählbar (`protected` möglich) | implizit `public` |
| Polymorphie | über `abstract`/`virtual` und `override` | jede Implementierung ist über den Interface-Typ polymorph |
| Nachträglich erweitern | neue `virtual`-Methode bricht nichts | neues Mitglied bricht alle Implementierungen (außer mit Default) |
| Beziehung | „ist ein“ | „kann das“ |

Die letzte Zeile ist die wichtigste. `Wand : StatischesObjekt` sagt: Eine Wand *ist ein* statisches Objekt, in jeder Hinsicht – mit Name, Position und Symbol. `Tuer : IInteragierbar` sagt: Eine Tür *kann* Interaktion – neben allem anderen, was sie als statisches Objekt auch ist.

## Entscheidungshilfe

Die Frage „abstrakte Klasse oder Interface?“ lässt sich meist mit drei Gegenfragen beantworten.

**Gibt es gemeinsamen Code oder gemeinsamen Zustand?** Alle Gegner haben einen Namen, eine Position, sie bewegen sich mit derselben `Bewegen`-Methode und tauchen im Spielfeld in derselben Liste auf. Das ist eine Menge Substanz, die `Wache` und `Verfolger` teilen. Ein Interface `IGegner` könnte das alles nur fordern, nicht liefern – jede Gegnerart müsste `Bewegen` selbst schreiben. Hier ist die abstrakte Klasse richtig, und nur der wirklich unterschiedliche Teil wird abstrakt:

```csharp
public abstract class Gegner : BeweglichesObjekt
{
    public abstract Richtung? NaechsterZug(Spielfeld feld);
}
```

**Beschreibst du eine Fähigkeit, die quer durch die Hierarchie auftaucht?** „Man kann damit interagieren“ trifft auf die Tür zu (eine Art Wand), auf die Truhe (ein Möbelstück) und später vielleicht auf einen Händler (ein bewegliches Objekt). Diese Objekte haben sonst nichts miteinander zu tun; eine gemeinsame Basisklasse gäbe es nur um den Preis, dass die Tür keine Wandeigenschaften mehr erben dürfte. Also Interface:

```csharp
public interface IInteragierbar
{
    string Interagieren(Spieler spieler);
}
```

**Brauchen die Nutzer der Abstraktion die Implementierung überhaupt?** Der Spielkern will nur wissen, welche Level es gibt und wie er eines lädt. Wo die Karten liegen, ist ihm egal – und es *soll* ihm egal sein, damit man die Quelle austauschen kann. `ILevelQuelle` als Interface hält den Kern frei von jedem Wissen über Dateien und HTTP. Eine abstrakte Klasse `LevelQuelle` hätte nichts Gemeinsames zu bieten und würde jeder Implementierung ihre einzige Basisklasse wegnehmen.

Im Zweifel Interface. Ein Interface verpflichtet zu nichts außer der Schnittstelle, lässt jeder Klasse ihre Basisklasse frei und lässt sich später immer noch um eine abstrakte Hilfsklasse ergänzen, die Teile davon implementiert. Umgekehrt ist es schwieriger: Wer sich früh auf eine abstrakte Basisklasse festlegt, blockiert die Vererbungslinie aller Unterklassen. In der .NET-Bibliothek ist dieses Muster überall zu sehen – `IEnumerable<T>` als Vertrag, dazu Klassen wie `List<T>`, die ihn erfüllen.
{: .notice--primary}

## Beides zusammen

Die beiden Konstrukte schließen sich nicht aus, im Gegenteil: Ein typischer Entwurf definiert ein Interface als Vertrag und bietet zusätzlich eine abstrakte Klasse an, die den mühsamen Teil davon schon erledigt. Wer die Basisklasse nutzen kann, spart Arbeit; wer eine andere Basisklasse braucht, implementiert das Interface direkt. Genau so ist `ISammelbar` gebaut:

```csharp
public abstract class Gegenstand : StatischesObjekt, ISammelbar
{
    public override bool IstPassierbar => true;
    public virtual string Aufheben(Spieler spieler) => $"{spieler.Name} hebt {Name} auf.";
}

public sealed class Schluessel : Gegenstand { /* nur noch Symbol => 'k' */ }

public class Auftrag : ISammelbar   // kein Spielobjekt, liegt nie auf der Karte
{
    public string Name { get; }
    public string Aufheben(Spieler spieler) => $"{spieler.Name} nimmt „{Name}“ an.";
}
```

`Schluessel`, `Trank` und `Schatz` nehmen den bequemen Weg über die Basisklasse und erben Position, Symbolvertrag und Passierbarkeit. Ein `Auftrag` erfüllt denselben Vertrag ohne eine Zeile geerbten Code. Für das `Inventar<T> where T : ISammelbar` sind beide gleichwertig – es sieht nur das Interface.

## Komposition statt Vererbung

Eine dritte Möglichkeit wird gern vergessen: gar nicht erben, sondern **enthalten**. Der Spieler muss seine Gegenstände verwalten – dafür erbt er nicht von einer Liste, sondern *hat* ein Inventar:

```csharp
public class Spieler : BeweglichesObjekt
{
    public int Lebenspunkte { get; private set; } = MaxLebenspunkte;
    public Inventar<Gegenstand> Inventar { get; } = new();
    // ...
}
```

Hätten wir stattdessen `class Spieler : Inventar<Gegenstand>` geschrieben, wäre die einzige Basisklasse verbraucht – der Spieler könnte kein `BeweglichesObjekt` mehr sein. Außerdem hätte er plötzlich `Hinzufuegen` und `Entfernen` in seiner eigenen Schnittstelle, obwohl das Aufgaben des Inventars sind. So dagegen kann dasselbe `Inventar<T>` später auch in einer Truhe oder bei einem Händler stecken.

Dieses Prinzip „Komposition vor Vererbung“ bevorzugen viele Entwickler, weil eine Vererbungsbeziehung schwer zu ändern ist, ein Feld dagegen jederzeit gegen ein anderes Objekt getauscht werden kann. Sehr oft treten Interface und Komposition gemeinsam auf: Eine Klasse *hat* ein Objekt, das sie nur über dessen Interface kennt – so wie unser Hauptprogramm eine `ILevelQuelle` hält. In Vorlesung 08 begegnen uns mit Adapter und Composite Entwurfsmuster, die ganz auf dieser Kombination beruhen.

Übung: Entscheide für jede Situation, ob abstrakte Klasse, Interface oder Komposition passt, und begründe: (a) eine neue Objektart `Feuerstelle`, die dem Spieler Schaden zufügt, wenn er darüberläuft, und gleichzeitig Licht spendet; (b) Gegner, die sich unterschiedlich bewegen, aber alle Lebenspunkte haben und beim Berühren schaden; (c) ein Spielfeld, das seinen Zustand wahlweise in eine Datei, in den Browserspeicher oder gar nicht sichern kann; (d) eine `Wache`, deren Patrouillenmuster (geradeaus, im Kreis, zufällig) beim Erzeugen festgelegt und später umgestellt werden soll.
{: .notice--info}

## Weitere Quellen

- [Vererbung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/object-oriented/inheritance)
- [Schnittstellen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/types/interfaces)
- [Objektorientierte Programmierung in C# – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/csharp/fundamentals/tutorials/oop)
