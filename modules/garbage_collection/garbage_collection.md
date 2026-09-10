---
title: "Garbage Collection"
layout: single
author_profile: true
author: Erik Rodner
licence: "CC-BY"
licence_desc: 2026 | HTW Berlin
toc: false
classes: wide
---

Mit `new` erzeugen wir ständig neue Objekte – Geister, Brüche, Listen. Jedes davon belegt Speicher. Aber wann wird dieser Speicher wieder frei? In Sprachen wie C oder C++ muss das Programm selbst daran denken und jedes Objekt explizit freigeben; vergisst man es, läuft der Speicher voll, gibt man zu früh frei, greift man auf Speicher zu, der jemand anderem gehört. In C# gibt es weder `delete` noch `free`. Stattdessen übernimmt die Laufzeitumgebung das Aufräumen: der **Garbage Collector** (GC). Man muss ihn nicht bedienen, aber man sollte verstehen, wie er arbeitet – sonst sucht man irgendwann an der falschen Stelle nach einem Speicherproblem.

## Objekte leben auf dem Heap

Aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/werttypen_referenztypen/werttypen_referenztypen/) wissen wir: Ein Objekt einer Klasse liegt auf dem **Heap**, die Variable enthält nur eine **Referenz** darauf. Was passiert, wenn die letzte Referenz verschwindet?

```csharp
Geist g = new Geist("Spooky");   // Objekt 1 auf dem Heap, g zeigt darauf
g = new Geist("Schleimi");       // Objekt 2 – Objekt 1 hat keine Referenz mehr
g = null;                        // auch Objekt 2 ist nun unerreichbar
```

Objekt 1 existiert nach der zweiten Zeile noch im Speicher, aber niemand kann es mehr benutzen: Es gibt keinen Weg mehr dorthin. Solche Objekte sind **Müll** (*garbage*). Genau diese sucht der Garbage Collector und gibt ihren Speicher frei. Das Gleiche gilt für Objekte, deren Variable am Ende einer Methode oder eines Blocks aus dem Gültigkeitsbereich fällt.

## Erreichbarkeit von den Wurzeln

Der GC zählt nicht mit, wie viele Referenzen ein Objekt hat. Er stellt eine andere Frage: **Ist das Objekt von einer Wurzel aus erreichbar?** Wurzeln (*roots*) sind alle Stellen, an denen ein laufendes Programm Referenzen halten kann: lokale Variablen und Parameter der gerade aktiven Methoden auf dem Stack sowie statische Felder. Von dort aus folgt der GC allen Referenzen – Feld für Feld, Listenelement für Listenelement – und markiert alles, was er erreicht. Was danach unmarkiert ist, wird freigegeben.

```csharp
Spukhaus haus = new Spukhaus();
haus.Bewohner.Add(new Geist("Spooky"));    // erreichbar über haus → Bewohner → [0]

Geist a = new Geist("A");
Geist b = new Geist("B");
a.Freund = b;
b.Freund = a;                              // a und b zeigen aufeinander
a = null;
b = null;                                  // trotzdem Müll: keine Wurzel erreicht sie
```

Das zweite Beispiel zeigt einen entscheidenden Vorteil: Zwei Objekte, die sich gegenseitig referenzieren, würden bei einer reinen Referenzzählung nie freigegeben werden. Für den erreichbarkeitsbasierten GC sind sie schlicht Müll, weil keine Wurzel mehr zu ihnen führt.

Ein **Speicherleck** in C# entsteht also nicht durch vergessenes Freigeben, sondern durch vergessene Referenzen: ein statisches `List<Geist>`, in das man immer nur hinzufügt, oder ein Ereignis-Abonnement, das nie gelöst wird (dazu mehr in [Vorlesung 06](/lectures/06/06.md)). Solange irgendeine Wurzel das Objekt erreicht, darf der GC es nicht anfassen.
{: .notice--warning}

## Generationen

Alle Objekte bei jeder Aufräumaktion zu durchsuchen wäre teuer. Der .NET-GC nutzt deshalb eine Beobachtung, die für die meisten Programme gilt: **Die meisten Objekte sterben jung.** Ein `Bruch` als Zwischenergebnis, ein `string` für eine Ausgabe – nach wenigen Millisekunden braucht sie niemand mehr. Der Heap ist darum in drei **Generationen** aufgeteilt:

- **Generation 0:** Hier landen alle neuen Objekte. Sie wird sehr häufig und sehr schnell aufgeräumt, weil sie klein ist und der Großteil der Objekte bereits Müll ist.
- **Generation 1:** Objekte, die eine Aufräumaktion in Generation 0 überlebt haben. Ein Puffer zwischen kurz- und langlebig.
- **Generation 2:** Langlebige Objekte – etwa die `FigurenVerwaltung` im Geometrieeditor, die das ganze Programm über existiert. Diese Generation wird selten durchsucht.

Für dich als Entwickler ist das im Alltag unsichtbar. Es erklärt aber, warum kurzlebige Objekte in .NET praktisch nichts kosten, während sehr viele *langlebige* Objekte mit vielen Referenzen die selteneren, aber teuren Generation-2-Sammlungen verlängern.

## Kein `delete`, kein `GC.Collect()`

Weil der GC selbst entscheidet, wann er läuft, gibt es in C# keinen Befehl zum Löschen eines Objekts – man kann höchstens Referenzen entfernen. Es gibt zwar `GC.Collect()`, mit dem man eine Sammlung erzwingen kann, aber in normalem Anwendungscode gehört dieser Aufruf nicht hin: Der GC kennt die Speichersituation besser als das Programm, und ein erzwungener Lauf unterbricht die Anwendung und stört die Generationen-Heuristik. Sinnvoll ist er fast nur in Messwerkzeugen – wie in der folgenden kleinen Demo:

```csharp
long vorher = GC.GetTotalMemory(forceFullCollection: true);

for (int i = 0; i < 100_000; i++)
{
    Geist g = new Geist($"Geist {i}");    // nach jedem Durchlauf unerreichbar
}

long mittendrin = GC.GetTotalMemory(forceFullCollection: false);
long nachher = GC.GetTotalMemory(forceFullCollection: true);

Console.WriteLine($"Vorher:     {vorher / 1024} KB");
Console.WriteLine($"Mittendrin: {mittendrin / 1024} KB");
Console.WriteLine($"Nachher:    {nachher / 1024} KB");
// Vorher:     84 KB
// Mittendrin: 2371 KB      (Wert schwankt – hängt davon ab, wann der GC zuletzt lief)
// Nachher:    85 KB
```

`GC.GetTotalMemory(true)` wartet auf eine vollständige Sammlung und liefert dann den belegten Speicher. Die 100.000 Geister sind danach vollständig verschwunden, obwohl nirgends etwas freigegeben wurde. Der Wert „mittendrin“ zeigt, dass der GC nicht sofort nach jedem Durchlauf aufräumt, sondern erst, wenn es sich lohnt.

## Finalizer und `IDisposable`

Der GC kümmert sich um **verwalteten** Speicher – also um Objekte, die mit `new` in .NET erzeugt wurden. Manche Objekte halten aber Ressourcen außerhalb der Laufzeitumgebung: eine geöffnete Datei, eine Netzwerkverbindung, ein Fensterhandle des Betriebssystems. Von diesen weiß der GC nichts. Eine Klasse kann dafür einen **Finalizer** (`~Geist() { ... }`) definieren, der vor der Freigabe aufgerufen wird – aber man weiß nie, *wann* das passiert, vielleicht erst Minuten später oder beim Programmende. Für Dateien und Verbindungen ist das unbrauchbar. Der richtige Weg ist das Interface `IDisposable` zusammen mit der `using`-Anweisung, die Ressourcen **deterministisch** freigibt, sobald man sie nicht mehr braucht. Das schauen wir uns in [Vorlesung 08](/modules/idisposable_using/idisposable_using.md) genau an, wenn wir mit Dateien und Streams arbeiten.

Im Zweifel: Vertraue dem Garbage Collector. Schreibe keine Finalizer, rufe nicht `GC.Collect()` auf, und setze Variablen nicht reflexartig auf `null` – der GC erkennt selbst, wenn eine lokale Variable nicht mehr gebraucht wird. Achte stattdessen darauf, keine Referenzen auf Objekte zu horten, die du nicht mehr brauchst.
{: .notice--primary}

Übung: Ein Programm hält alle jemals erzeugten Geister in einem statischen Feld `static List<Geist> alleGeister`, damit `Geist` im Konstruktor die Gesamtzahl mitzählen kann. Warum ist das ein Speicherleck, obwohl es in C# kein `delete` gibt? Wie könntest du die Anzahl zählen, ohne die Objekte am Leben zu halten?
{: .notice--info}

## Weitere Quellen

- [Grundlagen der Garbage Collection – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/garbage-collection/fundamentals)
- [Speicherverwaltung und Garbage Collection in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/garbage-collection/)
- [`GC.GetTotalMemory` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.gc.gettotalmemory)
- [Bereinigen nicht verwalteter Ressourcen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/garbage-collection/unmanaged)
