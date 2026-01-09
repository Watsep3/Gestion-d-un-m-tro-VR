using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Panel d'information pour afficher les détails d'un train
/// </summary>
public class TrainPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI trainIdText;
    public TextMeshProUGUI lineIdText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI passengersText;
    public TextMeshProUGUI currentStationText;
    public TextMeshProUGUI nextStationText;
    public TextMeshProUGUI speedText;
    
    [Header("Status Colors")]
    public Color movingColor = Color.green;
    public Color stoppedColor = Color.yellow;
    public Color maintenanceColor = Color.red;
    
    [Header("Action Buttons")]
    public Button repairButton;
    
    private TrainData currentTrain;
    
    void Start()
    {
        // Cacher le panel au démarrage
        Hide();
        
        // Configurer le bouton de réparation
        if (repairButton != null)
        {
            repairButton.onClick.AddListener(OnRepairButtonClicked);
        }
    }
    
    /// <summary>
    /// Affiche le panel avec les données du train
    /// </summary>
    public void Show(TrainData train)
    {
        if (train == null)
        {
            Debug.LogWarning("TrainPanel: train data is null!");
            return;
        }
        
        currentTrain = train;
        
        // Mettre à jour les textes
        if (trainIdText != null)
            trainIdText.text = $"Train: {train.trainId}";
        
        if (lineIdText != null)
            lineIdText.text = $"Ligne: {train.lineId}";
        
        if (statusText != null)
        {
            statusText.text = $"État: {train.status}";
            
            // Changer la couleur selon le status
            switch (train.status)
            {
                case TrainStatus.Moving:
                    statusText.color = movingColor;
                    break;
                case TrainStatus.Stopped:
                    statusText.color = stoppedColor;
                    break;
                case TrainStatus.Maintenance:
                    statusText.color = maintenanceColor;
                    break;
            }
        }
        
        if (passengersText != null)
            passengersText.text = $"Passagers: {train.currentPassengers}/{train.passengerCapacity}";
        
        if (currentStationText != null)
            currentStationText.text = $"Station actuelle: {train.currentStationId}";
        
        if (nextStationText != null)
            nextStationText.text = $"Prochaine station: {train.nextStationId}";
        
        if (speedText != null)
            speedText.text = $"Vitesse: {train.speed} m/s";
        
        // Afficher/cacher le bouton de réparation
        if (repairButton != null)
        {
            repairButton.gameObject.SetActive(train.status == TrainStatus.Maintenance);
        }
        
        // Activer le panel
        gameObject.SetActive(true);
        
        Debug.Log($"✅ TrainPanel affiché pour {train.trainId}");
    }
    
    /// <summary>
    /// Cache le panel
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        currentTrain = null;
        Debug.Log("❌ TrainPanel caché");
    }
    
    /// <summary>
    /// Met à jour l'affichage (appelé périodiquement)
    /// </summary>
    public void UpdateDisplay()
    {
        if (currentTrain != null && gameObject.activeSelf)
        {
            Show(currentTrain);
        }
    }
    
    /// <summary>
    /// Appelé quand on clique sur le bouton de réparation
    /// </summary>
    private void OnRepairButtonClicked()
    {
        if (currentTrain == null) return;
        
        Debug.Log($"🔧 Tentative de réparation du train {currentTrain.trainId}");
        
        // Trouver le TrainController dans la scène
        TrainController[] allTrains = FindObjectsOfType<TrainController>();
        
        foreach (var train in allTrains)
        {
            if (train.trainId == currentTrain.trainId)
            {
                train.Resume();
                
                // Afficher un toast de succès
                if (GameManager.Instance != null && GameManager.Instance.uiManager != null)
                {
                    GameManager.Instance.uiManager.ShowSuccessToast($"Train {currentTrain.trainId} réparé!");
                }
                
                // Mettre à jour l'affichage
                UpdateDisplay();
                break;
            }
        }
    }
    
    /// <summary>
    /// Appelé quand on clique sur le bouton de fermeture
    /// </summary>
    public void OnCloseButtonClicked()
    {
        Hide();
        
        // Désélectionner l'objet
        if (GameManager.Instance != null && GameManager.Instance.interactionManager != null)
        {
            GameManager.Instance.interactionManager.DeselectObject();
        }
    }
}