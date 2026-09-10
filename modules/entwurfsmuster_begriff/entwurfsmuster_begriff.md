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

Entwurfsmuster wirken **im Kleinen**: Sie lösen ein Problem innerhalb eines Bausteins und betreffen typischerweise zwei bis fünf zusammenarbeitende Klassen. Die Gesamtstruktur eines Systems – etwa die [Schichtenarchitektur](/modules/schichten_architektur/schichten_architektur.md) aus Vorlesung 03 – bezeichnet man dagegen als *Architekturmuster*. Beide Ebenen ergänzen sich: Innerhalb einer Schicht setzt man Entwurfsmuster ein.
{: .notice--primary}

## Warum Muster? Ein gemeinsames Vokabular

Der praktische Nutzen von Entwurfsmustern liegt nicht nur in der Lösung selbst, sondern in der **Kommunikation**. Stell dir zwei Varianten desselben Gesprächs im Team vor:

> „Ich habe eine Klasse gebaut, die eine Liste von Objekten hält, die alle ein Interface mit einer Methode `Aktualisieren` implementieren, und wenn sich mein Wert ändert, gehe ich die Liste durch und rufe bei jedem die Methode auf …“

> „Die Messstation ist ein Observer-Subjekt.“

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

Die fünf fett gedruckten Muster behandeln wir in dieser Vorlesung. Sie sind so ausgewählt, dass jede Kategorie vertreten ist und dass wir alle davon bereits *benutzt* haben, ohne sie beim Namen zu nennen: `foreach` funktioniert nur dank des Iterator-Musters, jedes `event` aus dem Modul [Ereignisse](/modules/ereignisse/ereignisse.md) ist ein Observer, und der HTML-DOM bzw. der Komponentenbaum von Blazor ist ein Composite.

Einige weitere Muster aus der Tabelle kennst du in Ansätzen ebenfalls schon. **Strategy** – ein austauschbarer Algorithmus hinter einem Interface – ist genau das, was wir mit `IComparer<T>` im Modul [`IComparable<T>` und Sortieren](/modules/icomparable_sortieren/icomparable_sortieren.md) gemacht haben, und in schlanker Form ist jeder `Func<T, bool>`-Parameter eine Strategie. **Template Method** steckt in der Klasse `Figur` des Geometrieeditors: `Beschreibung()` ist in der Basisklasse fertig, ruft aber die abstrakte Property `Flaeche` auf, die jede Unterklasse selbst ausfüllt.
{: .notice--primary}

## Muster sind kein Selbstzweck

Wer Entwurfsmuster gerade erst gelernt hat, neigt dazu, sie überall einzubauen – ein Phänomen, das so verbreitet ist, dass es einen eigenen Namen hat: *Patternitis*. Das Ergebnis sind Programme mit einer `FigurFactory`, einem `FigurenVerwaltungSingleton` und drei Adaptern, wo ein einziges `new Kreis(...)` gereicht hätte. Jedes Muster bringt zusätzliche Klassen und Indirektionen mit, und jede davon muss gelesen, verstanden und gewartet werden.

Die richtige Reihenfolge ist deshalb immer: Erst das Problem verstehen, dann prüfen, ob es *wirklich* dem Problem eines Musters entspricht, und erst dann das Muster einsetzen. Ein Muster ist nur dann eine gute Lösung, wenn man das Problem auch tatsächlich hat. Im Zweifel: Die einfachste Lösung, die funktioniert – und ein Muster erst, wenn der Code danach verlangt.
{: .notice--warning}

Übung: Öffne den Geometrieeditor aus Vorlesung 03 (`examples/03_blazor/Geometrieeditor`) und suche nach Stellen, an denen bereits Entwurfsmuster stecken – auch aus der Tabelle oben, nicht nur die fünf fetten. Tipp: Schau dir an, wie `FigurenVerwaltung` an ihren `IFigurSpeicher` kommt, und wie `Figur.Beschreibung()` mit `Flaeche` zusammenspielt.
{: .notice--info}

## Weitere Quellen

- [Entwurfsmuster – Wikipedia](https://de.wikipedia.org/wiki/Entwurfsmuster)
- [Design Patterns – Katalog mit C#-Beispielen – Refactoring.Guru](https://refactoring.guru/de/design-patterns/csharp)
- [Architekturprinzipien – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/architecture/modern-web-apps-azure/architectural-principles)
