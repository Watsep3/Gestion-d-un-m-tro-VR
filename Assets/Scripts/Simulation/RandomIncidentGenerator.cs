using UnityEngine;
using System.Collections.Generic;

public class RandomIncidentGenerator : MonoBehaviour
{
    [System.Serializable]
    public class IncidentTypeConfig
    {
        public IncidentType type;
        public float probability;
        public StationStatus resultStatus;
    }
    
    public List<IncidentTypeConfig> possibleIncidents = new List<IncidentTypeConfig>();
    
    public void GenerateIncident(StationData station) 
    { 
        IncidentTypeConfig incident = GetRandomIncident();
        if (incident != null)
        {
            station.status = incident.resultStatus;
            Debug.Log($" Incident generated: {incident.type} on {station.stationName}");
        }
    }
    
    public IncidentTypeConfig GetRandomIncident() 
    { 
        if (possibleIncidents == null || possibleIncidents.Count == 0)
        {
            Debug.LogWarning(" No incidents configured!");
            return null;
        }
        
        float totalProbability = 0f;
        foreach (var incident in possibleIncidents)
        {
            totalProbability += incident.probability;
        }
        
        float randomValue = Random.Range(0f, totalProbability);
        float cumulativeProbability = 0f;
        
        foreach (var incident in possibleIncidents)
        {
            cumulativeProbability += incident.probability;
            if (randomValue <= cumulativeProbability)
            {
                return incident;
            }
        }
        
        return possibleIncidents[0]; // Fallback
    }
}