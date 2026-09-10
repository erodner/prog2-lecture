// Kleine native Bibliothek für die P/Invoke-Demo (Vorlesung 10).
//
// Bauen:
//   macOS/Linux:  bash build-native.sh
//   Windows:      .\build-native.ps1
//
// Unter Windows muss jede Funktion, die aus der DLL sichtbar sein soll,
// mit __declspec(dllexport) markiert werden. Unter macOS/Linux sind
// Funktionen standardmäßig sichtbar – das Makro EXPORT ist dort leer.

#include <string.h>
#include <math.h>

#ifdef _WIN32
#define EXPORT __declspec(dllexport)
#else
#define EXPORT
#endif

EXPORT int addiere(int a, int b)
{
    return a + b;
}

EXPORT double vektorlaenge(double x, double y)
{
    return sqrt(x * x + y * y);
}

// Liefert die Anzahl der Bytes (nicht der Zeichen!) bis zum Nullbyte.
EXPORT int laenge(const char* s)
{
    return (int)strlen(s);
}
