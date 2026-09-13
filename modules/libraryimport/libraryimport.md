---
title: "LibraryImport – P/Invoke mit Source-Generator"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

`[DllImport]` funktioniert seit der ersten .NET-Version und wird auch nicht verschwinden. Es hat aber eine Eigenheit, die man erst bemerkt, wenn man genauer hinschaut: Der Marshalling-Code – also alles, was Strings kopiert, Arrays festhält und Rückgabewerte umwandelt – wird erst **zur Laufzeit** erzeugt, beim ersten Aufruf jeder Funktion. Das kostet Zeit, verbirgt Fehler bis zum Aufruf und funktioniert nicht, wenn das Programm vorab vollständig in Maschinencode übersetzt wird (*Native AOT*). Seit .NET 7 gibt es deshalb `[LibraryImport]`: Ein **Source-Generator** schreibt den Marshalling-Code schon beim Kompilieren als ganz normalen C#-Code ins Projekt. Man kann ihn lesen, debuggen – und der Compiler meckert, wenn etwas nicht zusammenpasst.

## Die Syntax

Die Deklaration sieht auf den ersten Blick fast gleich aus, unterscheidet sich aber in drei Schlüsselwörtern:

```csharp
using System.Runtime.InteropServices;

static partial class Mathe
{
    [LibraryImport("mathe", EntryPoint = "addiere")]
    public static partial int Addiere(int a, int b);

    [LibraryImport("mathe", EntryPoint = "vektorlaenge")]
    public static partial double Vektorlaenge(double x, double y);

    [LibraryImport("mathe", EntryPoint = "laenge", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int Laenge(string s);
}
```

Aus `extern` wird `partial`, und die Klasse muss ebenfalls `partial` sein. Der Grund: Ein Source-Generator kann bestehende Dateien nicht ändern, sondern nur neue hinzufügen. Er erzeugt eine zweite Datei mit demselben Klassennamen und liefert darin den fehlenden Rumpf der `partial`-Methode nach – genau der Mechanismus, der auch hinter Blazor steckt, wo der Compiler aus jeder `.razor`-Datei eine `partial class` erzeugt, die sich mit einer Code-Behind-Datei `.razor.cs` ergänzen lässt. `EntryPoint` erlaubt es, die C#-Methode in PascalCase zu benennen, obwohl die C-Funktion klein geschrieben ist.

Der Aufruf ist von `[DllImport]` nicht zu unterscheiden:

```csharp
Console.WriteLine(Mathe.Addiere(2, 40));       // 42
Console.WriteLine(Mathe.Vektorlaenge(3, 4));   // 5
Console.WriteLine(Mathe.Laenge("Grüße"));       // 7
```

## Strings brauchen eine Entscheidung

Der auffälligste Unterschied steckt in der dritten Deklaration: `StringMarshalling = StringMarshalling.Utf8`. Bei `[DllImport]` durfte man `CharSet` weglassen und bekam stillschweigend `Ansi` – mit unterschiedlicher Bedeutung je nach Betriebssystem. `[LibraryImport]` zwingt zur Entscheidung: Ohne `StringMarshalling` (oder einen eigenen Marshaller) gibt es einen **Kompilierfehler**, sobald ein `string` in der Signatur auftaucht. Zur Wahl stehen `Utf8`, `Utf16` und `Custom`. Für C-Bibliotheken unter macOS und Linux ist `Utf8` fast immer richtig, für die `W`-Funktionen der Windows-API `Utf16`.

Dasselbe Prinzip gilt für `bool`: Da ein C-`bool` ein Byte breit ist, ein Win32-`BOOL` aber vier, verlangt der Generator ein explizites `[MarshalAs(UnmanagedType.I1)]` bzw. `[MarshalAs(UnmanagedType.Bool)]`. Was bei `[DllImport]` eine unsichtbare Annahme war, wird hier zur sichtbaren Angabe.

`[LibraryImport]` nimmt dir keine Arbeit ab – es zwingt dich, die Arbeit **vor** dem Kompilieren zu erledigen. Jeder Fehler, den es anzeigt, wäre bei `[DllImport]` ein falsches Ergebnis zur Laufzeit gewesen.
{: .notice--primary}

## `AllowUnsafeBlocks` und der erzeugte Code

Der Generator schreibt Code mit Zeigern, weil er Strings selbst in nativen Speicher kopiert und Arrays selbst festhält. C# erlaubt Zeiger nur in Projekten, die es ausdrücklich gestatten. Deshalb braucht die Projektdatei eine Zeile:

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
</PropertyGroup>
```

Fehlt sie, meldet der Compiler den Fehler `SYSLIB1062` mit dem Hinweis auf genau diese Option. Was der Generator erzeugt, kann man sich anschauen – in Visual Studio und Rider unter „Abhängigkeiten → Analyzers“, in VS Code über *Go to Definition* auf der Methode. Für `Laenge` sieht der erzeugte Rumpf (gekürzt) so aus:

```csharp
public static partial int Laenge(string s)
{
    byte* __s_native = default;
    int __retVal = default;
    Utf8StringMarshaller.ManagedToUnmanagedIn __s_native__marshaller = new();
    try
    {
        __s_native__marshaller.FromManaged(s, stackalloc byte[...]);  // UTF-8 kopieren + Nullbyte
        __s_native = __s_native__marshaller.ToUnmanaged();
        __retVal = __PInvoke(__s_native);                              // eigentlicher Aufruf
    }
    finally
    {
        __s_native__marshaller.Free();                                 // nativen Speicher freigeben
    }
    return __retVal;

    [DllImport("mathe", EntryPoint = "laenge", ExactSpelling = true)]
    static extern unsafe int __PInvoke(byte* __s_native);
}
```

Hier wird sichtbar, was im Modul [Verwalteter und nativer Code](/modules/interop_ueberblick/interop_ueberblick.md) nur beschrieben war: Der String wird konvertiert, der Aufruf abgesetzt, der Speicher im `finally` wieder freigegeben. Und ganz unten steckt ein `[DllImport]` – allerdings nur noch mit einem `byte*`, also einem blittable Typ, für den die Laufzeit keinen eigenen Marshalling-Code mehr erzeugen muss. `[LibraryImport]` ersetzt `[DllImport]` also nicht, sondern übernimmt dessen schwierigen Teil.

## `DllImport` und `LibraryImport` im Vergleich

| | `[DllImport]` | `[LibraryImport]` |
| :--- | :--- | :--- |
| Verfügbar seit | .NET Framework 1.0 | .NET 7 |
| Deklaration | `static extern` in beliebiger Klasse | `static partial` in `partial class` |
| Marshalling-Code | zur Laufzeit erzeugt (IL-Stub) | beim Kompilieren erzeugt, einsehbar |
| Strings | `CharSet`, Standard `Ansi` | `StringMarshalling` Pflicht |
| `bool` | stillschweigend 4 Byte | `[MarshalAs]` Pflicht |
| Fehlerhafte Signatur | fällt zur Laufzeit auf (oder gar nicht) | teilweise Kompilierfehler |
| Native AOT, Trimming | eingeschränkt | vollständig unterstützt |
| Projektoption | keine | `AllowUnsafeBlocks` |
| Erster Aufruf | langsamer (Stub wird gebaut) | keine Verzögerung |

Aus der Tabelle ergibt sich die Faustregel: Neuer Code auf .NET 7 oder neuer nutzt `[LibraryImport]`. `[DllImport]` bleibt wichtig, weil man es in praktisch jedem bestehenden Projekt, jeder Antwort im Netz und in Umgebungen wie Unity oder .NET Framework antrifft – lesen können muss man beides. Wer sich unsicher ist, kann auch mit `[DllImport]` beginnen: Der Analyzer `SYSLIB1054` schlägt die Umstellung vor und erledigt sie in der IDE per Schnellkorrektur.

Der Source-Generator verarbeitet nur Typen, für die er das Marshalling kennt: Zahlen, Strings, Arrays, Strukturen mit `[StructLayout]`, `SafeHandle`. Für alles andere – etwa Delegaten als Callback oder komplexe Strukturen mit Zeigern – braucht man einen eigenen Marshaller oder greift auf `[DllImport]` zurück.
{: .notice--warning}

Übung: Schreibe die beiden Klassen `LibcUnix` und `LibcWindows` aus dem Modul [P/Invoke plattformübergreifend](/modules/pinvoke_crossplatform/pinvoke_crossplatform.md) mit `[LibraryImport]` um. Welche Deklarationen ändern sich nur im Attribut, wo musst du eine zusätzliche Entscheidung treffen?
{: .notice--info}

## Weitere Quellen

- [Quellgenerierung für P/Invoke – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/native-interop/pinvoke-source-generation)
- [LibraryImportAttribute – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.runtime.interopservices.libraryimportattribute)
- [Native AOT-Bereitstellung – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/core/deploying/native-aot/)
- [SharpLab](https://sharplab.io/) – macht sichtbar, was der Compiler aus deinem C#-Code wirklich erzeugt: derselbe Blick hinter die Kulissen wie beim Source-Generator oben.
