using UnityEngine;
using System.Collections.Generic;

public class MetroSystemManager : MonoBehaviour
{
    [Header("Data")]
    public List<StationDataSO> stationConfigs;
    public List<LineDataSO> lineConfigs;
    
    [Header("Runtime Data")]
    public Dictionary<string, StationData> stations = new Dictionary<string, StationData>();
    public Dictionary<string, LineData> lines = new Dictionary<string, LineData>();
    public Dictionary<string, TrainData> trains = new Dictionary<string, TrainData>();
    
    [Header("Simulation")]
    public float simulationSpeed = 1f;
    public bool isSimulating = false;
    
    public void Initialize() 
    { 
        Debug.Log(" MetroSystemManager - Initializing...");
        
        // Charger les données depuis les ScriptableObjects
        LoadStationsFromConfig();
        LoadLinesFromConfig();
        
        Debug.Log($" Loaded {stations.Count} stations and {lines.Count} lines");
    }
    
    private void LoadStationsFromConfig()
    {
        if (stationConfigs == null || stationConfigs.Count == 0)
        {
            Debug.LogWarning(" No station configs found!");
            return;
        }
        
        foreach (var config in stationConfigs)
        {
            StationData data = new StationData
            {
                stationId = config.stationId,
                stationName = config.stationName,
                position = config.worldPosition,
                status = StationStatus.Normal,
                passengerCount = 0,
                maxPassengers = config.maxPassengers
            };
            
            stations[data.stationId] = data;
        }
    }
    
    private void LoadLinesFromConfig()
    {
        if (lineConfigs == null || lineConfigs.Count == 0)
        {
            Debug.LogWarning(" No line configs found!");
            return;
        }
        
        foreach (var config in lineConfigs)
        {
            LineData data = new LineData
            {
                lineId = config.lineId,
                lineName = config.lineName,
                lineColor = config.lineColor,
                status = LineStatus.Active,
                trainCount = config.defaultTrainCount
            };
            
            // Convertir les StationDataSO en IDs
            foreach (var stationSO in config.stations)
            {
                data.stationIds.Add(stationSO.stationId);
            }
            
            lines[data.lineId] = data;
        }
    }
    
    public void StartSimulation() 
    { 
        isSimulating = true;
        Debug.Log(" Simulation started");
    }
    
    public void StopSimulation() 
    { 
        isSimulating = false;
        Debug.Log(" Simulation stopped");
    }
    
    public void UpdateSystem(float deltaTime) 
    { 
        if (!isSimulating) return;
        
        // TODO: Update logic ici plus tard
    }
    
    // Getters
    public StationData GetStation(string id) 
    { 
        if (stations.ContainsKey(id))
            return stations[id];
        
        Debug.LogWarning($"Station not found: {id}");
        return null;
    }
    
    public LineData GetLine(string id) 
    { 
        if (lines.ContainsKey(id))
            return lines[id];
        
        Debug.LogWarning($"Line not found: {id}");
        return null;
    }
    
    public TrainData GetTrain(string id) 
    { 
        if (trains.ContainsKey(id))
            return trains[id];
        
        Debug.LogWarning($"Train not found: {id}");
        return null;
    }
    
    // Actions
    public void UpdateStationStatus(string stationId, StationStatus newStatus) 
    { 
        if (stations.ContainsKey(stationId))
        {
            stations[stationId].status = newStatus;
            Debug.Log($" Station {stationId} status changed to {newStatus}");
        }
    }
    
    public void RerouteTrain(string trainId, string newLineId) 
    { 
        if (trains.ContainsKey(trainId))
        {
            trains[trainId].lineId = newLineId;
            Debug.Log($"Train {trainId} rerouted to line {newLineId}");
        }
    }
}