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

`dotnet new console`, `dotnet build` und `dotnet run` kennst du aus dem Modul [Die .NET-Plattform](https://www.erodner.de/prog-lecture/modules/dotnet/dotnet/) in Programmierung 1. Dort bestand ein Programm immer aus genau einem Projekt. In diesem Semester wird das Spiel **Adventure** aus mehreren Projekten bestehen, die voneinander abhängen. Dafür schauen wir uns an, was in einer Projektdatei steht und wie man mehrere Projekte zu einer **Solution** verbindet.

## Anatomie einer .csproj

Die Projektdatei, die `dotnet new console` anlegt, ist eine kleine XML-Datei:

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

- **`Sdk="Microsoft.NET.Sdk"`**: Alle `.cs`-Dateien im Ordner werden automatisch mitkompiliert.
- **`OutputType`**: `Exe` für ein startbares Programm, `Library` für eine Klassenbibliothek (`.dll`).
- **`TargetFramework`**: `net10.0` braucht das SDK 10, siehe [.NET 10 und Visual Studio einrichten](/modules/werkzeuge_sdk_ide/werkzeuge_sdk_ide.md).
- **`ImplicitUsings`**: Häufige Namespaces wie `System` sind automatisch importiert.
- **`Nullable`**: Der Compiler warnt, wenn eine Referenz `null` sein könnte und ungeprüft benutzt wird. Wir lassen das immer eingeschaltet.

In [prog2-adventure](https://github.com/erodner/prog2-adventure) und in `examples/` setzt eine gemeinsame `Directory.Build.props` diese Einstellungen für alle Projekte auf einmal. Deshalb sind die einzelnen `.csproj`-Dateien dort fast leer.
{: .notice--primary}

## Solutions mit mehreren Projekten

Eine wachsende Anwendung teilt man in mehrere Projekte: eine Klassenbibliothek mit der Fachlogik, ein Projekt für die Bedienung und später ein Testprojekt. Eine **Solution** fasst sie zusammen, damit ein einziger `dotnet build` alle baut. Adventure ist genau so aufgebaut. Den Anfang legen wir selbst an:

```bash
mkdir Adventure
cd Adventure
dotnet new sln -n Adventure
dotnet new classlib -o Adventure.Kern
dotnet new console -o Adventure.Konsole
dotnet sln add Adventure.Kern
dotnet sln add Adventure.Konsole
```

Mit dem .NET 10 SDK entsteht eine `Adventure.slnx`. Das ist das neue, schlanke XML-Format; ältere SDKs erzeugen eine `.sln`, die genauso funktioniert. Noch wissen die beiden Projekte nichts voneinander. Damit die Konsole Klassen aus dem Kern verwenden kann, braucht sie eine **Projektreferenz**:

```bash
dotnet add Adventure.Konsole reference Adventure.Kern
```

In `Adventure.Konsole.csproj` erscheint daraufhin:

```xml
<ItemGroup>
  <ProjectReference Include="..\Adventure.Kern\Adventure.Kern.csproj" />
</ItemGroup>
```

Die Richtung ist wichtig: Die Konsole kennt den Kern, aber nicht umgekehrt. Im Lauf des Semesters kommen `Adventure.Daten`, `Adventure.Web` und `Adventure.Tests` auf demselben Weg dazu. Alle Referenzen zeigen nach innen, und `Adventure.Kern` selbst verweist auf nichts. Dieses Muster ist die Grundlage der Schichten-Architektur aus [Vorlesung 04](/lectures/04/04.md).

Referenzieren sich zwei Projekte gegenseitig, meldet `dotnet build` einen Zirkelbezug. Das ist ein Designproblem: Entweder gehören die beiden Projekte zusammen, oder eines hängt an der falschen Stelle.
{: .notice--warning}

## Weitere Vorlagen

Neben `console`, `classlib` und `sln` zeigt `dotnet new list` alle Vorlagen des SDK. Für uns wichtig sind noch zwei: aus `blazor` entsteht in [Vorlesung 04](/lectures/04/04.md) das Projekt `Adventure.Web`, aus `nunit` in [Vorlesung 12](/lectures/12/12.md) das Projekt `Adventure.Tests`. Alles, was du per CLI anlegst, kannst du anschließend normal in deiner IDE öffnen.

Übung: Lege die Solution `Adventure` mit `Adventure.Kern` und `Adventure.Konsole` wie oben an. Schreibe im Kern eine Klasse `Koordinate` mit den Properties `X` und `Y` und einer Methode `Entfernung(Koordinate andere)`, die den Abstand in Feldern liefert. Gib im Konsolenprojekt die Entfernung zwischen zwei Positionen aus und baue alles mit einem einzigen `dotnet build` im Solution-Ordner. Was passiert, wenn du die Projektreferenz wieder aus der `.csproj` löschst? Und was meldet der Compiler, wenn du aus `Adventure.Kern` heraus eine Klasse aus `Adventure.Konsole` benutzen willst?
{: .notice--info}

## Weitere Quellen

- [Übersicht über die .NET-CLI – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/)
- [Projektdateien und MSBuild-Eigenschaften – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/project-sdk/overview)
- [dotnet sln – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-sln)
