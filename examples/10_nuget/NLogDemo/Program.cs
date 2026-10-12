using NLog;

// Der Logger bekommt automatisch den Namen der umgebenden Klasse –
// bei Top-Level-Statements ist das "Program".
Logger logger = LogManager.GetCurrentClassLogger();

logger.Info("Programm gestartet.");
logger.Warn("Konfigurationsdatei 'einstellungen.json' nicht gefunden, nutze Standardwerte.");

try
{
    int[] messwerte = { 12, 7, 42 };
    int index = 3;
    Console.WriteLine($"Messwert: {messwerte[index]}");
}
catch (IndexOutOfRangeException ex)
{
    // Die Exception wird als erstes Argument übergeben – NLog schreibt
    // Typ, Meldung und Stacktrace mit ins Log.
    logger.Error(ex, "Zugriff auf einen ungültigen Messwert.");
}

for (int i = 1; i <= 3; i++)
{
    // Debug-Meldungen erscheinen nur in der Datei, nicht auf der Konsole
    // (siehe Regeln in nlog.config).
    logger.Debug("Verarbeite Datensatz {Nummer} von 3", i);
}

logger.Info("Programm beendet.");

// Puffer leeren, damit die letzten Zeilen sicher in der Datei landen.
LogManager.Shutdown();
