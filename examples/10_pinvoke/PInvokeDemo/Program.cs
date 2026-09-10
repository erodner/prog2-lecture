// PInvokeDemo – Vorlesung 10: Einbindung nativer Bibliotheken
//
// Vorher die eigene C-Bibliothek bauen (siehe native/build-native.sh bzw. .ps1),
// dann: dotnet run

using System.Runtime.InteropServices;

// ---------------------------------------------------------------------------
// Abschnitt 1: Funktionen der C-Standardbibliothek (libc) aufrufen
// ---------------------------------------------------------------------------
// Die C-Standardbibliothek heißt je nach Plattform anders: "libc" unter
// macOS/Linux, "msvcrt" unter Windows. Der einfachste Weg sind zwei Klassen
// mit denselben Signaturen – zur Laufzeit wählen wir die passende.

Console.WriteLine("=== 1. libc: strlen und getpid ===");

string text = "Hallo P/Invoke";
if (OperatingSystem.IsWindows())
{
    Console.WriteLine($"strlen(\"{text}\") = {LibcWindows.strlen(text)}");
    Console.WriteLine($"Prozess-ID          = {LibcWindows.getpid()}");
}
else
{
    Console.WriteLine($"strlen(\"{text}\") = {LibcUnix.strlen(text)}");
    Console.WriteLine($"Prozess-ID          = {LibcUnix.getpid()}");
}
Console.WriteLine($"Zum Vergleich: Environment.ProcessId = {Environment.ProcessId}");

// ---------------------------------------------------------------------------
// Abschnitt 2: Mathe-Bibliothek libm – Bibliotheksname per DllImportResolver
// ---------------------------------------------------------------------------
// Statt für jede Plattform eine eigene Klasse zu schreiben, kann man dem Lader
// einmalig sagen, wie ein Bibliotheksname aufzulösen ist. Die Deklaration
// bleibt dann bei [DllImport("libm")].

NativeLibrary.SetDllImportResolver(typeof(Program).Assembly, (name, assembly, suchpfad) =>
{
    if (name == "libm" && OperatingSystem.IsLinux())
        return NativeLibrary.Load("libm.so.6");          // unter Linux gibt es kein libm.so ohne Dev-Paket
    if (name == "libm" && OperatingSystem.IsWindows())
        return NativeLibrary.Load("ucrtbase.dll");       // cos() steckt in der Universal C Runtime
    return IntPtr.Zero;                                  // sonst: Standardsuche des Laders
});

Console.WriteLine();
Console.WriteLine("=== 2. libm: cos ===");
Console.WriteLine($"cos(0)   = {Libm.cos(0)}");
Console.WriteLine($"cos(pi)  = {Libm.cos(Math.PI)}");
Console.WriteLine($"Math.Cos = {Math.Cos(Math.PI)}");

// ---------------------------------------------------------------------------
// Abschnitt 3: Eigene C-Bibliothek mit [DllImport]
// ---------------------------------------------------------------------------
// [DllImport("mathe")] genügt: Der Lader probiert selbst libmathe.dylib
// (macOS), libmathe.so (Linux) und mathe.dll (Windows) im bin/-Ordner aus.

Console.WriteLine();
Console.WriteLine("=== 3. Eigene Bibliothek mathe mit [DllImport] ===");
try
{
    Console.WriteLine($"addiere(2, 40)       = {MatheDllImport.addiere(2, 40)}");
    Console.WriteLine($"vektorlaenge(3, 4)   = {MatheDllImport.vektorlaenge(3, 4)}");
    Console.WriteLine($"laenge(\"Grüße\")      = {MatheDllImport.laenge("Grüße")}  (Bytes in UTF-8)");
    Console.WriteLine($"\"Grüße\".Length       = {"Grüße".Length}  (Zeichen in C#)");
}
catch (DllNotFoundException ex)
{
    // Die vollständige Meldung listet jeden probierten Pfad auf – hier reicht die erste Zeile.
    Console.WriteLine("Native Bibliothek nicht gefunden – bitte zuerst native/build-native.sh bzw. .ps1 ausführen.");
    Console.WriteLine($"  {ex.Message.Split('\n')[0]}");
}

// ---------------------------------------------------------------------------
// Abschnitt 4: Dieselbe Bibliothek mit [LibraryImport] (seit .NET 7)
// ---------------------------------------------------------------------------
// Der Marshalling-Code wird jetzt zur Kompilierzeit von einem Source-Generator
// erzeugt. Dafür: static partial class, partial-Methode, AllowUnsafeBlocks.

Console.WriteLine();
Console.WriteLine("=== 4. Eigene Bibliothek mathe mit [LibraryImport] ===");
try
{
    Console.WriteLine($"Addiere(2, 40)       = {MatheLibraryImport.Addiere(2, 40)}");
    Console.WriteLine($"Vektorlaenge(3, 4)   = {MatheLibraryImport.Vektorlaenge(3, 4)}");
    Console.WriteLine($"Laenge(\"Grüße\")      = {MatheLibraryImport.Laenge("Grüße")}");
}
catch (DllNotFoundException ex)
{
    Console.WriteLine($"Native Bibliothek nicht gefunden: {ex.Message.Split('\n')[0]}");
}

// ---------------------------------------------------------------------------
// Abschnitt 5: Windows-API (nur unter Windows)
// ---------------------------------------------------------------------------

Console.WriteLine();
Console.WriteLine("=== 5. Windows-API: MessageBoxW ===");
if (OperatingSystem.IsWindows())
{
    int ergebnis = User32.MessageBoxW(IntPtr.Zero, "Gruß aus C#!", "P/Invoke", 1);
    Console.WriteLine($"MessageBoxW lieferte {ergebnis} (1 = OK, 2 = Abbrechen)");
}
else
{
    Console.WriteLine("Übersprungen – user32.dll gibt es nur unter Windows.");
}

// ===========================================================================
// Deklarationen der nativen Funktionen
// ===========================================================================
// Bei Top-Level-Statements müssen Klassen nach dem ausführbaren Code stehen.

// Abschnitt 1: libc, zwei Klassen für zwei Bibliotheksnamen
static class LibcUnix
{
    [DllImport("libc")]
    public static extern int strlen(string s);

    [DllImport("libc")]
    public static extern int getpid();
}

static class LibcWindows
{
    [DllImport("msvcrt")]
    public static extern int strlen(string s);

    // Unter Windows heißt die Funktion _getpid – EntryPoint stellt den Namen richtig.
    [DllImport("msvcrt", EntryPoint = "_getpid")]
    public static extern int getpid();
}

// Abschnitt 2: libm, Name wird oben per DllImportResolver aufgelöst
static class Libm
{
    [DllImport("libm")]
    public static extern double cos(double x);
}

// Abschnitt 3: eigene Bibliothek, klassisches DllImport
static class MatheDllImport
{
    [DllImport("mathe")]
    public static extern int addiere(int a, int b);

    [DllImport("mathe")]
    public static extern double vektorlaenge(double x, double y);

    // CharSet.Ansi heißt unter macOS/Linux UTF-8, unter Windows die System-Codepage –
    // [LibraryImport] mit StringMarshalling.Utf8 (Abschnitt 4) ist hier eindeutiger.
    [DllImport("mathe", CharSet = CharSet.Ansi)]
    public static extern int laenge(string s);
}

// Abschnitt 4: eigene Bibliothek, LibraryImport mit Source-Generator
static partial class MatheLibraryImport
{
    [LibraryImport("mathe", EntryPoint = "addiere")]
    public static partial int Addiere(int a, int b);

    [LibraryImport("mathe", EntryPoint = "vektorlaenge")]
    public static partial double Vektorlaenge(double x, double y);

    [LibraryImport("mathe", EntryPoint = "laenge", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int Laenge(string s);
}

// Abschnitt 5: Windows-API
static class User32
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
