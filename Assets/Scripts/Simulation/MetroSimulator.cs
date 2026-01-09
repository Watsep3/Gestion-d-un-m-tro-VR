using UnityEngine;
using System.Collections;

/// <summary>
/// Gère la simulation continue du système de métro
/// Génère des passagers et déclenche des incidents
/// </summary>
public class MetroSimulator : MonoBehaviour
{
    [Header("Passenger Generation")]
    [Tooltip("Nombre de passagers ajoutés par seconde dans chaque station")]
    public float passengerGrowthRate = 2f;
    
    [Tooltip("Variation aléatoire du taux de croissance")]
    public float growthVariation = 1f;
    
    [Header("Incident Settings")]
    [Tooltip("Intervalle entre les tentatives de génération d'incidents (secondes)")]
    public float incidentCheckInterval = 30f;
    
    [Tooltip("Probabilité qu'un incident se produise à chaque intervalle (0-1)")]
    [Range(0f, 1f)]
    public float incidentProbability = 0.4f;
    
    [Header("References")]
    public RandomIncidentGenerator incidentGenerator;
    
    // Variables privées
    private MetroSystemManager metroSystem;
    private float incidentTimer = 0f;
    private bool isSimulating = false;
    
    void Start()
    {
        // Récupérer le MetroSystemManager
        metroSystem = GameManager.Instance.metroSystem;
        
        if (metroSystem == null)
        {
            Debug.LogError("MetroSimulator: MetroSystemManager not found!");
            return;
        }
        
        // Récupérer le générateur d'incidents
        if (incidentGenerator == null)
        {
            incidentGenerator = GetComponent<RandomIncidentGenerator>();
            
            if (incidentGenerator == null)
            {
                Debug.LogWarning("MetroSimulator: RandomIncidentGenerator not found! Adding it...");
                incidentGenerator = gameObject.AddComponent<RandomIncidentGenerator>();
            }
        }
        
        // Démarrer la simulation
        StartSimulation();
    }
    
    void Update()
    {
        if (isSimulating && GameManager.Instance.currentState == AppState.Running)
        {
            UpdateSimulation(Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Démarre la simulation
    /// </summary>
    public void StartSimulation()
    {
        isSimulating = true;
        Debug.Log("🎲 MetroSimulator started");
    }
    
    /// <summary>
    /// Arrête la simulation
    /// </summary>
    public void StopSimulation()
    {
        isSimulating = false;
        Debug.Log("🎲 MetroSimulator stopped");
    }
    
    /// <summary>
    /// Mise à jour de la simulation chaque frame
    /// </summary>
    public void UpdateSimulation(float deltaTime)
    {
        // 1. Générer des passagers dans toutes les stations
        GeneratePassengers(deltaTime);
        
        // 2. Vérifier si on doit générer un incident
        incidentTimer += deltaTime;
        
        if (incidentTimer >= incidentCheckInterval)
        {
            TryGenerateIncident();
            incidentTimer = 0f;
        }
        
        // 3. Les trains bougent automatiquement via TrainController.Update()
        // Pas besoin de code ici
    }
    
    /// <summary>
    /// Génère des passagers dans toutes les stations
    /// </summary>
    private void GeneratePassengers(float deltaTime)
    {
        if (metroSystem == null || metroSystem.stations == null) return;
        
        foreach (var station in metroSystem.stations.Values)
        {
            // Sauter les stations en panne
            if (station.status == StationStatus.Broken)
                continue;
            
            // Calculer le nombre de passagers à ajouter
            float baseRate = passengerGrowthRate;
            
            // Variation aléatoire
            float variation = Random.Range(-growthVariation, growthVariation);
            float actualRate = baseRate + variation;
            
            // Ajouter les passagers
            int passengersToAdd = Mathf.RoundToInt(actualRate * deltaTime);
            station.passengerCount += passengersToAdd;
            
            // Limiter au maximum
            if (station.passengerCount > station.maxPassengers)
            {
                station.passengerCount = station.maxPassengers;
                
                // Si surcharge, marquer comme Delayed
                if (station.status == StationStatus.Normal)
                {
                    station.status = StationStatus.Delayed;
                    GameManager.Instance.IncrementDelayCount();
                    
                    Debug.LogWarning($"⚠️ {station.stationName} surchargée! Passage en DELAYED");
                }
            }
        }
    }
    
    /// <summary>
    /// Essaye de générer un incident aléatoire
    /// </summary>
    private void TryGenerateIncident()
    {
        // Lancer le dé
        float roll = Random.value;
        
        if (roll < incidentProbability)
        {
            Debug.Log($"🎲 Incident roll: {roll:F2} < {incidentProbability:F2} → Génération incident!");
            
            if (incidentGenerator != null)
            {
                incidentGenerator.GenerateRandomIncident();
            }
        }
        else
        {
            Debug.Log($"🎲 Incident roll: {roll:F2} >= {incidentProbability:F2} → Pas d'incident cette fois");
        }
    }
    
    /// <summary>
    /// Gère l'arrivée d'un train à une station
    /// Appelé par TrainController quand un train arrive
    /// </summary>
    public void ProcessTrainArrival(string trainId, string stationId)
    {
        if (metroSystem == null) return;
        
        TrainData train = metroSystem.GetTrain(trainId);
        StationData station = metroSystem.GetStation(stationId);
        
        if (train == null || station == null) return;
        
        // 1. Tous les passagers du train descendent
        int disembarking = train.currentPassengers;
        train.currentPassengers = 0;
        
        // 2. Des passagers en attente montent
        int availableSpace = train.passengerCapacity;
        int waiting = station.passengerCount;
        int boarding = Mathf.Min(availableSpace, waiting);
        
        train.currentPassengers = boarding;
        station.passengerCount -= boarding;
        
        Debug.Log($"🚂 Train {trainId} à {station.stationName}: {disembarking} descendent, {boarding} montent");
        
        // 3. Si la station était en surcharge et redevient normale
        if (station.status == StationStatus.Delayed && station.passengerCount < station.maxPassengers * 0.7f)
        {
            station.status = StationStatus.Normal;
            GameManager.Instance.DecrementDelayCount();
            Debug.Log($"✅ {station.stationName} redevient normale");
        }
    }
}