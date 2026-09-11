# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Course website for **Programmierung 2 (C#)** at HTW Berlin – the continuation of [Programmierung 1](https://www.erodner.de/prog-lecture/) (repo `../prog-lecture`, same structure and style). Focus: object-oriented programming in depth (inheritance, interfaces, generics, delegates/events, design patterns), web GUIs with **Blazor** (Blazor Web App, Interactive Server render mode), and professional tooling (Git, NuGet, unit tests). Built with Jekyll 4.2 + Minimal Mistakes theme (contrast skin), deployed via GitHub Pages. All content is in German (informal "du").

## Build & Deploy

```bash
bundle install                    # Install dependencies
bundle exec jekyll serve          # Local dev server at http://localhost:4000
```

Deployment happens automatically via GitHub Actions on push to `main` (`.github/workflows/pages.yml`).

macOS Homebrew users may need:
```bash
export PATH="/Applications/Xcode.app/Contents/Developer/Toolchains/XcodeDefault.xctoolchain/usr/bin:$PATH"
export SDKROOT=$(xcrun --show-sdk-path)
```

## Content Architecture

**Two-level content model:**
- `lectures/NN/NN.md` — 13 lecture units (00–12), each a navigational hub linking to modules
- `modules/<name>/<name>.md` — standalone learning modules with explanations, C# code examples, and exercises

Lectures are the "table of contents"; modules contain the actual teaching content. Lectures reference modules via relative Markdown links like `[Title](/modules/name/name.md)`.

**Course progression:**
- Lecture 00: Einstieg – Rückblick auf Programmierung 1 (links to the Prog-1 site), .NET SDK, IDE, dotnet-CLI
- Lecture 01: Vererbung (virtual/override, sealed, Laufzeittyp, object, Garbage Collection)
- Lecture 02: Abstrakte Klassen und Interfaces
- Lecture 03: Git (CLI first, IDE integration second, HTW GitLab)
- Lecture 04: GUI mit Blazor + Schichten-Architekturen (replaces the former Windows-Forms lecture)
- Lecture 05: Generizität
- Lecture 06: Hashing, Sortieren, Suchen, Collections, LINQ (query syntax)
- Lecture 07: Delegaten, Func/Action, Lambdas, LINQ-Methodensyntax, Ereignisse
- Lecture 08: Entwurfsmuster (Singleton, Adapter, Composite, Iterator, Observer)
- Lecture 09: Dateien, Streams, IDisposable, JSON/XML-Serialisierung, HttpClient
- Lecture 10: Native Bibliotheken (P/Invoke, LibraryImport)
- Lecture 11: NuGet
- Lecture 12: Unit-Testing mit NUnit

**Computational Thinking modules:**
- `modules/aufgaben_<topic>/aufgaben_<topic>.md` — Exercises with collapsible solutions, one per lecture
- Linked in lecture tables with 🧩 emoji prefix (always the last table row)
- Solutions use `<details markdown="1"><summary>Lösung anzeigen</summary>` (the `markdown="1"` is required for kramdown to render Markdown inside HTML)

**Links to Programmierung 1:** `https://www.erodner.de/prog-lecture/modules/<name>/<name>/` (module names as in `../prog-lecture/modules/`), lectures `https://www.erodner.de/prog-lecture/lectures/NN/NN/`.

## examples/ – runnable projects

`examples/` is excluded from the Jekyll build but committed. It holds runnable .NET projects for everything that does not fit into a single snippet. `examples/Directory.Build.props` sets `net10.0`, `Nullable`, `ImplicitUsings`. Everything must build with `dotnet build` and tests must pass with `dotnet test` (see `examples/README.md`).

- `04_blazor/HalloBlazor` – minimal Blazor Web App (`dotnet new blazor --empty -int Server -ai`): input, button, output with `@bind`/`@onclick`
- `04_blazor/Geometrieeditor` – the running example of the course, a 3-layer solution (`Geometrieeditor.slnx`, the XML solution format of .NET 10):
  - `Geometrieeditor.Fachkonzept` (classlib): `Figur` (abstract), `Rechteck`, `Kreis`, `Dreieck`, `FigurenVerwaltung`, interface `IFigurSpeicher`
  - `Geometrieeditor.Datenhaltung` (classlib): `ArbeitsspeicherFigurSpeicher`, `JsonFigurSpeicher` (implement `IFigurSpeicher`)
  - `Geometrieeditor.Web` (Blazor): `Components/Pages/Home.razor`, `Components/NeueFigurDialog.razor`; `Program.cs` registers `IFigurSpeicher`/`FigurenVerwaltung` via DI (`AddScoped`), pages use `@inject`
  - `Geometrieeditor.Tests` (NUnit)
  - Dependency direction: Web → Fachkonzept; Datenhaltung → Fachkonzept (implements the interface the Fachkonzept defines); `Program.cs` of the Web project wires the concrete `IFigurSpeicher` into `FigurenVerwaltung`.
- `10_pinvoke/PInvokeDemo` – DllImport/LibraryImport with libc/libm and an own C library
- `11_nuget/NLogDemo` – console app using NLog
- `12_unittests/Bruch` – `Bruch` classlib + `Bruch.Tests` (NUnit)

Module snippets that show the Geometrieeditor must use exactly these identifiers.

## Content Conventions

- Front matter: `layout: single`, `author_profile: true`, `toc: false`, `classes: wide`
- Modules include `licence: "CC-BY"` and `licence_desc: 2026 | HTW Berlin`
- Exercises use `{: .notice--info}` blocks
- Important notes use `{: .notice--primary}`, warnings use `{: .notice--warning}`
- External references (Microsoft Learn de-de, docs.avaloniaui.net, git-scm.com/book/de, nunit.org) go in a `## Weitere Quellen` section at the bottom
- Code examples are C# with German variable/method names (PascalCase methods, camelCase parameters), umlauts transliterated in identifiers (`Flaeche`, `Groesse`)
- Modern .NET only: `System.Text.Json` (not Newtonsoft), `HttpClient` (not WebClient), NUnit 4 `Assert.That`, `PackageReference`, `[LibraryImport]` alongside `[DllImport]`, git CLI (not Sourcetree)
- Blazor: Razor components with markup + `@code`, `@onclick`/`@bind`, dialogs as components with `EventCallback<T>`, no JavaScript, no Bootstrap (plain CSS flexbox/grid in `wwwroot/app.css`)
- Cross-module references should mention previously learned concepts by name
- Connecting prose between code blocks is expected — no "code dump" style
- Never include organisational content (dates, team, exam rules), credentials, internal server addresses, or verbatim textbook prose from the slides

## Writing Style Preferences

- Motivating intro paragraphs that explain *why* a concept matters
- Verbindungssätze (connecting sentences) between code examples — explain transitions
- Real-world analogies where helpful (Bote for delegates, Ablagestapel for generics, Schließfächer for arrays, etc.)
- Back-references to earlier modules when building on prior concepts
- "Im Zweifel..." guidance for students (e.g., "Im Zweifel Interface statt abstrakte Klasse")
- The user (professor) dislikes: artificial overloading examples (no `bool` flags), summaries at end of responses, overly simple exercises

## Jekyll Configuration

- Markdown: kramdown with GFM input
- Relative links enabled for collections (`jekyll-relative-links` plugin)
- `modules/` and `lectures/` are explicitly included in the build
- `slides/` (symlink to the PowerPoint decks), `examples/` and `CLAUDE.md` are excluded from the build
- Collapsible boxes require `<details markdown="1">` (not plain `<details>`)
