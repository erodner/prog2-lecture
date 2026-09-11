# Beispielprojekte zur Vorlesung

Alle Projekte sind lauffähig und werden aus den Modulen der Webseite heraus referenziert. Sie sind **nicht** Teil des Jekyll-Builds.

## Voraussetzungen

- [.NET SDK 10](https://dotnet.microsoft.com/download) (`dotnet --version` sollte `10.x` zeigen)
- Für `10_pinvoke`: ein C-Compiler (`clang` unter macOS, `gcc` unter Linux, MSVC oder MinGW unter Windows)

Gemeinsame Einstellungen (Zielframework `net10.0`, `Nullable`, `ImplicitUsings`) stehen in `Directory.Build.props` und gelten für alle Projekte in diesem Ordner.

## Projekte

| Ordner | Vorlesung | Inhalt | Ausführen |
|---|---|---|---|
| `04_blazor/HalloBlazor` | 04 | Minimale Blazor Web App (Interactive Server): Eingabe, Button, Ausgabe mit `@bind`/`@onclick` | `dotnet run --project 04_blazor/HalloBlazor`, dann die angezeigte URL im Browser öffnen |
| `04_blazor/Geometrieeditor` | 02, 04, 09, 12 | Das durchgehende Beispiel: Solution mit `Fachkonzept` (Figur-Hierarchie, `FigurenVerwaltung`, `IFigurSpeicher`), `Datenhaltung` (Arbeitsspeicher- und JSON-Speicher), `Web` (Blazor: Seite mit Figurenliste, Details und modalem Dialog als Komponente) und `Tests` (NUnit) | `dotnet run --project 04_blazor/Geometrieeditor/Geometrieeditor.Web` und `dotnet test 04_blazor/Geometrieeditor` |
| `10_pinvoke/PInvokeDemo` | 10 | `DllImport`/`LibraryImport` mit libc, libm und einer eigenen C-Bibliothek (`native/mathe.c`) | erst `native/build-native.sh` (bzw. `.ps1`), dann `dotnet run --project 10_pinvoke/PInvokeDemo` |
| `11_nuget/NLogDemo` | 11 | Konsolenprogramm mit dem NuGet-Paket NLog und `nlog.config` | `dotnet run --project 11_nuget/NLogDemo` |
| `12_unittests/Bruch` | 12 | Klassenbibliothek `Bruch` mit NUnit-Testprojekt `Bruch.Tests` | `dotnet test 12_unittests/Bruch` |

## Alles auf einmal prüfen

```bash
cd examples
for p in 04_blazor/HalloBlazor 04_blazor/Geometrieeditor 10_pinvoke/PInvokeDemo 11_nuget/NLogDemo 12_unittests/Bruch; do
  dotnet build "$p" || exit 1
done
dotnet test 04_blazor/Geometrieeditor
dotnet test 12_unittests/Bruch
```
