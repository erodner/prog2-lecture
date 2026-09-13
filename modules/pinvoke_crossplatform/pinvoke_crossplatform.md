---
title: "P/Invoke plattformübergreifend"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Das `MessageBoxW`-Beispiel aus dem [vorigen Modul](/modules/pinvoke_dllimport/pinvoke_dllimport.md) hat einen Schönheitsfehler: Es läuft nur unter Windows. Dabei ist .NET längst plattformübergreifend, und in einem Kurs, in dem die Hälfte der Teilnehmer ein MacBook hat, wäre ein Windows-only-Beispiel wenig hilfreich. Die gute Nachricht: P/Invoke selbst ist nicht an Windows gebunden. Was sich unterscheidet, sind lediglich die Namen der Bibliotheken und die Werkzeuge, mit denen man eigene native Bibliotheken baut. Dieses Modul zeigt beides – und wie man eine `DllNotFoundException` liest, wenn der Lader die Bibliothek nicht findet.

## Bibliotheksnamen sind Plattformsache

Die C-Standardbibliothek gibt es überall, aber sie heißt überall anders. Unter macOS und Linux ist sie als `libc` bekannt, unter Windows liegt sie in `msvcrt.dll` (die klassische Microsoft C Runtime). Für die Mathematikfunktionen gilt dasselbe: `libm` auf Unix-Systemen, `ucrtbase.dll` unter Windows.

| Was | macOS | Linux | Windows |
| :--- | :--- | :--- | :--- |
| C-Standardbibliothek | `libc` (→ `libc.dylib`) | `libc` (→ `libc.so.6`) | `msvcrt` |
| Mathematik (`cos`, `sqrt`) | `libm` | `libm.so.6` | `ucrtbase` |
| Fensterfunktionen | Cocoa (nicht in C) | X11/Wayland | `user32.dll` |
| eigene Bibliothek `mathe` | `libmathe.dylib` | `libmathe.so` | `mathe.dll` |

Der einfachste Umgang damit: zwei Klassen mit denselben Signaturen, und zur Laufzeit wählt `OperatingSystem.IsWindows()` die passende.

```csharp
static class LibcUnix
{
    [DllImport("libc")] public static extern int strlen(string s);
    [DllImport("libc")] public static extern int getpid();
}

static class LibcWindows
{
    [DllImport("msvcrt")] public static extern int strlen(string s);
    [DllImport("msvcrt", EntryPoint = "_getpid")] public static extern int getpid();
}
```

Bei `getpid` zeigt sich, wofür `EntryPoint` gut ist: Die Windows-Variante heißt `_getpid`, soll aus C#-Sicht aber genauso aufgerufen werden wie unter Unix. Der Aufruf sieht dann so aus:

```csharp
string text = "Hallo P/Invoke";
int laenge = OperatingSystem.IsWindows() ? LibcWindows.strlen(text) : LibcUnix.strlen(text);
Console.WriteLine(laenge);               // 14
Console.WriteLine(Environment.ProcessId); // dieselbe Zahl wie getpid()
```

`strlen` zählt Bytes bis zum Nullbyte, nicht Zeichen. Für „Hallo P/Invoke“ ist das egal, für „Grüße“ liefert `strlen` unter macOS und Linux 7 (UTF-8 braucht für ü und ß je zwei Bytes), `"Grüße".Length` aber 5. Wer solche Werte vergleicht, muss wissen, in welcher Kodierung der Marshaller den String übergeben hat.

Für `cos` aus `libm` gibt es eine elegantere Lösung als zwei Klassen: Mit `NativeLibrary.SetDllImportResolver` sagt man dem Lader einmalig, wie ein Bibliotheksname aufzulösen ist, und die Deklaration bleibt bei `[DllImport("libm")]`. Wie das aussieht, zeigt Abschnitt 2 des Beispielprojekts.
{: .notice--primary}

## Eine eigene C-Bibliothek

Fremde Bibliotheken zu rufen ist eine Sache – aber erst wenn man beide Seiten selbst schreibt, sieht man den ganzen Weg. Die Datei `mathe.c` enthält drei kleine Funktionen:

```c
#include <string.h>
#include <math.h>

#ifdef _WIN32
#define EXPORT __declspec(dllexport)
#else
#define EXPORT
#endif

EXPORT int addiere(int a, int b)            { return a + b; }
EXPORT double vektorlaenge(double x, double y) { return sqrt(x * x + y * y); }
EXPORT int laenge(const char* s)            { return (int)strlen(s); }
```

Das Makro `EXPORT` ist die einzige Zeile, die mit Windows zu tun hat: Dort muss jede Funktion, die von außen sichtbar sein soll, mit `__declspec(dllexport)` markiert werden. Unter macOS und Linux sind Funktionen standardmäßig exportiert, deshalb ist das Makro dort leer. Aus dieser Quelle wird mit einem Befehl eine dynamische Bibliothek:

```bash
# macOS (clang kommt mit den Xcode Command Line Tools)
clang -shared -o libmathe.dylib mathe.c

# Linux (gcc, z. B. über build-essential)
gcc -shared -fPIC -o libmathe.so mathe.c -lm

# Windows, Developer PowerShell (MSVC) – oder gcc aus MinGW-w64
cl /LD mathe.c
```

`-shared` bzw. `/LD` erzeugt eine dynamische Bibliothek statt eines Programms, `-fPIC` macht den Code unter Linux positionsunabhängig (Pflicht für `.so`-Dateien), und `-lm` bindet die Mathebibliothek für `sqrt` ein. Das Ergebnis liegt neben der Quelldatei und muss beim Start neben der Programmdatei liegen – dazu gleich mehr.

## Aufruf aus C#

Auf der C#-Seite genügt eine Klasse mit drei Deklarationen. Der Bibliotheksname ist bewusst nur `mathe` – ohne Präfix `lib`, ohne Endung:

```csharp
static class Mathe
{
    [DllImport("mathe")] public static extern int addiere(int a, int b);
    [DllImport("mathe")] public static extern double vektorlaenge(double x, double y);
    [DllImport("mathe")] public static extern int laenge(string s);
}

Console.WriteLine(Mathe.addiere(2, 40));      // 42
Console.WriteLine(Mathe.vektorlaenge(3, 4));  // 5
Console.WriteLine(Mathe.laenge("Grüße"));      // 7 (macOS/Linux, UTF-8)
```

Dass dieselbe Deklaration auf allen drei Systemen funktioniert, liegt am Lader der .NET-Laufzeit: Er nimmt den Namen `mathe` und probiert die plattformüblichen Varianten durch – `mathe.dylib`, `libmathe.dylib`, `mathe`, `libmathe` unter macOS; `mathe.so`, `libmathe.so` unter Linux; `mathe.dll` unter Windows. Wer den vollen Dateinamen `libmathe.dylib` in das Attribut schreibt, verbaut sich diese Flexibilität.

Damit die Bibliothek beim Start neben `PInvokeDemo.dll` liegt, kopiert die Projektdatei sie in den Ausgabeordner. Der Platzhalter sorgt dafür, dass der Build auch dann klappt, wenn die Bibliothek noch nicht gebaut wurde:

```xml
<ItemGroup>
  <None Include="native/*.dylib;native/*.so;native/*.dll"
        Link="%(Filename)%(Extension)"
        CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

Das vollständige Projekt findest du im Repository unter `examples/10_pinvoke/PInvokeDemo` – mit `native/build-native.sh` für macOS und Linux sowie `native/build-native.ps1` für Windows.

## Wo der Lader sucht – und die `DllNotFoundException`

Findet der Lader die Bibliothek nicht, wirft der erste Aufruf eine `DllNotFoundException`. Die Meldung ist lang, aber lehrreich, denn sie listet jeden probierten Pfad auf:

```
Unable to load shared library 'mathe' or one of its dependencies. ...
dlopen(/Users/.../bin/Debug/net10.0/mathe.dylib, 0x0001): ... (no such file)
dlopen(/Users/.../bin/Debug/net10.0/libmathe.dylib, 0x0001): ... (no such file)
dlopen(libmathe.dylib, 0x0001): tried: 'libmathe.dylib', '/usr/lib/libmathe.dylib' ...
```

Daraus lässt sich die Suchreihenfolge ablesen: zuerst das Verzeichnis der Anwendung (`bin/Debug/net10.0/`), dann die Standardorte des Betriebssystems – `PATH` unter Windows, `LD_LIBRARY_PATH` und die `ldconfig`-Verzeichnisse unter Linux, `DYLD_LIBRARY_PATH` und `/usr/lib` unter macOS. Die drei häufigsten Ursachen für die Exception sind: Die Bibliothek wurde nicht gebaut, sie liegt nicht im Ausgabeordner, oder sie wurde für die falsche Architektur übersetzt (etwa x64 statt Apple Silicon). Der Zusatz „or one of its dependencies“ verrät den vierten Fall: Die Bibliothek selbst ist da, braucht aber eine weitere, die fehlt.

Verwechsle die `DllNotFoundException` nicht mit der `EntryPointNotFoundException`. Erstere heißt „Bibliothek nicht gefunden“, letztere „Bibliothek gefunden, aber darin keine Funktion mit diesem Namen“ – typisch für Tippfehler, falsche Groß-/Kleinschreibung oder ein vergessenes `__declspec(dllexport)` unter Windows.
{: .notice--warning}

Übung: Ergänze `mathe.c` um `int maximum(const int* werte, int n)`, baue die Bibliothek neu und rufe die Funktion aus C# mit einem `int[]` auf. Was passiert, wenn du `n` größer als die Array-Länge angibst – und warum gibt es dafür keine Exception?
{: .notice--info}

## Weitere Quellen

- [Laden nativer Bibliotheken – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/native-interop/native-library-loading)
- [Plattformübergreifendes P/Invoke – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/native-interop/cross-platform)
- [NativeLibrary-Klasse – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.runtime.interopservices.nativelibrary)
- [pinvoke.net](https://www.pinvoke.net/) – zeigt an hunderten Beispielen, wie stark sich Deklarationen je nach Bibliothek und Plattform unterscheiden.
