---
title: "P/Invoke mit DllImport"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Wie ruft man eine Funktion auf, deren Code nicht in C# vorliegt? Man müsste dem Compiler zumindest sagen, wie sie heißt, welche Parameter sie erwartet und wo sie zu finden ist – so wie ein Telefonbucheintrag: Name, Nummer, Adresse. Genau das leistet das Attribut `[DllImport]`. Man schreibt eine Methodendeklaration ohne Rumpf, hängt das Attribut mit dem Bibliotheksnamen davor, und ab dann lässt sich die native Funktion aufrufen, als wäre sie eine ganz normale statische Methode. Dieses Modul zeigt den Mechanismus am klassischen Beispiel der Windows-Nachrichtenbox und erklärt, was der Marshaller dabei mit den Parametern anstellt.

## Die erste Deklaration

Die Windows-Bibliothek `user32.dll` enthält die C-Funktion `MessageBoxW`, die ein kleines Dialogfenster anzeigt. In der Dokumentation steht ihre Signatur in C:

```c
int MessageBoxW(HWND hWnd, LPCWSTR lpText, LPCWSTR lpCaption, UINT uType);
```

Die entsprechende C#-Deklaration sieht so aus:

```csharp
using System.Runtime.InteropServices;

static class User32
{
    [DllImport("user32.dll")]
    public static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
```

Drei Dinge fallen auf. Erstens hat die Methode keinen Rumpf, sondern endet mit einem Semikolon – das Schlüsselwort `extern` sagt dem Compiler, dass die Implementierung von außerhalb kommt. Zweitens ist sie `static`: Eine C-Funktion gehört zu keinem Objekt, also gibt es auch kein `this`. Drittens haben die Parameter C#-Typen: Aus `HWND` (ein Fensterhandle, im Grunde ein Zeiger) wird `IntPtr`, aus `LPCWSTR` (Zeiger auf einen Unicode-String) wird `string`, aus `UINT` wird `uint`.

Der Aufruf unterscheidet sich dann nicht von einer normalen statischen Methode:

```csharp
if (OperatingSystem.IsWindows())
{
    int ergebnis = User32.MessageBoxW(IntPtr.Zero, "Gruß aus C#!", "P/Invoke", 1);
    Console.WriteLine(ergebnis); // 1 (OK) oder 2 (Abbrechen)
}
```

Die Prüfung `OperatingSystem.IsWindows()` ist kein Schönheitsdetail: `user32.dll` existiert nur unter Windows. Auf einem Mac würde der Aufruf mit einer `DllNotFoundException` scheitern – und zwar erst beim Aufruf, nicht beim Kompilieren, denn der Compiler prüft nur die C#-Seite der Deklaration.

Das Kompilieren einer `[DllImport]`-Deklaration beweist nichts. Ob die Bibliothek existiert, die Funktion so heißt und die Parameter stimmen, zeigt sich erst zur Laufzeit beim ersten Aufruf.
{: .notice--warning}

## Die Parameter des Attributs

Der Bibliotheksname ist das einzige Pflichtargument. Daneben gibt es benannte Parameter, mit denen man Abweichungen zwischen C-Signatur und C#-Deklaration ausgleicht:

```csharp
[DllImport("user32.dll",
    EntryPoint = "MessageBoxW",          // Name in der Bibliothek, falls er anders lauten soll
    CharSet = CharSet.Unicode,           // wie Strings übergeben werden
    CallingConvention = CallingConvention.Winapi,   // Aufrufkonvention der C-Funktion
    SetLastError = true)]                // Fehlercode nach dem Aufruf sichern
public static extern int Nachrichtenbox(IntPtr hWnd, string text, string caption, uint type);
```

- **`EntryPoint`** entkoppelt den C#-Namen vom nativen Namen. So kann die Methode `Nachrichtenbox` heißen und trotzdem `MessageBoxW` rufen – nützlich, wenn man C#-Namenskonventionen einhalten oder Namen mit Unterstrich wie `_getpid` verstecken will.
- **`CharSet`** legt fest, ob `string`-Parameter als 8-Bit-Zeichen (`Ansi`) oder als 16-Bit-Unicode (`Unicode`) übergeben werden. Der Standardwert ist `Ansi`. Unter Windows enthalten viele DLLs beide Varianten einer Funktion, erkennbar am Suffix `A` bzw. `W`.
- **`CallingConvention`** beschreibt, wie Parameter auf dem Stack übergeben und wer sie wieder entfernt. `Winapi` (der Standard) bedeutet: die Konvention, die das jeweilige Betriebssystem für seine API benutzt. Auf 64-Bit-Systemen gibt es praktisch nur noch eine Konvention; wichtig wird der Parameter bei alten 32-Bit-Windows-Bibliotheken, wo `Cdecl` und `StdCall` verschieden sind.
- **`SetLastError`** sorgt dafür, dass der native Fehlercode direkt nach dem Aufruf gesichert wird, sodass man ihn mit `Marshal.GetLastPInvokeError()` abfragen kann – die Laufzeit selbst könnte ihn sonst zwischendurch überschreiben.

In den meisten Fällen braucht man nur den Bibliotheksnamen und gelegentlich `CharSet`. Die anderen Parameter setzt man erst, wenn etwas nicht funktioniert und die Dokumentation der Bibliothek einen Grund liefert.

## Typabbildung zwischen C und C#

Der Marshaller kann Parameter nur übersetzen, wenn beide Seiten zusammenpassen. Für die häufigsten C-Typen gilt diese Abbildung:

| C-Typ | C#-Typ | Bemerkung |
| :--- | :--- | :--- |
| `int`, `int32_t` | `int` | blittable, keine Umwandlung |
| `unsigned int`, `UINT` | `uint` | |
| `short`, `unsigned short` | `short`, `ushort` | |
| `long long`, `int64_t` | `long` | |
| `long` | `int` oder `nint` | Windows: 32 Bit, macOS/Linux: 64 Bit – Vorsicht! |
| `size_t` | `nuint` | Breite des Zeigers |
| `float`, `double` | `float`, `double` | |
| `char` (als Zahl) | `sbyte` oder `byte` | |
| `bool` (C99 `_Bool`) | `bool` mit `[MarshalAs(UnmanagedType.I1)]` | Standard wäre 4 Byte (Win32 `BOOL`) |
| `const char*` | `string` | nullterminierte Zeichenkette |
| `int*` als Array | `int[]` | Zeiger auf das erste Element |
| `int*` als Ausgabe | `out int` oder `ref int` | Zeiger auf eine einzelne Variable |
| `void*`, Handles | `IntPtr` oder `nint` | undurchsichtiger Zeiger |
| `struct` nach Wert | `struct` mit `[StructLayout(LayoutKind.Sequential)]` | Felder in derselben Reihenfolge |

Die Zeile mit `long` ist die klassische Falle: Ein C-`long` ist unter Windows 32 Bit breit, unter macOS und Linux 64 Bit. Wer ihn blind als C#-`long` deklariert, liest unter Windows Speichermüll. Seriöse C-Bibliotheken benutzen deshalb `int32_t` und `int64_t` – und wo sie das nicht tun, muss man nachschlagen.

## Strings und `MarshalAs`

Für Strings reicht die Typabbildung allein nicht aus, weil ein C-`char*` mehrere Kodierungen haben kann. Statt `CharSet` für die ganze Deklaration kann man jeden Parameter einzeln mit `[MarshalAs]` beschreiben:

```csharp
[DllImport("user32.dll")]
public static extern int MessageBoxA(IntPtr hWnd,
    [MarshalAs(UnmanagedType.LPStr)] string text,
    [MarshalAs(UnmanagedType.LPStr)] string caption,
    uint type);
```

`UnmanagedType.LPStr` steht für einen Zeiger auf 8-Bit-Zeichen, `LPWStr` für 16-Bit-Unicode, `LPUTF8Str` für UTF-8. Bei einem Aufruf passiert im Hintergrund eine Menge: Der Marshaller reserviert nativen Speicher, kopiert den Inhalt des C#-Strings in der gewünschten Kodierung hinein, hängt das Nullbyte an, übergibt den Zeiger, wartet auf die Rückkehr und gibt den Speicher wieder frei. Das C#-Objekt selbst bleibt unangetastet auf dem verwalteten Heap – die C-Funktion sieht nur die Kopie.

Umgekehrt gilt: Erwartet eine C-Funktion einen Puffer, den *sie* füllt (etwa `void name(char* puffer, int groesse)`), passt ein unveränderlicher `string` nicht. Dafür nimmt man ein `byte[]`, das der Marshaller für die Dauer des Aufrufs an seinem Platz festhält (*pinning*), und wandelt es danach mit `Encoding.UTF8.GetString` um.

Bei Strings immer die Dokumentation der C-Funktion lesen: Ist der Parameter `const char*` (nur lesen, `string` reicht) oder `char*` (die Funktion schreibt hinein, Puffer nötig)? Und welche Kodierung erwartet sie? Der Standard `CharSet.Ansi` bedeutet unter Windows die System-Codepage, unter macOS und Linux UTF-8 – „Grüße“ ist dann einmal 5 und einmal 7 Bytes lang.
{: .notice--primary}

Übung: Die C-Funktion `int GetSystemMetrics(int nIndex)` aus `user32.dll` liefert für `nIndex = 0` die Bildschirmbreite in Pixeln und für `1` die Höhe. Schreibe die `[DllImport]`-Deklaration und einen Aufruf, der unter anderen Betriebssystemen nicht abstürzt, sondern eine Meldung ausgibt.
{: .notice--info}

## Weitere Quellen

- [Platform Invoke (P/Invoke) – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/native-interop/pinvoke)
- [DllImportAttribute – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.runtime.interopservices.dllimportattribute)
- [Marshalling von Zeichenfolgen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/native-interop/charset)
