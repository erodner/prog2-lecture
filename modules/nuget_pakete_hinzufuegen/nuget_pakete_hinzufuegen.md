---
title: "Pakete hinzufügen und verwalten"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Im Modul [NuGet-Grundlagen](/modules/nuget_grundlagen/nuget_grundlagen.md) haben wir gelernt, ein Paket zu bewerten. Jetzt holen wir es ins Projekt – und schauen dabei genau hin, was auf der Festplatte passiert. Wie bei Git in der [Vorlesung 03](/lectures/03/03.md) fangen wir mit der Kommandozeile an, weil man dort jeden Schritt sieht, und ordnen die Schaltflächen der IDEs danach ein. Als Beispiel dient das Logging-Paket NLog, mit dem wir im nächsten Modul arbeiten.

## Ein Paket hinzufügen

Ein Paket kommt mit einem einzigen Befehl ins Projekt, ausgeführt im Ordner der `.csproj`:

```bash
dotnet new console -o NLogDemo
cd NLogDemo
dotnet add package NLog
# info : Adding PackageReference for package 'NLog' into project 'NLogDemo.csproj'.
# info : Restoring packages for /Users/anna/NLogDemo/NLogDemo.csproj...
# info : PackageReference for package 'NLog' version '6.2.0' added to file 'NLogDemo.csproj'.
```

Ohne weitere Angabe nimmt NuGet die neueste stabile Version. Wer eine bestimmte Version möchte – weil das Team sie festgelegt hat oder eine neuere Probleme macht – gibt sie mit `--version` an:

```bash
dotnet add package NLog --version 6.2.0
```

Das Ergebnis ist eine neue Zeile in der Projektdatei. Mehr als diese Zeile ist ein Paket aus Sicht des Projekts nicht:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="NLog" Version="6.2.0" />
  </ItemGroup>

</Project>
```

`PackageReference` ist das Gegenstück zur `ProjectReference` aus dem Modul [Projekte mit der dotnet-CLI](/modules/dotnet_cli_projekte/dotnet_cli_projekte.md): Statt auf ein Projekt im selben Ordner zeigt es auf ein Paket in einem Feed. Man kann die Zeile auch von Hand in die `.csproj` schreiben – das Ergebnis ist dasselbe.

Manche Vorlagen nehmen einem den Befehl sogar ab: `dotnet new nunit` legt ein Projekt an, in dessen `.csproj` `NUnit`, `NUnit3TestAdapter` und `Microsoft.NET.Test.Sdk` bereits als `PackageReference` stehen. So ist auch `Adventure.Tests`, das Testprojekt unseres Spiels, zu seinen fünf Paketen gekommen – niemand hat sie einzeln hinzugefügt.

Die Angabe `Version="6.2.0"` bedeutet für NuGet „mindestens 6.2.0“. In der Praxis bekommt ihr genau diese Version, weil NuGet immer die kleinste passende wählt – außer ein anderes Paket verlangt eine höhere. Schreibt man `Version="6.*"`, nimmt NuGet bei jedem Restore die neueste 6er-Version; das klingt bequem, führt aber dazu, dass zwei Rechner mit derselben `.csproj` unterschiedlichen Code bauen. Lasst die Finger davon.
{: .notice--warning}

## Restore und der Paket-Cache

Die `.csproj` sagt nur, *welches* Paket gebraucht wird. Das Herunterladen übernimmt der **Restore**, den `dotnet build` und `dotnet run` automatisch ausführen; von Hand ruft man ihn mit `dotnet restore` auf. Dabei passieren drei Dinge:

1. NuGet liest alle `PackageReference`-Einträge und berechnet den vollständigen Abhängigkeitsbaum, inklusive der transitiven Abhängigkeiten.
2. Jedes fehlende Paket wird vom Feed heruntergeladen und in den **globalen Paket-Cache** entpackt: `~/.nuget/packages/` unter macOS und Linux, `%USERPROFILE%\.nuget\packages\` unter Windows. Dort liegt jedes Paket genau einmal pro Version, egal wie viele Projekte es benutzen.
3. Das Ergebnis der Berechnung wird in `obj/project.assets.json` geschrieben.

Ein Blick in den Cache zeigt, wie ein Paket aussieht, wenn es ausgepackt ist:

```bash
ls ~/.nuget/packages/nlog/6.2.0/lib
# net35  net45  net46  netstandard2.0  netstandard2.1
```

Für jedes Zielframework gibt es eine eigene `NLog.dll`. Welche davon unser `net10.0`-Projekt bekommt, steht in der `project.assets.json` – und das ist auch die Datei, die der Compiler tatsächlich liest:

```json
"NLog/6.2.0": {
  "type": "package",
  "compile": { "lib/netstandard2.1/NLog.dll": {} },
  "runtime": { "lib/netstandard2.1/NLog.dll": {} }
}
```

Die Datei liegt in `obj/` und wird bei jedem Restore neu erzeugt – sie gehört, wie alles in `obj/` und `bin/`, nicht ins Git-Repository. Was hingegen hineingehört, ist die `.csproj`: Wer das Repository klont und `dotnet build` aufruft, bekommt durch den Restore automatisch dieselben Pakete in derselben Version. Die Pakete selbst muss man also nie mit einchecken.

Der Cache ist auch der Grund, warum ein zweites Projekt mit NLog sofort baut, selbst offline: NuGet findet Version 6.2.0 bereits im Cache und fragt den Feed gar nicht erst. Erst eine Version, die noch nicht dort liegt, braucht eine Internetverbindung. Mit `dotnet nuget locals all --clear` leert man den Cache, falls er einmal beschädigt ist – danach lädt der nächste Restore alles neu.
{: .notice--primary}

## Pakete anzeigen, aktualisieren, entfernen

Welche Pakete ein Projekt benutzt, zeigt `dotnet list package`; mit `--outdated` fragt der Befehl den Feed, ob es neuere Versionen gibt:

```bash
dotnet list package --outdated
# Project 'NLogDemo' has the following updates to its packages
#    [net10.0]:
#    Top-level Package      Requested   Resolved   Latest
#    > NLog                 6.1.0       6.1.0      6.2.0
```

Ein eigenes „update“-Kommando gibt es nicht: Man ruft `dotnet add package NLog` erneut auf, wahlweise mit `--version`, und NuGet ersetzt die Versionsnummer in der `.csproj`. Bei einem Sprung der MAJOR-Version solltet ihr vorher die Release Notes lesen – die Regeln der semantischen Versionierung kennt ihr aus dem Grundlagenmodul. Ein Paket, das nicht mehr gebraucht wird, verschwindet mit:

```bash
dotnet remove package NLog
```

Der Befehl löscht nur die `PackageReference`-Zeile. Das Paket bleibt im Cache liegen, das ist so gewollt – andere Projekte könnten es noch benutzen.

## Der Weg über die IDE

Alle drei IDEs bieten für dieselben Schritte eine Oberfläche. Am Ende ändern sie ausschließlich die `.csproj` und stoßen einen Restore an – nichts anderes als die Befehle oben:

| Aktion | Kommandozeile | Visual Studio | Rider | VS Code (C# Dev Kit) |
| :--- | :--- | :--- | :--- | :--- |
| Paket suchen und hinzufügen | `dotnet add package` | Rechtsklick auf Projekt → „NuGet-Pakete verwalten“ → Tab „Durchsuchen“ | Tool-Fenster „NuGet“ → Suchfeld, Version wählen, „+“ beim Projekt | Befehlspalette → „NuGet: Add NuGet Package“ |
| Installierte Pakete und Updates | `dotnet list package --outdated` | Tabs „Installiert“ und „Updates“ | Tab „Installed Packages“ mit Update-Symbol | Projektmappen-Explorer → „Dependencies“ |
| Paket entfernen | `dotnet remove package` | Tab „Installiert“ → „Deinstallieren“ | Rechtsklick auf Paket → „Remove“ | Rechtsklick auf Paket → „Remove“ |

Visual Studio bietet zusätzlich die **Paket-Manager-Konsole**, ein PowerShell-Fenster mit Befehlen wie `Install-Package NLog` – eine ältere Schnittstelle, die dasselbe tut. In älteren Projekten, vor allem aus der Zeit von .NET Framework, findet man außerdem noch eine Datei `packages.config`, in der die Pakete außerhalb der `.csproj` aufgelistet sind; in modernen SDK-Projekten gibt es sie nicht mehr, und wer einer begegnet, sollte das Projekt auf `PackageReference` umstellen.

## Versionen im Team festhalten

Weil die `.csproj` die einzige Wahrheit über die Pakete ist, gilt im Team eine einfache Regel: **Versionen werden explizit eingetragen und mit committet.** Das hat drei Konsequenzen:

- Jedes Teammitglied und jede Build-Pipeline baut mit exakt derselben Paketversion. „Bei mir läuft es“ scheidet als Fehlerursache aus.
- Ein Paket-Update ist ein normaler Commit, den man im Diff sieht, reviewen und bei Problemen mit `git revert` zurücknehmen kann.
- Verwenden mehrere Projekte einer Solution dasselbe Paket, sollten sie dieselbe Version angeben – sonst streitet NuGet beim Bauen und wählt die höchste. Wer etwa `NLog` in `Adventure.Daten` und in `Adventure.Web` benutzt, trägt in beiden `.csproj` dieselbe Version ein; bei vielen Projekten hilft eine gemeinsame `Directory.Build.props` oder `Directory.Packages.props` im Solution-Ordner, in der die Versionen nur einmal stehen.

Übung: Lege ein Konsolenprojekt an und füge das Paket `Humanizer` hinzu – eine kleine Bibliothek, die zum Beispiel aus `DateTime.Now.AddHours(-3)` den Text „3 hours ago“ macht. Schau in die `.csproj`, in `obj/project.assets.json` und in `~/.nuget/packages/humanizer*`: Wie viele Pakete sind wirklich gelandet, und warum mehr als eines? Entferne das Paket danach wieder und prüfe, was aus den drei Orten verschwunden ist und was nicht.
{: .notice--info}

## Weitere Quellen

- [dotnet add package – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/tools/dotnet-add-package)
- [PackageReference in Projektdateien – Microsoft Learn](https://learn.microsoft.com/de-de/nuget/consume-packages/package-references-in-project-files)
- [dotnet restore und der Paket-Cache – Microsoft Learn](https://learn.microsoft.com/de-de/nuget/consume-packages/managing-the-global-packages-and-cache-folders)
- [Pakete in Visual Studio installieren und verwalten – Microsoft Learn](https://learn.microsoft.com/de-de/nuget/consume-packages/install-use-packages-visual-studio)
