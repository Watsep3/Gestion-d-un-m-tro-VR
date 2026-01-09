using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Génère des incidents aléatoires sur le réseau de métro
/// </summary>
public class RandomIncidentGenerator : MonoBehaviour
{
    [System.Serializable]
    public class IncidentProbability
    {
        public IncidentType type;
        [Range(0f, 1f)]
        public float probability = 0.25f;
    }
    
    [Header("Incident Probabilities")]
    public List<IncidentProbability> incidentTypes = new List<IncidentProbability>
    {
        new IncidentProbability { type = IncidentType.StationBreakdown, probability = 0.4f },
        new IncidentProbability { type = IncidentType.LineDelay, probability = 0.3f },
        new IncidentProbability { type = IncidentType.Overcrowding, probability = 0.2f },
        new IncidentProbability { type = IncidentType.TrainMalfunction, probability = 0.1f }
    };
    
    [Header("Incident Duration")]
    public float defaultIncidentDuration = 60f;
    
    private MetroSystemManager metroSystem;
    private UIManager uiManager;
    
    void Start()
    {
        metroSystem = GameManager.Instance.metroSystem;
        uiManager = GameManager.Instance.uiManager;
        
        if (metroSystem == null)
        {
            Debug.LogError("RandomIncidentGenerator: MetroSystemManager not found!");
        }
    }
    
    /// <summary>
    /// Génère un incident aléatoire
    /// </summary>
    public void GenerateRandomIncident()
    {
        if (metroSystem == null) return;
        
        // Choisir un type d'incident selon les probabilités
        IncidentType type = ChooseIncidentType();
        
        Debug.Log($"🚨 Génération incident de type: {type}");
        
        // Appliquer l'incident selon son type
        switch (type)
        {
            case IncidentType.StationBreakdown:
                ApplyStationBreakdown();
                break;
                
            case IncidentType.LineDelay:
                ApplyLineDelay();
                break;
                
            case IncidentType.Overcrowding:
                ApplyOvercrowding();
                break;
                
            case IncidentType.TrainMalfunction:
                ApplyTrainMalfunction();
                break;
        }
    }
    
    /// <summary>
    /// Choisit un type d'incident selon les probabilités
    /// </summary>
    private IncidentType ChooseIncidentType()
    {
        float totalProbability = incidentTypes.Sum(i => i.probability);
        float roll = Random.value * totalProbability;
        
        float cumulative = 0f;
        foreach (var incident in incidentTypes)
        {
            cumulative += incident.probability;
            if (roll <= cumulative)
            {
                return incident.type;
            }
        }
        
        // Par défaut
        return IncidentType.StationBreakdown;
    }
    
    /// <summary>
    /// INCIDENT 1: Panne de station
    /// </summary>
    private void ApplyStationBreakdown()
    {
        // Trouver une station qui n'est pas déjà en panne
        var availableStations = metroSystem.stations.Values
            .Where(s => s.status == StationStatus.Normal)
            .ToList();
        
        if (availableStations.Count == 0)
        {
            Debug.LogWarning("Aucune station disponible pour une panne");
            return;
        }
        
        // Choisir une station aléatoire
        StationData station = availableStations[Random.Range(0, availableStations.Count)];
        
        // Appliquer la panne
        station.status = StationStatus.Broken;
        GameManager.Instance.IncrementDelayCount();
        
        // Trouver le StationController pour mettre à jour le visuel
        UpdateStationVisual(station.stationId, StationStatus.Broken);
        
        Debug.LogWarning($"🚨 PANNE: {station.stationName}");
        
        // Afficher un toast
        if (uiManager != null)
        {
            uiManager.ShowToast($"🚨 PANNE: {station.stationName}!", Color.red);
        }
        
        // Auto-réparation après un certain temps (optionnel)
        StartCoroutine(AutoResolveIncident(station.stationId, defaultIncidentDuration));
    }
    
    /// <summary>
    /// INCIDENT 2: Retard sur une ligne
    /// </summary>
    private void ApplyLineDelay()
    {
        var availableLines = metroSystem.lines.Values
            .Where(l => l.status == LineStatus.Active)
            .ToList();
        
        if (availableLines.Count == 0)
        {
            Debug.LogWarning("Aucune ligne disponible pour un retard");
            return;
        }
        
        LineData line = availableLines[Random.Range(0, availableLines.Count)];
        
        // Appliquer le retard
        line.status = LineStatus.Delayed;
        
        // Toutes les stations de cette ligne passent en Delayed
        foreach (string stationId in line.stationIds)
        {
            StationData station = metroSystem.GetStation(stationId);
            if (station != null && station.status == StationStatus.Normal)
            {
                station.status = StationStatus.Delayed;
                UpdateStationVisual(stationId, StationStatus.Delayed);
                GameManager.Instance.IncrementDelayCount();
            }
        }
        
        Debug.LogWarning($"⚠️ RETARD: Ligne {line.lineName}");
        
        if (uiManager != null)
        {
            uiManager.ShowToast($"⚠️ Retard: {line.lineName}", new Color(1f, 0.6f, 0f)); // Orange
        }
        
        // Auto-résolution
        StartCoroutine(AutoResolveLineDelay(line.lineId, defaultIncidentDuration * 0.5f));
    }
    
    /// <summary>
    /// INCIDENT 3: Surcharge soudaine de passagers
    /// </summary>
    private void ApplyOvercrowding()
    {
        var availableStations = metroSystem.stations.Values
            .Where(s => s.status == StationStatus.Normal)
            .ToList();
        
        if (availableStations.Count == 0) return;
        
        StationData station = availableStations[Random.Range(0, availableStations.Count)];
        
        // Ajouter soudainement beaucoup de passagers
        int surge = Random.Range(150, 300);
        station.passengerCount += surge;
        
        // Limiter au max
        if (station.passengerCount > station.maxPassengers)
        {
            station.passengerCount = station.maxPassengers;
        }
        
        Debug.LogWarning($"👥 SURCHARGE: {station.stationName} (+{surge} passagers)");
        
        if (uiManager != null)
        {
            uiManager.ShowToast($"👥 Affluence à {station.stationName}!", Color.yellow);
        }
    }
    
    /// <summary>
    /// INCIDENT 4: Train en panne
    /// </summary>
    private void ApplyTrainMalfunction()
    {
        var availableTrains = metroSystem.trains.Values
            .Where(t => t.status == TrainStatus.Moving)
            .ToList();
        
        if (availableTrains.Count == 0)
        {
            Debug.LogWarning("Aucun train disponible pour une panne");
            return;
        }
        
        TrainData train = availableTrains[Random.Range(0, availableTrains.Count)];
        
        // Mettre le train en maintenance
        train.status = TrainStatus.Maintenance;
        
        // Trouver le TrainController pour l'arrêter
        TrainController controller = FindTrainController(train.trainId);
        if (controller != null)
        {
            controller.Stop();
        }
        
        Debug.LogWarning($"🔧 PANNE TRAIN: {train.trainId}");
        
        if (uiManager != null)
        {
            uiManager.ShowToast($"🔧 Train {train.trainId} en panne!", Color.red);
        }
        
        // Auto-réparation
        StartCoroutine(AutoResolveTrainMalfunction(train.trainId, defaultIncidentDuration * 0.3f));
    }
    
    /// <summary>
    /// Auto-résolution d'une panne de station
    /// </summary>
    private IEnumerator AutoResolveIncident(string stationId, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        StationData station = metroSystem.GetStation(stationId);
        
        if (station != null && station.status == StationStatus.Broken)
        {
            station.status = StationStatus.Normal;
            UpdateStationVisual(stationId, StationStatus.Normal);
            GameManager.Instance.DecrementDelayCount();
            
            Debug.Log($"✅ Auto-réparation: {station.stationName}");
            
            if (uiManager != null)
            {
                uiManager.ShowToast($"✅ {station.stationName} réparée", Color.green);
            }
        }
    }
    
    /// <summary>
    /// Auto-résolution d'un retard de ligne
    /// </summary>
    private IEnumerator AutoResolveLineDelay(string lineId, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        LineData line = metroSystem.GetLine(lineId);
        
        if (line != null && line.status == LineStatus.Delayed)
        {
            line.status = LineStatus.Active;
            
            // Remettre les stations en normal
            foreach (string stationId in line.stationIds)
            {
                StationData station = metroSystem.GetStation(stationId);
                if (station != null && station.status == StationStatus.Delayed)
                {
                    station.status = StationStatus.Normal;
                    UpdateStationVisual(stationId, StationStatus.Normal);
                    GameManager.Instance.DecrementDelayCount();
                }
            }
            
            Debug.Log($"✅ Retard résolu: {line.lineName}");
            
            if (uiManager != null)
            {
                uiManager.ShowToast($"✅ {line.lineName} rétablie", Color.green);
            }
        }
    }
    
    /// <summary>
    /// Auto-résolution d'une panne de train
    /// </summary>
    private IEnumerator AutoResolveTrainMalfunction(string trainId, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        TrainData train = metroSystem.GetTrain(trainId);
        
        if (train != null && train.status == TrainStatus.Maintenance)
        {
            train.status = TrainStatus.Moving;
            
            TrainController controller = FindTrainController(trainId);
            if (controller != null)
            {
                // Redémarrer le train (Personne 1 devra implémenter la méthode Resume)
                // controller.Resume();
            }
            
            Debug.Log($"✅ Train {trainId} réparé");
            
            if (uiManager != null)
            {
                uiManager.ShowToast($"✅ Train {trainId} rétabli", Color.green);
            }
        }
    }
    
    /// <summary>
    /// Met à jour le visuel d'une station
    /// </summary>
    private void UpdateStationVisual(string stationId, StationStatus newStatus)
    {
        StationController controller = FindStationController(stationId);
        
        if (controller != null)
        {
            controller.SetStatus(newStatus);
        }
    }
    
    /// <summary>
    /// Trouve un StationController par son ID
    /// </summary>
    private StationController FindStationController(string stationId)
    {
        StationController[] controllers = FindObjectsOfType<StationController>();
        
        foreach (var controller in controllers)
        {
            if (controller.stationId == stationId)
            {
                return controller;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Trouve un TrainController par son ID
    /// </summary>
    private TrainController FindTrainController(string trainId)
    {
        TrainController[] controllers = FindObjectsOfType<TrainController>();
        
        foreach (var controller in controllers)
        {
            if (controller.trainId == trainId)
            {
                return controller;
            }
        }
        
        return null;
    }
}
