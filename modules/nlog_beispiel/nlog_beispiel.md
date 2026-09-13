---
title: "Beispiel: Logging mit NLog"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Ein Programm, das nur `Console.WriteLine` kennt, ist wie ein Flugzeug ohne Flugschreiber: Solange jemand zuschaut, sieht man, was passiert – aber wenn es nachts auf dem Server eines Kunden abstürzt, ist die Ausgabe weg. **Logging** löst dieses Problem: Das Programm schreibt fortlaufend Meldungen mit Zeitstempel und Wichtigkeit in ein Protokoll, das man auch Tage später noch lesen kann. Statt sich das selbst zu bauen, greifen wir zum Paket **NLog** – ein Paradebeispiel dafür, wie man in wenigen Minuten eine ausgereifte Bibliothek ins Projekt holt. Das vollständige Projekt findest du im Repository unter `examples/11_nuget/NLogDemo`.

## Warum nicht einfach `Console.WriteLine`?

`Console.WriteLine` ist für Ausgaben gedacht, die der Benutzer sehen soll. Für Diagnosemeldungen fehlen ihm vier Dinge: eine **Wichtigkeit** (Ist das eine Notiz oder ein Absturz?), ein **Zeitstempel**, eine **Herkunft** (Welche Klasse hat das geschrieben?) und ein **abschaltbares Ziel** – die Debug-Ausgaben, die während der Entwicklung helfen, sollen beim Kunden nicht die Konsole fluten. Eine Logging-Bibliothek liefert genau das: Man schreibt `logger.Warn("…")`, und *wohin* die Meldung geht und *ob* sie überhaupt erscheint, entscheidet eine Konfigurationsdatei, die man ändern kann, ohne neu zu kompilieren.

## NLog einbauen

Wie im Modul [Pakete hinzufügen](/modules/nuget_pakete_hinzufuegen/nuget_pakete_hinzufuegen.md) beschrieben, reicht ein Befehl im Projektordner:

```bash
dotnet add package NLog
```

Damit ist die Bibliothek da, aber NLog weiß noch nicht, wohin es schreiben soll. Das legt eine Datei `nlog.config` fest, die neben der `.csproj` liegt und beim Bauen in den Ausgabeordner kopiert werden muss. Dafür braucht die `.csproj` einen zweiten Eintrag:

```xml
<ItemGroup>
  <PackageReference Include="NLog" Version="6.2.0" />
</ItemGroup>

<ItemGroup>
  <!-- Die Konfiguration muss neben der fertigen NLogDemo.dll liegen -->
  <None Update="nlog.config" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Fehlt diese Zeile, liegt die `nlog.config` zwar im Projektordner, aber nicht neben der gebauten `NLogDemo.dll` in `bin/Debug/net10.0/`. NLog findet dann keine Konfiguration und verwirft **stillschweigend jede Meldung** – das Programm läuft, nur das Log bleibt leer. Wer sich fragt, warum nichts geloggt wird, sollte als Erstes prüfen, ob die Datei im Ausgabeordner angekommen ist.
{: .notice--warning}

## Die Konfiguration: Ziele und Regeln

Die `nlog.config` ist eine XML-Datei mit zwei Abschnitten. **Targets** beschreiben, *wohin* Meldungen gehen, **Rules** legen fest, *welche* Meldungen welches Ziel erreichen:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <!-- Ziele: wohin die Meldungen geschrieben werden -->
  <targets>
    <target xsi:type="ColoredConsole" name="konsole"
            layout="${time} ${level:uppercase=true:padding=-5} ${message} ${exception:format=Message}" />

    <target xsi:type="File" name="datei"
            fileName="${currentdir}/logs/app.log"
            layout="${longdate} ${level:uppercase=true:padding=-5} ${logger} ${message} ${exception:format=ToString}" />
  </targets>

  <!-- Regeln: welche Meldungen in welches Ziel gehen -->
  <rules>
    <logger name="*" minlevel="Info" writeTo="konsole" />
    <logger name="*" minlevel="Debug" writeTo="datei" />
  </rules>

</nlog>
```

Wir definieren zwei Ziele: eine farbige Konsole und eine Datei `logs/app.log`, die NLog samt Ordner selbst anlegt. Das `layout` ist eine Schablone mit Platzhaltern wie `${longdate}`, `${level}`, `${logger}` und `${message}` – die Datei bekommt mehr Details als die Konsole, weil dort niemand auf Übersichtlichkeit angewiesen ist. Die beiden Regeln sagen: Auf die Konsole kommt alles ab `Info`, in die Datei alles ab `Debug`. `name="*"` heißt „für alle Logger“; mit `name="Adventure.Daten.*"` ließe sich eine Regel auf eine einzelne Schicht unseres Spiels beschränken.

## Meldungen schreiben

Im Code holt man sich pro Klasse einen Logger. `GetCurrentClassLogger()` gibt ihm automatisch den Namen der umgebenden Klasse – so steht später im Log, woher jede Zeile stammt:

```csharp
using NLog;

Logger logger = LogManager.GetCurrentClassLogger();

logger.Info("Programm gestartet.");
logger.Warn("Konfigurationsdatei 'einstellungen.json' nicht gefunden, nutze Standardwerte.");
```

Statt Zeichenketten zusammenzusetzen, übergibt man Werte als Platzhalter – NLog fügt sie ein und kann sie bei Bedarf sogar strukturiert speichern:

```csharp
for (int i = 1; i <= 3; i++)
{
    logger.Debug("Verarbeite Datensatz {Nummer} von 3", i);
}
```

Diese drei Meldungen haben das Level `Debug` und erscheinen nach unseren Regeln nur in der Datei, nicht auf der Konsole. Genau das ist der Punkt: Der Code bleibt gleich, die Konfiguration entscheidet.

## Log-Level

NLog kennt sechs Stufen, aufsteigend nach Wichtigkeit. Die Regel `minlevel="Info"` lässt alles ab `Info` durch – also `Info`, `Warn`, `Error` und `Fatal`:

| Level | Wann | Beispiel |
| :--- | :--- | :--- |
| `Trace` | Jedes Detail, nur zur Fehlersuche | „Betrete Methode `Laden`“ |
| `Debug` | Technische Zwischenschritte | „Verarbeite Datensatz 2 von 3“ |
| `Info` | Normale Ereignisse, die man später nachvollziehen will | „Programm gestartet“, „Level 'kerker' geladen“ |
| `Warn` | Ungewöhnlich, aber das Programm läuft weiter | „Konfigurationsdatei fehlt, nutze Standardwerte“ |
| `Error` | Eine Operation ist fehlgeschlagen | „Speichern nach spielstand.json fehlgeschlagen“ |
| `Fatal` | Das Programm kann nicht weitermachen | „Datenbank nicht erreichbar, beende“ |

Die schwierigste Entscheidung im Alltag ist die zwischen `Info` und `Debug`: Was würde ich wissen wollen, wenn ich morgen früh ein Log lese, in dem etwas schiefgegangen ist? Das ist `Info`. Was hilft mir nur, während ich gerade einen Fehler suche? Das ist `Debug`.

## Exceptions mit loggen

Der wichtigste Anwendungsfall: Eine Exception wird gefangen, und im Log soll nicht nur „Fehler“ stehen, sondern Typ, Meldung und Stacktrace. Dafür nimmt jede Log-Methode die Exception als **erstes** Argument entgegen:

```csharp
try
{
    int[] messwerte = { 12, 7, 42 };
    int index = 3;
    Console.WriteLine($"Messwert: {messwerte[index]}");
}
catch (IndexOutOfRangeException ex)
{
    logger.Error(ex, "Zugriff auf einen ungültigen Messwert.");
}
```

In der `nlog.config` steht beim Konsolenziel `${exception:format=Message}`, beim Dateiziel `${exception:format=ToString}` – die Konsole zeigt nur die Meldung, die Datei den kompletten Stacktrace. Ein Aufruf, zwei unterschiedlich detaillierte Ausgaben:

```bash
dotnet run
# 14:23:50.4717 INFO  Programm gestartet.
# 14:23:50.4798 WARN  Konfigurationsdatei 'einstellungen.json' nicht gefunden, nutze Standardwerte.
# 14:23:50.4809 ERROR Zugriff auf einen ungültigen Messwert. Index was outside the bounds of the array.
# 14:23:50.4885 INFO  Programm beendet.

cat logs/app.log
# 2026-09-10 14:23:50.4717 INFO  NLogDemo.Program Programm gestartet.
# 2026-09-10 14:23:50.4798 WARN  NLogDemo.Program Konfigurationsdatei 'einstellungen.json' nicht gefunden, nutze Standardwerte.
# 2026-09-10 14:23:50.4809 ERROR NLogDemo.Program Zugriff auf einen ungültigen Messwert. System.IndexOutOfRangeException: Index was outside the bounds of the array.
#    at Program.<Main>$(String[] args) in /Users/anna/NLogDemo/Program.cs:line 14
# 2026-09-10 14:23:50.4868 DEBUG NLogDemo.Program Verarbeite Datensatz 1 von 3
# 2026-09-10 14:23:50.4885 DEBUG NLogDemo.Program Verarbeite Datensatz 2 von 3
# 2026-09-10 14:23:50.4885 DEBUG NLogDemo.Program Verarbeite Datensatz 3 von 3
# 2026-09-10 14:23:50.4885 INFO  NLogDemo.Program Programm beendet.
```

Die Datei bekommt acht Einträge, die Konsole vier: Die drei `Debug`-Meldungen erscheinen nur dort, wo die Regel `minlevel="Debug"` gilt, und der Logger-Name `NLogDemo.Program` steht nur im Dateilayout. Derselbe Code, zwei Detailstufen – entschieden allein in der `nlog.config`.

Am Ende des Programms steht noch `LogManager.Shutdown();`. NLog puffert Dateiausgaben aus Geschwindigkeitsgründen; der Aufruf leert den Puffer, damit die letzten Zeilen sicher in der Datei landen, bevor der Prozess endet.

Eine Exception, die man fängt und nur loggt, ist damit nicht behandelt – das Programm läuft mit möglicherweise kaputtem Zustand weiter. Loggen ersetzt keine Fehlerbehandlung, es dokumentiert sie. Und umgekehrt: Wer eine Exception fängt, neu wirft und an jeder Schicht erneut loggt, produziert dieselbe Meldung dreimal. Im Zweifel loggt die Stelle, die die Exception wirklich behandelt, und nur sie.
{: .notice--primary}

## Alternativen

NLog ist nicht die einzige Wahl. **Serilog** ist ähnlich verbreitet und setzt konsequent auf strukturiertes Logging, bei dem jede Meldung als Datensatz mit benannten Feldern gespeichert wird – praktisch, wenn man Logs später mit Werkzeugen durchsucht. Microsoft selbst liefert mit **`Microsoft.Extensions.Logging`** eine schlanke Abstraktion (`ILogger<T>`), die in ASP.NET-Anwendungen fest eingebaut ist; sie definiert nur die Schnittstelle, und NLog oder Serilog stecken als Anbieter dahinter. Für eine Klassenbibliothek, die nicht wissen soll, welches Logging-Paket die Anwendung benutzt, ist diese Abstraktion die richtige Wahl – in unserem Spiel wäre das `Adventure.Kern`, das von der Datenhaltung ja auch nur die Interfaces `ILevelQuelle` und `ISpielstandSpeicher` kennt.

Übung: Ändere die `nlog.config` von `NLogDemo`, ohne den C#-Code anzufassen, sodass die Konsole nur noch Warnungen und Fehler zeigt und die Datei zusätzlich `Trace`-Meldungen aufnimmt. Füge dann ein drittes Ziel hinzu, das ausschließlich `Error` und `Fatal` in eine eigene Datei `logs/fehler.log` schreibt. Wie viele Zeilen landen bei einem Programmlauf in jeder der drei Ausgaben?
{: .notice--info}

## Weitere Quellen

- [NLog auf nuget.org](https://www.nuget.org/packages/NLog)
- [NLog-Konfigurationsdatei – NLog-Wiki](https://github.com/NLog/NLog/wiki/Configuration-file)
- [Protokollierung in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/extensions/logging)
- [Logging-Anbieter in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/extensions/logging-providers)
- [Semantic Versioning 2.0.0 (deutsch)](https://semver.org/lang/de/) – erklärt, was die drei Zahlen in `Version="6.2.0"` versprechen und woran du erkennst, dass ein Update dir den Code zerlegen darf.
- [Dependency Injection in Blazor – Microsoft Learn](https://learn.microsoft.com/de-de/aspnet/core/blazor/fundamentals/dependency-injection) – der Weg, auf dem ein `ILogger<T>` in eine Razor-Komponente kommt, wenn du die Abstraktion aus dem Abschnitt „Alternativen“ wirklich benutzt.
