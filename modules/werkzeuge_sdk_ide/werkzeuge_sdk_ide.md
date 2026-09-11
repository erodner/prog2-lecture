---
title: ".NET SDK und IDE einrichten"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

In Programmierung 1 hat die IDE vieles im Hintergrund erledigt: Klick auf „Start“, und das Programm lief. Sobald aber mehrere Personen an einem Projekt arbeiten, Tests automatisch laufen sollen oder eine Anwendung auf einem Server ohne grafische Oberfläche gebaut wird, muss klar sein, was da eigentlich passiert. Die Grundlage ist immer dieselbe: das **.NET SDK**. Es enthält Compiler, Laufzeitumgebung und das Kommandozeilenwerkzeug `dotnet`. Die IDE ist nur eine komfortable Hülle darum – und deshalb frei wählbar. In diesem Modul installieren wir das SDK und entscheiden uns für eine IDE, die auf deinem Betriebssystem gut funktioniert.

## Das .NET 10 SDK installieren

Wir arbeiten in diesem Semester mit **.NET 10**, einer LTS-Version (*Long Term Support*), die drei Jahre lang Updates erhält. Wichtig ist, dass du das **SDK** installierst und nicht nur die *Runtime*: Die Runtime kann fertige Programme ausführen, aber nur das SDK kann sie auch kompilieren.

- **Windows:** Installer von [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) herunterladen und ausführen. Wer Visual Studio 2022 installiert, bekommt das SDK über die Workload automatisch mit – kontrolliere trotzdem die Version.
- **macOS:** Ebenfalls der Installer von der Downloadseite (`.pkg`), passend zur Architektur (Apple Silicon oder Intel). Alternativ per Homebrew: `brew install --cask dotnet-sdk`.
- **Linux:** Die meisten Distributionen bieten Pakete an; die genauen Befehle je Distribution stehen in der [Installationsanleitung von Microsoft](https://learn.microsoft.com/de-de/dotnet/core/install/linux). Unter Ubuntu reicht in der Regel `sudo apt install dotnet-sdk-10.0`.

Nach der Installation öffnest du ein neues Terminal (unter Windows PowerShell oder das Windows Terminal) und prüfst, ob `dotnet` gefunden wird:

```bash
dotnet --version
# 10.0.100
```

Die genaue Nummer hinter `10.0.` kann abweichen – entscheidend ist die `10` am Anfang. Falls mehrere SDKs installiert sind, etwa noch ein .NET 8 aus Programmierung 1, zeigt der folgende Befehl alle an:

```bash
dotnet --list-sdks
# 8.0.404 [/usr/local/share/dotnet/sdk]
# 10.0.100 [/usr/local/share/dotnet/sdk]
```

Mehrere SDKs nebeneinander sind kein Problem: Für jedes Projekt entscheidet die Datei `.csproj` über das Zielframework, und `dotnet` wählt standardmäßig das neueste installierte SDK. Was in einer `.csproj` steht, schauen wir uns im Modul [Projekte mit der dotnet-CLI](/modules/dotnet_cli_projekte/dotnet_cli_projekte.md) an.

Wenn `dotnet` nach der Installation nicht gefunden wird, liegt es fast immer daran, dass das Terminal noch vor der Installation geöffnet wurde und die Umgebungsvariable `PATH` nicht neu gelesen hat. Terminal schließen, neu öffnen, noch einmal probieren – erst danach lohnt sich die Fehlersuche.
{: .notice--warning}

## Eine IDE wählen

Alle drei folgenden Umgebungen nutzen dasselbe SDK und erzeugen dieselben Projektdateien. Ein Projekt, das in Rider angelegt wurde, öffnet sich problemlos in Visual Studio oder VS Code – und umgekehrt. Du kannst dich also nach Betriebssystem und Geschmack entscheiden.

| IDE | Plattformen | Hinweise |
| :--- | :--- | :--- |
| **Visual Studio 2022 Community** | nur Windows | Kostenlos; bei der Installation die Workloads **„.NET-Desktopentwicklung“** und **„ASP.NET und Webentwicklung“** auswählen. Vermutlich kennst du sie aus Programmierung 1. |
| **JetBrains Rider** | Windows, macOS, Linux | Kostenlos für Studierende mit Hochschul-E-Mail-Adresse; sehr gute Unterstützung für Razor-Dateien und Unit-Tests. |
| **VS Code + C# Dev Kit** | Windows, macOS, Linux | Leichtgewichtiger Editor; die Erweiterung *C# Dev Kit* bringt Projektexplorer, Debugger und Testrunner mit. |

Für Visual Studio installierst du den [Visual Studio Installer](https://visualstudio.microsoft.com/de/vs/community/) und wählst dort die Workload aus. Für Rider lädst du die IDE von [jetbrains.com/rider](https://www.jetbrains.com/rider/) herunter und beantragst mit deiner Hochschuladresse eine Studierendenlizenz. Für VS Code installierst du den Editor von [code.visualstudio.com](https://code.visualstudio.com/docs/csharp/get-started) und suchst anschließend in der Erweiterungsansicht nach „C# Dev Kit“.

Egal welche IDE: Die Projektdatei `.csproj` ist die einzige Wahrheit. Wenn ein Projekt in deiner IDE läuft, aber auf dem Rechner deiner Teampartnerin nicht, dann prüft zuerst mit `dotnet build` auf der Kommandozeile, ob es überhaupt an der IDE liegt.
{: .notice--primary}

## Vorbereitung auf Blazor

Ab der [Vorlesung 04](/lectures/04/04.md) bauen wir mit **Blazor** Oberflächen, die im Browser laufen. Blazor ist Teil von ASP.NET Core und damit bereits im SDK enthalten – eine zusätzliche Installation ist nicht nötig. Ob die Projektvorlage da ist, zeigt:

```bash
dotnet new list blazor
```

Erscheint dort `Blazor Web App` mit dem Kurznamen `blazor`, ist alles bereit; `dotnet new blazor` legt später eine lauffähige Web-App an. Visual Studio, Rider und VS Code mit C# Dev Kit unterstützen Razor-Dateien (`.razor`) von Haus aus, mit Syntaxhervorhebung und Vervollständigung für Markup und C#. Praktisch ist außerdem `dotnet watch`: Es startet die App und lädt sie bei jeder Änderung an einer Razor-Datei im Browser neu (*Hot Reload*). Du musst das jetzt noch nicht ausprobieren – es reicht, wenn es bis zur dritten Vorlesung funktioniert.

## Alles installiert?

Übung: Prüfe mit `dotnet --list-sdks`, dass ein 10.x-SDK installiert ist. Öffne dann deine IDE, lege ein leeres Konsolenprojekt an, setze einen Haltepunkt in der ersten Zeile und starte das Programm im Debugger. Wenn der Debugger anhält und du den Wert einer Variablen sehen kannst, ist deine Umgebung bereit.
{: .notice--info}

KI-Assistenten wie GitHub Copilot oder die Chat-Funktionen der IDEs sind in der Übung **ausgeschaltet**. Der Grund ist nicht, dass die Werkzeuge schlecht wären – im Gegenteil. Aber sie schlagen Code vor, den du noch nicht selbst schreiben kannst, und genau diese Fähigkeit wollen wir in diesem Semester aufbauen. Wer erst versteht und dann Werkzeuge nutzt, kann deren Vorschläge bewerten; wer es umgekehrt macht, ist auf Gedeih und Verderb dem Vorschlag ausgeliefert.
{: .notice--warning}

## Weitere Quellen

- [.NET herunterladen – Microsoft](https://dotnet.microsoft.com/download)
- [.NET installieren unter Windows, macOS und Linux – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/install/)
- [C# in Visual Studio Code – Visual Studio Code Docs](https://code.visualstudio.com/docs/csharp)
- [JetBrains Rider](https://www.jetbrains.com/rider/)
