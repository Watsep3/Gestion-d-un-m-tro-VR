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
    
    [Header("Toast Messages")]
    public GameObject toastPrefab;
    public Transform toastContainer;
    
    /// <summary>
    /// Met à jour le dashboard global
    /// </summary>
    public void UpdateDashboard()
    {
        if (globalDashboard != null)
        {
            globalDashboard.UpdateDisplay();
        }
    }
    
    /// <summary>
    /// Affiche le panel d'information d'une station
    /// </summary>
    public void ShowStationPanel(StationData station)
    {
        if (stationPanel != null)
        {
            stationPanel.Show(station);
        }
        else
        {
            Debug.LogWarning("UIManager: StationPanel is null!");
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
        }
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
        
        Debug.Log(string.Format("Toast: {0}", message));
    }
}