---
title: "🧩 Aufgaben und Beispiele: Native Bibliotheken"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Programmieren lernt man nicht nur durch Codezeilen tippen – sondern auch durch **Nachdenken**. Die folgenden Aufgaben trainieren *Computational Thinking* an der Grenze zwischen C und C#: C-Signaturen systematisch übersetzen, Fehlermeldungen des Laders lesen, eine native Ressource sicher in eine Klasse einpacken und native Aufrufe hinter einem Interface verstecken. Für die meisten Aufgaben brauchst du keinen C-Compiler, nur Papier und die Module dieser Vorlesung. Nimm dir für jede Aufgabe Zeit, bevor du die Lösung aufklappst.

## Aufgabe 1 — Mustererkennung

Eine Header-Datei einer C-Bibliothek `statistik` enthält diese Deklarationen:

```c
int    summe(const int* werte, int n);
double mittelwert(const double* werte, size_t n);
bool   ist_sortiert(const int* werte, int n);
void   beschreibung(char* puffer, int groesse);
int    minmax(const int* werte, int n, int* minimum, int* maximum);
```

Übersetze jede Zeile in eine `[DllImport]`-Deklaration. Überlege für jeden Parameter:
- Zeigt der Zeiger auf ein Array oder auf eine einzelne Variable? Liest die Funktion nur, oder schreibt sie hinein?
- Welche Breite hat `size_t`? Welche hat ein C-`bool` – und welche nimmt der Marshaller standardmäßig an?
- Welcher C#-Typ passt zu einem Puffer, den die C-Funktion füllt?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Zeiger auf Arrays:**

`const int* werte` mit einer Länge `n` daneben ist das C-Muster für „Array, nur lesen“. In C# wird daraus `int[]`: Der Marshaller hält das Array für die Dauer des Aufrufs fest und übergibt die Adresse des ersten Elements – ohne Kopie, weil `int` blittable ist.

**Schritt 2 — `size_t` und `bool`:**

`size_t` ist so breit wie ein Zeiger, in C# also `nuint`. Ein C-`bool` belegt ein Byte, der Marshaller nimmt bei `bool` aber standardmäßig das vier Byte breite Win32-`BOOL` an – ohne `[MarshalAs]` liest C# drei Bytes Speichermüll mit.

**Schritt 3 — Puffer und Ausgabeparameter:**

`char* puffer` ohne `const` heißt: Die Funktion schreibt hinein. Ein unveränderlicher `string` passt nicht, ein `byte[]` schon. `int* minimum` ohne Längenangabe ist kein Array, sondern eine einzelne Ausgabevariable – dafür gibt es `out`.

```csharp
[DllImport("statistik")] static extern int summe(int[] werte, int n);
[DllImport("statistik")] static extern double mittelwert(double[] werte, nuint n);

[DllImport("statistik")]
[return: MarshalAs(UnmanagedType.I1)]
static extern bool ist_sortiert(int[] werte, int n);

[DllImport("statistik")] static extern void beschreibung(byte[] puffer, int groesse);
[DllImport("statistik")] static extern int minmax(int[] werte, int n, out int minimum, out int maximum);

byte[] puffer = new byte[256];
beschreibung(puffer, puffer.Length);
string text = Encoding.UTF8.GetString(puffer).TrimEnd('\0');
```

**Zentrale Designentscheidungen:**

- **Das Muster „Zeiger + Länge“ erkennen:** Ein Zeiger mit einer Zahl daneben ist fast immer ein Array, ein Zeiger allein fast immer eine Ausgabe. C sagt es nicht explizit – man liest es aus Namen und Dokumentation.
- **`const` als Richtungsangabe:** `const` bedeutet „nur lesen“, also reicht `string` oder `int[]`. Ohne `const` muss auf der C#-Seite etwas Beschreibbares stehen.
- **Bei `bool` nie dem Standard vertrauen:** Die Vier-Byte-Annahme stammt aus der Win32-Welt. `[LibraryImport]` würde die Deklaration ohne `[MarshalAs]` gar nicht erst übersetzen.

</details>

## Aufgabe 2 — Algorithmenentwurf

Drei Studierende haben P/Invoke-Deklarationen für die Bibliothek `mathe` aus dem Beispielprojekt geschrieben. Jede kompiliert fehlerfrei, jede scheitert zur Laufzeit – aber jeweils anders.

```csharp
// (a) Auf dem Mac der Autorin läuft es, auf dem Linux-Rechner des Tutors nicht.
[DllImport("libmathe.dylib")] static extern int addiere(int a, int b);

// (b) Wirft beim ersten Aufruf eine Exception, obwohl die Bibliothek gefunden wird.
[DllImport("mathe")] static extern int Addiere(int a, int b);

// (c) Auf 64-Bit-Systemen unauffällig, auf einem alten 32-Bit-Windows Speicherfehler.
[DllImport("mathe", CallingConvention = CallingConvention.StdCall)]
static extern double vektorlaenge(double x, double y);
```

Entwickle ein Prüfschema, mit dem du bei einer fehlschlagenden P/Invoke-Deklaration die Ursache eingrenzt:
- Welche Exception verrät, ob das Problem beim Bibliotheksnamen, beim Funktionsnamen oder bei der Signatur liegt?
- In welcher Reihenfolge prüft man am sinnvollsten?
- Welche Fehlerklasse zeigt sich gar nicht als Exception?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die drei Stufen des Ladens:**

| Stufe | Frage | Fehlerbild |
| :--- | :--- | :--- |
| 1. Bibliothek laden | Gibt es eine Datei mit passendem Namen im Suchpfad? | `DllNotFoundException` |
| 2. Funktion suchen | Exportiert die Bibliothek eine Funktion mit genau diesem Namen? | `EntryPointNotFoundException` |
| 3. Aufrufen | Passen Parametertypen, Breite und Aufrufkonvention? | falsche Werte, Absturz, keine Exception |

Das Prüfschema folgt dieser Reihenfolge: Erst den Typ der Exception lesen, dann die betroffene Stufe untersuchen, und erst wenn Stufe 1 und 2 sauber sind, die Signatur gegen die Header-Datei vergleichen.

**Schritt 2 — Fall (a), `DllNotFoundException` unter Linux:**

`libmathe.dylib` ist ein macOS-Dateiname; unter Linux probiert der Lader Varianten wie `libmathe.dylib.so` und findet nichts. Die Lösung ist der plattformneutrale Name `"mathe"`, aus dem der Lader selbst `libmathe.dylib`, `libmathe.so` oder `mathe.dll` macht.

**Schritt 3 — Fall (b), `EntryPointNotFoundException`:**

C unterscheidet Groß- und Kleinschreibung: Exportiert wird `addiere`, gesucht wird `Addiere`. Wer den C#-Namen groß schreiben will, nutzt `EntryPoint = "addiere"`. Dieselbe Exception erscheint unter Windows, wenn `__declspec(dllexport)` fehlt.

**Schritt 4 — Fall (c), falsche Aufrufkonvention:**

Eine gewöhnliche C-Funktion ist `Cdecl`: Der Aufrufer räumt den Stack auf. Bei `StdCall` soll es die gerufene Funktion tun. Auf 32-Bit-Windows räumt so niemand oder jeder zweimal auf – der Stack gerät durcheinander, die Symptome reichen von falschen Rückgabewerten bis zum Absturz an einer scheinbar unbeteiligten Stelle. Auf 64-Bit-Systemen gibt es nur eine Konvention, deshalb fällt der Fehler dort nicht auf. Richtig ist `Cdecl` oder einfach der Standard.

**Zentrale Designentscheidungen:**

- **Exception-Typ vor Meldungstext:** Der Typ nennt die Stufe, die Meldung erst die Details.
- **Stufe 3 ist die gefährlichste:** Sie erzeugt keine Exception. Deshalb gehört zu jeder P/Invoke-Deklaration ein Test mit bekannten Werten.
- **Plattformabhängige Fehler brauchen plattformübergreifende Tests:** Fall (a) und (c) fallen nur auf der jeweils anderen Plattform auf – ein Argument für einen CI-Lauf auf mehreren Betriebssystemen.

</details>

## Aufgabe 3 — Zerlegung

Eine C-Bibliothek `zaehler` verwaltet einen Zähler als undurchsichtige Ressource:

```c
void* zaehler_erzeugen(void);          // legt Speicher mit malloc an
void  zaehler_erhoehen(void* z);
int   zaehler_wert(void* z);
void  zaehler_freigeben(void* z);      // gibt den Speicher mit free zurück
```

Entwirf eine C#-Klasse `NativerZaehler`, die diese vier Funktionen kapselt, sodass der Rest des Programms weder `IntPtr` noch `[LibraryImport]` zu sehen bekommt. Zerlege das Problem:
- Wo lebt der Zeiger, wer ruft `zaehler_erzeugen`, wer `zaehler_freigeben`?
- Was passiert, wenn jemand `zaehler_freigeben` vergisst – und was, wenn es zweimal aufgerufen wird?
- Welches Muster aus Vorlesung 09 passt hier?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Die native Schicht isolieren:**

Die vier Deklarationen wandern in eine private, verschachtelte Klasse. Damit ist die einzige Stelle, die vom nativen Code weiß, auch die einzige, die sich ändern muss, wenn sich die C-Seite ändert.

**Schritt 2 — Lebensdauer an ein Objekt binden:**

Der Zeiger aus `zaehler_erzeugen` ist eine Ressource wie ein Dateihandle: Er muss genau einmal freigegeben werden. Das ist exakt der Fall für `IDisposable` aus dem Modul [`IDisposable` und `using`](/modules/idisposable_using/idisposable_using.md). Der Konstruktor holt die Ressource, `Dispose` gibt sie zurück.

```csharp
class NativerZaehler : IDisposable
{
    private static partial class Nativ
    {
        [LibraryImport("zaehler", EntryPoint = "zaehler_erzeugen")]  public static partial IntPtr Erzeugen();
        [LibraryImport("zaehler", EntryPoint = "zaehler_erhoehen")]  public static partial void Erhoehen(IntPtr z);
        [LibraryImport("zaehler", EntryPoint = "zaehler_wert")]      public static partial int Wert(IntPtr z);
        [LibraryImport("zaehler", EntryPoint = "zaehler_freigeben")] public static partial void Freigeben(IntPtr z);
    }

    private IntPtr handle;

    public NativerZaehler()
    {
        handle = Nativ.Erzeugen();
        if (handle == IntPtr.Zero)
            throw new InvalidOperationException("Zähler konnte nicht angelegt werden.");
    }

    public void Erhoehen()
    {
        ObjectDisposedException.ThrowIf(handle == IntPtr.Zero, this);
        Nativ.Erhoehen(handle);
    }

    public int Wert
    {
        get
        {
            ObjectDisposedException.ThrowIf(handle == IntPtr.Zero, this);
            return Nativ.Wert(handle);
        }
    }

    public void Dispose()
    {
        if (handle != IntPtr.Zero)
        {
            Nativ.Freigeben(handle);
            handle = IntPtr.Zero;      // schützt vor doppeltem Freigeben
        }
        GC.SuppressFinalize(this);
    }

    ~NativerZaehler() => Dispose();    // Sicherheitsnetz, falls using vergessen wurde
}

using (NativerZaehler zaehler = new NativerZaehler())
{
    zaehler.Erhoehen();
    zaehler.Erhoehen();
    Console.WriteLine(zaehler.Wert); // 2
}                                    // hier wird zaehler_freigeben gerufen
```

**Zentrale Designentscheidungen:**

- **`IntPtr.Zero` als „bereits freigegeben“:** Das Feld ist Handle und Zustandsmerker zugleich. Ein zweites `Dispose` tut nichts, ein Aufruf nach `Dispose` wirft eine saubere `ObjectDisposedException`, statt in C mit einem ungültigen Zeiger abzustürzen.
- **Finalizer als Notbremse, nicht als Plan:** Der Garbage Collector weiß nichts vom nativen Speicher und räumt irgendwann oder nie auf. `using` bleibt Pflicht; der Finalizer fängt nur Vergesslichkeit ab.
- **Der professionelle Weg heißt `SafeHandle`:** .NET bringt eine Basisklasse mit, die genau dieses Muster inklusive Thread-Sicherheit implementiert und direkt als Parametertyp in `[LibraryImport]` verwendbar ist.

</details>

## Aufgabe 4 — Abstraktion

Der Geometrieeditor soll Berechnungen künftig wahlweise mit einer nativen Bibliothek oder rein in C# durchführen – etwa weil auf dem Build-Server kein C-Compiler steht oder weil Unit-Tests nicht von einer `.dylib` abhängen sollen.

Entwirf ein Interface `IMatheBibliothek` mit den Operationen `Addiere` und `Vektorlaenge` und zwei Implementierungen. Überlege:
- Wer entscheidet, welche Implementierung benutzt wird – und wann?
- Wie erfährt das Programm, dass die native Bibliothek fehlt, ohne dass die Entscheidung in jeder Berechnung neu getroffen wird?
- Welches Entwurfsmuster aus Vorlesung 08 beschreibt die native Implementierung?

<details markdown="1">
<summary>Lösung anzeigen</summary>

**Schritt 1 — Das Interface als Vertrag und zwei Implementierungen:**

Der Rest des Programms hängt nur vom Interface ab – wie beim `IFigurSpeicher` im Geometrieeditor, hinter dem Arbeitsspeicher und JSON-Datei austauschbar waren. Die verwaltete Variante ist trivial und läuft überall. Die native Variante ist ein **Adapter**: Sie passt die statischen, klein geschriebenen C-Funktionen an die Instanzmethoden des Interfaces an.

```csharp
interface IMatheBibliothek
{
    int Addiere(int a, int b);
    double Vektorlaenge(double x, double y);
}

class VerwalteteMatheBibliothek : IMatheBibliothek
{
    public int Addiere(int a, int b) => a + b;
    public double Vektorlaenge(double x, double y) => Math.Sqrt(x * x + y * y);
}

class NativeMatheBibliothek : IMatheBibliothek
{
    private static partial class Nativ
    {
        [LibraryImport("mathe", EntryPoint = "addiere")]     public static partial int Addiere(int a, int b);
        [LibraryImport("mathe", EntryPoint = "vektorlaenge")] public static partial double Vektorlaenge(double x, double y);
    }

    public int Addiere(int a, int b) => Nativ.Addiere(a, b);
    public double Vektorlaenge(double x, double y) => Nativ.Vektorlaenge(x, y);
}
```

**Schritt 2 — Die Entscheidung an einer Stelle:**

Ob die native Bibliothek verfügbar ist, weiß man erst beim ersten Aufruf. Eine Fabrikmethode probiert es genau einmal aus und liefert danach das passende Objekt:

```csharp
static class MatheBibliothekFabrik
{
    public static IMatheBibliothek Erzeugen()
    {
        try
        {
            NativeMatheBibliothek nativ = new NativeMatheBibliothek();
            nativ.Addiere(0, 0);                 // Probeaufruf: lädt die Bibliothek
            return nativ;
        }
        catch (DllNotFoundException)
        {
            Console.WriteLine("Native Bibliothek fehlt – verwende C#-Implementierung.");
            return new VerwalteteMatheBibliothek();
        }
    }
}

IMatheBibliothek mathe = MatheBibliothekFabrik.Erzeugen();
Console.WriteLine(mathe.Vektorlaenge(3, 4)); // 5 – egal welche Implementierung
```

**Zentrale Designentscheidungen:**

- **Entscheidung beim Start, nicht bei jedem Aufruf:** Der Probeaufruf löst das Laden aus und fängt die `DllNotFoundException` genau einmal. Danach gibt es im Programm keine `try`/`catch`-Blöcke um Rechenoperationen mehr.
- **Tests bekommen die verwaltete Variante:** Unit-Tests für Klassen, die `IMatheBibliothek` benutzen, übergeben einfach `new VerwalteteMatheBibliothek()` – kein Compiler, keine Plattformabhängigkeit.
- **Das Interface gehört ins Fachkonzept, der Adapter in eine äußere Schicht:** Wie bei `IFigurSpeicher` und `JsonFigurSpeicher` zeigt die Abhängigkeit nach innen. Die native Bibliothek ist ein Implementierungsdetail, das der Kern nicht kennen muss.

</details>
