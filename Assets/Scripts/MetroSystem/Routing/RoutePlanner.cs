using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Planifie les routes entre les stations
/// Utilitaire pour TrainController
/// </summary>
public class RoutePlanner : MonoBehaviour
{
    [Header("Route Configuration")]
    public string lineId; // e.g., "Ligne Bleue"
    public Color lineColor = Color.blue; // Visual color for the line
    public List<Transform> stations = new List<Transform>();
    public bool loopLine = true; // ✅ If true, train goes Station Z -> Station A

    // --- NEW: AUTO-FILL FIX FOR MISSING STATIONS ---
    private void Start()
    {
        // 🚨 SAFETY CHECK: If the list is empty, find the stations in the scene automatically
        // This fixes the issue where the Spawner forgets to link stations to the line.
        if (stations.Count == 0)
        {
            // Look for the folder named "Stations" in the Hierarchy (Case Sensitive!)
            GameObject stationsParent = GameObject.Find("Stations");

            if (stationsParent != null)
            {
                stations.Clear();
                // Add every child object (Gare Centrale, etc.) to the list
                foreach (Transform child in stationsParent.transform)
                {
                    stations.Add(child);
                }
                Debug.Log($"🧩 {name} auto-found {stations.Count} stations.");
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find a 'Stations' object in the Hierarchy. Route is empty. Make sure your station container is named 'Stations'.");
            }
        }
    }

    /// <summary>
    /// Returns the next station based on the current one.
    /// Handles the "Loop" logic automatically.
    /// </summary>
    public Transform GetNextStation(Transform currentStation)
    {
        if (stations == null || stations.Count == 0)
        {
            Debug.LogWarning($"Route {name} has no stations!");
            return null;
        }

        // 1. If we are just starting (no current station), go to the first one.
        if (currentStation == null)
        {
            return stations[0];
        }

        // 2. Find where we are in the list
        int currentIndex = stations.IndexOf(currentStation);

        // If the station isn't on this line, default to the first one
        if (currentIndex == -1)
        {
            return stations[0];
        }

        // 3. Calculate next index
        int nextIndex = currentIndex + 1;

        // 4. Handle End of Line (Looping)
        if (nextIndex >= stations.Count)
        {
            if (loopLine)
            {
                return stations[0]; // 🔄 Loop back to start
            }
            else
            {
                // If not looping, stay at the last station
                return stations[stations.Count - 1];
            }
        }

        return stations[nextIndex];
    }

    /// <summary>
    /// Helper to get specific coordinates
    /// </summary>
    public Vector3 GetStationPosition(int index)
    {
        if (index >= 0 && index < stations.Count && stations[index] != null)
        {
            return stations[index].position;
        }
        return Vector3.zero;
    }

    // --- VISUAL DEBUGGING ---
    // This draws colored lines in the Editor so you can see the route!
    private void OnDrawGizmos()
    {
        if (stations == null || stations.Count < 2) return;

        Gizmos.color = lineColor;

        // Draw lines between stations
        for (int i = 0; i < stations.Count - 1; i++)
        {
            if (stations[i] != null && stations[i + 1] != null)
            {
                Gizmos.DrawLine(stations[i].position, stations[i + 1].position);
                Gizmos.DrawSphere(stations[i].position, 0.2f);
            }
        }

        // Draw the closing loop line
        if (loopLine && stations.Count > 1)
        {
            if (stations[stations.Count - 1] != null && stations[0] != null)
            {
                Gizmos.DrawLine(stations[stations.Count - 1].position, stations[0].position);
            }
        }
    }
}
    private MetroSystemManager metroSystem;
    
    void Start()
    {
        metroSystem = GameManager.Instance.metroSystem;
    }
    
    /// <summary>
    /// Retourne la prochaine station pour un train donn�
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
            return line.stationIds[0]; // Retourner la premi�re station
        }
        
        // Prochaine station
        int nextIndex = currentIndex + 1;
        
        // Boucler en fin de ligne
        if (nextIndex >= line.stationIds.Count)
        {
            nextIndex = 0;
        }
        
        string nextStationId = line.stationIds[nextIndex];
        
        // V�rifier si la station est accessible
        StationData nextStation = metroSystem.GetStation(nextStationId);
        
        if (nextStation != null && nextStation.status == StationStatus.Broken)
        {
            Debug.LogWarning($"RoutePlanner: Next station {nextStationId} is broken, skipping");
            
            // Mettre � jour temporairement et r�essayer (r�cursif)
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
