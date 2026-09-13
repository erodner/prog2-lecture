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

Bis hierhin haben wir Seiten gebaut und Handler geschrieben – und dabei die vielleicht wichtigste Frage der Oberflächenprogrammierung noch gar nicht gestellt: *Wo gehört welcher Code hin?* Wer die Kollisionsprüfung im Tastatur-Handler erledigt und die Level direkt aus der Komponente heraus aus einer Datei liest, hat nach zwei Wochen ein Programm, in dem alles mit allem zusammenhängt. Eine kleine Änderung am Levelformat zieht Änderungen an drei Komponenten nach sich, testen lässt sich nichts, ohne einen Browser zu starten, und die Konsolenversion des Spiels stirbt einen leisen Tod, weil ihr plötzlich die halben Spielregeln fehlen. Gegen dieses Verfilzen hilft ein Bauplan auf einer Ebene über den Klassen: die **Software-Architektur**. Ein Haus hat Fundament, Geschosse und Dach, und die Elektrik führt nicht kreuz und quer durch die Wände – genau diese Ordnung wollen wir auch im Code.

## Was ist ein Architekturstil?

Ein **Architekturstil** ist ein bewährtes Grundmuster dafür, wie ein Programm in große Bausteine zerlegt wird und welche Bausteine miteinander reden dürfen. Während ein [Entwurfsmuster](/modules/entwurfsmuster_begriff/entwurfsmuster_begriff.md) (Vorlesung 08) das Zusammenspiel weniger Klassen beschreibt, geht es beim Architekturstil um die Struktur der ganzen Anwendung. Zwei Stile stellen wir gegenüber: den Monolithen, der historisch der Normalfall war, und die Schichtenarchitektur, die heute der Standard für Desktop-, Web- und Geschäftsanwendungen ist.

## Monolithisch

Bei der **monolithischen** Struktur besteht die Anwendung aus einem einzigen ausführbaren Programm, in das alle Bestandteile fest einkompiliert sind. Bis in die 1980er Jahre war das die einzige Möglichkeit, denn dynamische Bibliotheken (`.dll` unter Windows, `.so` unter Linux, `.dylib` unter macOS), die mehrere Programme gemeinsam nutzen, kamen erst später auf. Der Stil hat zwei Vorteile: Die Implementierung ist einfach, weil es keine Abhängigkeiten von anderen Bausteinen gibt, und die Installation ist trivial, weil eine einzige Datei kopiert wird. Dem stehen gewichtige Nachteile gegenüber:

- Keine Wiederverwendung auf Bibliotheksebene – jedes Programm bringt seine eigene Kopie aller Bausteine mit und lädt sie erneut in den Arbeitsspeicher.
- Kein innerer Aufbau, der Änderungen eingrenzt – alles darf alles aufrufen.
- Nur für einfache Anforderungen geeignet.

Wäre unser Adventure ein Monolith, gäbe es genau ein Projekt, in dem Spielregeln, Levelkarten und Razor-Komponenten nebeneinanderlägen. Eine zweite Oberfläche wäre dann nicht „ein weiteres Projekt“, sondern eine Kopie des halben Programms. Heute begegnet man reinen Monolithen kaum noch; selbst ein kleines Kommandozeilenwerkzeug in C# referenziert die .NET-Laufzeitbibliotheken dynamisch. Sinnvoll bleibt der Stil für kleine Werkzeuge und für Prototypen, bei denen die Struktur nicht wichtig ist. Für alles andere gilt: vermeiden.

## Schichten

Bei der **Schichtenarchitektur** (*layered architecture*) wird eine Anwendung nach funktionalen Gesichtspunkten in horizontale Schichten zerlegt, die übereinander liegen. Eine höhere Schicht darf die Dienste der darunterliegenden nutzen, aber nie umgekehrt – und der Zugriff erfolgt nicht direkt auf die Klassen der tieferen Schicht, sondern über Abstraktionen, in C# also über [Interfaces](/modules/interfaces_grundlagen/interfaces_grundlagen.md). Daraus ergeben sich vier Regeln:

1. Eine Schicht *n* hängt nur von der direkt darunterliegenden Schicht *n − 1* ab.
2. Eine Schicht hängt **nie** von einer höheren Schicht ab.
3. Jede Schicht bietet der darüberliegenden Schicht Dienste an.
4. Der Zugriff auf einen Dienst der tieferen Schicht erfolgt über deren Schnittstellen, nicht über konkrete Klassen.

Regel 2 ist die wichtigste und die, die am häufigsten verletzt wird. Sobald die Datenhaltung „nur mal kurz“ eine Meldung auf der Seite anzeigt, kennt die unterste Schicht die oberste, und die Ordnung ist dahin.

Die Schichten sind ein Bauplan der *logischen* Architektur. In C# bildet man sie meist eins zu eins auf Projekte einer Solution ab, wie wir es aus [Projekte mit der dotnet-CLI](/modules/dotnet_cli_projekte/dotnet_cli_projekte.md) kennen – dann prüft der Compiler die Regeln 1 und 2 gleich mit, denn ein Zirkelbezug zwischen Projekten lässt sich nicht bauen.
{: .notice--primary}

## Vorteile und Nachteile

Was gewinnt man durch die Disziplin? Zunächst eine **klare Aufgabenteilung** und einen klaren Datenfluss: Man weiß, in welcher Schicht eine Änderung stattfindet. Weil eine Schicht nur von Abstraktionen der darunterliegenden abhängt, sollten Änderungen am Quellcode einer Schicht keine andere Schicht berühren. Die Schichten lassen sich **von unten nach oben** integrieren und testen – die Spielregeln können fertig und geprüft sein, bevor die erste Seite existiert. Eine Schicht kann **ausgetauscht** werden, solange die Schnittstelle gleich bleibt: Aus fest einprogrammierten Levelkarten wird eine Textdatei, später ein Webdienst, ohne dass die Spielregeln davon erfahren (genau das bauen wir in [Vorlesung 09](/lectures/09/09.md)). Und schließlich können die Schichten sogar auf verschiedenen Rechnern laufen – Oberfläche auf dem Client, Datenhaltung auf einem Server.

Es gibt auch Kosten. Eine Anfrage, die von oben nach unten durch mehrere Schichten gereicht wird, ist langsamer als ein direkter Aufruf; bei vielen Schichten und hoher Last kann die **Performance** leiden. Der Ausweg, aus Geschwindigkeitsgründen eine Schicht zu überspringen, heißt **Layer Bridging** – er löst das Problem, schafft aber genau die zusätzlichen Abhängigkeiten, die man vermeiden wollte. Das sollte eine bewusste, dokumentierte Ausnahme bleiben.

## Die Drei-Schichten-Architektur

Das Standardmodell für Anwendungen mit einem nennenswerten Fachmodell – CAD-Programme, Office-Pakete, Entwicklungsumgebungen, aber eben auch ein Spiel – hat drei Schichten. So sieht unser Adventure aus:

<svg viewBox="0 0 700 444" role="img" aria-labelledby="titel-drei-schichten" xmlns="http://www.w3.org/2000/svg" style="max-width:100%;height:auto;font-family:system-ui,sans-serif">
<title id="titel-drei-schichten">Drei Schichten des Adventure: oben die beiden Oberflächen Adventure.Konsole und Adventure.Web, darunter Adventure.Kern mit den Interfaces ILevelQuelle und ISpielstandSpeicher am unteren Rand, darunter Adventure.Daten, dessen Pfeil nach oben auf diese Interfaces zeigt.</title>
<defs>
<marker id="pfeil-schichten-nutzt" viewBox="0 0 10 10" refX="0" refY="5" markerWidth="10" markerHeight="10" markerUnits="userSpaceOnUse" orient="auto">
<path d="M0 0 L10 5 L0 10" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linejoin="round" stroke-linecap="round"/>
</marker>
<marker id="pfeil-schichten-vertrag" viewBox="0 0 10 10" refX="0" refY="5" markerWidth="10" markerHeight="10" markerUnits="userSpaceOnUse" orient="auto">
<path d="M0 0 L10 5 L0 10 Z" fill="none" stroke="#d33682" stroke-width="1.5" stroke-dasharray="none" stroke-linejoin="round"/>
</marker>
</defs>
<text x="8" y="18" font-size="15" fill="currentColor">Die drei Schichten des Adventure</text>
<g fill="currentColor" fill-opacity="0.06" stroke="currentColor" stroke-width="1.5">
<rect x="8" y="36" width="684" height="86" rx="6"/>
<rect x="8" y="160" width="684" height="124" rx="6"/>
<rect x="8" y="330" width="684" height="86" rx="6"/>
</g>
<g fill="currentColor" fill-opacity="0.06" stroke="currentColor" stroke-width="1.5">
<rect x="90" y="66" width="250" height="46" rx="4"/>
<rect x="360" y="66" width="250" height="46" rx="4"/>
<rect x="30" y="190" width="120" height="40" rx="4"/>
<rect x="175" y="190" width="215" height="40" rx="4"/>
<rect x="415" y="190" width="110" height="40" rx="4"/>
<rect x="550" y="190" width="110" height="40" rx="4"/>
<rect x="30" y="360" width="190" height="40" rx="4"/>
<rect x="240" y="360" width="190" height="40" rx="4"/>
<rect x="450" y="360" width="200" height="40" rx="4"/>
</g>
<g fill="currentColor" fill-opacity="0.12" stroke="currentColor" stroke-width="1.5">
<rect x="155" y="238" width="150" height="38" rx="4"/>
<rect x="455" y="238" width="190" height="38" rx="4"/>
</g>
<g fill="none" stroke="currentColor" stroke-width="1.5">
<path d="M215 112 V150" marker-end="url(#pfeil-schichten-nutzt)"/>
<path d="M485 112 V150" marker-end="url(#pfeil-schichten-nutzt)"/>
</g>
<g fill="none" stroke="#d33682" stroke-width="1.5" stroke-dasharray="5 4">
<path d="M125 330 V312"/>
<path d="M335 330 V312"/>
<path d="M125 312 H335"/>
<path d="M230 312 V286" marker-end="url(#pfeil-schichten-vertrag)"/>
<path d="M550 330 V286" marker-end="url(#pfeil-schichten-vertrag)"/>
</g>
<g fill="currentColor">
<text x="20" y="56" font-size="13">GUI-Schicht</text>
<text x="680" y="56" font-size="12" text-anchor="end">zeigt an, nimmt Eingaben entgegen</text>
<text x="20" y="180" font-size="13">Fachkonzeptschicht</text>
<text x="155" y="180" font-size="12" font-family="ui-monospace,monospace">Adventure.Kern</text>
<text x="680" y="180" font-size="12" text-anchor="end">Spielregeln und Domänenmodell</text>
<text x="20" y="350" font-size="13">Datenhaltungsschicht</text>
<text x="170" y="350" font-size="12" font-family="ui-monospace,monospace">Adventure.Daten</text>
<text x="680" y="350" font-size="12" text-anchor="end">Level und Spielstände beschaffen</text>
<text x="30" y="253" font-size="12">Der Kern</text>
<text x="30" y="269" font-size="12">definiert:</text>
</g>
<g fill="currentColor" text-anchor="middle">
<text x="215" y="88" font-size="12" font-family="ui-monospace,monospace">Adventure.Konsole</text>
<text x="215" y="104" font-size="12">AlsText(), Console.ReadKey</text>
<text x="485" y="88" font-size="12" font-family="ui-monospace,monospace">Adventure.Web</text>
<text x="485" y="104" font-size="12">Home.razor, Statusleiste.razor</text>
<text x="90" y="215" font-size="12" font-family="ui-monospace,monospace">Spielfeld</text>
<text x="283" y="207" font-size="12" font-family="ui-monospace,monospace">Spielobjekt</text>
<text x="283" y="223" font-size="11">Wand, Tuer, Truhe, Gegenstand</text>
<text x="470" y="215" font-size="12" font-family="ui-monospace,monospace">Spieler</text>
<text x="605" y="215" font-size="12" font-family="ui-monospace,monospace">Gegner</text>
<text x="230" y="253" font-size="11">«interface»</text>
<text x="230" y="269" font-size="12" font-family="ui-monospace,monospace">ILevelQuelle</text>
<text x="550" y="253" font-size="11">«interface»</text>
<text x="550" y="269" font-size="12" font-family="ui-monospace,monospace">ISpielstandSpeicher</text>
<text x="125" y="385" font-size="12" font-family="ui-monospace,monospace">EingebauteLevelQuelle</text>
<text x="335" y="385" font-size="12" font-family="ui-monospace,monospace">TextdateiLevelQuelle</text>
<text x="550" y="385" font-size="12" font-family="ui-monospace,monospace">JsonSpielstandSpeicher</text>
<text x="350" y="142" font-size="12">beide nutzen dieselben Dienste</text>
</g>
<text x="350" y="302" font-size="12" fill="#d33682" text-anchor="middle">Abhängigkeit zeigt nach oben</text>
<text x="8" y="434" font-size="12" fill="currentColor">Pfeil = »nutzt« · gestrichelter Pfeil nach oben = »implementiert« · «interface» = Schnittstelle</text>
</svg>

Die **GUI-Schicht** enthält alles, was der Benutzer sieht, und nichts, was er nicht sieht. Dass hier zwei Kästen nebeneinander stehen, ist der eigentliche Punkt dieser Vorlesung: Die Konsolenversion und die Browserversion sind zwei austauschbare Bewohner derselben Schicht. Keine von beiden ist privilegiert, und keine von beiden weiß von der anderen.

Die **Fachkonzeptschicht** ist der funktionale Kern: die Klassen des Anwendungsgebiets (`Spielobjekt`, `Wand`, `Tuer`, `Wache`, `Spielfeld`) und die Regeln, die für sie gelten – wann ein Zug erlaubt ist, wann eine Wache trifft, wann das Spiel gewonnen ist. Sie weiß nicht, ob sie von einem Browser, einer Konsole oder einem Test aufgerufen wird. Die **Datenhaltungsschicht** beschafft die Level und weiß nichts über Regeln oder Oberflächen.

Ein Detail im Bild verdient besondere Aufmerksamkeit: Der hervorgehobene Pfeil ganz unten zeigt **nach oben**. Alle anderen Abhängigkeiten laufen von oben nach unten – die beiden Oberflächen nutzen den Kern –, aber zwischen Kern und Daten ist es umgekehrt. `ILevelQuelle` und `ISpielstandSpeicher` stehen im Bild am unteren Rand der **Kern**-Schicht, nicht in der Datenschicht: Der Kern schreibt vor, *was* er von einer Levelquelle und von einem Spielstandspeicher braucht, und `EingebauteLevelQuelle`, `TextdateiLevelQuelle` und `JsonSpielstandSpeicher` erfüllen diesen Vertrag von unten. Deshalb geht vom Kern kein einziger Pfeil nach unten – er kennt `Adventure.Daten` überhaupt nicht. Dieses Prinzip heißt **Dependency Inversion**; ohne es müsste der Kern die Datenschicht kennen und wäre nicht mehr unabhängig von ihr.
{: .notice--primary}

## Die Konsolen-Probe

Wo ist die Grenze zwischen GUI und Fachkonzept? Für unser Spiel gibt es dafür eine besonders handfeste Probe, weil wir die Konsolenversion tatsächlich besitzen: **Müsste ich diese Zeile in `Adventure.Konsole` noch einmal schreiben?** Wenn ja, gehört sie nicht in die Komponente, sondern in den Kern.

| Zeile | Gehört nach … | Warum |
| :--- | :--- | :--- |
| `feld.SpielerZieht(Richtung.Oben)` | Kern (Aufruf in der GUI) | Die Runde selbst ist eine Spielregel |
| `e.Key switch { "ArrowUp" => ... }` | GUI | Nur der Browser kennt `"ArrowUp"` |
| „Eine Wache trifft, wenn sie auf den Spieler zieht“ | Kern | Gilt in jeder Oberfläche |
| `Wand => "🧱"` | GUI | Die Konsole zeichnet `#` |
| `new string('♥', Lebenspunkte)` | GUI | Die Konsole schreibt `3/3` |
| „Der Trank heilt einen Lebenspunkt“ | Kern | Gilt in jeder Oberfläche |

Die Probe funktioniert auch ohne zweite Oberfläche – dann stellt man sich die Konsolenversion eben vor. Ihr Vorteil ist, dass sie eine *technische* Frage in eine *fachliche* verwandelt: nicht „fühlt sich das nach GUI an?“, sondern „wüsste ein anderes Programm das auch?“.

## Zwei und mehr Schichten

Die **Zwei-Schichten-Architektur** fasst Oberfläche und Fachkonzept zu einer Anwendungsschicht zusammen und setzt nur die Datenhaltung darunter. Man findet sie in einfachen und in älteren Programmen – sie ist genau das, was entsteht, wenn die Spiellogik in den Handlern landet. Sie ist zu vermeiden, denn die Fachlogik ist dann nicht ohne Oberfläche testbar und nicht wiederverwendbar; unsere zweite Oberfläche gäbe es in dieser Welt nicht. Bei der **Mehr-Schichten-Architektur** kommen dagegen zusätzliche Zugriffsschichten hinzu, die die Komplexität des Fachkonzepts oder der Datenhaltung vor dem Aufrufer verbergen, etwa eine Schicht, die aus vielen kleinen Fachoperationen wenige große Anwendungsfälle zusammensetzt. Für das Adventure reichen drei Schichten; wie wir sie mit Blazor konkret umsetzen, zeigt das Modul [Schichten mit Blazor umsetzen](/modules/schichten_mit_blazor/schichten_mit_blazor.md).

Übung: Ordne die folgenden Aufgaben eines Notenverwaltungsprogramms den drei Schichten zu: Durchschnitt berechnen, Studierendenliste als CSV lesen, prüfen, dass eine Note zwischen 1,0 und 5,0 liegt, den Fehlertext rot anzeigen, die Liste nach Namen sortieren, den Dateipfad aus einem Dialog holen. Wende bei jeder Aufgabe die Konsolen-Probe an. Bei welchen Aufgaben ist die Zuordnung nicht eindeutig, und welche Frage entscheidet dann?
{: .notice--info}

## Weitere Quellen

- [Allgemeine Webanwendungsarchitekturen (Schichten) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [Architekturprinzipien – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/architecture/modern-web-apps-azure/architectural-principles)
- [Schichtenarchitektur – Wikipedia](https://de.wikipedia.org/wiki/Schichtenarchitektur)
- [Game Programming Patterns – freies Online-Buch](https://gameprogrammingpatterns.com/) – das Einleitungskapitel „Architecture, Performance, and Games“ beschreibt genau unseren Zielkonflikt zwischen sauberer Trennung und Geschwindigkeit.
- [Liskovsches Substitutionsprinzip – Wikipedia](https://de.wikipedia.org/wiki/Liskovsches_Substitutionsprinzip) – die Regel dahinter, dass eine Schicht ihre Nachbarin gegen jede Implementierung derselben Schnittstelle tauschen darf.
