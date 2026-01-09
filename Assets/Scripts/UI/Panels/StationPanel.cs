using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel d'information affiché quand on clique sur une station
/// </summary>
public class StationPanel : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI stationNameText;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI passengersText;
    public TextMeshProUGUI connectedLinesText;
    
    [Header("Buttons")]
    public Button repairButton;
    public Button closeButton;
    
    [Header("Colors")]
    public Color normalColor = Color.green;
    public Color delayedColor = Color.yellow;
    public Color brokenColor = Color.red;
    
    // Référence à la station actuellement affichée
    private StationData currentStation;
    private bool isRepairing = false;
    
    void Start()
    {
        // Lier les boutons
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
        
        if (repairButton != null)
        {
            repairButton.onClick.AddListener(OnRepairClicked);
        }
        
        // Cacher au démarrage
        Hide();
    }
    
    /// <summary>
    /// Affiche le panel avec les infos de la station
    /// </summary>
    public void Show(StationData station)
    {
        if (station == null)
        {
            Debug.LogWarning("StationPanel: Tentative d'affichage avec station null!");
            return;
        }
        
        currentStation = station;
        gameObject.SetActive(true);
        
        // Mettre à jour tous les textes
        UpdateDisplay();
        
        Debug.Log(string.Format("StationPanel ouvert pour: {0}", station.stationName));
    }
    
    /// <summary>
    /// Cache le panel
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        currentStation = null;
        isRepairing = false;
        
        Debug.Log("StationPanel fermé");
    }
    
    /// <summary>
    /// Met à jour l'affichage avec les données actuelles
    /// </summary>
    public void UpdateDisplay()
    {
        if (currentStation == null) return;
        
        // Récupérer les données à jour depuis le StationController
        StationController controller = FindStationController(currentStation.stationId);
        if (controller != null && controller.data != null)
        {
            currentStation = controller.data; // ✅ Utiliser directement la propriété 'data'
        }
        
        // Nom de la station
        if (stationNameText != null)
        {
            stationNameText.text = currentStation.stationName.ToUpper();
        }
        
        // État avec couleur
        if (statusText != null)
        {
            Color statusColor;
            string statusString;
            
            switch (currentStation.status)
            {
                case StationStatus.Normal:
                    statusColor = normalColor;
                    statusString = "NORMAL";
                    break;
                case StationStatus.Delayed:
                    statusColor = delayedColor;
                    statusString = "RETARD";
                    break;
                case StationStatus.Broken:
                    statusColor = brokenColor;
                    statusString = "PANNE";
                    break;
                default:
                    statusColor = Color.white;
                    statusString = "INCONNU";
                    break;
            }
            
            statusText.text = string.Format("État: {0}", statusString);
            statusText.color = statusColor;
        }
        
        // Passagers
        if (passengersText != null)
        {
            int passengers = currentStation.passengerCount;
            int maxPassengers = currentStation.maxPassengers;
            float percentage = (float)passengers / maxPassengers * 100f;
            
            passengersText.text = string.Format("Passagers: {0} / {1} ({2:F0}%)", 
                passengers, maxPassengers, percentage);
            
            // Couleur selon le taux de remplissage
            if (percentage > 90f)
                passengersText.color = brokenColor;
            else if (percentage > 70f)
                passengersText.color = delayedColor;
            else
                passengersText.color = normalColor;
        }
        
        // Lignes connectées
        if (connectedLinesText != null)
        {
            connectedLinesText.text = "Lignes: -";
        }
        
        // Visibilité du bouton réparer
        if (repairButton != null)
        {
            bool canRepair = currentStation.status == StationStatus.Broken && !isRepairing;
            repairButton.gameObject.SetActive(canRepair);
        }
    }
    
    /// <summary>
    /// Appelé quand on clique sur le bouton Réparer
    /// </summary>
    private void OnRepairClicked()
    {
        if (currentStation == null || isRepairing) return;
        
        Debug.Log(string.Format("Début réparation de {0}", currentStation.stationName));
        
        StartCoroutine(RepairStation());
    }
    
    /// <summary>
    /// Coroutine de réparation (5 secondes)
    /// </summary>
    private IEnumerator RepairStation()
    {
        isRepairing = true;
        
        // Désactiver le bouton pendant la réparation
        if (repairButton != null)
        {
            repairButton.interactable = false;
            
            // Changer le texte
            TextMeshProUGUI buttonText = repairButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Réparation...";
            }
        }
        
        // Attendre 5 secondes
        yield return new WaitForSeconds(5f);
        
        // Réparer la station
        if (currentStation != null)
        {
            // Trouver le StationController
            StationController controller = FindStationController(currentStation.stationId);
            
            if (controller != null)
            {
                controller.SetStatus(StationStatus.Normal);
            }
            
            // Mettre à jour MetroSystemManager
            GameManager.Instance.metroSystem.UpdateStationStatus(
                currentStation.stationId, 
                StationStatus.Normal
            );
            
            // Décrémenter le compteur de retards
            GameManager.Instance.DecrementDelayCount();
            
            Debug.Log(string.Format("{0} réparée!", currentStation.stationName));
            
            // Afficher un toast (si disponible)
            UIManager uiManager = GameManager.Instance.uiManager;
            if (uiManager != null)
            {
                uiManager.ShowToast(string.Format("{0} réparée!", currentStation.stationName), Color.green);
            }
        }
        
        // Fermer le panel
        Hide();
    }
    
    /// <summary>
    /// Trouve le StationController correspondant à un ID
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
    /// Update appelé chaque frame pour rafraîchir les données
    /// </summary>
    void Update()
    {
        // Si le panel est ouvert, mettre à jour l'affichage
        if (gameObject.activeSelf && currentStation != null)
        {
            UpdateDisplay();
        }
    }
}