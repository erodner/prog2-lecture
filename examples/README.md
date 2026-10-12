# Beispielprojekte zur Vorlesung

Kleine, eigenständige Beispiele, die aus den Modulen der Webseite heraus referenziert werden. Sie sind **nicht** Teil des Jekyll-Builds. Das durchgehende Beispiel der Vorlesung – das Spiel **Adventure** – liegt im eigenen Repository [erodner/prog2-adventure](https://github.com/erodner/prog2-adventure).

## Voraussetzungen

- [.NET SDK 10](https://dotnet.microsoft.com/download) (`dotnet --version` sollte `10.x` zeigen)

Gemeinsame Einstellungen (Zielframework `net10.0`, `Nullable`, `ImplicitUsings`) stehen in `Directory.Build.props` und gelten für alle Projekte in diesem Ordner.

## Projekte

| Ordner | Vorlesung | Inhalt | Ausführen |
|---|---|---|---|
| `10_nuget/NLogDemo` | 10 | Konsolenprogramm mit dem NuGet-Paket NLog und `nlog.config` | `dotnet run --project 10_nuget/NLogDemo` |
| `11_unittests/Bruch` | 11 | Klassenbibliothek `Bruch` mit NUnit-Testprojekt `Bruch.Tests` | `dotnet test 11_unittests/Bruch` |

## Alles auf einmal prüfen

```bash
cd examples
for p in 10_nuget/NLogDemo 11_unittests/Bruch; do
  dotnet build "$p" || exit 1
done
dotnet test 11_unittests/Bruch
```
