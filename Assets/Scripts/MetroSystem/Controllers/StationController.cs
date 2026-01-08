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
    
    public void Initialize(StationData stationData) { }
    public void UpdateVisuals() { }
    public void SetStatus(StationStatus newStatus) { }
    public void OnStationClicked() { }
    // ========================================
    // INTERFACE IINTERACTABLE
    // ========================================
    
    /// <summary>
    /// Appelé quand on clique sur la station
    /// </summary>
    public void OnSelected()
    {
        Debug.Log($"✅ Station {data.stationName} sélectionnée");
        
        // Afficher les infos dans la console
        Debug.Log($"   - État: {data.status}");
        Debug.Log($"   - Passagers: {data.passengerCount}/{data.maxPassengers}");
        
        // TODO: Plus tard, Personne 3 (UI) utilisera ça pour ouvrir le panel
        // UIManager uiManager = FindObjectOfType<UIManager>();
        // if (uiManager != null)
        // {
        //     uiManager.ShowStationPanel(data);
        // }
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
            Debug.Log($"{data.stationName} n'a pas besoin de réparation");
        }
    }
    
    /// <summary>
    /// Retourne une description de la station
    /// </summary>
    public string GetInteractionInfo()
    {
        return $"{data.stationName} - {data.passengerCount} passagers - État: {data.status}";
    }
}