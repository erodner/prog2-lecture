---
title: "Verwalteter und nativer Code"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Stell dir vor, du ziehst in ein Haus mit Hausmeister: Der Müll wird abgeholt, kaputte Lampen werden getauscht, und wenn du versuchst, in die Wohnung des Nachbarn zu laufen, steht jemand freundlich im Weg. So fühlt sich das Programmieren in C# an – die .NET-Laufzeit passt auf. Manchmal muss man dieses Haus aber verlassen, weil draußen etwas steht, das drinnen nicht existiert: eine Bibliothek, die vor zwanzig Jahren in C geschrieben wurde, ein Gerätetreiber oder eine Funktion des Betriebssystems. Dieses Modul erklärt, was „draußen“ bedeutet, wann man wirklich hinaus muss und welche Wege es dafür gibt.

## Verwalteter und nativer Code

Alles, was wir bisher geschrieben haben, ist **verwalteter Code** (*managed code*). Der C#-Compiler übersetzt ihn nicht in Maschinencode, sondern in die Zwischensprache IL, die erst zur Laufzeit von der .NET-Laufzeit (CLR) für den jeweiligen Prozessor übersetzt wird. „Verwaltet“ heißt: Die Laufzeit kennt jeden Typ, jedes Objekt und jede Referenz. Deshalb kann der [Garbage Collector](/modules/garbage_collection/garbage_collection.md) Speicher aufräumen, deshalb gibt es eine `IndexOutOfRangeException` statt eines stillen Speicherfehlers, und deshalb läuft dieselbe Assembly auf macOS, Linux und Windows.

**Nativer Code** (*unmanaged code*) ist das Gegenteil: fertiger Maschinencode für einen bestimmten Prozessor und ein bestimmtes Betriebssystem, typischerweise aus C oder C++ übersetzt. Er liegt in dynamischen Bibliotheken – `.dll` unter Windows, `.so` unter Linux, `.dylib` unter macOS. Die .NET-Laufzeit weiß nichts über sein Inneres: keine Typinformationen, kein Garbage Collector, keine Exceptions. Ein C-Aufruf bekommt Zahlen und Zeiger und liefert Zahlen und Zeiger zurück – mehr nicht.

| | Verwalteter Code | Nativer Code |
| :--- | :--- | :--- |
| Sprache | C#, F#, VB.NET | C, C++, Rust, Fortran, … |
| Übersetzt zu | IL, JIT zur Laufzeit | Maschinencode |
| Speicher | Garbage Collector | manuell (`malloc`/`free`) |
| Fehler | `Exception` | Rückgabewert, Absturz |
| Portabel | ja, dieselbe Assembly | nein, pro Plattform neu übersetzen |

## Wann braucht man nativen Code?

Meistens gar nicht – die .NET-Klassenbibliothek und NuGet decken erstaunlich viel ab. Es gibt aber drei typische Situationen, in denen es nicht anders geht:

- **Vorhandene Bibliotheken:** Es existiert eine ausgereifte C- oder C++-Bibliothek, die niemand neu schreiben will – etwa für Bildverarbeitung, numerische Berechnungen, Kompression oder Kryptografie. Viele NuGet-Pakete wie `SkiaSharp` oder `SQLitePCLRaw` sind im Kern genau solche Bibliotheken mit einer dünnen C#-Hülle.
- **Hardware und Treiber:** Ein Messgerät, eine Kamera oder ein Industrie-Bus kommt vom Hersteller mit einem SDK in C. Die Dokumentation beschreibt C-Funktionen, und genau die muss man rufen.
- **Betriebssystem-Funktionen:** Für einige Dinge gibt es keine .NET-API – ein Beispiel ist die klassische Windows-Nachrichtenbox `MessageBox`, ein anderes sind Systemaufrufe unter Linux, die nicht in `System.IO` oder `System.Diagnostics` abgebildet sind.

Im Zweifel: erst suchen, ob es eine verwaltete Lösung oder ein fertiges NuGet-Paket gibt. Nativer Code ist ein Werkzeug für den Notfall, nicht für den Alltag – jede native Abhängigkeit bedeutet, dass das Programm nur noch dort läuft, wo die passende Bibliothek liegt.
{: .notice--primary}

## Drei Wege nach draußen

.NET bietet je nach Art des nativen Codes einen anderen Mechanismus. Der Unterschied liegt darin, *was* aufgerufen werden soll: einzelne Funktionen, ganze C++-Klassen oder COM-Objekte.

**Platform Invoke (P/Invoke)** ist der Weg für einzelne Funktionen im C-Stil. Man deklariert die Funktion in C# mit dem Attribut `[DllImport]` oder `[LibraryImport]`, gibt den Namen der Bibliothek an, und die Laufzeit kümmert sich um das Laden und die Umwandlung der Parameter. P/Invoke funktioniert auf allen Plattformen und ist das Thema der nächsten drei Module.

**C++/CLI** ist ein Windows-Dialekt von C++, der verwalteten und nativen Code in einer Datei mischen kann. Man benutzt ihn, um ganze C++-Klassenhierarchien – mit Vererbung, Templates und Überladungen – für .NET zugänglich zu machen. Dazu schreibt man einen **Wrapper**: eine dünne Schicht, die für jede native Klasse eine verwaltete Klasse anbietet und Aufrufe durchreicht.

```
+--------------------+     +----------------------+     +------------------------+
|  C#-Programm       | --> |  C++/CLI-Wrapper     | --> |  native C++-Bibliothek |
|  (verwaltet)       |     |  (verwaltet + nativ) |     |  (nativ)               |
|  new Bild("a.png") |     |  ref class Bild {    |     |  class Image { ... };  |
|                    |     |    Image* inner; }   |     |                        |
+--------------------+     +----------------------+     +------------------------+
```

Der Wrapper übersetzt in beide Richtungen: `System::String^` zu `std::string`, Exceptions zu Fehlercodes und zurück. C++/CLI wird nur vom Microsoft-Compiler unter Windows unterstützt – für plattformübergreifende Projekte ist es keine Option. Dort schreibt man stattdessen eine kleine C-Schnittstelle (`extern "C"`) um die C++-Bibliothek und ruft diese per P/Invoke auf.

**COM-Interop** ist der dritte Weg und ebenfalls Windows-spezifisch. COM (*Component Object Model*) ist Microsofts alte Komponententechnik, über die etwa Office-Anwendungen automatisierbar sind. .NET kann COM-Objekte wie normale Objekte benutzen, weil die Laufzeit hinter den Kulissen dieselben Umwandlungsmechanismen wie bei P/Invoke einsetzt – nur mit COM-spezifischen Datentypen. Für diese Vorlesung reicht es, den Namen zu kennen.

## Marshalling – das Übersetzen der Daten

Egal welcher Weg: Zwischen den beiden Welten müssen Daten übersetzt werden. Ein C#-`int` und ein C-`int` sind zufällig gleich aufgebaut, aber ein C#-`string` und ein C-`char*` haben nichts gemeinsam. Der C#-String liegt auf dem verwalteten Heap, besteht aus UTF-16-Zeichen und kennt seine Länge. Das `char*` zeigt auf Bytes, die mit einem Nullbyte enden. Diese Übersetzung nennt man **Marshalling**, und der Teil der Laufzeit, der sie erledigt, heißt **Marshaller**.

```
verwalteter Heap                                nativer Speicher
+----------------------------+     Marshaller    +----------------------+
| string "Hallo"             | ---------------> | 48 61 6C 6C 6F 00     |
| Länge 5, UTF-16, beweglich |    kopiert +     | Bytes, nullterminiert |
+----------------------------+    konvertiert   +----------------------+
```

Der Marshaller kopiert also, konvertiert die Kodierung und hängt das Nullbyte an. Nach dem Aufruf gibt er den nativen Speicher wieder frei. Ein wichtiger Grund für das Kopieren ist der Garbage Collector: Er darf Objekte auf dem verwalteten Heap jederzeit verschieben, um Lücken zu schließen. Ein nativer Zeiger auf einen C#-String wäre deshalb nach der nächsten Aufräumrunde ungültig. Für einfache Zahlentypen ist kein Kopieren nötig – sie werden direkt übergeben. Solche Typen heißen **blittable**, und je mehr blittable Parameter ein Aufruf hat, desto billiger ist er.

## Die Sicherheitsleine reißt

Wer nativen Code aufruft, verlässt den Schutz der Laufzeit. Das hat konkrete Folgen:

- **Kein Typcheck:** Deklarierst du einen Parameter als `int`, den die C-Funktion als `long` erwartet, merkt das niemand. Der Aufruf liefert falsche Werte oder überschreibt Speicher.
- **Abstürze statt Exceptions:** Ein Nullzeiger oder ein Zugriff über das Array-Ende hinaus führt in C nicht zu einer `NullReferenceException`, sondern zu einem sofortigen Absturz des gesamten Prozesses (*Segmentation fault*, *Access violation*) – ohne `catch`, ohne Stacktrace.
- **Kein Garbage Collector:** Speicher, den eine C-Funktion mit `malloc` anlegt, muss von dir wieder freigegeben werden – am besten über ein `IDisposable`-Muster wie im Modul [`IDisposable` und `using`](/modules/idisposable_using/idisposable_using.md).
- **Plattformbindung:** Die native Bibliothek muss für jedes Zielsystem in der passenden Variante vorliegen.

Ein P/Invoke-Aufruf ist ein Versprechen an die Laufzeit: „Ich habe die Signatur richtig abgeschrieben.“ Prüfe jede Deklaration gegen die Header-Datei der Bibliothek, und kapsle native Aufrufe in einer kleinen Klasse, damit der Rest des Programms nichts davon merkt.
{: .notice--warning}

Übung: Überlege für drei Bibliotheken, die du kennst oder nachschlägst (z. B. SQLite, OpenCV, eine Grafikkarten-API), welcher der drei Wege sich eignen würde und warum. Was bedeutet deine Wahl für Nutzer unter Linux?
{: .notice--info}

## Weitere Quellen

- [Native Interoperabilität – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/native-interop/)
- [Typmarshalling – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/native-interop/type-marshalling)
- [.NET-Programmierung mit C++/CLI – Microsoft Learn](https://learn.microsoft.com/de-de/cpp/dotnet/dotnet-programming-with-cpp-cli-visual-cpp)
