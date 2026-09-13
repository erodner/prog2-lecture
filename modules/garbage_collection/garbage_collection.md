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

Mit `new` erzeugen wir ständig neue Objekte – Wände, Spielobjekte, Listen, Zeichenketten. Jedes davon belegt Speicher. Aber wann wird dieser Speicher wieder frei? In Sprachen wie C oder C++ muss das Programm selbst daran denken und jedes Objekt explizit freigeben; vergisst man es, läuft der Speicher voll, gibt man zu früh frei, greift man auf Speicher zu, der jemand anderem gehört. In C# gibt es weder `delete` noch `free`. Stattdessen übernimmt die Laufzeitumgebung das Aufräumen: der **Garbage Collector** (GC). Man muss ihn nicht bedienen, aber man sollte verstehen, wie er arbeitet – sonst sucht man irgendwann an der falschen Stelle nach einem Speicherproblem.

## Objekte leben auf dem Heap

Aus [Programmierung 1](https://www.erodner.de/prog-lecture/modules/werttypen_referenztypen/werttypen_referenztypen/) wissen wir: Ein Objekt einer Klasse liegt auf dem **Heap**, die Variable enthält nur eine **Referenz** darauf. Was passiert, wenn die letzte Referenz verschwindet?

```csharp
Spielobjekt? o = new Wand(new Position(5, 2));   // Objekt 1 auf dem Heap, o zeigt darauf
o = new Wand(new Position(5, 3));                // Objekt 2 – Objekt 1 hat keine Referenz mehr
o = null;                                        // auch Objekt 2 ist nun unerreichbar
```

Objekt 1 existiert nach der zweiten Zeile noch im Speicher, aber niemand kann es mehr benutzen: Es gibt keinen Weg mehr dorthin. Solche Objekte sind **Müll** (*garbage*). Genau diese sucht der Garbage Collector und gibt ihren Speicher frei. Das Gleiche gilt für Objekte, deren Variable am Ende einer Methode oder eines Blocks aus dem Gültigkeitsbereich fällt.

## Erreichbarkeit von den Wurzeln

Der GC zählt nicht mit, wie viele Referenzen ein Objekt hat. Er stellt eine andere Frage: **Ist das Objekt von einer Wurzel aus erreichbar?** Wurzeln (*roots*) sind alle Stellen, an denen ein laufendes Programm Referenzen halten kann: lokale Variablen und Parameter der gerade aktiven Methoden auf dem Stack sowie statische Felder. Von dort aus folgt der GC allen Referenzen – Feld für Feld, Listenelement für Listenelement – und markiert alles, was er erreicht. Was danach unmarkiert ist, wird freigegeben.

Im Spiel lässt sich diese Kette gut verfolgen. Solange das Spielfeld erreichbar ist, ist auch jedes Objekt darauf erreichbar:

```csharp
Spielfeld feld = new Spielfeld(10, 6, held);
feld.Hinzufuegen(new Wand(new Position(5, 2)));
// erreichbar über: feld → objekte (List<Spielobjekt>) → [0]
```

Die Wand hat keine eigene Variable – trotzdem darf der GC sie nicht anfassen, weil ein Weg von der lokalen Variablen `feld` zu ihr führt. Spannend wird es, wenn ein Objekt das Spielfeld verlässt. Sobald wir eine Methode ergänzen, die ein Objekt aus der Liste entfernt (etwa wenn der Held einen Schlüssel aufsammelt oder eine Tür verschwindet), reißt diese Kette:

```csharp
public void Entfernen(Spielobjekt objekt)
{
    objekte.Remove(objekt);
}

feld.Entfernen(feld.ObjektAn(new Position(5, 2))!);
// keine Wurzel führt mehr zu dieser Wand – sie ist Müll
```

Wir müssen die Wand nicht löschen und dürfen es auch gar nicht. Es genügt, sie aus der Liste zu nehmen: Sie ist ab diesem Moment unerreichbar, und irgendwann räumt der GC sie weg. Wann genau, weiß niemand – und es spielt für die Korrektheit des Programms keine Rolle.

Auch **Referenzkreise** sind für den GC kein Problem. Stell dir vor, wir geben dem Spieler zusätzlich ein Feld `Welt`, damit er ohne Parameter weiß, wo er steht – dann zeigen Spielfeld und Spieler aufeinander:

```csharp
static void EinePartie()
{
    Spieler held = new Spieler("Held", new Position(1, 1));
    Spielfeld feld = new Spielfeld(10, 6, held);   // feld kennt den Helden ...
    held.Welt = feld;                              // ... und der Held das Feld
    feld.Hinzufuegen(new Wand(new Position(5, 2)));
    // ... spielen ...
}   // beide Variablen fallen weg – Held, Spielfeld und Wand sind Müll
```

Zwei Objekte, die sich gegenseitig referenzieren, würden bei einer reinen Referenzzählung nie freigegeben werden: Jedes hält den Zähler des anderen über null. Für den erreichbarkeitsbasierten GC sind sie schlicht Müll, weil nach dem Ende der Methode keine Wurzel mehr zu ihnen führt.

Ein **Speicherleck** in C# entsteht also nicht durch vergessenes Freigeben, sondern durch vergessene Referenzen: eine statische `List<Spielobjekt>`, in die man immer nur hinzufügt, oder ein Ereignis-Abonnement, das nie gelöst wird (dazu mehr in [Vorlesung 07](/lectures/07/07.md)). Solange irgendeine Wurzel das Objekt erreicht, darf der GC es nicht anfassen.
{: .notice--warning}

## Generationen

Alle Objekte bei jeder Aufräumaktion zu durchsuchen wäre teuer. Der .NET-GC nutzt deshalb eine Beobachtung, die für die meisten Programme gilt: **Die meisten Objekte sterben jung.** Ein `string`, den `AlsText()` in jeder Runde neu zusammenbaut, ein Zwischenergebnis in einer Schleife – nach wenigen Millisekunden braucht sie niemand mehr. Der Heap ist darum in drei **Generationen** aufgeteilt:

- **Generation 0:** Hier landen alle neuen Objekte. Sie wird sehr häufig und sehr schnell aufgeräumt, weil sie klein ist und der Großteil der Objekte bereits Müll ist.
- **Generation 1:** Objekte, die eine Aufräumaktion in Generation 0 überlebt haben. Ein Puffer zwischen kurz- und langlebig.
- **Generation 2:** Langlebige Objekte – etwa das `Spielfeld` und seine Wände, die vom Start bis zum Spielende existieren. Diese Generation wird selten durchsucht.

Für dich als Entwickler ist das im Alltag unsichtbar. Es erklärt aber, warum die vielen kurzlebigen Zeichenketten der Konsolenausgabe in .NET praktisch nichts kosten, während sehr viele *langlebige* Objekte mit vielen Referenzen die selteneren, aber teuren Generation-2-Sammlungen verlängern.

## Kein `delete`, kein `GC.Collect()`

Weil der GC selbst entscheidet, wann er läuft, gibt es in C# keinen Befehl zum Löschen eines Objekts – man kann höchstens Referenzen entfernen. Es gibt zwar `GC.Collect()`, mit dem man eine Sammlung erzwingen kann, aber in normalem Anwendungscode gehört dieser Aufruf nicht hin: Der GC kennt die Speichersituation besser als das Programm, und ein erzwungener Lauf unterbricht die Anwendung und stört die Generationen-Heuristik. Sinnvoll ist er fast nur in Messwerkzeugen – wie in der folgenden kleinen Demo:

```csharp
long vorher = GC.GetTotalMemory(forceFullCollection: true);

for (int i = 0; i < 100_000; i++)
{
    Wand w = new Wand(new Position(i % 10, i % 6));   // nach jedem Durchlauf unerreichbar
}

long mittendrin = GC.GetTotalMemory(forceFullCollection: false);
long nachher = GC.GetTotalMemory(forceFullCollection: true);

Console.WriteLine($"Vorher:     {vorher / 1024} KB");
Console.WriteLine($"Mittendrin: {mittendrin / 1024} KB");
Console.WriteLine($"Nachher:    {nachher / 1024} KB");
// Vorher:     126 KB
// Mittendrin: 3271 KB      (Wert schwankt – hängt davon ab, wann der GC zuletzt lief)
// Nachher:    118 KB
```

`GC.GetTotalMemory(true)` wartet auf eine vollständige Sammlung und liefert dann den belegten Speicher. Die 100.000 Wände sind danach vollständig verschwunden, obwohl nirgends etwas freigegeben wurde. Der Wert „mittendrin“ zeigt, dass der GC nicht sofort nach jedem Durchlauf aufräumt, sondern erst, wenn es sich lohnt.

## Finalizer und `IDisposable`

Der GC kümmert sich um **verwalteten** Speicher – also um Objekte, die mit `new` in .NET erzeugt wurden. Manche Objekte halten aber Ressourcen außerhalb der Laufzeitumgebung: eine geöffnete Datei, eine Netzwerkverbindung, ein Fensterhandle des Betriebssystems. Von diesen weiß der GC nichts. Eine Klasse kann dafür einen **Finalizer** (`~Spielfeld() { ... }`) definieren, der vor der Freigabe aufgerufen wird – aber man weiß nie, *wann* das passiert, vielleicht erst Minuten später oder beim Programmende. Für Dateien und Verbindungen ist das unbrauchbar. Der richtige Weg ist das Interface `IDisposable` zusammen mit der `using`-Anweisung, die Ressourcen **deterministisch** freigibt, sobald man sie nicht mehr braucht. Das schauen wir uns im Modul [`IDisposable` und `using`](/modules/idisposable_using/idisposable_using.md) in Vorlesung 09 genau an, wenn wir Spielstände in Dateien schreiben.

Im Zweifel: Vertraue dem Garbage Collector. Schreibe keine Finalizer, rufe nicht `GC.Collect()` auf, und setze Variablen nicht reflexartig auf `null` – der GC erkennt selbst, wenn eine lokale Variable nicht mehr gebraucht wird. Achte stattdessen darauf, keine Referenzen auf Objekte zu horten, die du nicht mehr brauchst.
{: .notice--primary}

Übung: Ein Programm soll mitzählen, wie viele Spielobjekte im Laufe einer Partie erzeugt wurden, und legt dafür jedes neue Objekt im Konstruktor von `Spielobjekt` in ein statisches Feld `static List<Spielobjekt> alleObjekte`. Warum ist das ein Speicherleck, obwohl es in C# kein `delete` gibt – und warum hilft `feld.Entfernen(...)` hier nicht mehr? Wie könntest du die Anzahl zählen, ohne die Objekte am Leben zu halten?
{: .notice--info}

## Weitere Quellen

- [Grundlagen der Garbage Collection – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/garbage-collection/fundamentals)
- [Speicherverwaltung und Garbage Collection in .NET – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/garbage-collection/)
- [`GC.GetTotalMemory` – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/api/system.gc.gettotalmemory)
- [Bereinigen nicht verwalteter Ressourcen – Microsoft Learn](https://learn.microsoft.com/de-de/dotnet/standard/garbage-collection/unmanaged)
- [.NET Fiddle – C# ohne Installation ausführen](https://dotnetfiddle.net/) – die Messung mit `GC.GetTotalMemory` oben lässt sich hier in einer Minute nachbauen; die Zahlen sehen auf jedem Rechner ein wenig anders aus.
