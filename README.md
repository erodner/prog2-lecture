# Programmierung 2 – HTW Berlin

Kurswebseite für **Programmierung 2 (C#)** an der HTW Berlin – die Fortsetzung von [Programmierung 1](https://github.com/erodner/prog-lecture).

Gebaut mit [Jekyll](https://jekyllrb.com/) und dem [Minimal Mistakes](https://mmistakes.github.io/minimal-mistakes/)-Theme (Contrast-Skin).

## Voraussetzungen

- Ruby (empfohlen via Homebrew)
- Bundler
- Für die Beispielprojekte: .NET SDK 10

## Lokale Entwicklung

```bash
bundle install
bundle exec jekyll serve
```

Die Seite ist dann unter `http://localhost:4000` erreichbar.

> **Hinweis für macOS mit Homebrew:** Falls beim Build Linker-Fehler auftreten (`ld: unsupported tapi file type`), den Xcode-Toolchain-Pfad vorschalten:
> ```bash
> export PATH="/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin:$PATH"
> export SDKROOT=$(xcrun --show-sdk-path)
> ```

## Struktur

```
lectures/       # Eine Datei pro Vorlesungseinheit (00–12)
modules/        # Einzelne Lehrmodule mit Inhalt und Codebeispielen
examples/       # Lauffähige .NET-Beispielprojekte (nicht Teil der Webseite)
assets/         # Bilder, Daten und statische Dateien
_data/          # Navigation und Autorenprofile
```

## Beispielprojekte bauen

```bash
cd examples
dotnet build 03_blazor/Geometrieeditor                          # Blazor-App + Schichten-Architektur
dotnet test  03_blazor/Geometrieeditor                          # NUnit-Tests des Fachkonzepts
dotnet run --project 03_blazor/Geometrieeditor/Geometrieeditor.Web  # dann http://localhost:5xxx im Browser öffnen
```

Details zu allen Projekten stehen in [`examples/README.md`](examples/README.md).

## Lizenz

Inhalte stehen unter [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) – Erik Rodner, HTW Berlin.
