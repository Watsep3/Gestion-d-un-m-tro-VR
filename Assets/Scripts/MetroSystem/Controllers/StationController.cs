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
    
    [Header("Selection")]
    public Color selectionColor = Color.cyan;
    private bool isSelected = false;
    private Color currentStatusColor;
    
    void Start()
    {
        // Auto-assigner le Renderer si non assigné
        if (statusRenderer == null)
        {
            statusRenderer = GetComponent<Renderer>();
            
            if (statusRenderer == null)
            {
                statusRenderer = GetComponentInChildren<Renderer>();
            }
            
            if (statusRenderer != null)
            {
                Debug.Log($"✅ Station {stationId}: Renderer auto-assigné ({statusRenderer.name})");
            }
            else
            {
                Debug.LogWarning($"⚠️ Station {stationId}: Aucun Renderer trouvé!");
            }
        }
        
        // Auto-assigner le label si non assigné
        if (stationLabel == null)
        {
            stationLabel = GetComponentInChildren<TextMeshPro>();
            
            if (stationLabel != null)
            {
                Debug.Log($"✅ Station {stationId}: Label auto-assigné");
            }
        }
    }
    
void Update()
{
    // Mettre à jour l'affichage si la station est sélectionnée
    if (isSelected && GameManager.Instance != null && GameManager.Instance.uiManager != null)
    {
        // Rafraîchir les données depuis le système
        RefreshData();
        
        // Mettre à jour le panel UI toutes les 0.5 secondes
        if (Time.frameCount % 30 == 0) // ~0.5s à 60 FPS
        {
            GameManager.Instance.uiManager.ShowStationPanel(data);
        }
    }
    
    // ✅ AJOUT : Mettre à jour le label avec le nombre de passagers
    if (stationLabel != null && data != null)
    {
        // Rafraîchir les données depuis le système
        RefreshData();
        
        // Mettre à jour le texte du label
        stationLabel.text = $"{data.stationName}\n{data.passengerCount}/{data.maxPassengers}";
    }
}
    
    /// <summary>
    /// Rafraîchit les données de la station depuis le MetroSystemManager
    /// </summary>
    private void RefreshData()
    {
        if (GameManager.Instance != null && GameManager.Instance.metroSystem != null)
        {
            StationData freshData = GameManager.Instance.metroSystem.GetStation(stationId);
            if (freshData != null)
            {
                data = freshData;
            }
        }
    }
    
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
            Debug.LogWarning($"⚠️ Station {data?.stationName ?? stationId}: statusRenderer is null!");
            return;
        }
        
        // Si la station est sélectionnée, garder la couleur de sélection
        if (isSelected)
        {
            statusRenderer.material.color = selectionColor;
            Debug.Log($"💠 Station {data?.stationName ?? stationId}: Couleur de sélection maintenue");
            return;
        }
        
        if (data == null)
        {
            Debug.LogWarning($"⚠️ Station {stationId}: data is null!");
            return;
        }
        
        // Changer la couleur selon le status
        switch (data.status)
        {
            case StationStatus.Normal:
                currentStatusColor = normalColor;
                statusRenderer.material.color = normalColor;
                Debug.Log($"🟢 Station {data.stationName}: Normal - couleur verte");
                break;
            case StationStatus.Delayed:
                currentStatusColor = delayedColor;
                statusRenderer.material.color = delayedColor;
                Debug.Log($"🟡 Station {data.stationName}: Delayed - couleur jaune");
                break;
            case StationStatus.Broken:
                currentStatusColor = brokenColor;
                statusRenderer.material.color = brokenColor;
                Debug.Log($"🔴 Station {data.stationName}: Broken - couleur rouge");
                break;
        }
    }
    
    /// <summary>
    /// Change le status de la station
    /// </summary>
    public void SetStatus(StationStatus newStatus)
    {
        if (data == null)
        {
            Debug.LogWarning($"⚠️ Station {stationId}: Cannot set status, data is null!");
            return;
        }
        
        data.status = newStatus;
        
        // Mettre à jour aussi dans le MetroSystemManager
        if (GameManager.Instance != null && GameManager.Instance.metroSystem != null)
        {
            StationData systemData = GameManager.Instance.metroSystem.GetStation(stationId);
            if (systemData != null)
            {
                systemData.status = newStatus;
            }
        }
        
        // Ne mettre à jour les visuels que si pas sélectionné
        if (!isSelected)
        {
            UpdateVisuals();
        }
        
        Debug.Log($"🔄 {data.stationName} status changed to: {newStatus}");
        
        // Afficher un toast
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            string message = "";
            Color toastColor = Color.white;
            
            switch (newStatus)
            {
                case StationStatus.Normal:
                    message = $"{data.stationName} fonctionne normalement";
                    toastColor = Color.green;
                    break;
                case StationStatus.Delayed:
                    message = $"{data.stationName} est en retard!";
                    toastColor = Color.yellow;
                    break;
                case StationStatus.Broken:
                    message = $"{data.stationName} est en panne!";
                    toastColor = Color.red;
                    break;
            }
            
            GameManager.Instance.uiManager.ShowToast(message, toastColor);
        }
    }
    
    /// <summary>
    /// Appelé quand on clique sur la station (legacy)
    /// </summary>
    public void OnStationClicked()
    {
        Debug.Log($"Station clicked: {data?.stationName ?? stationId}");
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
        // Rafraîchir les données
        RefreshData();
        
        if (data == null)
        {
            Debug.LogWarning($"⚠️ Station {stationId}: data is null in OnSelected!");
            return;
        }
        
        Debug.Log($"✅ Station {data.stationName} sélectionnée");
        Debug.Log($"   - État: {data.status}");
        Debug.Log($"   - Passagers: {data.passengerCount}/{data.maxPassengers}");
        Debug.Log($"   - Position: {data.position}");
        
        // Marquer comme sélectionné AVANT de changer la couleur
        isSelected = true;
        
        // ✅ Changer la couleur en cyan pour la sélection
        if (statusRenderer != null)
        {
            statusRenderer.material.color = selectionColor;
            Debug.Log($"💠 Station {data.stationName}: Couleur de sélection (cyan) appliquée");
        }
        
        // Ouvrir le panel UI
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.ShowStationPanel(data);
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
        if (data != null)
        {
            Debug.Log($"❌ Station {data.stationName} désélectionnée");
        }
        
        // Marquer comme non sélectionné
        isSelected = false;
        
        // ✅ Restaurer immédiatement la couleur selon le status
        UpdateVisuals();
        Debug.Log($"🎨 Station {data?.stationName ?? stationId}: Couleur restaurée après désélection");
        
        // Cacher le panel UI
        if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
        {
            GameManager.Instance.uiManager.HideStationPanel();
        }
    }
    
    /// <summary>
    /// Action spéciale: réparer la station si en panne
    /// </summary>
    public void OnAction()
    {
        if (data == null)
        {
            Debug.LogWarning($"⚠️ Station {stationId}: Cannot perform action, data is null!");
            return;
        }
        
        if (data.status == StationStatus.Broken)
        {
            Debug.Log($"🔧 Réparation de {data.stationName}...");
            SetStatus(StationStatus.Normal);
            
            // Décrémenter le compteur de retards
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DecrementDelayCount();
            }
            
            Debug.Log($"✅ {data.stationName} réparée!");
            
            // Toast de succès
            if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
            {
                GameManager.Instance.uiManager.ShowSuccessToast($"{data.stationName} réparée avec succès!");
            }
        }
        else if (data.status == StationStatus.Delayed)
        {
            Debug.Log($"🔧 {data.stationName} retour à la normale...");
            SetStatus(StationStatus.Normal);
            
            // Décrémenter le compteur de retards
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DecrementDelayCount();
            }
            
            Debug.Log($"✅ {data.stationName} fonctionne normalement!");
        }
        else
        {
            Debug.Log($"ℹ️ {data.stationName} fonctionne normalement (état: {data.status})");
        }
    }
    
    /// <summary>
    /// Retourne une description de la station
    /// </summary>
    public string GetInteractionInfo()
    {
        if (data == null)
        {
            return $"Station {stationId} (pas de données)";
        }
        
        return $"{data.stationName} - {data.passengerCount}/{data.maxPassengers} passagers - État: {data.status}";
    }
    
    /// <summary>
    /// Force la mise à jour du label (utile pour les tests)
    /// </summary>
    public void UpdateLabel()
    {
        if (stationLabel != null && data != null)
        {
            stationLabel.text = $"{data.stationName}\n{data.passengerCount}/{data.maxPassengers}";
        }
    }
}