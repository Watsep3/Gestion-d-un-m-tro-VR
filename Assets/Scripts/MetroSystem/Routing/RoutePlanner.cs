using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Planifie les routes entre les stations
/// Utilitaire pour TrainController
/// </summary>
public class RoutePlanner : MonoBehaviour
{
    private MetroSystemManager metroSystem;
    
    void Start()
    {
        metroSystem = GameManager.Instance.metroSystem;
    }
    
    /// <summary>
    /// Retourne la prochaine station pour un train donné
    /// </summary>
    public string GetNextStation(string trainId)
    {
        if (metroSystem == null) return null;
        
        TrainData train = metroSystem.GetTrain(trainId);
        if (train == null) return null;
        
        LineData line = metroSystem.GetLine(train.lineId);
        if (line == null) return null;
        
        // Trouver l'index de la station actuelle
        int currentIndex = line.stationIds.IndexOf(train.currentStationId);
        
        if (currentIndex < 0)
        {
            Debug.LogWarning($"RoutePlanner: Current station {train.currentStationId} not found in line {train.lineId}");
            return line.stationIds[0]; // Retourner la première station
        }
        
        // Prochaine station
        int nextIndex = currentIndex + 1;
        
        // Boucler en fin de ligne
        if (nextIndex >= line.stationIds.Count)
        {
            nextIndex = 0;
        }
        
        string nextStationId = line.stationIds[nextIndex];
        
        // Vérifier si la station est accessible
        StationData nextStation = metroSystem.GetStation(nextStationId);
        
        if (nextStation != null && nextStation.status == StationStatus.Broken)
        {
            Debug.LogWarning($"RoutePlanner: Next station {nextStationId} is broken, skipping");
            
            // Mettre à jour temporairement et réessayer (récursif)
            train.currentStationId = nextStationId;
            return GetNextStation(trainId);
        }
        
        return nextStationId;
    }
    
    /// <summary>
    /// Retourne la position 3D d'une station
    /// </summary>
    public Vector3 GetStationPosition(string stationId)
    {
        if (metroSystem == null) return Vector3.zero;
        
        StationData station = metroSystem.GetStation(stationId);
        
        if (station != null)
        {
            return station.position;
        }
        
        Debug.LogWarning($"RoutePlanner: Station {stationId} not found");
        return Vector3.zero;
    }
    
    /// <summary>
    /// Retourne toutes les stations d'une ligne dans l'ordre
    /// </summary>
    public List<string> GetLineStations(string lineId)
    {
        if (metroSystem == null) return new List<string>();
        
        LineData line = metroSystem.GetLine(lineId);
        
        if (line != null)
        {
            return new List<string>(line.stationIds);
        }
        
        return new List<string>();
    }
    
    /// <summary>
    /// Calcule la distance entre deux stations
    /// </summary>
    public float GetDistanceBetweenStations(string stationId1, string stationId2)
    {
        Vector3 pos1 = GetStationPosition(stationId1);
        Vector3 pos2 = GetStationPosition(stationId2);
        
        return Vector3.Distance(pos1, pos2);
    }
}
