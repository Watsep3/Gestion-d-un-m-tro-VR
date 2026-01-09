using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StationController : MonoBehaviour, IInteractable
{
    [Header("Data")]
    public string stationId;
    public StationData data;
    
    [Header("Visual")]
    public GameObject visualModel;
    public Renderer statusRenderer;
    public TextMeshPro stationLabel;
    
    [Header("Status Colors")]
    public Color normalColor = Color.green;
    public Color delayedColor = Color.yellow;
    public Color brokenColor = Color.red;
    
    /// <summary>
    /// Initialise la station avec ses données
    /// </summary>
    public void Initialize(StationData stationData)
    {
        // Sauvegarder les données
        data = stationData;
        stationId = stationData.stationId;
        
        Debug.Log($"📍 Initializing station: {stationData.stationName}");
        Debug.Log($"   → ID: {stationData.stationId}");
        Debug.Log($"   → Position: {stationData.position}");
        Debug.Log($"   → Max Passengers: {stationData.maxPassengers}");
        
        // Configurer le label
        if (stationLabel != null)
        {
            stationLabel.text = stationData.stationName;
        }
        else
        {
            Debug.LogWarning($"⚠️ Station {stationData.stationName}: stationLabel is null!");
        }
        
        // Mettre à jour l'apparence
        UpdateVisuals();
        
        Debug.Log($"✅ Station {stationData.stationName} initialized");
    }
    
    /// <summary>
    /// Met à jour l'apparence visuelle selon le status
    /// </summary>
    public void UpdateVisuals()
    {
        if (statusRenderer == null)
        {
            Debug.LogWarning($"⚠️ Station {data.stationName}: statusRenderer is null!");
            return;
        }
        
        // Changer la couleur selon le status
        switch (data.status)
        {
            case StationStatus.Normal:
                statusRenderer.material.color = normalColor;
                break;
            case StationStatus.Delayed:
                statusRenderer.material.color = delayedColor;
                break;
            case StationStatus.Broken:
                statusRenderer.material.color = brokenColor;
                break;
        }
    }
    
    /// <summary>
    /// Change le status de la station
    /// </summary>
    public void SetStatus(StationStatus newStatus)
    {
        data.status = newStatus;
        UpdateVisuals();
        
        Debug.Log($"🔄 {data.stationName} status changed to: {newStatus}");
    }
    
    /// <summary>
    /// Appelé quand on clique sur la station (legacy)
    /// </summary>
    public void OnStationClicked()
    {
        Debug.Log($"Station clicked: {data.stationName}");
        OnSelected();
    }
    
    // ========================================
    // INTERFACE IINTERACTABLE
    // ========================================
    
    /// <summary>
    /// Appelé quand on clique sur la station
    /// </summary>
    public void OnSelected()
    {
        Debug.Log($"✅ Station {data.stationName} sélectionnée");

            if (GameManager.Instance != null && GameManager.Instance.metroSystem != null)
    {
        StationData freshData = GameManager.Instance.metroSystem.GetStation(stationId);
        if (freshData != null)
        {
            data = freshData; // Mettre à jour la référence locale
        }
    }
    
        
        // Afficher les infos dans la console
        Debug.Log($"   - État: {data.status}");
        Debug.Log($"   - Passagers: {data.passengerCount}/{data.maxPassengers}");
        Debug.Log($"   - Position: {data.position}");
        
        // Ouvrir le panel UI
        UIManager uiManager = GameManager.Instance.uiManager;
        if (uiManager != null)
        {
            uiManager.ShowStationPanel(data);
        }
        else
        {
            Debug.LogWarning("UIManager not found!");
        }
    }
    
    /// <summary>
    /// Appelé quand on clique ailleurs (désélection)
    /// </summary>
    public void OnDeselected()
    {
        Debug.Log($"❌ Station {data.stationName} désélectionnée");
    }
    
    /// <summary>
    /// Action spéciale: réparer la station si en panne
    /// </summary>
    public void OnAction()
    {
        if (data.status == StationStatus.Broken)
        {
            Debug.Log($"🔧 Réparation de {data.stationName}...");
            SetStatus(StationStatus.Normal);
            Debug.Log($"✅ {data.stationName} réparée!");
        }
        else
        {
            Debug.Log($"ℹ️ {data.stationName} n'a pas besoin de réparation (état: {data.status})");
        }
    }
    
    /// <summary>
    /// Retourne une description de la station
    /// </summary>
    public string GetInteractionInfo()
    {
        return $"{data.stationName} - {data.passengerCount}/{data.maxPassengers} passagers - État: {data.status}";
    }
}