---
title: "Projekte mit der dotnet-CLI"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Der Knopf „Neues Projekt“ in der IDE ruft im Hintergrund genau das auf, was wir in diesem Modul selbst tippen: das Kommandozeilenwerkzeug `dotnet`. Wer die CLI (*Command Line Interface*) kennt, versteht, was die IDE erzeugt, kann Projekte auch ohne IDE bauen – etwa auf einem Server oder in einer automatischen Test-Pipeline – und kann seinen Kolleginnen präzise sagen, welcher Befehl fehlgeschlagen ist. Außerdem brauchen wir die CLI ab der [Vorlesung 04](/lectures/04/04.md), wenn eine Anwendung aus mehreren Projekten besteht, die voneinander abhängen. Wir gehen den Weg vom einzelnen Konsolenprojekt bis zur Solution mit Klassenbibliothek.

## Ein Konsolenprojekt anlegen

Ein neues Projekt entsteht mit `dotnet new`, gefolgt vom Namen einer Vorlage. Die Option `-o` legt den Ordner fest, der zugleich als Projektname dient:

```bash
dotnet new console -o HalloWelt
cd HalloWelt
```

Im Ordner liegen jetzt zwei Dateien: `Program.cs` mit einer einzigen Zeile `Console.WriteLine("Hello, World!");` und die Projektdatei `HalloWelt.csproj`; dazu kommt ein Ordner `obj/`, in dem das SDK Zwischenergebnisse ablegt. Die kurze `Program.cs` ohne `Main` und Klasse kennst du aus Programmierung 1 als *Top-Level-Statements* – der Compiler erzeugt die umgebende Klasse selbst.

## Anatomie einer .csproj

Die Projektdatei ist eine kleine XML-Datei und beschreibt alles, was der Compiler wissen muss. Öffne sie in einem Editor:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

Jede Zeile hat eine klare Aufgabe:

- **`Sdk="Microsoft.NET.Sdk"`** – welches Regelwerk gilt. Alle `.cs`-Dateien im Ordner werden automatisch mitkompiliert; man muss sie nirgends eintragen.
- **`OutputType`** – `Exe` für ein startbares Programm, `Library` für eine Klassenbibliothek (`.dll`), die von anderen Projekten genutzt wird.
- **`TargetFramework`** – gegen welche .NET-Version gebaut wird. `net10.0` bedeutet: Das SDK 10 muss installiert sein, siehe [.NET SDK und IDE einrichten](/modules/werkzeuge_sdk_ide/werkzeuge_sdk_ide.md).
- **`ImplicitUsings`** – die häufigsten Namespaces wie `System` und `System.Collections.Generic` sind automatisch importiert. Deshalb fehlt in `Program.cs` das `using System;`.
- **`Nullable`** – der Compiler warnt, wenn eine Referenz `null` sein könnte und trotzdem ohne Prüfung benutzt wird. Wir lassen das in allen Projekten eingeschaltet.

Die kleinen Zusatzbeispiele der Vorlesung liegen im Repository dieser Webseite unter `examples/`; das durchgehende Beispiel, das Spiel **Adventure**, hat mit [prog2-adventure](https://github.com/erodner/prog2-adventure) ein eigenes Repository. In beiden setzt eine gemeinsame Datei `Directory.Build.props` Zielframework, `Nullable` und `ImplicitUsings` für alle Projekte auf einmal, sodass die einzelnen `.csproj`-Dateien fast leer sind. Jedes Beispiel lässt sich mit `dotnet build` bauen.
{: .notice--primary}

## Bauen und starten

Zwei Befehle brauchst du ständig. `dotnet build` übersetzt das Projekt und legt das Ergebnis unter `bin/Debug/net10.0/` ab; `dotnet run` baut ebenfalls, falls nötig, und startet das Programm anschließend:

```bash
dotnet build
# HalloWelt -> /Users/anna/HalloWelt/bin/Debug/net10.0/HalloWelt.dll
# Build succeeded.

dotnet run
# Hello, World!
```

Compilerfehler erscheinen bei `dotnet build` mit Dateiname, Zeile und Spalte – dieselben Meldungen, die die IDE in der Fehlerliste anzeigt. Die Ordner `bin/` und `obj/` sind reine Build-Ausgaben; sie gehören nicht in ein Git-Repository, worauf wir in der [Vorlesung 03](/lectures/03/03.md) zurückkommen.

## Solutions mit mehreren Projekten

Sobald eine Anwendung wächst, teilt man sie in mehrere Projekte: eine Klassenbibliothek mit der Fachlogik, ein Konsolen- oder GUI-Projekt für die Bedienung, später ein Testprojekt. Eine **Solution** fasst diese Projekte zusammen, damit ein einziger `dotnet build` alle baut. Unser Semesterprojekt, das Spiel Adventure, ist genau so aufgebaut: `Adventure.Kern` enthält die Spielregeln, `Adventure.Konsole` spielt es im Terminal, `Adventure.Daten` lädt Level und speichert Spielstände, `Adventure.Web` bringt es später in den Browser, `Adventure.Tests` prüft alles nach. Wir legen den Anfang davon selbst an – die Bibliothek und das Konsolenprogramm:

```bash
mkdir Adventure
cd Adventure
dotnet new sln -n Adventure
dotnet new classlib -o Adventure.Kern
dotnet new console -o Adventure.Konsole
dotnet sln add Adventure.Kern
dotnet sln add Adventure.Konsole
```

`dotnet new sln` erzeugt mit dem .NET 10 SDK eine Datei `Adventure.slnx` – das neue, schlanke XML-Format für Solutions; ältere SDKs erzeugen eine `.sln`, die genauso funktioniert. Die beiden `dotnet sln add`-Zeilen tragen die Projekte darin ein. Bis hierhin wissen die Projekte allerdings nichts voneinander: Das Konsolenprogramm könnte keine Klasse aus dem Kern verwenden. Dafür braucht es eine **Projektreferenz**:

```bash
dotnet add Adventure.Konsole reference Adventure.Kern
```

In der `Adventure.Konsole.csproj` erscheint daraufhin ein neuer Eintrag:

```xml
<ItemGroup>
  <ProjectReference Include="..\Adventure.Kern\Adventure.Kern.csproj" />
</ItemGroup>
```

Die Richtung ist wichtig: Die Konsole kennt den Kern, aber nicht umgekehrt. Im Lauf des Semesters kommen auf genau diesem Weg drei weitere Projekte dazu, und die `Adventure.slnx` listet am Ende alle fünf:

```xml
<Solution>
  <Project Path="Adventure.Daten/Adventure.Daten.csproj" />
  <Project Path="Adventure.Kern/Adventure.Kern.csproj" />
  <Project Path="Adventure.Konsole/Adventure.Konsole.csproj" />
  <Project Path="Adventure.Tests/Adventure.Tests.csproj" />
  <Project Path="Adventure.Web/Adventure.Web.csproj" />
</Solution>
```

Auch dort zeigen alle Projektreferenzen nach innen: `Adventure.Konsole`, `Adventure.Web` und `Adventure.Tests` verweisen auf `Adventure.Kern` und `Adventure.Daten`, `Adventure.Daten` verweist auf `Adventure.Kern` – und `Adventure.Kern` selbst verweist auf nichts. Fachlogik in einer Bibliothek, Oberfläche in einem eigenen Projekt, das darauf zeigt: Dieses Muster ist die Grundlage der Schichten-Architektur, die wir in der [Vorlesung 04](/lectures/04/04.md) systematisch anschauen. Das vollständige Projekt findest du im Repository [prog2-adventure](https://github.com/erodner/prog2-adventure).

Referenziert Projekt A das Projekt B und B wiederum A, meldet `dotnet build` einen Zirkelbezug und bricht ab. Das ist kein Werkzeugfehler, sondern ein Designproblem: Zwei Projekte, die sich gegenseitig brauchen, gehören entweder zusammen oder eines von beiden hängt an der falschen Stelle.
{: .notice--warning}

## Welche Vorlagen gibt es?

Neben `console`, `classlib` und `sln` bringt das SDK viele weitere Vorlagen mit – auch `blazor` für Web-Oberflächen ist von Haus aus dabei. Die vollständige Liste zeigt:

```bash
dotnet new list
# Template Name       Short Name    Language    Tags
# --------------------------------------------------------------------
# Blazor Web App      blazor        [C#]        Web/Blazor/WebAssembly
# Class Library       classlib      [C#],F#,VB  Common/Library
# Console App         console       [C#],F#,VB  Common/Console
# NUnit Test Project  nunit         [C#],F#,VB  Test/NUnit/Desktop/Web
# Solution File       sln,solution              Solution
```

Die Vorlage `nunit` werden wir in der [Vorlesung 12](/lectures/12/12.md) für Unit-Tests einsetzen – daraus entsteht `Adventure.Tests`; aus `blazor` wird in der Vorlesung 04 das Projekt `Adventure.Web`. Alles, was du hier per CLI anlegst, kannst du anschließend ganz normal in Rider, Visual Studio oder VS Code öffnen.

Übung: Lege die Solution `Adventure` mit `Adventure.Kern` und `Adventure.Konsole` wie oben an. Schreibe im Kern eine Klasse `Position` mit den Properties `X` und `Y` und einer Methode `Entfernung(Position andere)`, die den Abstand in Feldern liefert. Gib im Konsolenprojekt die Entfernung zwischen zwei Positionen aus und baue alles mit einem einzigen `dotnet build` im Solution-Ordner. Was passiert, wenn du die Projektreferenz aus der `.csproj` wieder löschst? Und was meldet der Compiler, wenn du umgekehrt versuchst, aus `Adventure.Kern` heraus eine Klasse aus `Adventure.Konsole` zu benutzen?
{: .notice--info}

## Weitere Quellen

- [dotnet new – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-new)
- [Übersicht über die .NET-CLI – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/)
- [Projektdateien und MSBuild-Eigenschaften – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/project-sdk/overview)
- [dotnet sln – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-sln)
- [GitHub Actions – Dokumentation](https://docs.github.com/de/actions) – zeigt, wozu die CLI wirklich gut ist: Genau diese `dotnet build`- und `dotnet test`-Befehle laufen dort bei jedem Push automatisch auf einem Server ohne IDE.
