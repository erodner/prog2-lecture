---
title: "NuGet-Grundlagen"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Niemand baut ein Haus, indem er zuerst Ziegel brennt. Genauso schreibt niemand ernsthaft seinen eigenen JSON-Parser, seine eigene PDF-Bibliothek oder sein eigenes GUI-Framework, bevor er mit der eigentlichen Anwendung anfängt. Selbermachen kostet Zeit, man baut Fehler ein, die andere längst gefunden haben, und meistens bleibt das Nebenprodukt halbfertig liegen, weil es nie das eigentliche Ziel war. Die Alternative ist, Bibliotheken zu verwenden, die andere Entwicklerinnen und Entwickler veröffentlicht haben – und dafür braucht man eine Stelle, an der man sie findet, und ein Werkzeug, das sie ins Projekt holt. Beides zusammen ist **NuGet**: der Paketmanager von .NET und die dazugehörige Sammlung [nuget.org](https://www.nuget.org) mit weit über 400.000 Paketen. Wer schon einmal `pip` in Python oder `npm` in JavaScript benutzt hat, kennt das Prinzip.

## Was ist ein Paket?

Ein NuGet-Paket ist eine ZIP-Datei mit der Endung `.nupkg`. Darin liegen die kompilierten Bibliotheken (`.dll`) – oft mehrfach, für verschiedene Zielframeworks wie `netstandard2.0` oder `net8.0` – und eine Beschreibungsdatei `.nuspec` mit den **Metadaten**: Name, Version, Autor, Lizenz, Beschreibung und die Liste der Pakete, von denen dieses Paket selbst abhängt. Ein Paket ist also mehr als eine `.dll`, die man irgendwo herunterlädt: Es ist eine **versionierte Bibliothek mit Beipackzettel**.

Genau dieser Beipackzettel macht den Unterschied zum manuellen Kopieren einer `.dll`. NuGet weiß, welche Version ihr benutzt, kann prüfen, ob es eine neuere gibt, und lädt die Abhängigkeiten gleich mit. Wenn du das Testprojekt des Geometrieeditors gebaut hast, hast du genau das erlebt: fünf Zeilen in der `.csproj`, und beim ersten Bauen kamen über zwanzig Pakete von selbst. Blazor dagegen ist nie als Paket gekommen – es gehört zu ASP.NET Core, das mit dem SDK installiert wird.

## Woher kommen die Pakete?

Die zentrale Quelle ist nuget.org, ein öffentlicher **Feed**, den das SDK standardmäßig kennt. Jede und jeder kann dort Pakete veröffentlichen – das ist Stärke und Schwäche zugleich, dazu gleich mehr. Firmen betreiben daneben oft **private Feeds** (etwa in GitLab, GitHub Packages oder Azure Artifacts), auf denen interne Bibliotheken liegen, die nicht öffentlich werden sollen. Welche Feeds ein Projekt benutzt, steht in einer Datei `nuget.config`; solange sie fehlt, gilt nur nuget.org. Ein Paket muss also nicht öffentlich sein, um mit NuGet verteilt zu werden.

## Eine Paketseite lesen

Bevor ein Paket ins Projekt kommt, lohnt sich ein Blick auf seine Seite auf nuget.org. Nimm als Beispiel die Seite von `NLog`, dem Logging-Paket, das wir im Modul [Beispiel: Logging mit NLog](/modules/nlog_beispiel/nlog_beispiel.md) einbauen. Fünf Angaben verraten fast alles:

- **Downloads** – die Gesamtzahl und die der letzten Wochen. Hunderte Millionen bedeuten: Sehr viele Projekte verlassen sich darauf, Fehler fallen schnell auf.
- **Versionen** – die Liste aller Veröffentlichungen mit Datum. Regelmäßige Releases zeigen, dass das Paket gepflegt wird; liegt die letzte Version Jahre zurück, ist Vorsicht angebracht.
- **Lizenz** – unter welchen Bedingungen du das Paket benutzen darfst (siehe unten).
- **Abhängigkeiten** – welche weiteren Pakete mitkommen. Ein Paket ohne Abhängigkeiten ist ein gutes Zeichen; eine lange Liste bedeutet mehr Angriffsfläche und mehr Versionskonflikte.
- **Quellcode** – ein Link auf GitHub oder GitLab. Dort siehst du offene Issues, letzte Commits und ob überhaupt jemand auf Fragen antwortet.

Das Kästchen „Reserved prefix“ neben manchen Paketnamen bedeutet, dass nur der eingetragene Besitzer Pakete mit diesem Präfix veröffentlichen darf – `Microsoft.*` oder `Newtonsoft.*` kann also niemand fälschen. Das ist ein einfacher, aber wirksamer Schutz gegen Nachahmer.
{: .notice--primary}

## Semantische Versionierung

Fast alle Pakete folgen der **semantischen Versionierung** (SemVer): Eine Version wie `6.2.0` besteht aus `MAJOR.MINOR.PATCH`, und jede Stelle hat eine Bedeutung:

- **PATCH** (`6.2.0` → `6.2.1`): Fehlerbehebung, nichts an der Schnittstelle ändert sich. Gefahrlos übernehmen.
- **MINOR** (`6.2.0` → `6.3.0`): neue Funktionen, aber alles Alte funktioniert weiter. Meist gefahrlos.
- **MAJOR** (`6.2.0` → `7.0.0`): **Breaking Changes** – Methoden wurden umbenannt oder entfernt, dein Code kompiliert vielleicht nicht mehr. Erst die Release Notes lesen.

Ein Anhang wie `7.0.0-beta.2` markiert eine **Vorabversion**, die vor der stabilen `7.0.0` liegt und nicht für produktive Projekte gedacht ist. Die Regeln sind ein Versprechen der Autoren, kein technischer Zwang – aber bei etablierten Paketen kann man sich darauf verlassen. Wie man zwei Versionsnummern nach diesen Regeln korrekt vergleicht, ist übrigens eine schöne Aufgabe für `IComparable<T>`, das wir im Modul [IComparable<T>](/modules/icomparable_sortieren/icomparable_sortieren.md) kennengelernt haben – siehe die Aufgaben zu dieser Vorlesung.

## Transitive Abhängigkeiten

Ein Paket hängt oft von weiteren Paketen ab, und diese wieder von anderen. Alles, was nicht direkt in eurer `.csproj` steht, aber trotzdem mitgeladen wird, heißt **transitive Abhängigkeit**. Beim Testprojekt des Geometrieeditors sieht das so aus:

```bash
dotnet list package --include-transitive
# Top-level Package                Requested   Resolved
# > coverlet.collector             6.0.4       6.0.4
# > Microsoft.NET.Test.Sdk         17.14.0     17.14.0
# > NUnit                          4.3.2       4.3.2
# > NUnit.Analyzers                4.7.0       4.7.0
# > NUnit3TestAdapter              5.0.0       5.0.0
#
# Transitive Package               Resolved
# > Microsoft.CodeCoverage         17.14.0
# > Microsoft.TestPlatform.TestHost 17.14.0
# > Newtonsoft.Json                13.0.1
# > System.Reflection.Metadata     1.6.0
# ...                              (insgesamt über 25 Pakete)
```

Drei Zeilen in der `.csproj` ziehen also einen ganzen Baum nach sich – darunter `SkiaSharp`, das die eigentliche Zeichenarbeit erledigt, und dessen native Bibliotheken für jedes Betriebssystem. Genau die Art von nativer Bibliothek, die wir in der [Vorlesung 10](/lectures/10/10.md) noch von Hand eingebunden haben, kommt hier fertig verpackt mit. NuGet löst dabei auch Konflikte: Verlangen zwei Pakete unterschiedliche Versionen derselben Abhängigkeit, wählt es die kleinste Version, die beide Anforderungen erfüllt.

Transitive Abhängigkeiten sind auch der Grund, warum eine Sicherheitslücke in einem winzigen Hilfspaket Tausende Anwendungen treffen kann, deren Entwickler das Paket nie bewusst ausgewählt haben. `dotnet list package --vulnerable` prüft eure Abhängigkeiten gegen eine Datenbank bekannter Schwachstellen – ein Befehl, der in jede Build-Pipeline gehört.
{: .notice--warning}

## Vertrauen und Risiken

Jedes Paket, das ihr einbindet, führt Code mit denselben Rechten aus wie euer eigenes Programm. Drei Gefahren solltet ihr kennen:

- **Typosquatting:** Jemand veröffentlicht `Newtonsoft.Jsom` oder `NLog.Core` in der Hoffnung, dass sich jemand vertippt oder den Namen für offiziell hält. Solche Pakete kopieren häufig die Beschreibung des Originals. Prüft Autor, Downloadzahlen und das Reserved-prefix-Symbol, bevor ihr auf „Installieren“ klickt.
- **Unbetreute Pakete:** Ein Paket, dessen letztes Release vier Jahre alt ist und dessen Issues niemand beantwortet, bekommt auch keine Sicherheitsupdates mehr. Für ein kleines Werkzeug mag das egal sein, für eine Bibliothek, die Netzwerkdaten verarbeitet, nicht.
- **Lizenzen:** **MIT** und **BSD** erlauben fast alles, auch die Verwendung in kommerzieller, geschlossener Software – nur der Lizenztext muss mitgeliefert werden. **Apache 2.0** ist ähnlich großzügig und regelt zusätzlich Patentfragen. **GPL** verlangt dagegen, dass Software, die eine GPL-Bibliothek enthält, selbst unter der GPL veröffentlicht wird – für ein Open-Source-Projekt unproblematisch, für ein Produkt, dessen Quellcode geheim bleiben soll, ein Ausschlusskriterium. Ein Paket ganz ohne Lizenzangabe darf man streng genommen gar nicht verwenden.

Im Zweifel: Nimm das Paket mit den meisten Downloads, einem Release in den letzten zwölf Monaten, einer MIT-, BSD- oder Apache-Lizenz und einem verlinkten Repository, in dem sich jemand um Issues kümmert. Und wenn eine Aufgabe in zehn Zeilen eigenem Code erledigt ist, brauchst du dafür kein Paket – jede Abhängigkeit ist auch eine Verpflichtung, sie aktuell zu halten.
{: .notice--primary}

Übung: Öffne auf nuget.org die Seiten von `NLog`, `Serilog` und `log4net` – drei Logging-Bibliotheken. Notiere für jedes Paket Downloads der letzten sechs Wochen, Datum des letzten Releases, Lizenz und Zahl der Abhängigkeiten. Welches würdest du für ein neues Projekt wählen, und mit welcher Begründung? Suche anschließend nach „NLog“ und schaue dir die Treffer auf Seite 2 und 3 an: Welche davon sind offizielle Erweiterungen, welche sehen nach Nachahmern aus?
{: .notice--info}

## Weitere Quellen

- [Was ist NuGet? – Microsoft Learn](https://learn.microsoft.com/de-de/nuget/what-is-nuget)
- [Paketversionsverwaltung – Microsoft Learn](https://learn.microsoft.com/de-de/nuget/concepts/package-versioning)
- [Abhängigkeitsauflösung – Microsoft Learn](https://learn.microsoft.com/de-de/nuget/concepts/dependency-resolution)
- [Bewährte Methoden für die sichere Nutzung von Paketen – Microsoft Learn](https://learn.microsoft.com/de-de/nuget/concepts/security-best-practices)
