# Beispielprojekte zur Vorlesung

Kleine, eigenständige Beispiele, die aus den Modulen der Webseite heraus referenziert werden. Sie sind **nicht** Teil des Jekyll-Builds. Das durchgehende Beispiel der Vorlesung – das Spiel **Adventure** – liegt im eigenen Repository [erodner/prog2-adventure](https://github.com/erodner/prog2-adventure).

## Voraussetzungen

- [.NET SDK 10](https://dotnet.microsoft.com/download) (`dotnet --version` sollte `10.x` zeigen)
- Für `10_pinvoke`: ein C-Compiler (`clang` unter macOS, `gcc` unter Linux, MSVC oder MinGW unter Windows)

Gemeinsame Einstellungen (Zielframework `net10.0`, `Nullable`, `ImplicitUsings`) stehen in `Directory.Build.props` und gelten für alle Projekte in diesem Ordner.

## Projekte

| Ordner | Vorlesung | Inhalt | Ausführen |
|---|---|---|---|
| `10_pinvoke/PInvokeDemo` | 10 | `DllImport`/`LibraryImport` mit libc, libm und einer eigenen C-Bibliothek (`native/mathe.c`) | erst `native/build-native.sh` (bzw. `.ps1`), dann `dotnet run --project 10_pinvoke/PInvokeDemo` |
| `11_nuget/NLogDemo` | 11 | Konsolenprogramm mit dem NuGet-Paket NLog und `nlog.config` | `dotnet run --project 11_nuget/NLogDemo` |
| `12_unittests/Bruch` | 12 | Klassenbibliothek `Bruch` mit NUnit-Testprojekt `Bruch.Tests` | `dotnet test 12_unittests/Bruch` |

## Alles auf einmal prüfen

```bash
cd examples
for p in 10_pinvoke/PInvokeDemo 11_nuget/NLogDemo 12_unittests/Bruch; do
  dotnet build "$p" || exit 1
done
dotnet test 12_unittests/Bruch
```
