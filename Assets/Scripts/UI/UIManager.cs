using UnityEngine;
using TMPro;

/// <summary>
/// Gère toute l'interface utilisateur du jeu
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Dashboard")]
    public GlobalDashboardPanel globalDashboard;
    
    [Header("Panels")]
    public StationPanel stationPanel;
    public TrainPanel trainPanel;
    
    [Header("Toast Messages")]
    public GameObject toastPrefab;
    public Transform toastContainer;
    
    [Header("Selection Info")]
    public TextMeshProUGUI selectionInfoText;
    
    void Start()
    {
        // Initialiser l'UI
        HideAllPanels();
        
        if (selectionInfoText != null)
        {
            selectionInfoText.text = "Cliquez sur un objet pour le sélectionner";
        }
        
        // Test initial
        Debug.Log($"✅ UIManager Start - GlobalDashboard: {(globalDashboard != null ? "OK" : "NULL")}");
        
        if (globalDashboard != null)
        {
            Debug.Log("✅ Première mise à jour du dashboard...");
            globalDashboard.UpdateDisplay();
        }
        else
        {
            Debug.LogError("❌ GlobalDashboard is NULL in UIManager!");
        }
    }
    
    void Update()
    {
        // Mettre à jour le dashboard en continu
        if (globalDashboard != null && GameManager.Instance != null)
        {
            if (GameManager.Instance.currentState == AppState.Running)
            {
                globalDashboard.UpdateDisplay();
            }
        }
    }
    
    /// <summary>
    /// Met à jour le dashboard global
    /// </summary>
    public void UpdateDashboard()
    {
        if (globalDashboard != null)
        {
            globalDashboard.UpdateDisplay();
        }
        else
        {
            Debug.LogWarning("❌ UIManager: globalDashboard is null!");
        }
    }
    
    /// <summary>
    /// Affiche le panel d'information d'une station
    /// </summary>
    public void ShowStationPanel(StationData station)
    {
        if (stationPanel != null)
        {
            // Cacher les autres panels
            HideTrainPanel();
            
            // Afficher le panel de station
            stationPanel.Show(station);
            
            Debug.Log($"✅ Station panel affiché pour {station.stationName}");
        }
        else
        {
            Debug.LogWarning("UIManager: StationPanel is null!");
        }
        
        // Mettre à jour le texte de sélection
        if (selectionInfoText != null)
        {
            selectionInfoText.text = $"Station: {station.stationName} - {station.passengerCount} passagers";
        }
    }
    
    /// <summary>
    /// Cache le panel de station
    /// </summary>
    public void HideStationPanel()
    {
        if (stationPanel != null)
        {
            stationPanel.Hide();
            //Debug.Log("❌ Station panel caché");
        }
    }
    
    /// <summary>
    /// Affiche le panel d'information d'un train
    /// </summary>
    public void ShowTrainPanel(TrainData train)
    {
        if (trainPanel != null)
        {
            // Cacher les autres panels
            HideStationPanel();
            
            // Afficher le panel de train
            trainPanel.Show(train);
            
            Debug.Log($"✅ Train panel affiché pour {train.trainId}");
        }
        else
        {
            Debug.LogWarning("UIManager: TrainPanel is null!");
        }
        
        // Mettre à jour le texte de sélection
        if (selectionInfoText != null)
        {
            selectionInfoText.text = $"Train {train.trainId} - Ligne {train.lineId} - {train.currentPassengers}/{train.passengerCapacity} passagers";
        }
    }
    
    /// <summary>
    /// Cache le panel de train
    /// </summary>
    public void HideTrainPanel()
    {
        if (trainPanel != null)
        {
            trainPanel.Hide();
           // Debug.Log("❌ Train panel caché");
        }
    }
    
    /// <summary>
    /// Cache tous les panels
    /// </summary>
    public void HideAllPanels()
    {
        HideStationPanel();
        HideTrainPanel();
        
        if (selectionInfoText != null)
        {
            selectionInfoText.text = "Aucune sélection";
        }
        
        Debug.Log("❌ Tous les panels cachés");
    }
    
    /// <summary>
    /// Affiche un message toast temporaire
    /// </summary>
    public void ShowToast(string message, Color color)
    {
        if (toastPrefab == null || toastContainer == null)
        {
            Debug.LogWarning("UIManager: Toast prefab or container is null!");
            return;
        }
        
        // Instancier le toast
        GameObject toastObj = Instantiate(toastPrefab, toastContainer);
        
        // Configurer
        ToastMessage toast = toastObj.GetComponent<ToastMessage>();
        if (toast != null)
        {
            toast.Show(message, color);
        }
        
        Debug.Log($"📢 Toast: {message}");
    }
    
    /// <summary>
    /// Affiche un toast d'incident
    /// </summary>
    public void ShowIncidentToast(string incidentMessage)
    {
        ShowToast(incidentMessage, Color.red);
    }
    
    /// <summary>
    /// Affiche un toast de succès
    /// </summary>
    public void ShowSuccessToast(string message)
    {
        ShowToast(message, Color.green);
    }
    
    /// <summary>
    /// Affiche un toast d'avertissement
    /// </summary>
    public void ShowWarningToast(string message)
    {
        ShowToast(message, Color.yellow);
    }
    
    /// <summary>
    /// Met à jour les informations de sélection
    /// </summary>
    public void UpdateSelectionInfo(string info)
    {
        if (selectionInfoText != null)
        {
            selectionInfoText.text = info;
        }
    }
}
