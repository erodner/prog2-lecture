---
title: ".NET 10 und Visual Studio einrichten"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Was SDK, CLR und BCL sind, kennst du aus dem Modul [Die .NET-Plattform](https://www.erodner.de/prog-lecture/modules/dotnet/dotnet/) in Programmierung 1. Wie in Programmierung 1 arbeiten wir mit **Visual Studio** unter Windows und mit **.NET 10**. Neu ist ab Vorlesung 04 **Blazor**, dafür braucht Visual Studio eine zusätzliche Workload.

## .NET 10

**.NET 10** ist eine LTS-Version (*Long Term Support*) mit drei Jahren Updates. Mit Visual Studio kommt das passende SDK über den Installer automatisch mit. Ohne Visual Studio installierst du das **SDK** (nicht nur die Runtime) von [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download). Dort gibt es auch Installer für macOS und Linux.

Mehrere SDKs dürfen nebeneinander installiert sein. `dotnet --list-sdks` zeigt alle an:

```bash
dotnet --list-sdks
# 8.0.404 [/usr/local/share/dotnet/sdk]
# 10.0.100 [/usr/local/share/dotnet/sdk]
```

Welche .NET-Version ein Projekt nutzt, legt seine `.csproj` fest (siehe [Projekte mit der dotnet-CLI](/modules/dotnet_cli_projekte/dotnet_cli_projekte.md)). Ohne weitere Vorgabe nimmt `dotnet` das neueste installierte SDK.

Wird `dotnet` nach der Installation nicht gefunden, war das Terminal fast immer schon vorher offen und kennt den neuen `PATH` noch nicht. Terminal neu öffnen und noch einmal probieren.
{: .notice--warning}

## Visual Studio vorbereiten

Öffne den [Visual Studio Installer](https://visualstudio.microsoft.com/de/vs/community/) und wähle zusätzlich zur „.NET-Desktopentwicklung“ aus Programmierung 1 die Workload **„ASP.NET und Webentwicklung“** aus. Sie bringt die Blazor-Vorlagen und die Unterstützung für `.razor`-Dateien mit.

Wer mit macOS oder Linux arbeitet, kann [VS Code mit der Erweiterung C# Dev Kit](https://code.visualstudio.com/docs/csharp/get-started) nutzen. Deshalb siehst du in der Vorlesung manchmal VS Code auf einem Mac. [JetBrains Rider](https://www.jetbrains.com/rider/) ist eine weitere Alternative und für Studierende kostenlos. Alle drei nutzen dasselbe SDK und dieselben Projektdateien. Im Zweifel nimm Visual Studio, denn in der Übung helfen wir dir damit am schnellsten weiter.

Die `.csproj` ist die einzige Wahrheit, nicht die IDE. Läuft ein Projekt bei dir, aber nicht bei deiner Teampartnerin, prüft zuerst mit `dotnet build` auf der Kommandozeile, ob es überhaupt an der IDE liegt.
{: .notice--primary}

## Vorbereitung auf Blazor

Ab der [Vorlesung 04](/lectures/04/04.md) bauen wir mit **Blazor** Oberflächen, die im Browser laufen. Blazor gehört zu ASP.NET Core und ist schon im SDK enthalten. Ob die Vorlage da ist, zeigt:

```bash
dotnet new list blazor
```

Erscheint `Blazor Web App` mit dem Kurznamen `blazor`, ist alles bereit. Später praktisch: `dotnet watch` startet die App und lädt sie bei jeder Änderung neu im Browser (*Hot Reload*).


## Weitere Quellen

- [.NET installieren unter Windows, macOS und Linux – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/install/)
- [C# in Visual Studio Code – Visual Studio Code Docs](https://code.visualstudio.com/docs/languages/csharp)
- [SharpLab – C# im Browser ausprobieren](https://sharplab.io/) – nützlich, solange die eigene Installation noch klemmt.
