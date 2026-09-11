---
title: "Schichten-Architekturen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Bis hierhin haben wir Fenster gebaut und Handler geschrieben – und dabei die vielleicht wichtigste Frage der Oberflächenprogrammierung noch gar nicht gestellt: *Wo gehört welcher Code hin?* Wer die Flächensumme im Klick-Handler ausrechnet und die Datei direkt aus dem Menü-Handler schreibt, hat nach zwei Wochen ein Programm, in dem alles mit allem zusammenhängt. Eine kleine Änderung an der Speicherung zieht Änderungen an drei Fenstern nach sich, und testen lässt sich nichts, ohne die Fenster zu starten. Gegen dieses Verfilzen hilft ein Bauplan auf einer Ebene über den Klassen: die **Software-Architektur**. Ein Haus hat Fundament, Geschosse und Dach, und die Elektrik führt nicht kreuz und quer durch die Wände – genau diese Ordnung wollen wir auch im Code.

## Was ist ein Architekturstil?

Ein **Architekturstil** ist ein bewährtes Grundmuster dafür, wie ein Programm in große Bausteine zerlegt wird und welche Bausteine miteinander reden dürfen. Während ein [Entwurfsmuster](/modules/entwurfsmuster_begriff/entwurfsmuster_begriff.md) (Vorlesung 08) das Zusammenspiel weniger Klassen beschreibt, geht es beim Architekturstil um die Struktur der ganzen Anwendung. Zwei Stile stellen wir gegenüber: den Monolithen, der historisch der Normalfall war, und die Schichtenarchitektur, die heute der Standard für Desktop- und Geschäftsanwendungen ist.

## Monolithisch

Bei der **monolithischen** Struktur besteht die Anwendung aus einem einzigen ausführbaren Programm, in das alle Bestandteile fest einkompiliert sind. Bis in die 1980er Jahre war das die einzige Möglichkeit, denn dynamische Bibliotheken (`.dll` unter Windows, `.so` unter Linux, `.dylib` unter macOS), die mehrere Programme gemeinsam nutzen, kamen erst später auf. Der Stil hat zwei Vorteile: Die Implementierung ist einfach, weil es keine Abhängigkeiten von anderen Bausteinen gibt, und die Installation ist trivial, weil eine einzige Datei kopiert wird. Dem stehen gewichtige Nachteile gegenüber:

- Keine Wiederverwendung auf Bibliotheksebene – jedes Programm bringt seine eigene Kopie aller Bausteine mit und lädt sie erneut in den Arbeitsspeicher.
- Kein innerer Aufbau, der Änderungen eingrenzt – alles darf alles aufrufen.
- Nur für einfache Anforderungen geeignet.

Heute begegnet man reinen Monolithen kaum noch; selbst ein kleines Kommandozeilenwerkzeug in C# referenziert die .NET-Laufzeitbibliotheken dynamisch. Sinnvoll bleibt der Stil für kleine Werkzeuge und für Prototypen einer Oberfläche, bei denen die Struktur nicht wichtig ist. Für alles andere gilt: vermeiden.

## Schichten

Bei der **Schichtenarchitektur** (*layered architecture*) wird eine Anwendung nach funktionalen Gesichtspunkten in horizontale Schichten zerlegt, die übereinander liegen. Eine höhere Schicht darf die Dienste der darunterliegenden nutzen, aber nie umgekehrt – und der Zugriff erfolgt nicht direkt auf die Klassen der tieferen Schicht, sondern über Abstraktionen, in C# also über [Interfaces](/modules/interfaces_grundlagen/interfaces_grundlagen.md). Daraus ergeben sich vier Regeln:

1. Eine Schicht *n* hängt nur von der direkt darunterliegenden Schicht *n − 1* ab.
2. Eine Schicht hängt **nie** von einer höheren Schicht ab.
3. Jede Schicht bietet der darüberliegenden Schicht Dienste an.
4. Der Zugriff auf einen Dienst der tieferen Schicht erfolgt über deren Schnittstellen, nicht über konkrete Klassen.

Regel 2 ist die wichtigste und die, die am häufigsten verletzt wird. Sobald die Datenhaltung „nur mal kurz“ eine Meldung im Fenster anzeigt, kennt die unterste Schicht die oberste, und die Ordnung ist dahin.

Die Schichten sind ein Bauplan der *logischen* Architektur. In C# bildet man sie meist eins zu eins auf Projekte einer Solution ab, wie wir es aus [Projekte mit der dotnet-CLI](/modules/dotnet_cli_projekte/dotnet_cli_projekte.md) kennen – dann prüft der Compiler die Regeln 1 und 2 gleich mit, denn ein Zirkelbezug zwischen Projekten lässt sich nicht bauen.
{: .notice--primary}

## Vorteile und Nachteile

Was gewinnt man durch die Disziplin? Zunächst eine **klare Aufgabenteilung** und einen klaren Datenfluss: Man weiß, in welcher Schicht eine Änderung stattfindet. Weil eine Schicht nur von Abstraktionen der darunterliegenden abhängt, sollten Änderungen am Quellcode einer Schicht keine andere Schicht berühren. Die Schichten lassen sich **von unten nach oben** integrieren und testen – die Fachlogik kann fertig und geprüft sein, bevor das erste Fenster existiert. Eine Schicht kann **ausgetauscht** werden, solange die Schnittstelle gleich bleibt: Aus der Speicherung im Arbeitsspeicher wird eine JSON-Datei, später eine Datenbank, ohne dass die Fachlogik davon erfährt. Und schließlich können die Schichten sogar auf verschiedenen Rechnern laufen – Oberfläche auf dem Client, Datenhaltung auf einem Server.

Es gibt auch Kosten. Eine Anfrage, die von oben nach unten durch mehrere Schichten gereicht wird, ist langsamer als ein direkter Aufruf; bei vielen Schichten und hoher Last kann die **Performance** leiden. Der Ausweg, aus Geschwindigkeitsgründen eine Schicht zu überspringen, heißt **Layer Bridging** – er löst das Problem, schafft aber genau die zusätzlichen Abhängigkeiten, die man vermeiden wollte. Das sollte eine bewusste, dokumentierte Ausnahme bleiben.

## Die Drei-Schichten-Architektur

Das Standardmodell für Desktopanwendungen mit einem nennenswerten Fachmodell – CAD-Programme, Office-Pakete, Entwicklungsumgebungen – hat drei Schichten:

```
┌──────────────────────────────────────────────────┐
│  GUI-Schicht                                     │  Fenster, Dialoge, Steuerelemente
│  zeigt an, nimmt Eingaben entgegen               │  (Geometrieeditor.Web)
├──────────────────────────────────────────────────┤
│                        │ nutzt                   │
│                        ▼                         │
│  Fachkonzeptschicht                              │  Domänenmodell und Geschäftslogik
│  Figuren, Regeln, Berechnungen                   │  (Geometrieeditor.Fachkonzept)
├──────────────────────────────────────────────────┤
│                        │ nutzt über IFigurSpeicher
│                        ▼                         │
│  Datenhaltungsschicht                            │  Speichern und Laden
│  Arbeitsspeicher, JSON-Datei, Datenbank          │  (Geometrieeditor.Datenhaltung)
└──────────────────────────────────────────────────┘
```

Die **GUI-Schicht** enthält alles, was der Benutzer sieht, und nichts, was er nicht sieht. Die **Fachkonzeptschicht** ist der funktionale Kern: die Klassen des Anwendungsgebiets (im Geometrieeditor `Figur`, `Rechteck`, `Kreis`) und die Regeln, die für sie gelten (kein doppelter Name, Flächensumme). Sie weiß nicht, ob sie von einem Fenster, einer Konsole oder einem Test aufgerufen wird. Die **Datenhaltungsschicht** bringt die Objekte auf die Platte und wieder zurück und weiß nichts über Regeln oder Fenster.

Wo ist die Grenze zwischen GUI und Fachkonzept? Eine brauchbare Probe: Könnte diese Zeile Code unverändert in einer Konsolenversion des Programms stehen? Wenn ja, gehört sie ins Fachkonzept. Wenn sie ein Steuerelement anfasst, gehört sie in die GUI.
{: .notice--primary}

## Zwei und mehr Schichten

Die **Zwei-Schichten-Architektur** fasst Oberfläche und Fachkonzept zu einer Anwendungsschicht zusammen und setzt nur die Datenhaltung darunter. Man findet sie in einfachen und in älteren Programmen – sie ist genau das, was entsteht, wenn die Geschäftslogik in den Klick-Handlern landet. Sie ist zu vermeiden, denn die Fachlogik ist dann nicht ohne Oberfläche testbar und nicht wiederverwendbar. Bei der **Mehr-Schichten-Architektur** kommen dagegen zusätzliche Zugriffsschichten hinzu, die die Komplexität des Fachkonzepts oder der Datenhaltung vor dem Aufrufer verbergen, etwa eine Schicht, die aus vielen kleinen Fachoperationen wenige große Anwendungsfälle zusammensetzt, oder eine, die den Zugriff auf eine entfernte Datenbank kapselt. Für den Geometrieeditor reichen drei Schichten; wie wir sie mit Blazor konkret umsetzen, zeigt das Modul [Schichten mit Blazor umsetzen](/modules/schichten_mit_blazor/schichten_mit_blazor.md).

Übung: Ordne die folgenden Aufgaben eines Notenverwaltungsprogramms den drei Schichten zu: Durchschnitt berechnen, Studierendenliste als CSV lesen, prüfen, dass eine Note zwischen 1,0 und 5,0 liegt, den Fehlertext rot anzeigen, die Liste nach Namen sortieren, den Dateipfad aus einem Dialog holen. Bei welchen Aufgaben ist die Zuordnung nicht eindeutig, und welche Frage entscheidet dann?
{: .notice--info}

## Weitere Quellen

- [Allgemeine Webanwendungsarchitekturen (Schichten) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [Architekturprinzipien – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/architecture/modern-web-apps-azure/architectural-principles)
- [Schichtenarchitektur – Wikipedia](https://de.wikipedia.org/wiki/Schichtenarchitektur)
