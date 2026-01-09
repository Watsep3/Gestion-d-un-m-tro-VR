using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simule le flux de passagers dans le système de métro
/// Gère l'embarquement, le débarquement et le mouvement des passagers
/// </summary>
public class PassengerFlowSimulator : MonoBehaviour
{
    [Header("Flow Settings")]
    [Tooltip("Taux de base d'embarquement de passagers par seconde")]
    public float boardingRate = 50f;
    
    [Tooltip("Taux de base de débarquement de passagers par seconde")]
    public float alightingRate = 60f;
    
    [Tooltip("Temps minimal d'arrêt à une station (secondes)")]
    public float minimumStopTime = 1f;
    
    [Tooltip("Temps supplémentaire par 10 passagers qui montent/descendent")]
    public float timePerTenPassengers = 0.5f;
    
    [Header("References")]
    private MetroSystemManager metroSystem;
    
    void Start()
    {
        metroSystem = GameManager.Instance?.metroSystem;
        
        if (metroSystem == null)
        {
            Debug.LogError("PassengerFlowSimulator: MetroSystemManager not found!");
        }
    }
    
    /// <summary>
    /// Traite l'arrivée d'un train à une station
    /// Gère le débarquement puis l'embarquement des passagers
    /// </summary>
    public float ProcessTrainArrival(string trainId, string stationId)
    {
        if (metroSystem == null) return minimumStopTime;
        
        TrainData train = metroSystem.GetTrain(trainId);
        StationData station = metroSystem.GetStation(stationId);
        
        if (train == null || station == null)
        {
            Debug.LogWarning($"PassengerFlowSimulator: Train {trainId} or Station {stationId} not found!");
            return minimumStopTime;
        }
        
        // 1. DÉBARQUEMENT - Tous les passagers descendent
        int passengersAlighting = train.currentPassengers;
        train.currentPassengers = 0;
        
        // 2. EMBARQUEMENT - Des passagers en attente montent
        int availableSpace = train.passengerCapacity - train.currentPassengers;
        int waitingPassengers = station.passengerCount;
        int passengersBoarding = Mathf.Min(availableSpace, waitingPassengers);
        
        // Mettre à jour les données
        train.currentPassengers += passengersBoarding;
        station.passengerCount -= passengersBoarding;
        
        // 3. CALCULER LE TEMPS D'ARRÊT
        int totalMovement = passengersAlighting + passengersBoarding;
        float stopDuration = CalculateStopDuration(totalMovement);
        
        // 4. LOG
        Debug.Log($"🚂 {trainId} à {station.stationName}: {passengersAlighting}↓ {passengersBoarding}↑ (arrêt: {stopDuration:F1}s)");
        
        // 5. Vérifier si la station redevient normale après départ du train
        CheckStationStatus(station);
        
        return stopDuration;
    }
    
    /// <summary>
    /// Calcule la durée d'arrêt en fonction du nombre de passagers
    /// </summary>
    private float CalculateStopDuration(int totalPassengers)
    {
        if (totalPassengers == 0)
            return minimumStopTime;
        
        // Temps de base + temps supplémentaire proportionnel
        float extraTime = (totalPassengers / 10f) * timePerTenPassengers;
        float totalTime = minimumStopTime + extraTime;
        
        // Limiter à un maximum raisonnable (10 secondes max)
        return Mathf.Min(totalTime, 10f);
    }
    
    /// <summary>
    /// Vérifie et met à jour le statut d'une station
    /// </summary>
    private void CheckStationStatus(StationData station)
    {
        if (station == null) return;
        
        float occupancyRate = (float)station.passengerCount / station.maxPassengers;
        
        // Station surchargée (>90%)
        if (occupancyRate > 0.9f && station.status == StationStatus.Normal)
        {
            station.status = StationStatus.Delayed;
            UpdateStationVisual(station.stationId, StationStatus.Delayed);
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.IncrementDelayCount();
            }
            
            Debug.LogWarning($"⚠️ {station.stationName} surchargée ({occupancyRate * 100:F0}%)! Status: DELAYED");
        }
        // Station redevient normale (<70%)
        else if (occupancyRate < 0.7f && station.status == StationStatus.Delayed)
        {
            station.status = StationStatus.Normal;
            UpdateStationVisual(station.stationId, StationStatus.Normal);
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DecrementDelayCount();
            }
            
            Debug.Log($"✅ {station.stationName} redevient normale ({occupancyRate * 100:F0}%)");
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
    /// Simule l'afflux soudain de passagers à une station
    /// </summary>
    public void SimulatePassengerSurge(string stationId, int surgeAmount)
    {
        if (metroSystem == null) return;
        
        StationData station = metroSystem.GetStation(stationId);
        
        if (station == null)
        {
            Debug.LogWarning($"PassengerFlowSimulator: Station {stationId} not found!");
            return;
        }
        
        int previousCount = station.passengerCount;
        station.passengerCount += surgeAmount;
        
        // Limiter au maximum
        if (station.passengerCount > station.maxPassengers)
        {
            station.passengerCount = station.maxPassengers;
        }
        
        Debug.Log($"👥 Afflux à {station.stationName}: {previousCount} → {station.passengerCount} (+{surgeAmount})");
        
        // Vérifier le statut
        CheckStationStatus(station);
    }
    
    /// <summary>
    /// Évacue progressivement les passagers d'une station (pour urgence)
    /// </summary>
    public void EvacuateStation(string stationId)
    {
        if (metroSystem == null) return;
        
        StationData station = metroSystem.GetStation(stationId);
        
        if (station == null)
        {
            Debug.LogWarning($"PassengerFlowSimulator: Station {stationId} not found!");
            return;
        }
        
        int evacuatedCount = station.passengerCount;
        station.passengerCount = 0;
        
        Debug.LogWarning($"🚨 Évacuation de {station.stationName}: {evacuatedCount} passagers évacués");
        
        // Mettre à jour le visuel
        CheckStationStatus(station);
    }
    
    /// <summary>
    /// Retourne le taux d'occupation d'une station (0-1)
    /// </summary>
    public float GetStationOccupancy(string stationId)
    {
        if (metroSystem == null) return 0f;
        
        StationData station = metroSystem.GetStation(stationId);
        
        if (station == null || station.maxPassengers == 0)
            return 0f;
        
        return (float)station.passengerCount / station.maxPassengers;
    }
    
    /// <summary>
    /// Retourne le taux de remplissage d'un train (0-1)
    /// </summary>
    public float GetTrainOccupancy(string trainId)
    {
        if (metroSystem == null) return 0f;
        
        TrainData train = metroSystem.GetTrain(trainId);
        
        if (train == null || train.passengerCapacity == 0)
            return 0f;
        
        return (float)train.currentPassengers / train.passengerCapacity;
    }
    
    /// <summary>
    /// Retourne les statistiques de flux de passagers
    /// </summary>
    public PassengerFlowStats GetFlowStats()
    {
        if (metroSystem == null)
            return new PassengerFlowStats();
        
        PassengerFlowStats stats = new PassengerFlowStats();
        
        // Compter les passagers dans les stations
        foreach (var station in metroSystem.stations.Values)
        {
            stats.totalWaitingPassengers += station.passengerCount;
            
            if (station.passengerCount > station.maxPassengers * 0.9f)
            {
                stats.overcrowdedStationCount++;
            }
        }
        
        // Compter les passagers dans les trains
        foreach (var train in metroSystem.trains.Values)
        {
            stats.totalPassengersInTransit += train.currentPassengers;
            
            if (train.currentPassengers > train.passengerCapacity * 0.9f)
            {
                stats.fullTrainCount++;
            }
        }
        
        stats.totalPassengers = stats.totalWaitingPassengers + stats.totalPassengersInTransit;
        
        return stats;
    }
}

/// <summary>
/// Structure contenant les statistiques de flux de passagers
/// </summary>
[System.Serializable]
public struct PassengerFlowStats
{
    public int totalPassengers;
    public int totalWaitingPassengers;
    public int totalPassengersInTransit;
    public int overcrowdedStationCount;
    public int fullTrainCount;
    
    public override string ToString()
    {
        return $"Total: {totalPassengers} | Attente: {totalWaitingPassengers} | En transit: {totalPassengersInTransit} | Stations surchargées: {overcrowdedStationCount} | Trains pleins: {fullTrainCount}";
    }
}