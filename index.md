---
title: "Programmierung 2 – Objektorientierte Programmierung mit C#"
layout: single
author_profile: true
author: Erik Rodner
lecture_name: "Programmierung 2"
lecture_desc: "Objektorientierte Programmierung mit C#"
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Willkommen zur Veranstaltung **Programmierung 2** für den Studiengang Ingenieurinformatik an der HTW Berlin. Diese Veranstaltung setzt [Programmierung 1](https://www.erodner.de/prog-lecture/) fort: Du kannst bereits Programme mit Variablen, Schleifen, Methoden und ersten Klassen schreiben – jetzt lernst du, wie man mit **Objektorientierung** größere Programme strukturiert und welche Werkzeuge in der professionellen Softwareentwicklung dazugehören.

## ➤ Worum geht es?

In Programmierung 1 ging es um die „Grundrechenarten“ des Programmierens. In Programmierung 2 geht es um **Entwurf**: Wie baut man aus Klassen ein System, das man erweitern kann, ohne alles umzuschreiben? Vererbung, Interfaces, Generizität, Delegaten und Entwurfsmuster sind die Antworten der Objektorientierung auf diese Frage. Als Anwendung bauen wir eine grafische Oberfläche im Browser mit **Blazor** und lernen dabei, Software in Schichten zu organisieren. Dazu kommt das Handwerkszeug, ohne das kein Softwareprojekt auskommt: Versionsverwaltung mit Git gleich zu Beginn, später Bibliotheken über NuGet, native Bibliotheken und automatisierte Unit-Tests.

Durch die gesamte Veranstaltung zieht sich ein Beispiel: der **Geometrieeditor**. Er beginnt als kleine Klassenhierarchie, bekommt eine Oberfläche, speichert seine Daten als JSON und wird am Ende automatisch getestet. Alle Beispielprojekte findest du im Repository unter `examples/`.

Auch hier gilt *probieren geht über studieren*: Schreib die Beispiele selbst, verändere sie, bring sie zum Absturz und finde heraus, warum. KI-Assistenten sind ein Werkzeug für später – erst wenn du selbst beurteilen kannst, ob ein Vorschlag gut ist, helfen sie dir wirklich.

## ➤ Struktur der Veranstaltung

Die Vorlesung besteht aus:
- einer **Vorlesung** mit Theorie und Beispielen,
- **Übungen** mit praktischen Aufgaben und
- einem **Tutorium** als zusätzliche Unterstützung

## ➤ Inhalt der Veranstaltung

### Teil 0: Einstieg

0. [Einstieg – Rückblick und Werkzeuge](/lectures/00/00.md) – Was du aus Programmierung 1 mitbringst, .NET SDK, IDE, dotnet-CLI

### Teil 1: Objektorientierung vertieft

1. [Vererbung](/lectures/01/01.md) – `virtual`/`override`, `sealed`, Laufzeittyp, `object`, Garbage Collection
2. [Abstrakte Klassen und Interfaces](/lectures/02/02.md) – `abstract`, `interface`, Schnittstellen- vs. Implementierungsvererbung

### Teil 2: Zusammenarbeit

3. [Git – Versionsverwaltung](/lectures/03/03.md) – Konzepte, Kommandozeile, Branches, Merge-Konflikte, IDE-Integration

### Teil 3: Anwendungen bauen

4. [GUI mit Blazor und Schichten-Architekturen](/lectures/04/04.md) – Razor-Komponenten, Layout, Ereignisse, Datenbindung, Dialoge, Drei-Schichten-Architektur

### Teil 4: Generisch und funktional

5. [Generizität](/lectures/05/05.md) – Generische Methoden und Typen, Constraints
6. [Collections, Algorithmen und LINQ](/lectures/06/06.md) – Hashing, Sortieren, Suchen, Collections, LINQ-Abfragen
7. [Delegaten, Lambdas und Ereignisse](/lectures/07/07.md) – `delegate`, `Func`/`Action`, Lambda-Ausdrücke, `event`

### Teil 5: Entwurf

8. [Entwurfsmuster](/lectures/08/08.md) – Singleton, Adapter, Composite, Iterator, Observer

### Teil 6: Daten rein und raus

9. [Dateien, Streams und Serialisierung](/lectures/09/09.md) – `File`, Streams, `IDisposable`, JSON/XML, `HttpClient`

### Teil 7: Werkzeuge der Softwareentwicklung

10. [Native Bibliotheken – P/Invoke](/lectures/10/10.md) – `DllImport`, `LibraryImport`, Marshalling
11. [NuGet – Paketverwaltung](/lectures/11/11.md) – Pakete finden, einbinden, bewerten
12. [Unit-Testing](/lectures/12/12.md) – NUnit, Assertions, `dotnet test`

---

## ★ Lernziele

Am Ende dieser Veranstaltung kannst du:

1. **Klassenhierarchien** mit Vererbung, abstrakten Klassen und Interfaces entwerfen und begründen, wann welches Mittel passt.
2. **Generische Typen und Methoden** schreiben und die Collections von .NET gezielt einsetzen.
3. **Delegaten, Lambda-Ausdrücke und Ereignisse** verstehen und für LINQ-Abfragen und lose gekoppelte Komponenten nutzen.
4. Gängige **Entwurfsmuster** erkennen und anwenden.
5. Eine **grafische Oberfläche** mit Blazor bauen und die Anwendung in **Schichten** strukturieren.
6. Daten mit **Dateien, Streams und JSON** dauerhaft speichern und über HTTP laden.
7. Mit **Git** im Team arbeiten, **NuGet-Pakete** einbinden und deinen Code mit **Unit-Tests** absichern.

## ➤ Bewertung

Alle relevanten Informationen zur Prüfungsleistung findest du auf **Moodle**.

## Literaturempfehlungen

1. [Microsoft C# Dokumentation](https://learn.microsoft.com/de-de/dotnet/csharp/) – offizielle Referenz, auf Deutsch verfügbar
2. Wurm, Roman: *Schrödinger programmiert C#* (Rheinwerk) – unterhaltsamer Einstieg, viele Kapitel decken diese Vorlesung ab
3. [ASP.NET Core Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/) – Referenz für das GUI-Framework der Vorlesung
4. [Pro Git](https://git-scm.com/book/de/v2) – das freie Git-Buch, auf Deutsch
5. Gamma, Helm, Johnson, Vlissides: *Entwurfsmuster* (Addison-Wesley) – das Original der „Gang of Four“
