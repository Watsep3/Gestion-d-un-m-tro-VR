using UnityEngine;
using System;
using System.Collections.Generic;

// ============================================
// CLASSES DE DONNÉES POUR LE SYSTÈME MÉTRO
// ============================================

/// <summary>
/// Données d'une station de métro
/// </summary>
[System.Serializable]
public class StationData
{
    public string stationId;
    public string stationName;
    public Vector3 position;
    public StationStatus status;
    public int passengerCount;
    public int maxPassengers;
    public List<string> connectedStations;
    
    public StationData()
    {
        connectedStations = new List<string>();
        status = StationStatus.Normal;
        passengerCount = 0;
        maxPassengers = 500;
    }
}

/// <summary>
/// Données d'un train
/// </summary>
[System.Serializable]
public class TrainData
{
    public string trainId;
    public string lineId;
    public TrainStatus status;
    public string currentStationId;
    public string nextStationId;
    public int passengerCapacity;
    public int currentPassengers;
    public float speed;
    
    public TrainData()
    {
        status = TrainStatus.Moving;
        passengerCapacity = 200;
        currentPassengers = 0;
        speed = 5f;
    }
}

/// <summary>
/// Données d'une ligne de métro
/// </summary>
[System.Serializable]
public class LineData
{
    public string lineId;
    public string lineName;
    public Color lineColor;
    public List<string> stationIds;
    public LineStatus status;
    public int trainCount;
    
    public LineData()
    {
        stationIds = new List<string>();
        status = LineStatus.Active;
        trainCount = 0;
        lineColor = Color.white;
    }
}

/// <summary>
/// Configuration complète du réseau de métro
/// Utilisé pour charger/sauvegarder depuis JSON
/// </summary>
[System.Serializable]
public class MetroConfig
{
    public List<StationData> stations;
    public List<LineData> lines;
    public List<TrainData> trains;
    
    public MetroConfig()
    {
        stations = new List<StationData>();
        lines = new List<LineData>();
        trains = new List<TrainData>();
    }
}

/// <summary>
/// Données d'un incident
/// </summary>
[System.Serializable]
public class IncidentData
{
    public string incidentId;
    public IncidentType type;
    public string targetId;
    public float duration;
    public float timeRemaining;
    public bool isActive;
    
    public IncidentData()
    {
        incidentId = System.Guid.NewGuid().ToString();
        isActive = false;
        timeRemaining = 0f;
    }
}