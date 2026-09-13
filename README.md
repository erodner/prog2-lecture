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

Jeder Push auf `main` baut und veröffentlicht die Seite automatisch über GitHub Pages (`.github/workflows/pages.yml`).

> **Hinweis für macOS mit Homebrew:** Falls beim Build Linker-Fehler auftreten (`ld: unsupported tapi file type`), den Xcode-Toolchain-Pfad vorschalten:
> ```bash
> export PATH="/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin:$PATH"
> export SDKROOT=$(xcrun --show-sdk-path)
> ```

## Struktur

```
lectures/       # Eine Datei pro Vorlesungseinheit (00–12)
modules/        # Einzelne Lehrmodule mit Inhalt und Codebeispielen
examples/       # Kleine .NET-Beispielprojekte (nicht Teil der Webseite); das Spiel liegt in prog2-adventure
assets/         # Bilder, Daten und statische Dateien
_data/          # Navigation und Autorenprofile
```

## Beispielspiel „Adventure“

Das durchgehende Beispiel der Vorlesung ist ein 2D-Dungeon-Spiel im eigenen Repository [erodner/prog2-adventure](https://github.com/erodner/prog2-adventure). Git-Tags markieren dort den Stand nach den Vorlesungen 01, 02, 04, 09 und 12. Die kleinen Zusatzbeispiele unter `examples/` (P/Invoke, NuGet, Unit-Tests) baut man mit `dotnet build`; Details in [`examples/README.md`](examples/README.md).

## Lizenz

Inhalte stehen unter [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) – Erik Rodner, HTW Berlin.
