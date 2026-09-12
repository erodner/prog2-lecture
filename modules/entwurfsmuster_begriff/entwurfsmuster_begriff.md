---
title: "Was ist ein Entwurfsmuster?"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Wer ein Haus baut, erfindet die Tür nicht neu. Es gibt bewährte Lösungen für wiederkehrende Probleme: eine Tür für den Durchgang, ein Fenster für Licht, ein Treppenhaus für die Verbindung von Stockwerken. Im Maschinenbau heißen solche Bausteine Konstruktionselemente – Welle, Lager, Schraubverbindung – und niemand käme auf die Idee, für jedes Getriebe eine neue Art von Lager zu entwerfen. In der Softwareentwicklung ist das anders: Jede Klasse ist Handarbeit, und es ist verführerisch, jedes Problem von Grund auf neu zu lösen. Dabei tauchen bestimmte Entwurfsprobleme in fast jedem Projekt wieder auf: „Ich brauche genau eine Instanz dieser Klasse“, „Diese fremde Klasse hat die falsche Schnittstelle“, „Diese Objekte sollen erfahren, wenn sich jenes ändert“. Für genau diese Probleme gibt es **Entwurfsmuster** – erprobte Lösungen, die schon tausendfach funktioniert haben.

## Definition

Ein **Entwurfsmuster** (*Design Pattern*) ist eine wiederverwendbare, bewährte Lösung für ein wiederkehrendes Entwurfsproblem in einem bestimmten Kontext. Drei Eigenschaften machen diese Definition aus:

- **Wiederkehrend:** Das Problem tritt nicht nur einmal auf, sondern in vielen verschiedenen Projekten in ähnlicher Form.
- **Bewährt:** Die Lösung ist nicht ausgedacht, sondern aus der Praxis abgeleitet – sie hat sich in echten Systemen als tragfähig erwiesen.
- **Wiederverwendbar:** Das Muster beschreibt keine fertige Klasse zum Kopieren, sondern ein Schema aus Rollen, Beziehungen und Verantwortlichkeiten, das man an die eigene Situation anpasst.

Ein Muster ist also keine Bibliothek und kein Codeschnipsel. Es ist eine *Idee*, die man kennen und im richtigen Moment anwenden muss. Genau deshalb enthält die Beschreibung eines Musters immer mehr als nur Code: einen Namen, das Problem, die Lösung mit den beteiligten Klassen, Beispiele und – ganz wichtig – die Vor- und Nachteile. Nach diesem Schema sind auch die fünf Module dieser Vorlesung aufgebaut.

Entwurfsmuster wirken **im Kleinen**: Sie lösen ein Problem innerhalb eines Bausteins und betreffen typischerweise zwei bis fünf zusammenarbeitende Klassen. Die Gesamtstruktur eines Systems – etwa die [Schichtenarchitektur](/modules/schichten_architektur/schichten_architektur.md) aus Vorlesung 04, die `Adventure.Web` und `Adventure.Konsole` von `Adventure.Kern` trennt – bezeichnet man dagegen als *Architekturmuster*. Beide Ebenen ergänzen sich: Innerhalb einer Schicht setzt man Entwurfsmuster ein.
{: .notice--primary}

## Warum Muster? Ein gemeinsames Vokabular

Der praktische Nutzen von Entwurfsmustern liegt nicht nur in der Lösung selbst, sondern in der **Kommunikation**. Stell dir zwei Varianten desselben Gesprächs im Team vor:

> „Ich habe eine Klasse gebaut, die eine Liste von Objekten hält, die alle ein Interface mit einer Methode `Aktualisieren` implementieren, und wenn sich mein Wert ändert, gehe ich die Liste durch und rufe bei jedem die Methode auf …“

> „Der Spieler ist ein Observer-Subjekt.“

Beide Sätze beschreiben dasselbe Design. Der zweite braucht fünf Worte, und jede Kollegin, die das Muster kennt, weiß sofort, welche Klassen es gibt, wie sie zusammenhängen und wo die typischen Fallstricke liegen. Muster sind damit eine Art Fachsprache – wie „Welle“ und „Lager“ im Maschinenbau. Ein zweiter Vorteil: Wer ein Muster erkennt, kann fremden Code viel schneller lesen. Sieht man in einer Klasse einen privaten Konstruktor und eine statische Property `Instanz`, muss man den Rest nicht mehr entziffern – das ist ein Singleton, und man weiß, was einen erwartet.

## Ein kurzer Blick in die Geschichte

Die Idee stammt nicht aus der Informatik. Der Architekt Christopher Alexander beschrieb in den 1970er-Jahren wiederkehrende Lösungen im Städte- und Hausbau als „Muster“ und stellte daraus eine *Mustersprache* zusammen. Kent Beck und Ward Cunningham übertrugen den Ansatz 1987 auf die objektorientierte Programmierung mit Smalltalk. Den Durchbruch brachte 1994 das Buch *Design Patterns – Elements of Reusable Object-Oriented Software* von Erich Gamma, Richard Helm, Ralph Johnson und John Vlissides. Die vier Autoren werden seitdem als **Gang of Four** (GoF) bezeichnet, und ihr Katalog von 23 Mustern ist bis heute die gemeinsame Referenz – auch wenn die Beispiele darin noch in C++ und Smalltalk geschrieben sind. Eine gut lesbare deutschsprachige Darstellung mit C#-Beispielen bietet Matthias Geirhos in *Entwurfsmuster – Das umfassende Handbuch* (Rheinwerk Verlag).

## Die drei Kategorien der GoF

Die Gang of Four hat ihre 23 Muster nach der Frage sortiert, *welche Art* von Problem sie lösen:

- **Erzeugungsmuster** (*Creational Patterns*) kümmern sich darum, *wie* Objekte entstehen. Wer darf `new` aufrufen, wie viele Instanzen gibt es, und wie kann man das Erzeugen komplexer Objekte kapseln?
- **Strukturmuster** (*Structural Patterns*) beschreiben, wie Klassen und Objekte zu größeren Strukturen zusammengesetzt werden – Schablonen für die Beziehungen zwischen Klassen.
- **Verhaltensmuster** (*Behavioral Patterns*) regeln die Interaktion zwischen Objekten und die Aufteilung von Zuständigkeiten: Wer benachrichtigt wen, wer durchläuft was, wer entscheidet was?

| Erzeugungsmuster | Strukturmuster | Verhaltensmuster |
| :--- | :--- | :--- |
| Abstract Factory | **Adapter** | Chain of Responsibility |
| Builder | Bridge | Command |
| Factory Method | **Composite** | Interpreter |
| Prototype | Decorator | **Iterator** |
| **Singleton** | Facade | Mediator |
| | Flyweight | Memento |
| | Proxy | **Observer** |
| | | State |
| | | Strategy |
| | | Template Method |
| | | Visitor |

Die fünf fett gedruckten Muster behandeln wir in dieser Vorlesung. Sie sind so ausgewählt, dass jede Kategorie vertreten ist – und dass wir die meisten davon bereits *benutzt* haben, ohne sie beim Namen zu nennen.

## Die Muster im Adventure

Das durchgehende Beispiel dieses Kurses ist dafür eine Fundgrube. Fast jedes Muster dieser Vorlesung steckt schon irgendwo im Spiel, und die restlichen ergänzen wir in den folgenden Modulen:

| Muster | Wo es im Adventure steckt |
| :--- | :--- |
| **Iterator** | `Spielfeld.AlleObjekte` liefert mit `yield return` erst die statischen Objekte, dann die Gegner, dann den Spieler. `Inventar<T>` implementiert `IEnumerable<T>`, damit `foreach` über die Gegenstände läuft. |
| **Observer** | `Spieler.SchatzGefunden` und `Spielfeld.RundeBeendet`: Die Konsole piept, die Statusleiste zeichnet neu – und der Kern kennt keinen von beiden. |
| **Adapter** | Fehlt noch: Konsole und Blazor übersetzen `ConsoleKey` bzw. `KeyboardEventArgs.Key` jeweils selbst in eine `Richtung`. Wir bauen daraus ein Interface `IEingabe`. |
| **Composite** | Fehlt noch: Ein `Raum` als Gruppe von Wänden, die sich wie ein einzelnes Bauteil verschieben und aufs Feld setzen lässt. |
| **Singleton** | Bewusst nicht verwendet: Sichtweite und Startleben stehen im Konstruktor von `Verfolger` bzw. als `const` in `Spieler`. Wir bauen im Singleton-Modul eine `Spielkonfiguration` – und diskutieren, warum sie mehr schadet als nützt. |
| **Strategy** | `Gegner.NaechsterZug` ist ein austauschbarer Algorithmus hinter einer abstrakten Methode – und in der Delegat-Variante aus [Vorlesung 07](/lectures/07/07.md) ist jeder `Func<Spielfeld, Gegner, Richtung?>` eine Strategie. |
| **Template Method** | `Spielfeld.SpielerZieht` legt den Ablauf einer Runde fest – erst der Held, dann alle Gegner, dann die Meldung – und überlässt den variablen Schritt den Gegnerklassen. |
| **Factory Method** | `LevelParser.ObjektFuer(char, Position)` entscheidet, welche Klasse hinter einem Zeichen der Textkarte steckt. Der Rest des Parsers kennt nur `Spielobjekt`. |

Dass wir diese Lösungen gefunden haben, ohne die Muster zu kennen, ist typisch: Gute Entwürfe konvergieren. Der Nutzen des Katalogs ist, dass man sie beim nächsten Mal *schneller* findet und mit einem Wort benennen kann.
{: .notice--primary}

## Muster sind kein Selbstzweck

Wer Entwurfsmuster gerade erst gelernt hat, neigt dazu, sie überall einzubauen – ein Phänomen, das so verbreitet ist, dass es einen eigenen Namen hat: *Patternitis*. Das Ergebnis sind Programme mit einer `SpielobjektFactory`, einem `SpielfeldSingleton` und drei Adaptern, wo ein einziges `new Wand(position)` gereicht hätte. Jedes Muster bringt zusätzliche Klassen und Indirektionen mit, und jede davon muss gelesen, verstanden und gewartet werden.

Die richtige Reihenfolge ist deshalb immer: Erst das Problem verstehen, dann prüfen, ob es *wirklich* dem Problem eines Musters entspricht, und erst dann das Muster einsetzen. Ein Muster ist nur dann eine gute Lösung, wenn man das Problem auch tatsächlich hat. Im Zweifel: Die einfachste Lösung, die funktioniert – und ein Muster erst, wenn der Code danach verlangt.
{: .notice--warning}

Übung: Öffne das Repository [prog2-adventure](https://github.com/erodner/prog2-adventure) (Tag `v04-blazor`) und suche nach Stellen, an denen bereits Entwurfsmuster stecken – auch solche aus der Tabelle, die wir nicht behandeln. Tipp: Schau dir an, wie `Adventure.Web/Program.cs` die `ILevelQuelle` registriert und wie `Home.razor` sie über `@inject` bekommt. Welches Problem löst das, und welches Muster ist das nicht ganz?
{: .notice--info}

## Weitere Quellen

- [Entwurfsmuster – Wikipedia](https://de.wikipedia.org/wiki/Entwurfsmuster)
- [Design Patterns – Katalog mit C#-Beispielen – Refactoring.Guru](https://refactoring.guru/de/design-patterns/csharp)
- [Architekturprinzipien – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/architecture/modern-web-apps-azure/architectural-principles)
