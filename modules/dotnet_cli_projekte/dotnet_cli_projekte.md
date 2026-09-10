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

Der Knopf „Neues Projekt“ in der IDE ruft im Hintergrund genau das auf, was wir in diesem Modul selbst tippen: das Kommandozeilenwerkzeug `dotnet`. Wer die CLI (*Command Line Interface*) kennt, versteht, was die IDE erzeugt, kann Projekte auch ohne IDE bauen – etwa auf einem Server oder in einer automatischen Test-Pipeline – und kann seinen Kolleginnen präzise sagen, welcher Befehl fehlgeschlagen ist. Außerdem brauchen wir die CLI ab der [Vorlesung 03](/lectures/03/03.md), wenn eine Anwendung aus mehreren Projekten besteht, die voneinander abhängen. Wir gehen den Weg vom einzelnen Konsolenprojekt bis zur Solution mit Klassenbibliothek.

## Ein Konsolenprojekt anlegen

Ein neues Projekt entsteht mit `dotnet new`, gefolgt vom Namen einer Vorlage. Die Option `-o` legt den Ordner fest, der zugleich als Projektname dient:

```bash
dotnet new console -o HalloWelt
cd HalloWelt
```

Im Ordner liegen jetzt zwei Dateien: `Program.cs` mit einer einzigen Zeile `Console.WriteLine("Hello, World!");` und die Projektdatei `HalloWelt.csproj`. Die kurze `Program.cs` ohne `Main` und Klasse kennst du aus Programmierung 1 als *Top-Level-Statements* – der Compiler erzeugt die umgebende Klasse selbst.

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

Alle Beispielprojekte der Vorlesung liegen im Repository unter `examples/`. Dort setzt eine gemeinsame Datei `Directory.Build.props` Zielframework, `Nullable` und `ImplicitUsings` für alle Projekte auf einmal, sodass die einzelnen `.csproj`-Dateien fast leer sind. Jedes Beispiel lässt sich mit `dotnet build` bauen.
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

Compilerfehler erscheinen bei `dotnet build` mit Dateiname, Zeile und Spalte – dieselben Meldungen, die die IDE in der Fehlerliste anzeigt. Die Ordner `bin/` und `obj/` sind reine Build-Ausgaben; sie gehören nicht in ein Git-Repository, worauf wir in der [Vorlesung 09](/lectures/09/09.md) zurückkommen.

## Solutions mit mehreren Projekten

Sobald eine Anwendung wächst, teilt man sie in mehrere Projekte: eine Klassenbibliothek mit der Fachlogik, ein Konsolen- oder GUI-Projekt für die Bedienung, später ein Testprojekt. Eine **Solution** fasst diese Projekte zusammen, damit ein einziger `dotnet build` alle baut. Wir legen eine Solution mit einer Bibliothek und einem Konsolenprogramm an:

```bash
mkdir Notenverwaltung
cd Notenverwaltung
dotnet new sln
dotnet new classlib -o Notenverwaltung.Fachkonzept
dotnet new console -o Notenverwaltung.Konsole
dotnet sln add Notenverwaltung.Fachkonzept
dotnet sln add Notenverwaltung.Konsole
```

`dotnet new sln` erzeugt mit dem .NET 10 SDK eine Datei `Notenverwaltung.slnx` – das neue, schlanke XML-Format für Solutions; ältere SDKs erzeugen eine `.sln`, die genauso funktioniert. Die beiden `dotnet sln add`-Zeilen tragen die Projekte darin ein. Bis hierhin wissen die Projekte allerdings nichts voneinander: Das Konsolenprogramm könnte keine Klasse aus dem Fachkonzept verwenden. Dafür braucht es eine **Projektreferenz**:

```bash
dotnet add Notenverwaltung.Konsole reference Notenverwaltung.Fachkonzept
```

In der `Notenverwaltung.Konsole.csproj` erscheint daraufhin ein neuer Eintrag:

```xml
<ItemGroup>
  <ProjectReference Include="..\Notenverwaltung.Fachkonzept\Notenverwaltung.Fachkonzept.csproj" />
</ItemGroup>
```

Die Richtung ist wichtig: Die Konsole kennt das Fachkonzept, aber nicht umgekehrt. Genau dieses Muster – Fachlogik in einer Bibliothek, Oberfläche in einem eigenen Projekt, das darauf verweist – ist die Grundlage der Schichten-Architektur, mit der wir in der [Vorlesung 03](/lectures/03/03.md) den Geometrieeditor bauen. Dort findest du im Repository unter `examples/03_blazor/Geometrieeditor/` eine Solution mit vier Projekten, die genau so entstanden ist.

Referenziert Projekt A das Projekt B und B wiederum A, meldet `dotnet build` einen Zirkelbezug und bricht ab. Das ist kein Werkzeugfehler, sondern ein Designproblem: Zwei Projekte, die sich gegenseitig brauchen, gehören entweder zusammen oder eines von beiden hängt an der falschen Stelle.
{: .notice--warning}

## Welche Vorlagen gibt es?

Neben `console`, `classlib` und `sln` bringt das SDK viele weitere Vorlagen mit – auch `blazor` für Web-Oberflächen ist von Haus aus dabei. Die vollständige Liste zeigt:

```bash
dotnet new list
# Vorlagenname          Kurzname       Sprache     Tags
# ---------------------------------------------------------------
# Blazor Web App        blazor         [C#]        Web/Blazor
# Console App           console        [C#],F#,VB  Common/Console
# Class Library         classlib       [C#],F#,VB  Common/Library
# NUnit 3 Test Project  nunit          [C#],F#,VB  Test/NUnit
# Solution File         sln            ...         Solution
```

Die Vorlage `nunit` werden wir in der [Vorlesung 12](/lectures/12/12.md) für Unit-Tests einsetzen; `blazor` in der Vorlesung 03. Alles, was du hier per CLI anlegst, kannst du anschließend ganz normal in Rider, Visual Studio oder VS Code öffnen.

Übung: Lege die Solution `Notenverwaltung` wie oben an. Schreibe im Fachkonzept eine Klasse `Student` mit `Name` und einer `List<double>` für Noten sowie einer Methode `Durchschnitt()`. Erzeuge im Konsolenprojekt zwei Studierende, gib ihre Durchschnitte aus und baue alles mit einem einzigen `dotnet build` im Solution-Ordner. Was passiert, wenn du die Projektreferenz aus der `.csproj` wieder löschst?
{: .notice--info}

## Weitere Quellen

- [dotnet new – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-new)
- [Übersicht über die .NET-CLI – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/)
- [Projektdateien und MSBuild-Eigenschaften – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/project-sdk/overview)
- [dotnet sln – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-sln)
