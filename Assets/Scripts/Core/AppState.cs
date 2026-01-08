using UnityEngine;

// ============================================
// ENUMS GLOBAUX DU SYSTÈME
// ============================================

/// <summary>
/// États de l'application
/// </summary>
public enum AppState
{
    Initializing,
    Running,
    Paused,
    GameOver
}

/// <summary>
/// États possibles d'une station
/// </summary>
public enum StationStatus
{
    Normal,
    Delayed,
    Broken
}

/// <summary>
/// États possibles d'un train
/// </summary>
public enum TrainStatus
{
    Moving,
    Stopped,
    Maintenance
}

/// <summary>
/// États possibles d'une ligne
/// </summary>
public enum LineStatus
{
    Active,
    Delayed,
    Closed
}

/// <summary>
/// Types d'incidents possibles
/// </summary>
public enum IncidentType
{
    StationBreakdown,      // Panne de station
    TrainMalfunction,      // Train en panne
    LineDelay,             // Retard sur ligne
    Overcrowding,          // Surcharge passagers
    SignalFailure,         // Panne de signalisation
    TrackMaintenance       // Maintenance voie
}