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
    public float passengerGrowthRate = 10f;
    
    [Tooltip("Variation aléatoire du taux de croissance")]
    public float growthVariation = 2f;
    
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
        metroSystem = GameManager.Instance?.metroSystem;
        
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
        // ⏸️ Ne rien faire si le jeu est en pause
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
        {
            return;
        }
        
        if (isSimulating && GameManager.Instance != null && GameManager.Instance.currentState == AppState.Running)
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
        incidentTimer = 0f;
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
        // Debug: vérifier que la méthode est appelée
       // if (Time.frameCount % 300 == 0)
       // {
       //     Debug.Log($"🎲 MetroSimulator.UpdateSimulation() appelée - deltaTime: {deltaTime:F3}");
       // }
        
        // 1. Générer des passagers dans toutes les stations
        GeneratePassengers(deltaTime);
        
        // 2. Vérifier si on doit générer un incident
        incidentTimer += deltaTime;
        
        if (incidentTimer >= incidentCheckInterval)
        {
            TryGenerateIncident();
            incidentTimer = 0f;
        }
    }
    
    /// <summary>
    /// Génère des passagers dans toutes les stations
    /// </summary>
    private void GeneratePassengers(float deltaTime)
    {
        if (metroSystem == null || metroSystem.stations == null) return;
        
        foreach (var stationData in metroSystem.stations.Values)
        {
            // Sauter les stations en panne
            if (stationData.status == StationStatus.Broken)
                continue;
            
            // Calculer le nombre de passagers à ajouter
            float baseRate = passengerGrowthRate;
            
            // Variation aléatoire
            float variation = Random.Range(-growthVariation, growthVariation);
            float actualRate = baseRate + variation;
            
            // Ajouter les passagers (deltaTime pour avoir un taux par seconde)
            float passengersToAddFloat = actualRate * deltaTime;
            int passengersToAdd = Mathf.RoundToInt(passengersToAddFloat);
            
            if (passengersToAdd > 0)
            {
                stationData.passengerCount += passengersToAdd;
                
                // Limiter au maximum
                if (stationData.passengerCount > stationData.maxPassengers)
                {
                    stationData.passengerCount = stationData.maxPassengers;
                    
                    // Si surcharge, marquer comme Delayed
                    if (stationData.status == StationStatus.Normal)
                    {
                        stationData.status = StationStatus.Delayed;
                        UpdateStationVisual(stationData.stationId, StationStatus.Delayed);
                        
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.IncrementDelayCount();
                        }
                        
                        Debug.LogWarning($"⚠️ {stationData.stationName} surchargée! Passage en DELAYED");
                    }
                }
                
                // Debug occasionnel (toutes les 5 secondes environ)
               // if (Random.value < 0.02f) // 2% de chance à chaque frame
               // {
                //    Debug.Log($"👥 {stationData.stationName}: +{passengersToAdd} passagers (total: {stationData.passengerCount}/{stationData.maxPassengers})");
               // }
            }
        }
    }
    
    /// <summary>
    /// Essaye de générer un incident aléatoire
    /// </summary>
    private void TryGenerateIncident()
    {
        // Ne pas générer d'incident si le jeu est en pause
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
        {
            return;
        }
        
        // Lancer le dé
        float roll = Random.value;
        
        if (roll < incidentProbability)
        {
            Debug.Log($"🎲 Incident roll: {roll:F2} < {incidentProbability:F2} → Génération incident!");
            
            if (incidentGenerator != null)
            {
                incidentGenerator.GenerateRandomIncident();
            }
            else
            {
                Debug.LogWarning("⚠️ RandomIncidentGenerator not found! Cannot generate incident.");
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
        // ⏸️ Ne pas traiter les arrivées si le jeu est en pause
        if (GameManager.Instance != null && GameManager.Instance.IsPaused)
        {
            return;
        }
        
        if (metroSystem == null) return;
        
        TrainData train = metroSystem.GetTrain(trainId);
        StationData station = metroSystem.GetStation(stationId);
        
        if (train == null)
        {
            Debug.LogWarning($"MetroSimulator: Train {trainId} not found!");
            return;
        }
        
        if (station == null)
        {
            Debug.LogWarning($"MetroSimulator: Station {stationId} not found!");
            return;
        }
        
        // 1. Tous les passagers du train descendent
        int disembarking = train.currentPassengers;
        train.currentPassengers = 0;
        
        // 2. Des passagers en attente montent
        int availableSpace = train.passengerCapacity;
        int waiting = station.passengerCount;
        int boarding = Mathf.Min(availableSpace, waiting);
        
        train.currentPassengers = boarding;
        station.passengerCount -= boarding;
        
        //Debug.Log($"🚂 Train {trainId} à {station.stationName}: {disembarking} descendent, {boarding} montent");
        
        // 3. Si la station était en surcharge et redevient normale
        if (station.status == StationStatus.Delayed && station.passengerCount < station.maxPassengers * 0.7f)
        {
            station.status = StationStatus.Normal;
            UpdateStationVisual(stationId, StationStatus.Normal);
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DecrementDelayCount();
            }
            
            Debug.Log($"✅ {station.stationName} redevient normale");
        }
    }
    
    /// <summary>
    /// Met à jour le visuel d'une station
    /// </summary>
    private void UpdateStationVisual(string stationId, StationStatus newStatus)
    {
        StationController[] controllers = FindObjectsOfType<StationController>();
        
        foreach (var controller in controllers)
        {
            if (controller.stationId == stationId)
            {
                controller.SetStatus(newStatus);
                return;
            }
        }
    }
    
    /// <summary>
    /// Force la génération d'un incident (pour testing)
    /// </summary>
    public void ForceGenerateIncident()
    {
        Debug.Log("🔧 Force generating incident...");
        
        if (incidentGenerator != null)
        {
            incidentGenerator.GenerateRandomIncident();
        }
        else
        {
            Debug.LogWarning("⚠️ RandomIncidentGenerator not found!");
        }
    }
    
    /// <summary>
    /// Réinitialise le timer d'incidents
    /// </summary>
    public void ResetIncidentTimer()
    {
        incidentTimer = 0f;
        Debug.Log("🔄 Incident timer reset");
    }
    
    /// <summary>
    /// Retourne le temps restant avant le prochain check d'incident
    /// </summary>
    public float GetTimeUntilNextIncidentCheck()
    {
        return Mathf.Max(0, incidentCheckInterval - incidentTimer);
    }
    
    /// <summary>
    /// Retourne si la simulation est active
    /// </summary>
    public bool IsSimulating()
    {
        return isSimulating;
    }
}
